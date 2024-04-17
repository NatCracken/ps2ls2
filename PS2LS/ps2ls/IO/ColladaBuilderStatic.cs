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
    static class ColladaBuilderStatic
    {
        public static geometry CreateGeometryFromMesh(Mesh mesh, string meshName, VertexLayout vertexLayout, ModelExporterStatic.ExportOptions options)
        {

            geometry geom = new geometry()
            {
                name = meshName,
                id = meshName + "-mesh",
            };

            mesh cMesh = new mesh();
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
                source = '#' + positionArray.id,
                count = mesh.vertexCount,
                stride = 3
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
                        source = '#' + normalArray.id,
                        count = mesh.vertexCount,
                        stride = 3
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
                        source = '#' + textureArray.id,
                        count = mesh.vertexCount,
                        stride = 2
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
                    source = '#' + positionSource.id,
                },
            };
            #endregion

            #region triangles

            UIntSet[] indexBuffer = ModelExporterStatic.GetIndexTriBuffer(mesh);
            triangles triangles = new triangles()
            {
                count = Convert.ToUInt64(indexBuffer.Length),
            };

            ulong triOffset = 0;
            List<InputLocalOffset> triangleInputList = new List<InputLocalOffset>(){ new InputLocalOffset()
                {
                    semantic = "VERTEX",
                    source = '#' + vertices.id,
                    offset = triOffset++,
                },
            };
            if (hasNormals) triangleInputList.Add(new InputLocalOffset()
            {
                semantic = "NORMAL",
                source = '#' + geom.id + "-normals",
                offset = triOffset++,
            });
            if (hasTexCoords) triangleInputList.Add(new InputLocalOffset()
            {
                semantic = "TEXCOORD",
                source = '#' + geom.id + "-map-0",
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

        public static controller CreateControllerFromMesh(Mesh mesh, string meshName)
        {
            controller controller = new controller()
            {
                id = meshName + "-skin",
                name = "Armature"
            };


            skin skin = new skin()
            {
                bind_shape_matrix = MatrixToString(Matrix4.Identity), //TODO What actually is this?
                source1 = '#' + meshName + "-mesh",
            };

            List<source> sourceList = new List<source>();

            #region Joints
            source jointsSource = new source()
            {
                id = meshName + "-skin-joints"
            };

            Name_array jointArray = new Name_array()
            {
                id = meshName + "-skin-joints-array",
                count = 0,//todo joint count

            };

            string[] joinNameList = new string[0]; //TODO fill joint list

            jointArray.Values = joinNameList;
            jointsSource.Item = jointArray;

            sourceTechnique_common jointTechCommon = new sourceTechnique_common();
            accessor jointAccessor = new accessor()
            {
                source = '#' + jointArray.id,
                count = 0,//todo should be listLength/stride
                stride = 1
            };
            jointAccessor.param = new param[]
            {
                new param()
                {
                    name = "JOINT",
                    type = "name",
                },
            };
            jointTechCommon.accessor = jointAccessor;
            jointsSource.technique_common = jointTechCommon;
            sourceList.Add(jointsSource);
            #endregion

            #region Bind Poses
            source bindPosesSource = new source()
            {
                id = meshName + "-skin-bind-poses"
            };

            float_array bindPoseArray = new float_array()
            {
                id = meshName + "-skin-bind-poses-array",
                count = 0,//todo count

            };

            double[] bindPosesList = new double[0]; //TODO matrix list

            bindPoseArray.Values = bindPosesList;
            bindPosesSource.Item = bindPoseArray;

            sourceTechnique_common bindPosesTechCommon = new sourceTechnique_common();
            accessor bindPosesAccessor = new accessor()
            {
                source = '#' + bindPoseArray.id,
                count = 0,//todo should be listLength/stride
                stride = 16,
            };
            bindPosesAccessor.param = new param[]
            {
                new param()
                {
                    name = "TRANSFORM",
                    type = "float4x4",
                },
            };
            bindPosesTechCommon.accessor = bindPosesAccessor;
            bindPosesSource.technique_common = bindPosesTechCommon;
            sourceList.Add(bindPosesSource);
            #endregion

            #region SkinWeights
            source skinWeightsSource = new source()
            {
                id = meshName + "-skin-weights"
            };

            float_array skinWeightsArray = new float_array()
            {
                id = meshName + "-skin-weights-array",
                count = 0,//todo count

            };

            double[] skinWeightsList = new double[0]; //TODO matrix list

            skinWeightsArray.Values = skinWeightsList;
            skinWeightsSource.Item = skinWeightsArray;

            sourceTechnique_common skinWeightsTechCommon = new sourceTechnique_common();
            accessor skinWeightsAccessor = new accessor()
            {
                source = '#' + skinWeightsArray.id,
                count = 0,//todo should be listLength/stride
                stride = 1,
            };
            skinWeightsAccessor.param = new param[]
            {
                new param()
                {
                    name = "WEIGHT",
                    type = "float",
                },
            };
            skinWeightsTechCommon.accessor = skinWeightsAccessor;
            skinWeightsSource.technique_common = skinWeightsTechCommon;
            sourceList.Add(skinWeightsSource);
            #endregion

            skin.source = sourceList.ToArray();


            List<InputLocal> jointInputs = new List<InputLocal>();
            jointInputs.Add(new InputLocal()
            {
                semantic = "JOINT",
                source = '#' + jointsSource.id
            });
            jointInputs.Add(new InputLocal()
            {
                semantic = "INV_BIND_MATRIX",
                source = '#' + bindPosesSource.id
            });
            skin.joints = new skinJoints()
            {
                input = jointInputs.ToArray()
            };

            skinVertex_weights vertexWeights = new skinVertex_weights();


            List<InputLocalOffset> vertWeightInputs = new List<InputLocalOffset>();
            vertWeightInputs.Add(new InputLocalOffset()
            {
                semantic = "JOINT",
                source = '#' + jointsSource.id,
                offset = 0
            });
            vertWeightInputs.Add(new InputLocalOffset()
            {
                semantic = "WEIGHT",
                source = '#' + skinWeightsSource.id,
                offset = 1
            });
            vertexWeights.input = vertWeightInputs.ToArray();

            StringBuilder vCountBuilder = new StringBuilder();//TODO fill vCount
            vertexWeights.vcount = vCountBuilder.ToString();

            StringBuilder vBuilder = new StringBuilder();//TODO fill v
            vertexWeights.v = vBuilder.ToString();

            skin.vertex_weights = vertexWeights;

            controller.Item = skin;
            return controller;
        }

        private static string MatrixToString(Matrix4 matrix)
        {
            StringBuilder matrixString = new StringBuilder();
            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    matrixString.Append(matrix[x, y]).Append(' ');
                }
            }
            return matrixString.ToString();
        }
        public static node BuildNode(string name, NodeType type)
        {
            return BuildNode(name, type, Matrix4.Identity);
        }
        public static node BuildNode(string name, NodeType type, Matrix4 transform)
        {
            node node = type == NodeType.JOINT ? new node()
            {
                id = "Armature_" + name,
                name = name,
                sid = name,
                type = type,
            } : new node()
            {
                id = name,
                name = name,
                type = type,
            };

            StringBuilder matrixString = new StringBuilder();
            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    matrixString.Append(transform[x, y]).Append(' ');
                }
            }

            matrix nodeMatrix = new matrix()
            {
                sid = "transform",
                _Text_ = matrixString.ToString(),
            };
            node.Items = new object[] { nodeMatrix };
            node.ItemsElementName = new ItemsChoiceType2[] { ItemsChoiceType2.matrix };
            return node;
        }

        public static node BuildArmature(Model model, string cleanName)
        {
            node rootNode = BuildNode(cleanName, NodeType.JOINT);
            node workingNode = rootNode;
            foreach (Bone bone in model.bones)
            {
                node newNode = BuildArmatureRecursive(bone);
                workingNode.node1 = new node[] { newNode };
                workingNode = newNode;
            }
            return rootNode;
        }

        private static node BuildArmatureRecursive(Bone bone)
        {
            node activeNode = BuildNode(bone.name, NodeType.JOINT, bone.inverseBindPose);
            return activeNode;
        }
    }
}
