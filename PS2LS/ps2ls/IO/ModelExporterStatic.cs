using OpenTK;
using ps2ls.Assets;
using ps2ls.Graphics.Materials;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Collada141;
using SharpGLTF;
using SharpGLTF.Geometry;
using SharpGLTF.Scenes;
using SharpGLTF.Materials;
using SharpGLTF.Geometry.VertexTypes;

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
            Dae,
            glTF2,
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
            ExportFormatInfos.Add(ExportFormats.Obj, new ExportFormatInfo
            {
                ExportFormat = ExportFormats.Obj,
                Name = "Wavefront OBJ",
                CanExportNormals = false,
                CanExportTextureCoordinates = true,
                CanExportBones = false,
                CanExportMaterials = false,
                CanExportTexutres = false
            });

            /*ExportFormatInfos.Add(ExportFormats.Dae, new ExportFormatInfo
            {
                ExportFormat = ExportFormats.Dae,
                Name = "Collada",
                CanExportNormals = true,
                CanExportTextureCoordinates = true,
                CanExportBones = false,
                CanExportMaterials = false,
                CanExportTexutres = false
            });*/

            ExportFormatInfos.Add(ExportFormats.glTF2, new ExportFormatInfo
            {
                ExportFormat = ExportFormats.glTF2,
                Name = "GL Transmission Format",
                CanExportNormals = true,
                CanExportTextureCoordinates = true,
                CanExportBones = false,
                CanExportMaterials = false,
                CanExportTexutres = false
            });
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
            //TODO: Figure out what to do with non-version 4 models.
            if (model != null && model.version != 4)
            {
                return;
            }

            //try
            //{
            switch (exportOptions.ExportFormatInfo.ExportFormat)
            {
                case ExportFormats.Obj:
                    ExportModelAsOBJToDirectory(model, directory, exportOptions);
                    break;
                case ExportFormats.Dae:
                    //exportModelAsDAEToDirectory(model, directory, exportOptions);
                    break;
                case ExportFormats.glTF2:
                    ExportModelAsGLTF2ToDirectory(model, directory, exportOptions);
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

        private static void ExportModelAsOBJToDirectory(Model model, string directory, ExportOptions options)
        {
            PackageDirectory(model.name, ref directory, options);

            ExportLinkedTextures(model, directory, options);
#if DEBUG
            ExportBonesAsTextToDirectory(model, directory);
#endif
            string path = directory + @"\" + Path.GetFileNameWithoutExtension(model.name) + ".obj";

            FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write);
            StreamWriter sw = new StreamWriter(fileStream);

            for (int i = 0; i < model.meshes.Length; ++i)
            {
                Mesh mesh = model.meshes[i];

                //positions

                VertexLayout vertexLayout = GetVertexLayoutFromMaterialHash(model.dma.materials[(int)mesh.drawCallOffset].MaterialDefinitionHash);
                Vector3[] positionBuffer = GetPositionBuffer(mesh, vertexLayout, options, out bool _, out _);

                foreach (Vector3 v3 in positionBuffer)
                {
                    sw.WriteLine("v " + v3.X.ToString(decimalFormat) + " " + v3.Y.ToString(decimalFormat) + " " + v3.Z.ToString(decimalFormat));
                }


                //texture coordinates
                if (options.TextureCoordinates)
                {
                    Vector2[] texCoordBuffer = GetTextureCoords0Buffer(mesh, vertexLayout, options, out bool hasCoords, out _);
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

            for (int i = 0; i < model.meshes.Length; ++i)
            {
                Mesh mesh = model.meshes[i];

                sw.WriteLine("g Mesh" + i);

                UIntSet[] indexBuffer = GetIndexBuffer(mesh);

                foreach (UIntSet uis in indexBuffer)
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

                vertexCount += mesh.vertexCount;
            }

            sw.Close();

        }

        public static VertexLayout GetVertexLayoutFromMaterialHash(uint materialHash)
        {
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

        public static Vector3[] GetPositionBuffer(Mesh mesh, VertexLayout vertexLayout, ExportOptions options, out bool hasPositions, out int bytesPerVertex)
        {
            bytesPerVertex = 0;
            hasPositions = vertexLayout != null;
            if (!hasPositions) return new Vector3[0];
            hasPositions = vertexLayout.GetEntryInfoFromDataUsageAndUsageIndex(VertexLayout.Entry.DataUsages.Position, 0, out VertexLayout.Entry.DataTypes dataType, out int streamIndex, out int streamOffset);
            if (!hasPositions) return new Vector3[0];

            Mesh.VertexStream positionStream = mesh.vertexStreams[streamIndex];
            bytesPerVertex = positionStream.bytesPerVertex;
            Console.WriteLine(mesh.vertexCount + " - " + mesh.vertexCount + " = " + (mesh.vertexCount - mesh.vertexCount));
            Vector3[] buffer = new Vector3[mesh.vertexCount];
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = ReadByteVector3(options, positionStream, dataType, streamOffset, i);
            }

            return buffer;
        }

        static float ReadByteValue(Mesh.VertexStream vertexStream, VertexLayout.Entry.DataTypes dataType, ref int offset)
        {
            if (dataType == VertexLayout.Entry.DataTypes.Float3)
            {
                float value = BitConverter.ToSingle(vertexStream.data, offset);
                offset += 4;
                return value;
            }
            if (dataType == VertexLayout.Entry.DataTypes.ubyte4n)
            {
                float value = vertexStream.data[offset] / (float)byte.MaxValue;
                value *= 2;
                value -= 1;
                offset += 1;
                return value;
            }
            Console.WriteLine("Unknown Data Type: " + dataType);
            return 0f;
        }

        static Vector3 ReadByteVector3(ExportOptions exportOptions, Mesh.VertexStream vertexStream, VertexLayout.Entry.DataTypes dataType, int initialOffset, int vertexIndex)
        {
            Vector3 vector3 = new Vector3();

            int offset = initialOffset + (vertexStream.bytesPerVertex * vertexIndex);
            float value = ReadByteValue(vertexStream, dataType, ref offset);
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
            value = ReadByteValue(vertexStream, dataType, ref offset);
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
            value = ReadByteValue(vertexStream, dataType, ref offset);
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

            vector3 *= exportOptions.Scale;
            return vector3;
        }

        static Vector4 ReadByteVector4(ExportOptions exportOptions, Mesh.VertexStream vertexStream, VertexLayout.Entry.DataTypes dataType, int initialOffset, int vertexIndex)
        {
            Vector4 vector4 = new Vector4();

            int offset = initialOffset + (vertexStream.bytesPerVertex * vertexIndex);
            float value = ReadByteValue(vertexStream, dataType, ref offset);
            switch (exportOptions.LeftAxis)
            {
                case Axes.X:
                    vector4.X = value;
                    break;
                case Axes.Y:
                    vector4.Y = value;
                    break;
                case Axes.Z:
                    vector4.Z = value;
                    break;
            }
            value = ReadByteValue(vertexStream, dataType, ref offset);
            switch (exportOptions.UpAxis)
            {
                case Axes.X:
                    vector4.X = value;
                    break;
                case Axes.Y:
                    vector4.Y = value;
                    break;
                case Axes.Z:
                    vector4.Z = value;
                    break;
            }

            Axes forwardAxis = getForwardAxis(exportOptions.LeftAxis, exportOptions.UpAxis);
            value = ReadByteValue(vertexStream, dataType, ref offset);
            switch (forwardAxis)
            {
                case Axes.X:
                    vector4.X = value;
                    break;
                case Axes.Y:
                    vector4.Y = value;
                    break;
                case Axes.Z:
                    vector4.Z = value;
                    break;
            }

            if (dataType != VertexLayout.Entry.DataTypes.Float3)
            {
                vector4.W = ReadByteValue(vertexStream, dataType, ref offset);
            }
            else
            {
                vector4.W = 1;
            }

            return vector4;
        }

        public static readonly ExportOptions sampleOptions = new ExportOptions()
        {
            Scale = Vector3.One,
            UpAxis = Axes.Y,
            LeftAxis = Axes.X,
        };

        public static Vector3[] GetNormalBuffer(Mesh mesh, VertexLayout vertexLayout, out bool hasNormals, out int bytesPerVertex)
        {
            bytesPerVertex = 0;
            hasNormals = vertexLayout != null;
            if (!hasNormals) return null;
            hasNormals = vertexLayout.GetEntryInfoFromDataUsageAndUsageIndex(VertexLayout.Entry.DataUsages.Normal, 0, out VertexLayout.Entry.DataTypes dataType, out int streamIndex, out int streamOffset);
            if (!hasNormals) return GetNormalBufferFromCross(mesh, vertexLayout, out hasNormals, out bytesPerVertex);
            Mesh.VertexStream positionStream = mesh.vertexStreams[streamIndex];
            bytesPerVertex = positionStream.bytesPerVertex;
            Vector3[] buffer = new Vector3[mesh.vertexCount];
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = ReadByteVector3(sampleOptions, positionStream, dataType, streamOffset, i);
            }

            return buffer;
        }

        public static Vector3[] GetNormalBufferFromCross(Mesh mesh, VertexLayout vertexLayout, out bool hasNormals, out int bytesPerVertex)
        {
            Console.WriteLine("No Normals Found, attempting to reconstruct");
            bytesPerVertex = 0;
            hasNormals = vertexLayout != null;
            if (!hasNormals) return null;
            Vector4[] tangentBuffer = GetTangentBuffer(mesh, vertexLayout, out bool hasTangents, out int bytesPerTangent);
            Vector4[] biNormalBuffer = GetBiNormalBuffer(mesh, vertexLayout, out bool hasBiNormals, out int bytesPerBiNormals);
            hasNormals = hasTangents && hasBiNormals;
            if (!hasNormals)
            {

                Console.WriteLine("Failed to reconstruct");
                return null;
            }

            bytesPerVertex = 12;//since they're being converted to vector3 floats, 3 x 4
            Vector3[] buffer = new Vector3[mesh.vertexCount];
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = -Vector3.Cross(tangentBuffer[i].Xyz, biNormalBuffer[i].Xyz);
            }

            return buffer;
        }

        public static Vector4[] GetTangentBuffer(Mesh mesh, VertexLayout vertexLayout, out bool hasTangents, out int bytesPerVertex)
        {
            bytesPerVertex = 0;
            hasTangents = vertexLayout != null;
            if (!hasTangents) return null;
            hasTangents = vertexLayout.GetEntryInfoFromDataUsageAndUsageIndex(VertexLayout.Entry.DataUsages.Tangent, 0, out VertexLayout.Entry.DataTypes dataType, out int streamIndex, out int streamOffset);
            if (!hasTangents) return null;

            Mesh.VertexStream positionStream = mesh.vertexStreams[streamIndex];
            bytesPerVertex = positionStream.bytesPerVertex;
            Vector4[] buffer = new Vector4[mesh.vertexCount];
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = ReadByteVector4(sampleOptions, positionStream, dataType, streamOffset, i);
            }
            return buffer;
        }

        public static Vector4[] GetBiNormalBuffer(Mesh mesh, VertexLayout vertexLayout, out bool hasBiNormals, out int bytesPerVertex)
        {
            bytesPerVertex = 0;
            hasBiNormals = vertexLayout != null;
            if (!hasBiNormals) return null;
            hasBiNormals = vertexLayout.GetEntryInfoFromDataUsageAndUsageIndex(VertexLayout.Entry.DataUsages.Binormal, 0, out VertexLayout.Entry.DataTypes dataType, out int streamIndex, out int streamOffset);
            if (!hasBiNormals) return null;

            Mesh.VertexStream positionStream = mesh.vertexStreams[streamIndex];
            bytesPerVertex = positionStream.bytesPerVertex;
            Vector4[] buffer = new Vector4[mesh.vertexCount];
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = ReadByteVector4(sampleOptions, positionStream, dataType, streamOffset, i);
            }
            return buffer;
        }

        public static Vector2[] GetTextureCoords0Buffer(Mesh mesh, VertexLayout vertexLayout, ExportOptions options, out bool hasTexCoords0, out int bytesPerVertex)
        {
            bytesPerVertex = 0;
            hasTexCoords0 = vertexLayout != null;
            if (!hasTexCoords0) return new Vector2[0];
            hasTexCoords0 = vertexLayout.GetEntryInfoFromDataUsageAndUsageIndex(VertexLayout.Entry.DataUsages.Texcoord, 0, out VertexLayout.Entry.DataTypes dataType, out int streamIndex, out int streamOffset);
            if (!hasTexCoords0) return new Vector2[0];

            Mesh.VertexStream texCoord0Stream = mesh.vertexStreams[streamIndex];
            bytesPerVertex = texCoord0Stream.bytesPerVertex;
            Vector2[] buffer = new Vector2[mesh.vertexCount];
            for (int i = 0; i < buffer.Length; i++)
            {
                Vector2 texCoord;
                int baseOffset = (i * texCoord0Stream.bytesPerVertex) + streamOffset;
                switch (dataType)
                {
                    case VertexLayout.Entry.DataTypes.Float2:
                        texCoord.X = BitConverter.ToSingle(texCoord0Stream.data, baseOffset);
                        texCoord.Y = 1.0f - BitConverter.ToSingle(texCoord0Stream.data, baseOffset + 4);
                        break;
                    case VertexLayout.Entry.DataTypes.float16_2:
                        texCoord.X = Half.FromBytes(texCoord0Stream.data, baseOffset).ToSingle();//index out of range
                        texCoord.Y = 1.0f - Half.FromBytes(texCoord0Stream.data, baseOffset + 2).ToSingle();
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

        public static UIntSet[] GetIndexBuffer(Mesh mesh)
        {
            UIntSet[] buffer = new UIntSet[mesh.indexCount / 3];
            int indexSize = (int)mesh.indexSize;
            int doubleIndexSize = indexSize + indexSize;
            for (int i = 0; i < buffer.Length; i++)
            {
                uint index0, index1, index2;
                int baseOffset = i * 3 * indexSize;
                switch (mesh.indexSize)
                {
                    case 2:
                        index0 = BitConverter.ToUInt16(mesh.indexData, baseOffset);
                        index1 = BitConverter.ToUInt16(mesh.indexData, baseOffset + indexSize);
                        index2 = BitConverter.ToUInt16(mesh.indexData, baseOffset + doubleIndexSize);
                        break;
                    case 4:
                        index0 = BitConverter.ToUInt32(mesh.indexData, baseOffset);
                        index1 = BitConverter.ToUInt32(mesh.indexData, baseOffset + indexSize);
                        index2 = BitConverter.ToUInt32(mesh.indexData, baseOffset + doubleIndexSize);
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

        public static Vector3[] GetBoneBuffer(Mesh mesh, BoneDrawCall bdc, VertexLayout vertexLayout, ExportOptions options)
        {
            //bytesPerVertex = 0;
            //hasPositions = vertexLayout != null;
            //if (!hasPositions) return new Vector3[0];
            bool hasPositions = vertexLayout.GetEntryInfoFromDataUsageAndUsageIndex(VertexLayout.Entry.DataUsages.Position, 0, out VertexLayout.Entry.DataTypes dataType, out int streamIndex, out int streamOffset);
            if (!hasPositions) return new Vector3[0];


            Mesh.VertexStream positionStream = mesh.vertexStreams[streamIndex];
            //bytesPerVertex = positionStream.BytesPerVertex;
            streamOffset += Convert.ToInt32(bdc.VertexOffset);
            Vector3[] buffer = new Vector3[bdc.BoneCount];
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = ReadByteVector3(options, positionStream, dataType, streamOffset, i);

                buffer[i].X *= options.Scale.X;
                buffer[i].Y *= options.Scale.Y;
                buffer[i].Z *= options.Scale.Z;

            }

            return buffer;
        }
        private static void ExportModelAsSTLToDirectory(Model model, string directory, ExportOptions options)
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

        private static void ExportBonesAsTextToDirectory(Model model, string directory)
        {
            String path = directory + @"\" + Path.GetFileNameWithoutExtension(model.name) + ".txt";

            FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write);
            StreamWriter sw = new StreamWriter(fileStream);

            sw.WriteLine("-------------------------------------------------");
            sw.WriteLine("--------------####BoneDrawCalls###---------------");
            sw.WriteLine("-------------------------------------------------");
            foreach (BoneDrawCall bm in model.boneDrawCalls)
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
            foreach (BoneMapEntry bme in model.boneMapEntries) sw.WriteLine(bme.boneIndex + ", " + bme.globalIndex);
            sw.Close();
        }

        private static void PackageDirectory(string modelName, ref string directory, ExportOptions options)
        {
            if (options.Package)
            {
                try
                {
                    DirectoryInfo directoryInfo = Directory.CreateDirectory(directory + @"\" + Path.GetFileNameWithoutExtension(modelName));
                    directory = directoryInfo.FullName;
                }
                catch (Exception) { }
            }
        }

        private static void ExportLinkedTextures(Model model, string directory, ExportOptions options)
        {
            if (options.Textures)
            {
                foreach (string textureString in model.dma.textureStrings)
                {
                    TextureExporterStatic.exportTexture(textureString, directory, options.TextureFormat);
                }
            }
        }

        private static void ExportModelAsDAEToDirectory(Model model, string directory, ExportOptions options)
        {
            PackageDirectory(model.name, ref directory, options);

            ExportLinkedTextures(model, directory, options);

            string path = directory + @"\" + Path.GetFileNameWithoutExtension(model.name) + ".dae";

            // Load the Collada model
            COLLADA coll = new COLLADA();

            asset collAsset = new asset
            {
                contributor = new assetContributor[]
                {
                    new assetContributor()
                    {
                        author = "ps2ls2 user",
                        authoring_tool ="ps2ls2"
                    }
                },
                created = DateTime.Now,
                modified = DateTime.Now,
                unit = new assetUnit(),
                up_axis = UpAxisType.Y_UP
            };
            coll.asset = collAsset;

            string cleanName = Path.GetFileNameWithoutExtension(model.name);
            library_geometries libGeometries = new library_geometries();
            List<geometry> geometryList = new List<geometry>();
            List<node> nodeList = new List<node>();
            foreach (Mesh mesh in model.meshes)
            {
                VertexLayout vertexLayout = GetVertexLayoutFromMaterialHash(model.dma.materials[(int)mesh.drawCallOffset].MaterialDefinitionHash);

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

            COLLADAScene cScene = new COLLADAScene
            {
                instance_visual_scene = new InstanceWithExtra
                {
                    url = "#" + libVisualScenes.visual_scene[0].id,
                }
            };
            coll.scene = cScene;



            // Save the model
            Console.WriteLine("collada structure built, saving at" + path);
            coll.Save(path);
        }


        private static void ExportModelAsGLTF2ToDirectory(Model model, string directory, ExportOptions options)
        {
            PackageDirectory(model.name, ref directory, options);

            ExportLinkedTextures(model, directory, options);

            string path = directory + @"\" + Path.GetFileNameWithoutExtension(model.name) + ".dae";

            SceneBuilder scene = new SceneBuilder();

            for (int i = 0; i < model.meshes.Length; i++)
            {

                MaterialBuilder mat = new MaterialBuilder()
                    .WithDoubleSide(true);

                Mesh mesh = model.meshes[i];
                VertexLayout vertexLayout = GetVertexLayoutFromMaterialHash(model.dma.materials[(int)mesh.drawCallOffset].MaterialDefinitionHash);
                Vector3[] positionBuffer = GetPositionBuffer(mesh, vertexLayout, options, out bool _, out int _);
                Vector3[] normalBuffer = GetNormalBuffer(mesh, vertexLayout, out bool hasNormals, out int _);
                if (!hasNormals)
                {
                    normalBuffer = new Vector3[mesh.vertexCount];
                    for (int j = 0; j < normalBuffer.Length; j++) normalBuffer[j].Y = 1;
                }
                Vector4[] tangentBuffer = GetTangentBuffer(mesh, vertexLayout, out bool hasTangents, out int _);
                if (!hasTangents)
                {
                    tangentBuffer = new Vector4[mesh.vertexCount];
                    for (int j = 0; j < tangentBuffer.Length; j++)
                    {
                        tangentBuffer[j].X = 1;
                        tangentBuffer[j].W = 1;
                    }
                }
                UIntSet[] indexBuffer = GetIndexBuffer(mesh);

                MeshBuilder<VertexPositionNormalTangent> meshBuilder = new MeshBuilder<VertexPositionNormalTangent>(model.name + "_Mesh" + i);
                var primativeBuilder = meshBuilder.UsePrimitive(mat);

                foreach (UIntSet tri in indexBuffer)
                {
                    //glts triangles are flipped ¯\_(ツ)_/¯
                    primativeBuilder.AddTriangle(
                        VectorToVertex(positionBuffer[tri.z], normalBuffer[tri.z], tangentBuffer[tri.z]),
                        VectorToVertex(positionBuffer[tri.y], normalBuffer[tri.y], tangentBuffer[tri.y]),
                        VectorToVertex(positionBuffer[tri.x], normalBuffer[tri.x], tangentBuffer[tri.x])
                        );
                }

                scene.AddRigidMesh(meshBuilder, SharpGLTF.Transforms.AffineTransform.Identity);
            }

            SharpGLTF.Schema2.ModelRoot modelRoot = scene.ToGltf2();
            modelRoot.SaveGLTF(path);
        }
        private static System.Numerics.Vector3 VectorToNumVertex(Vector3 value)
        {
            return new System.Numerics.Vector3(value.X, value.Y, value.Z);
        }
        private static System.Numerics.Vector4 VectorToNumVertex(Vector4 value)
        {
            return new System.Numerics.Vector4(value.X, value.Y, value.Z, value.W);
        }
        private static VertexPositionNormalTangent VectorToVertex(Vector3 position, Vector3 normal, Vector4 Tangent)
        {
            return new VertexPositionNormalTangent(VectorToNumVertex(position), VectorToNumVertex(normal), VectorToNumVertex(Tangent));
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
