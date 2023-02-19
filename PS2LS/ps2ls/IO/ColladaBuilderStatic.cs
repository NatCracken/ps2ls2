using OpenTK;
using ps2ls.Assets;
using ps2ls.Graphics.Materials;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Collada141;
using System.Text;

namespace ps2ls.IO
{
    class ColladaBuilderStatic
    {

        public static geometry createGeometryFromMesh(Mesh mesh, string meshName, VertexLayout vertexLayout, ModelExporterStatic.ExportOptions options)
        {

            geometry geom = new geometry()
            {
                name = meshName,
                id = meshName + "-mesh",
            };

            Collada141.mesh cMesh = new mesh();
            List<source> sourceList = new List<source>();

            #region positions
            source positionSource = new source()
            {
                id = geom.id + "-positions"
            };
            float_array positionArray = new float_array()
            {
                id = positionSource.id + "-array",
                count = mesh.vertexCount * 3,
            };

            //positions
            Vector3[] positionBuffer = ModelExporterStatic.GetPositionBuffer(mesh, vertexLayout, options, out bool _, out int bytesPerVertex);

            double[] positions = new double[positionArray.count];
            for (int i = 0; i < positionBuffer.Length; i++)
            {
                Vector3 position = positionBuffer[i];
                int offset = i * 3;
                positions[offset] = position.X;
                positions[offset + 1] = position.Y;
                positions[offset + 2] = position.Z;
            }

            positionArray.Values = positions;
            positionSource.Item = positionArray;
            sourceTechnique_common posTechCommon = new sourceTechnique_common();
            accessor posAccessor = new accessor()
            {
                source = "#" + positionArray.id,
                count = mesh.vertexCount,
            };
            string paramType = bytesPerVertex == 12 ? "float" : "half";
            posAccessor.param = new param[]
            {
                new param()
                {
                    name = "X",
                    type = paramType,
                },
                new param()
                {
                    name = "Y",
                    type = paramType,
                },
                new param()
                {
                    name = "Z",
                    type = paramType,
                },
            };
            posAccessor.stride = Convert.ToUInt64(posAccessor.param.Length);
            posTechCommon.accessor = posAccessor;
            positionSource.technique_common = posTechCommon;

            sourceList.Add(positionSource);
            #endregion

            #region normals
            bool hasNormals = false;
            if (options.Normals)
            {
                Vector3[] normalsBuffer = ModelExporterStatic.GetNormalBuffer(mesh, vertexLayout, out hasNormals, out bytesPerVertex);
                if (hasNormals)
                {
                    source normalSource = new source()
                    {
                        id = geom.id + "-normals"
                    };
                    float_array normalArray = new float_array()
                    {
                        id = normalSource.id + "-array",
                        count = mesh.vertexCount * 3,
                    };

                    //normals
                    double[] normals = new double[normalArray.count];
                    for (int i = 0; i < normalsBuffer.Length; i++)
                    {
                        Vector3 normal = normalsBuffer[i];
                        int offset = i * 3;
                        normals[offset] = normal.X;
                        normals[offset + 1] = normal.Y;
                        normals[offset + 2] = normal.Z;
                    }

                    normalArray.Values = normals;
                    normalSource.Item = normalArray;
                    sourceTechnique_common normTechCommon = new sourceTechnique_common();
                    accessor normAccessor = new accessor()
                    {
                        source = "#" + normalArray.id,
                        count = mesh.vertexCount,
                    };
                    normAccessor.param = new param[]
                    {
                new param()
                {
                    name = "X",
                    type = "float",
                },
                new param()
                {
                    name = "Y",
                    type = "float",
                },
                new param()
                {
                    name = "Z",
                    type = "float",
                },
                    };
                    normAccessor.stride = Convert.ToUInt64(normAccessor.param.Length);
                    normTechCommon.accessor = normAccessor;
                    normalSource.technique_common = normTechCommon;

                    sourceList.Add(normalSource);
                }
            }
            #endregion

            #region texCoords
            bool hasTexCoords = false;
            if (options.TextureCoordinates)
            {
                Vector2[] texCoordsBuffer = ModelExporterStatic.GetTextureCoords0Buffer(mesh, vertexLayout, options, out hasTexCoords, out bytesPerVertex);
                if (hasTexCoords)
                {
                    source textureSource = new source()
                    {
                        id = geom.id + "-map-0",
                    };
                    float_array textureArray = new float_array
                    {
                        id = textureSource.id + "-array",
                        count = mesh.vertexCount * 3
                    };

                    double[] coordinates = new double[textureArray.count];
                    for (int j = 0; j < texCoordsBuffer.Length; ++j)
                    {
                        Vector2 texCoord = texCoordsBuffer[j];

                        int offset = j * 2;
                        coordinates[offset] = texCoord.X;
                        coordinates[offset + 1] = texCoord.Y;
                    }

                    textureArray.Values = coordinates;
                    textureSource.Item = textureArray;
                    sourceTechnique_common texTechCommon = new sourceTechnique_common();
                    accessor texAccessor = new accessor
                    {
                        source = "#" + textureArray.id,
                        count = mesh.vertexCount,

                    };
                    paramType = bytesPerVertex == 8 ? "float" : "half";
                    texAccessor.param = new param[]
                    {
                        new param()
                        {
                            name = "T",
                            type = paramType,
                        },
                        new param()
                        {
                            name = "S",
                            type = paramType,
                        },
                    };
                    texAccessor.stride = Convert.ToUInt64(texAccessor.param.Length);
                    texTechCommon.accessor = texAccessor;
                    textureSource.technique_common = texTechCommon;

                    sourceList.Add(textureSource);
                }
            }
            #endregion

            cMesh.source = sourceList.ToArray();

            #region verticies
            vertices vertices = new vertices()
            {
                id = geom.id + "-vertices",
            };
            vertices.input = new InputLocal[] { new InputLocal()
                {
                    semantic = "POSITION",
                    source = "#" + positionSource.id,
                },
            };
            #endregion

            #region triangles

            UIntSet[] indexBuffer = ModelExporterStatic.GetIndexBuffer(mesh);
            triangles triangles = new triangles()
            {
                count = Convert.ToUInt64(indexBuffer.Length),
            };

            ulong triOffset = 0;
            List<InputLocalOffset> triangleInputList = new List<InputLocalOffset>(){ new InputLocalOffset()
                {
                    semantic = "VERTEX",
                    source = "#" + vertices.id,
                    offset = triOffset++,
                },
            };
            if (hasNormals) triangleInputList.Add(new InputLocalOffset()
            {
                semantic = "NORMAL",
                source = "#" + geom.id + "-normals",
                offset = triOffset++,
            });
            if (hasTexCoords) triangleInputList.Add(new InputLocalOffset()
            {
                semantic = "TEXCOORD",
                source = "#" + geom.id + "-map-0",
                offset = triOffset++,
                set = 0,
            });
            triangles.input = triangleInputList.ToArray();

            StringBuilder sb = new StringBuilder();
            foreach (UIntSet uis in indexBuffer)
            {
                uint index0 = uis.x;
                uint index1 = uis.y;
                uint index2 = uis.z;

                for (uint j = 0; j < triOffset; j++) sb.Append(index2 + " ");
                for (uint j = 0; j < triOffset; j++) sb.Append(index1 + " ");
                for (uint j = 0; j < triOffset; j++) sb.Append(index0 + " ");
            }
            sb.Remove(sb.Length - 1, 1);
            triangles.p = sb.ToString();

            #endregion

            cMesh.Items = new object[] { vertices, triangles };

            geom.Item = cMesh;
            return geom;
        }
    }
}
