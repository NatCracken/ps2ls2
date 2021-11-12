using OpenTK;
using ps2ls.Assets.Dme;
using ps2ls.Assets.Pack;
using ps2ls.Graphics.Materials;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace ps2ls.IO
{
    public static class ModelExporterStatic
    {
        public enum Axes
        {
            X,
            Y,
            Z
        }

        public enum ExportFormats
        {
            Obj
        }

        public static string outputDirectory;

        public class ExportFormatInfo
        {
            public ExportFormats ExportFormat { get; internal set; }
            public string Name { get; internal set; }
            public string Extension { get; internal set; }
            public bool CanExportNormals { get; internal set; }
            public bool CanExportTextureCoordinates { get; internal set; }

            public override string ToString()
            {
                return Name + @" (*." + Extension + @")";
            }
        }

        public class ExportOptions
        {
            public Axes UpAxis;
            public Axes LeftAxis;
            public bool Normals;
            public bool TextureCoordinates;
            public Vector3 Scale;
            public bool Textures;
            public bool Package;
            public ExportFormatInfo ExportFormatInfo;
            public TextureExporterStatic.TextureFormatInfo TextureFormat;
        }

        public struct ModelAxesPreset
        {
            public string Name;
            public Axes UpAxis;
            public Axes LeftAxis;

            public override string ToString()
            {
                return Name;
            }
        }

        private static Axes getForwardAxis(Axes leftAxis, Axes upAxis)
        {
            if (leftAxis != Axes.X && upAxis != Axes.X)
                return Axes.X;
            else if (leftAxis != Axes.Y && upAxis != Axes.Y)
                return Axes.Y;
            else
                return Axes.Z;
        }

        public static Dictionary<ExportFormats, ExportFormatInfo> ExportFormatInfos;

        public static List<ModelAxesPreset> ModelAxesPresets { get; private set; }

        static ModelExporterStatic()
        {
            ExportFormatInfos = new Dictionary<ExportFormats, ExportFormatInfo>();

            createExportFormatOptions();
            createModelAxesPresets();
        }

        private static void createExportFormatOptions()
        {
            ExportFormatInfo exportFormat = new ExportFormatInfo();

            exportFormat.ExportFormat = ExportFormats.Obj;
            exportFormat.Name = "Wavefront OBJ (*.obj)";
            exportFormat.CanExportNormals = false;
            exportFormat.CanExportTextureCoordinates = true;

            ExportFormatInfos.Add(ExportFormats.Obj, exportFormat);
        }

        private static void createModelAxesPresets()
        {
            ModelAxesPresets = new List<ModelAxesPreset>();

            ModelAxesPreset modelAxesPreset = new ModelAxesPreset();
            modelAxesPreset.Name = "Default";
            modelAxesPreset.UpAxis = Axes.Y;
            modelAxesPreset.LeftAxis = Axes.X;
            ModelAxesPresets.Add(modelAxesPreset);

            modelAxesPreset = new ModelAxesPreset();
            modelAxesPreset.Name = "Autodesk® 3ds Max";
            modelAxesPreset.UpAxis = Axes.Z;
            modelAxesPreset.LeftAxis = Axes.Y;
            ModelAxesPresets.Add(modelAxesPreset);
        }

        public static void ExportModelToDirectory(Model model, string directory, ExportOptions exportOptions)
        {

            try
            {
                exportModelAsOBJToDirectory(model, directory, exportOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to export " + model.Name);
                Console.WriteLine(ex.StackTrace);
            }

            /*//TODO - support other formats
            switch (exportOptions.ExportFormatInfo.ExportFormat)
            {
                case ExportFormats.Obj:
                    exportModelAsOBJToDirectory(model, directory, exportOptions);
                    break;
                //case ModelExportFormats.STL:
                //    exportModelAsSTLToDirectory(model, directory, formatOptions.Options);
                //    break;
            }*/
        }

        private static void exportModelAsOBJToDirectory(Model model, string directory, ExportOptions options)
        {
            //TODO: Figure out what to do with non-version 4 models.
            if (model != null && model.Version != 4)
            {
                return;
            }

            NumberFormatInfo format = new NumberFormatInfo();
            format.NumberDecimalSeparator = ".";

            if (options.Package)
            {
                try
                {
                    DirectoryInfo directoryInfo = Directory.CreateDirectory(directory + @"\" + Path.GetFileNameWithoutExtension(model.Name));
                    directory = directoryInfo.FullName;
                }
                catch (Exception) { }
            }

            if (options.Textures)
            {
                foreach (string textureString in model.TextureStrings)
                {
                    TextureExporterStatic.exportTexture(textureString, directory, options.TextureFormat);
                }
            }

            exportBonesAsTextToDirectory(model, directory);

            String path = directory + @"\" + Path.GetFileNameWithoutExtension(model.Name) + ".obj";

            FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write);
            StreamWriter sw = new StreamWriter(fileStream);

            for (Int32 i = 0; i < model.Meshes.Length; ++i)
            {
                Mesh mesh = model.Meshes[i];

                uint materialHash = model.Materials[(Int32)mesh.drawCallOffset].MaterialDefinitionHash;
                VertexLayout vertexLayout = null;
                if (MaterialDefinitionManager.Instance.MaterialDefinitions.ContainsKey(materialHash))
                {
                    MaterialDefinition materialDefinition = MaterialDefinitionManager.Instance.MaterialDefinitions[materialHash];
                    vertexLayout = MaterialDefinitionManager.Instance.VertexLayouts[materialDefinition.DrawStyles[0].VertexLayoutNameHash];
                }
                else
                {
                    Console.WriteLine("Missing Material: " + materialHash.ToString("X"));
                }

                //position
                VertexLayout.Entry.DataTypes positionDataType;
                Int32 positionOffset = 0;
                Int32 positionStreamIndex = 0;

                bool hasPositions = vertexLayout == null ? false : vertexLayout.GetEntryInfoFromDataUsageAndUsageIndex(VertexLayout.Entry.DataUsages.Position, 0, out positionDataType, out positionStreamIndex, out positionOffset);

                Mesh.VertexStream positionStream = mesh.VertexStreams[positionStreamIndex];

                for (Int32 j = 0; j < mesh.VertexCount; ++j)
                {
                    Vector3 position = readVector3(options, positionOffset, positionStream, j);

                    position.X *= options.Scale.X;
                    position.Y *= options.Scale.Y;
                    position.Z *= options.Scale.Z;

                    sw.WriteLine("v " + position.X.ToString(format) + " " + position.Y.ToString(format) + " " + position.Z.ToString(format));
                }

                //texture coordinates
                if (options.TextureCoordinates)
                {
                    VertexLayout.Entry.DataTypes texCoord0DataType = VertexLayout.Entry.DataTypes.None;
                    Int32 texCoord0Offset = 0;
                    Int32 texCoord0StreamIndex = 0;

                    Boolean texCoord0Present = vertexLayout == null ? false : vertexLayout.GetEntryInfoFromDataUsageAndUsageIndex(VertexLayout.Entry.DataUsages.Texcoord, 0, out texCoord0DataType, out texCoord0StreamIndex, out texCoord0Offset);

                    if (texCoord0Present)
                    {
                        Mesh.VertexStream texCoord0Stream = mesh.VertexStreams[texCoord0StreamIndex];

                        for (Int32 j = 0; j < mesh.VertexCount; ++j)
                        {
                            Vector2 texCoord;

                            switch (texCoord0DataType)
                            {
                                case VertexLayout.Entry.DataTypes.Float2:
                                    texCoord.X = BitConverter.ToSingle(texCoord0Stream.Data, (j * texCoord0Stream.BytesPerVertex) + 0);
                                    texCoord.Y = 1.0f - BitConverter.ToSingle(texCoord0Stream.Data, (j * texCoord0Stream.BytesPerVertex) + 4);
                                    break;
                                case VertexLayout.Entry.DataTypes.float16_2:
                                    texCoord.X = Half.FromBytes(texCoord0Stream.Data, (j * texCoord0Stream.BytesPerVertex) + texCoord0Offset + 0).ToSingle();//index out of range
                                    texCoord.Y = 1.0f - Half.FromBytes(texCoord0Stream.Data, (j * texCoord0Stream.BytesPerVertex) + texCoord0Offset + 2).ToSingle();
                                    break;
                                default:
                                    texCoord.X = 0;
                                    texCoord.Y = 0;
                                    break;
                            }

                            sw.WriteLine("vt " + texCoord.X.ToString(format) + " " + texCoord.Y.ToString(format));
                        }
                    }
                }
            }

            //faces
            UInt32 vertexCount = 0;

            for (Int32 i = 0; i < model.Meshes.Length; ++i)
            {
                Mesh mesh = model.Meshes[i];

                sw.WriteLine("g Mesh" + i);

                for (Int32 j = 0; j < mesh.IndexCount; j += 3)
                {
                    UInt32 index0, index1, index2;

                    switch (mesh.IndexSize)
                    {
                        case 2:
                            index0 = vertexCount + BitConverter.ToUInt16(mesh.IndexData, (j * 2) + 0) + 1;
                            index1 = vertexCount + BitConverter.ToUInt16(mesh.IndexData, (j * 2) + 2) + 1;
                            index2 = vertexCount + BitConverter.ToUInt16(mesh.IndexData, (j * 2) + 4) + 1;
                            break;
                        case 4:
                            index0 = vertexCount + BitConverter.ToUInt32(mesh.IndexData, (j * 4) + 0) + 1;
                            index1 = vertexCount + BitConverter.ToUInt32(mesh.IndexData, (j * 4) + 4) + 1;
                            index2 = vertexCount + BitConverter.ToUInt32(mesh.IndexData, (j * 4) + 8) + 1;
                            break;
                        default:
                            index0 = 0;
                            index1 = 0;
                            index2 = 0;
                            break;
                    }

                    if (options.Normals && options.TextureCoordinates)
                    {
                        sw.WriteLine("f " + index2 + "/" + index2 + "/" + index2 + " " + index1 + "/" + index1 + "/" + index1 + " " + index0 + "/" + index0 + "/" + index0);
                    }
                    else if (options.Normals)
                    {
                        sw.WriteLine("f " + index2 + "//" + index2 + " " + index1 + "//" + index1 + " " + index0 + "//" + index0);
                    }
                    else if (options.TextureCoordinates)
                    {
                        sw.WriteLine("f " + index2 + "/" + index2 + " " + index1 + "/" + index1 + " " + index0 + "/" + index0);
                    }
                    else
                    {
                        sw.WriteLine("f " + index2 + " " + index1 + " " + index0);
                    }
                }

                vertexCount += (UInt32)mesh.VertexCount;
            }

            sw.Close();

        }

        private static Vector3 readVector3(ExportOptions exportOptions, Int32 offset, Mesh.VertexStream vertexStream, Int32 index)
        {
            Vector3 vector3 = new Vector3();

            float value = BitConverter.ToSingle(vertexStream.Data, (vertexStream.BytesPerVertex * index) + offset + 0);
            switch (exportOptions.LeftAxis)
            {
                case Axes.X:
                    vector3.X = value;
                    break;
                case Axes.Y:
                    vector3.Y = value;
                    break;
                case Axes.Z:
                    vector3.Z = value;
                    break;
            }

            value = BitConverter.ToSingle(vertexStream.Data, (vertexStream.BytesPerVertex * index) + offset + 4);
            switch (exportOptions.UpAxis)
            {
                case Axes.X:
                    vector3.X = value;
                    break;
                case Axes.Y:
                    vector3.Y = value;
                    break;
                case Axes.Z:
                    vector3.Z = value;
                    break;
            }

            Axes forwardAxis = getForwardAxis(exportOptions.LeftAxis, exportOptions.UpAxis);
            value = BitConverter.ToSingle(vertexStream.Data, (vertexStream.BytesPerVertex * index) + offset + 8);
            switch (forwardAxis)
            {
                case Axes.X:
                    vector3.X = value;
                    break;
                case Axes.Y:
                    vector3.Y = value;
                    break;
                case Axes.Z:
                    vector3.Z = value;
                    break;
            }

            return vector3;
        }

        private static void exportModelAsSTLToDirectory(Model model, string directory, ExportOptions options)
        {
            //NumberFormatInfo format = new NumberFormatInfo();
            //format.NumberDecimalSeparator = ".";

            //String path = directory + @"\" + Path.GetFileNameWithoutExtension(model.Name) + ".stl";

            //FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write);
            //StreamWriter streamWriter = new StreamWriter(fileStream);

            //for (Int32 i = 0; i < model.Meshes.Length; ++i)
            //{
            //    Mesh mesh = model.Meshes[i];

            //    for (Int32 j = 0; j < mesh.Indices.Length; j += 3)
            //    {
            //        Vector3 normal = Vector3.Zero;
            //        normal += mesh.Vertices[mesh.Indices[j + 0]].Normal;
            //        normal += mesh.Vertices[mesh.Indices[j + 1]].Normal;
            //        normal += mesh.Vertices[mesh.Indices[j + 2]].Normal;
            //        normal.Normalize();

            //        streamWriter.WriteLine("facet normal " + normal.X.ToString("E", format) + " " + normal.Y.ToString("E", format) + " " + normal.Z.ToString("E", format));
            //        streamWriter.WriteLine("outer loop");

            //        for (Int32 k = 0; k < 3; ++k)
            //        {
            //            Vector3 vertex = mesh.Vertices[mesh.Indices[j + k]].Position;

            //            streamWriter.WriteLine("vertex " + vertex.X.ToString("E", format) + " " + vertex.Y.ToString("E", format) + " " + vertex.Z.ToString("E", format));
            //        }

            //        streamWriter.WriteLine("endloop");
            //        streamWriter.WriteLine("endfacet");
            //    }
            //}

            //streamWriter.Close();
        }

        private static void exportBonesAsTextToDirectory(Model model, string directory)
        {
            String path = directory + @"\" + Path.GetFileNameWithoutExtension(model.Name) + ".txt";

            FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write);
            StreamWriter sw = new StreamWriter(fileStream);

            sw.WriteLine("-------------------------------------------------");
            sw.WriteLine("--------------####BoneDrawCalls###---------------");
            sw.WriteLine("-------------------------------------------------");
            foreach (BoneDrawCall bm in model.BoneDrawCalls)
            {
                sw.WriteLine(bm.Unknown0
                + ", " + bm.BoneStart
                + ", " + bm.BoneCount
                + ", " + bm.Delta
                + ", " + bm.Unknown1
                + ", " + bm.VertexOffset
                + ", " + bm.VertexCount
                + ", " + bm.IndexOffset
                + ", " + bm.IndexCount);
            }
            sw.WriteLine("-------------------------------------------------");
            sw.WriteLine("----------------####MapEntries###----------------");
            sw.WriteLine("-------------------------------------------------");
            foreach (BoneMapEntry bme in model.BoneMapEntries) sw.WriteLine(bme.BoneIndex + ", " + bme.GlobalIndex);
            sw.Close();
        }
    }
}
