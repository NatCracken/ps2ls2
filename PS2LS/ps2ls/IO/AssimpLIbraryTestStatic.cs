using Assimp;
using OpenTK;
using ps2ls.Assets;
using ps2ls.Graphics.Materials;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace ps2ls.IO
{
    public static class AssimpLIbraryTestStatic
    {
        public static void TestWork(Model model, string directory, ModelExporterStatic.ExportOptions options)
        {
            ModelExporterStatic.PackageDirectory(model.name, ref directory, options);

            //ModelExporterStatic.ExportLinkedTextures(model, directory, options);

            string path = directory + @"\" + Path.GetFileNameWithoutExtension(model.name) + ".gltf2";

            AssimpContext context = new AssimpContext();
            Scene scene = new Scene();
            scene.RootNode = new Node("Root");
            for (int i = 0; i < model.meshes.Length; i++)
            {
                Assimp.Mesh aMesh = new Assimp.Mesh(model.name + "_Mesh" + i, PrimitiveType.Triangle);
                Assets.Mesh mesh = model.meshes[i];
                VertexLayout vertexLayout = ModelExporterStatic.GetVertexLayoutFromMaterialHash(model.dma.materials[(int)mesh.drawCallOffset].MaterialDefinitionHash);
                Vector3[] positionBuffer = ModelExporterStatic.GetPositionBuffer(mesh, vertexLayout, options, out bool _, out int _);
                aMesh.Vertices.AddRange(VectorTKToAVector(positionBuffer));
                int[] indexBuffer = ModelExporterStatic.GetIndexBuffer(mesh);
                if (!aMesh.SetIndices(indexBuffer, 3)) Console.WriteLine("Failed To Set Indices");
                scene.Meshes.Add(aMesh);
            }
            Console.WriteLine(path);
            context.ExportFile(scene, path, "gltf2");
        }

        private static Vector3D[] VectorTKToAVector(Vector3[] list)
        {
            Vector3D[] toReturn = new Vector3D[list.Length];
            for(int i = 0; i < list.Length; i++)
            {
                toReturn[i] = new Vector3D(list[i].X, list[i].Y, list[i].Z);
            }
            return toReturn;
        }
    }
}
