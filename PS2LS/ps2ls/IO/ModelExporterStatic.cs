using OpenTK;
using ps2ls.Assets.Dme;
using ps2ls.Assets.Pack;
using ps2ls.Graphics.Materials;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Collada141;

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
            Obj,
            Dae
        }

        public static string outputDirectory;

        public class ExportFormatInfo
        {
            public ExportFormats ExportFormat { get; internal set; }
            public string Name { get; internal set; }
            public string Extension { get; internal set; }
            public bool CanExportNormals { get; internal set; }
            public bool CanExportTextureCoordinates { get; internal set; }
            public bool CanExportBones { get; internal set; }
            public bool CanExportTexutres { get; internal set; }
            public bool CanExportMaterials { get; internal set; }

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
            public bool Bones;
            public bool Materials;
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
            ExportFormatInfo objFormat = new ExportFormatInfo();

            objFormat.ExportFormat = ExportFormats.Obj;
            objFormat.Name = "Wavefront OBJ";
            objFormat.CanExportNormals = false;
            objFormat.CanExportTextureCoordinates = true;
            objFormat.CanExportBones = false;
            objFormat.CanExportMaterials = false;
            objFormat.CanExportTexutres = false;
            ExportFormatInfos.Add(ExportFormats.Obj, objFormat);

            ExportFormatInfo daeFormat = new ExportFormatInfo();
            daeFormat.ExportFormat = ExportFormats.Dae;
            daeFormat.Name = "Collada";
            daeFormat.CanExportNormals = false;
            daeFormat.CanExportTextureCoordinates = true;
            daeFormat.CanExportBones = true;
            daeFormat.CanExportMaterials = true;
            daeFormat.CanExportTexutres = true;

            ExportFormatInfos.Add(ExportFormats.Dae, daeFormat);

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

            //try
            //{
            switch (exportOptions.ExportFormatInfo.ExportFormat)
            {
                case ExportFormats.Obj:
                    exportModelAsOBJToDirectory(model, directory, exportOptions);
                    break;
                case ExportFormats.Dae:
                    exportModelAsDAEToDirectory(model, directory, exportOptions);
                    break;
            }
            /* }
             catch (Exception ex)
             {
                 Console.WriteLine("Failed to export " + model.Name);
                 Console.WriteLine(ex.StackTrace);
             }*/



        }

        static readonly NumberFormatInfo decimalFormat = new NumberFormatInfo()
        {
            NumberDecimalSeparator = ".",
        };

        private static void exportModelAsOBJToDirectory(Model model, string directory, ExportOptions options)
        {
            //TODO: Figure out what to do with non-version 4 models.
            if (model != null && model.Version != 4)
            {
                return;
            }

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

            for (int i = 0; i < model.Meshes.Length; ++i)
            {
                Mesh mesh = model.Meshes[i];

                //positions
                VertexLayout vertexLayout = getVertexLayoutFromMesh(model.Materials, mesh.drawCallOffset);
                Vector3[] positionBuffer = getPositionBuffer(mesh, vertexLayout, options, out bool _, out _);

                foreach (Vector3 v3 in positionBuffer)
                {
                    sw.WriteLine("v " + v3.X.ToString(decimalFormat) + " " + v3.Y.ToString(decimalFormat) + " " + v3.Z.ToString(decimalFormat));
                }


                //texture coordinates
                if (options.TextureCoordinates)
                {
                    Vector2[] texCoordBuffer = getTextureCoords0Buffer(mesh, vertexLayout, options, out bool hasCoords, out _);
                    if (hasCoords)
                    {
                        foreach (Vector2 v2 in texCoordBuffer)
                        {
                            sw.WriteLine("vt " + v2.X.ToString(decimalFormat) + " " + v2.Y.ToString(decimalFormat));
                        }
                    }

                }
            }

            //faces
            uint vertexCount = 0;

            for (int i = 0; i < model.Meshes.Length; ++i)
            {
                Mesh mesh = model.Meshes[i];

                sw.WriteLine("g Mesh" + i);

                UIntSet[] indexBuffer = getIndexBuffer(mesh);

                foreach(UIntSet uis in indexBuffer)
                {
                    uint index0 = uis.x + vertexCount + 1;
                    uint index1 = uis.y + vertexCount + 1;
                    uint index2 = uis.z + vertexCount + 1;
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

                vertexCount += mesh.VertexCount;
            }

            sw.Close();

        }

        public static VertexLayout getVertexLayoutFromMesh(List<Assets.Dma.Material> Materials, uint meshDrawCallOffset)
        {
            uint materialHash = Materials[(Int32)meshDrawCallOffset].MaterialDefinitionHash;
            if (MaterialDefinitionManager.Instance.MaterialDefinitions.ContainsKey(materialHash))
            {
                MaterialDefinition materialDefinition = MaterialDefinitionManager.Instance.MaterialDefinitions[materialHash];
                return MaterialDefinitionManager.Instance.VertexLayouts[materialDefinition.DrawStyles[0].VertexLayoutNameHash];
            }
            else
            {
                Console.WriteLine("Missing Material: " + materialHash.ToString("X"));
                return null;
            }
        }

        public static Vector3[] getPositionBuffer(Mesh mesh, VertexLayout vertexLayout, ExportOptions options, out bool hasPositions, out int bytesPerVertex)
        {
            bytesPerVertex = 0;
            hasPositions = vertexLayout != null;
            if (!hasPositions) return new Vector3[0];
            hasPositions = vertexLayout.GetEntryInfoFromDataUsageAndUsageIndex(VertexLayout.Entry.DataUsages.Position, 0, out VertexLayout.Entry.DataTypes dataType, out int streamIndex, out int streamOffset);
            if (!hasPositions) return new Vector3[0];

            Mesh.VertexStream positionStream = mesh.VertexStreams[streamIndex];
            bytesPerVertex = positionStream.BytesPerVertex;
            Vector3[] buffer = new Vector3[mesh.VertexCount];
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = readVector3(options, streamOffset, positionStream, i);

                buffer[i].X *= options.Scale.X;
                buffer[i].Y *= options.Scale.Y;
                buffer[i].Z *= options.Scale.Z;

            }

            return buffer;
        }

        static Vector3 readVector3(ExportOptions exportOptions, Int32 offset, Mesh.VertexStream vertexStream, Int32 index)
        {
            Vector3 vector3 = new Vector3();

            int baseOffset = (vertexStream.BytesPerVertex * index) + offset;
            float value = BitConverter.ToSingle(vertexStream.Data, baseOffset);
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

            value = BitConverter.ToSingle(vertexStream.Data, baseOffset + 4);
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
            value = BitConverter.ToSingle(vertexStream.Data, baseOffset + 8);
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

        public static Vector2[] getTextureCoords0Buffer(Mesh mesh, VertexLayout vertexLayout, ExportOptions options, out bool hasTexCoords0, out int bytesPerVertex)
        {
            bytesPerVertex = 0;
            hasTexCoords0 = vertexLayout != null;
            if (!hasTexCoords0) return new Vector2[0];
            hasTexCoords0 = vertexLayout.GetEntryInfoFromDataUsageAndUsageIndex(VertexLayout.Entry.DataUsages.Texcoord, 0, out VertexLayout.Entry.DataTypes dataType, out int streamIndex, out int streamOffset);
            if (!hasTexCoords0) return new Vector2[0];

            Mesh.VertexStream texCoord0Stream = mesh.VertexStreams[streamIndex];
            bytesPerVertex = texCoord0Stream.BytesPerVertex;
            Vector2[] buffer = new Vector2[mesh.VertexCount];
            for (int i = 0; i < buffer.Length; i++)
            {
                Vector2 texCoord;
                int baseOffset = (i * texCoord0Stream.BytesPerVertex) + streamOffset;
                switch (dataType)
                {
                    case VertexLayout.Entry.DataTypes.Float2:
                        texCoord.X = BitConverter.ToSingle(texCoord0Stream.Data, baseOffset);
                        texCoord.Y = 1.0f - BitConverter.ToSingle(texCoord0Stream.Data, baseOffset + 4);
                        break;
                    case VertexLayout.Entry.DataTypes.float16_2:
                        texCoord.X = Half.FromBytes(texCoord0Stream.Data, baseOffset).ToSingle();//index out of range
                        texCoord.Y = 1.0f - Half.FromBytes(texCoord0Stream.Data, baseOffset + 2).ToSingle();
                        break;
                    default:
                        texCoord.X = 0;
                        texCoord.Y = 0;
                        break;
                }
                buffer[i] = texCoord;
            }
            return buffer;
        }

        public static UIntSet[] getIndexBuffer(Mesh mesh)
        {
            UIntSet[] buffer = new UIntSet[mesh.IndexCount / 3];
            int indexSize = (int)mesh.IndexSize;
            int doubleIndexSize = indexSize + indexSize;
            for (int i = 0; i < buffer.Length; i++)
            {
                uint index0, index1, index2;
                int baseOffset = i * 3 * indexSize;
                switch (mesh.IndexSize)
                {
                    case 2:
                        index0 = BitConverter.ToUInt16(mesh.IndexData, baseOffset);
                        index1 = BitConverter.ToUInt16(mesh.IndexData, baseOffset + indexSize);
                        index2 = BitConverter.ToUInt16(mesh.IndexData, baseOffset + doubleIndexSize);
                        break;
                    case 4:
                        index0 = BitConverter.ToUInt32(mesh.IndexData, baseOffset);
                        index1 = BitConverter.ToUInt32(mesh.IndexData, baseOffset + indexSize);
                        index2 = BitConverter.ToUInt32(mesh.IndexData, baseOffset + doubleIndexSize);
                        break;
                    default:
                        index0 = 0;
                        index1 = 0;
                        index2 = 0;
                        break;
                }
                buffer[i] = new UIntSet(index0, index1, index2);
            }
            return buffer;
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

        private static void exportModelAsDAEToDirectory(Model model, string directory, ExportOptions options)
        {
            // Load the Collada model
            COLLADA coll = new COLLADA();

            asset collAsset = new asset();
            collAsset.contributor = new assetContributor[]
            {
                new assetContributor()
                {
                    author = "ps2ls2 user",
                    authoring_tool ="ps2ls2"
                }
            };
            collAsset.created = DateTime.Now;
            collAsset.modified = DateTime.Now;
            collAsset.unit = new assetUnit();
            collAsset.up_axis = UpAxisType.Y_UP;
            coll.asset = collAsset;

            string cleanName = Path.GetFileNameWithoutExtension(model.Name);
            library_geometries libGeometries = new library_geometries();
            List<geometry> geometryList = new List<geometry>();
            List<node> nodeList = new List<node>();
            foreach (Mesh mesh in model.Meshes)
            {
                VertexLayout vertexLayout = getVertexLayoutFromMesh(model.Materials, mesh.drawCallOffset);

                string meshName = cleanName + "_" + mesh.drawCallOffset;
                geometryList.Add(ColladaBuilderStatic.createGeometryFromMesh(mesh, meshName, vertexLayout, options));

                node meshNode = new node()
                {
                    id = meshName,
                    name = meshName,
                    type = NodeType.NODE,
                };
                matrix nodeMatrix = new matrix()
                {
                    sid = "transform",
                    _Text_ = "1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1",//no transform
                };
                meshNode.Items = new object[] { nodeMatrix };
                meshNode.ItemsElementName = new ItemsChoiceType2[] { ItemsChoiceType2.matrix };
                meshNode.instance_geometry = new instance_geometry[]
                {
                    new instance_geometry()
                    {
                        url = "#" + meshNode.id + "-mesh",
                        name = meshNode.name,
                    },
                };

                nodeList.Add(meshNode);
            }
            libGeometries.geometry = geometryList.ToArray();

            library_visual_scenes libVisualScenes = new library_visual_scenes();
            visual_scene visualScene = new visual_scene()
            {
                id = "Scene",
                name = "Scene",
            };
            visualScene.node = nodeList.ToArray();
            libVisualScenes.visual_scene = new visual_scene[] { visualScene };

            coll.Items = new object[] { libGeometries, libVisualScenes };

            COLLADAScene cScene = new COLLADAScene();
            cScene.instance_visual_scene = new InstanceWithExtra
            {
                url = "#" + libVisualScenes.visual_scene[0].id,
            };
            coll.scene = cScene;

            string path = directory + @"\" + Path.GetFileNameWithoutExtension(model.Name) + ".dae";
            // Save the model
            Console.WriteLine("collada structure built, saving at" + path);
            coll.Save(path);
        }
    }

    public struct UIntSet
    {
        public readonly uint x, y, z;
        public UIntSet(uint getX, uint getY, uint getZ)
        {
            x = getX;
            y = getY;
            z = getZ;
        }
    }
}
