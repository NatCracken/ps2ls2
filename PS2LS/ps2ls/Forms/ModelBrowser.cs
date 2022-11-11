﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using ps2ls.Cameras;
using ps2ls.Assets.Dme;
using ps2ls.Assets.Pack;
using System.Diagnostics;
using ps2ls.Graphics.Materials;
using System.IO;
using System.Xml;
using System.Runtime.InteropServices;

namespace ps2ls.Forms
{
    public partial class ModelBrowser : UserControl
    {
        #region Singleton
        private static ModelBrowser instance = null;

        public static void CreateInstance()
        {
            instance = new ModelBrowser();
        }

        public static void DeleteInstance()
        {
            instance = null;
        }

        public static ModelBrowser Instance { get { return instance; } }
        #endregion

        private Model model = null;
        private ColorDialog backgroundColorDialog = new ColorDialog();
        private Int32 texturedShader = 0;
        private int untexturedShader = 0;
        private int currentShader = 0;
        private List<ToolStripButton> renderModeButtons = new List<ToolStripButton>();

        private List<int> textures = new List<int>(new int[] { 0, 0, 0, 0, 0 });

        public int gray;

        #region Mesh Colors
        // a series of nice pastel colors we'll use to color meshes
        Color[] meshColors = {
                                 Color.FromArgb(162, 206, 250),
                                 Color.FromArgb(244, 228, 139),
                                 Color.FromArgb(206, 128, 236),
                                 Color.FromArgb(212, 201, 158),
                                 Color.FromArgb(252, 247, 158),
                                 Color.FromArgb(162, 140, 166),
                                 Color.FromArgb(224, 166, 157),
                                 Color.FromArgb(199, 188, 183),
                                 Color.FromArgb(226, 247, 150),
                                 Color.FromArgb(128, 197, 167),
                                 Color.FromArgb(219, 152, 223),
                                 Color.FromArgb(241, 167, 249),
                                 Color.FromArgb(131, 179, 175),
                                 Color.FromArgb(167, 167, 151),
                                 Color.FromArgb(230, 163, 139),
                                 Color.FromArgb(176, 165, 128),
                                 Color.FromArgb(168, 199, 185),
                                 Color.FromArgb(231, 166, 254),
                                 Color.FromArgb(153, 177, 250),
                                 Color.FromArgb(163, 251, 178),
                                 Color.FromArgb(246, 198, 243),
                                 Color.FromArgb(198, 220, 216),
                                 Color.FromArgb(242, 235, 193),
                                 Color.FromArgb(145, 195, 137),
                                 Color.FromArgb(135, 186, 207),
                                 Color.FromArgb(254, 187, 169),
                                 Color.FromArgb(238, 207, 158),
                                 Color.FromArgb(166, 178, 208),
                                 Color.FromArgb(165, 137, 128),
                                 Color.FromArgb(250, 218, 178),
                                 Color.FromArgb(144, 223, 183),
                                 Color.FromArgb(252, 175, 224)
                             };
        #endregion

        private ModelBrowser()
        {
            InitializeComponent();

            //HACK: Can't load ModelBrowser.cs in design mode unless we have at least one item for some reason.
            //Clear items after construction.
            modelsListBox.Items.Clear();

            Dock = DockStyle.Fill;

            backgroundColorDialog.Color = Color.FromArgb(32, 32, 32);

            renderModeButtons.Add(renderModeWireframeButton);
            renderModeButtons.Add(renderModeSmoothButton);
        }

        public void onEnter(object sender, EventArgs e)
        {
            modelsListBox.LoadAndSortAssets();
            refreshModelsListBox();
        }

        //TODO: move this elsehwere
        private void compileShader(Int32 shader, String source)
        {
            ErrorCode e;

            GL.ShaderSource(shader, source);
            if ((e = GL.GetError()) != ErrorCode.NoError) { Console.WriteLine(e); }
            GL.CompileShader(shader);
            if ((e = GL.GetError()) != ErrorCode.NoError) { Console.WriteLine(e); }

            String info = String.Empty;
            GL.GetShaderInfoLog(shader, out info);
            Console.WriteLine(info);

            Int32 compileResult;
            GL.GetShader(shader, ShaderParameter.CompileStatus, out compileResult);
            if ((e = GL.GetError()) != ErrorCode.NoError) { Console.WriteLine(e); }

            if (compileResult != 1)
            {
                Console.WriteLine("Compile error!");
                Console.WriteLine(source);
            }
        }




        private void createShaderProgram()
        {
            //TODO: Use external shader source files.
            String vertexShaderSource = @"
void main(void)
{
    gl_Position = ftransform();

    gl_TexCoord[0] = gl_MultiTexCoord0;
}
";
            String fragmentShaderSource = @"
uniform sampler2D colorMap;

void main(void)
{
   vec4 col = texture2D(colorMap, gl_TexCoord[0].st);
   if(col.a <= 0) discard;
   gl_FragColor = texture2D(colorMap, gl_TexCoord[0].st);
}
";

            texturedShader = createShaderProgram(vertexShaderSource, fragmentShaderSource);

            vertexShaderSource = @"
varying vec3 normal;
varying vec3 lightDirection;


void main() 
{ 
    gl_Position = ftransform();

    gl_FrontColor = gl_Color;


   
    normal = gl_NormalMatrix * gl_Normal;

    lightDirection = vec3(1, -1, 1);
}
";
            fragmentShaderSource = @"
varying vec3 normal; 
varying vec3 lightDirection;

void main()
{
	const vec4 ambientColor = vec4(0.25, 0.25, 0.25, 1.0);
    const vec4 diffuseColor = vec4(1.0, 1.0, 1.0, 1.0);

    vec3 normalizedNormal = normalize(normal);
    vec3 noralizedLightDirection = normalize(lightDirection);

    float diffuseTerm = clamp(dot(normalizedNormal, noralizedLightDirection), 0.0, 1.0);

    gl_FragColor = gl_Color * (ambientColor + (diffuseColor * diffuseTerm));
 

}
";
            untexturedShader = createShaderProgram(vertexShaderSource, fragmentShaderSource);

            currentShader = texturedShader;

            gray = LoadTexture("grey.dds");

        }

        //TODO: move this elsehwere
        private int createShaderProgram(string vertexShaderSource, string fragmentShaderSource)
        {
            ErrorCode e;

            GL.GetError(); //clear error

            int shaderProgram = GL.CreateProgram();
            if ((e = GL.GetError()) != ErrorCode.NoError) { Console.WriteLine(e); }

            Int32 vertexShader = GL.CreateShader(ShaderType.VertexShader);
            if ((e = GL.GetError()) != ErrorCode.NoError) { Console.WriteLine(e); }
            Int32 fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            if ((e = GL.GetError()) != ErrorCode.NoError) { Console.WriteLine(e); }



            compileShader(vertexShader, vertexShaderSource);
            int res = 0;
            GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out res);
            if ((All)res == All.False)
            {
                MessageBox.Show(GL.GetShaderInfoLog(vertexShader));
                throw new Exception(GL.GetShaderInfoLog(vertexShader));
            }
            compileShader(fragmentShader, fragmentShaderSource);
            GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out res);
            if ((All)res == All.False)
            {
                MessageBox.Show(GL.GetShaderInfoLog(fragmentShader));
                throw new Exception(GL.GetShaderInfoLog(fragmentShader));
            }

            GL.AttachShader(shaderProgram, fragmentShader);
            if ((e = GL.GetError()) != ErrorCode.NoError) { Console.WriteLine(e); }
            GL.AttachShader(shaderProgram, vertexShader);
            if ((e = GL.GetError()) != ErrorCode.NoError) { Console.WriteLine(e); }
            GL.LinkProgram(shaderProgram);
            if ((e = GL.GetError()) != ErrorCode.NoError) { Console.WriteLine(e); }

            String info;
            GL.GetProgramInfoLog(shaderProgram, out info);

            Console.WriteLine(info);

            if (fragmentShader != 0)
            {
                GL.DeleteShader(fragmentShader);
                if ((e = GL.GetError()) != ErrorCode.NoError) { Console.WriteLine(e); }
            }

            if (vertexShader != 0)
            {
                GL.DeleteShader(vertexShader);
                if ((e = GL.GetError()) != ErrorCode.NoError) { Console.WriteLine(e); }
            }

            return shaderProgram;
        }

        private void update()
        {
            glControl1.Camera.AspectRatio = glControl1.ClientSize.Width / (float)glControl1.ClientSize.Height;
            glControl1.Camera.Update();
        }

        /*
        Vector3[] bonePositions = new Vector3[] {
            new Vector3(0f,0f,0f),
            new Vector3(0f,1f,0f),
            new Vector3(1f,1.5f,1f),
            new Vector3(-1f,1.5f,1f),
            new Vector3(0,2,0f),
        };

        int[] boneIndices = new int[]
        {
            0,1,
            1,4,
            1,2,
            1,3,
            3,4
        };*/

        public static Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }

        private void render()
        {
            glControl1.MakeCurrent();

            GL.Viewport(0, 0, glControl1.ClientSize.Width, glControl1.ClientSize.Height);

            //clear
            GL.ClearColor(backgroundColorDialog.Color);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //projection matrix
            Matrix4 projection = glControl1.Camera.Projection;
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);

            //view matrix
            Matrix4 view = glControl1.Camera.View;
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref view);


            if (showAxesButton.Checked)
            {
                // debug axes
                GL.Begin(PrimitiveType.Lines);
                //x
                GL.Color3(Color.Red);
                GL.Vertex3(Vector3.Zero); GL.Vertex3(Vector3.UnitX);
                GL.Vertex3(Vector3.UnitX); GL.Vertex3(Vector3.UnitX + new Vector3(-0.125f, 0.125f, 0.0f));
                GL.Vertex3(Vector3.UnitX); GL.Vertex3(Vector3.UnitX + new Vector3(-0.125f, -0.125f, 0.0f));

                //y
                GL.Color3(Color.Green);
                GL.Vertex3(Vector3.Zero); GL.Vertex3(Vector3.UnitY);
                GL.Vertex3(Vector3.UnitY); GL.Vertex3(Vector3.UnitY + new Vector3(0.125f, -0.125f, 0.0f));
                GL.Vertex3(Vector3.UnitY); GL.Vertex3(Vector3.UnitY + new Vector3(-0.125f, -0.125f, 0.0f));

                //z
                GL.Color3(Color.Blue);
                GL.Vertex3(Vector3.Zero); GL.Vertex3(Vector3.UnitZ);
                GL.Vertex3(Vector3.UnitZ); GL.Vertex3(Vector3.UnitZ + new Vector3(0, -0.125f, -0.125f));
                GL.Vertex3(Vector3.UnitZ); GL.Vertex3(Vector3.UnitZ + new Vector3(0, 0.125f, -0.125f));

                GL.End();
            }



            //TODO: Decide what to do with non-version 4 models.
            if (model != null && model.Version == 4)
            {
                GL.PushMatrix();

                GL.PushAttrib(AttribMask.PolygonBit | AttribMask.EnableBit | AttribMask.LightingBit | AttribMask.CurrentBit);

                GL.UseProgram(currentShader);

                GL.Enable(EnableCap.DepthTest);
                GL.Enable(EnableCap.CullFace);
                GL.Enable(EnableCap.Texture2D);
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                GL.CullFace(CullFaceMode.Back);
                GL.FrontFace(FrontFaceDirection.Cw);

                for (Int32 i = 0; i < model.Meshes.Length; ++i)
                {
                    Mesh mesh = model.Meshes[i];

                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.Texture2D, textures[i]);

                    if (currentShader == texturedShader)
                    {

                        int loc = GL.GetUniformLocation(currentShader, "colorMap");
                        GL.Uniform1(loc, 0);
                    }

                    //pin handles to stream data
                    GCHandle[] streamDataGCHandles = new GCHandle[mesh.VertexStreams.Length];

                    for (Int32 j = 0; j < streamDataGCHandles.Length; ++j)
                    {
                        streamDataGCHandles[j] = GCHandle.Alloc(mesh.VertexStreams[j].Data, GCHandleType.Pinned);
                    }

                    //fetch material definition and vertex layout
                    VertexLayout vertexLayout = IO.ModelExporterStatic.getVertexLayoutFromMaterialHash(model.Materials[(int)mesh.drawCallOffset].MaterialDefinitionHash);

                    //Console.WriteLine("Mesh:" + mesh.drawCallOffset + " / " + model.Materials.Count + " = " + vertexLayout.Name);
                    GL.Color3(meshColors[i % meshColors.Length]);

                    if (renderModeWireframeButton.Checked)
                    {
                        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                    }
                    else
                    {
                        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                    }

                    // IO.ModelExporterStatic.getPositionBuffer(mesh, vertexLayout, IO.ModelExporterStatic.sampleOptions, out bool _, out int _);

                    //position
                    VertexLayout.Entry.DataTypes positionDataType = VertexLayout.Entry.DataTypes.None;
                    int positionStream = 0;
                    int positionOffset = 0;
                    bool positionExists = vertexLayout == null ? false : vertexLayout.GetEntryInfoFromDataUsageAndUsageIndex(VertexLayout.Entry.DataUsages.Position, 0, out positionDataType, out positionStream, out positionOffset);

                    //assume position exsists, and that 0,0 are the right values for stream and offset if material/layout not found
                    IntPtr positionData = streamDataGCHandles[positionStream].AddrOfPinnedObject();

                    GL.EnableClientState(ArrayCap.VertexArray);
                    GL.VertexPointer(3, VertexPointerType.Float, mesh.VertexStreams[positionStream].BytesPerVertex, positionData + positionOffset);


                    //normal
                    VertexLayout.Entry.DataTypes normalDataType = VertexLayout.Entry.DataTypes.None;
                    int normalStream = 0;
                    int normalOffset = 0;
                    bool normalExists = vertexLayout == null ? false : vertexLayout.GetEntryInfoFromDataUsageAndUsageIndex(VertexLayout.Entry.DataUsages.Normal, 0, out normalDataType, out normalStream, out normalOffset);

                    if (normalExists)
                    {
                        IntPtr normalData = streamDataGCHandles[normalStream].AddrOfPinnedObject();

                        GL.EnableClientState(ArrayCap.NormalArray);
                        GL.NormalPointer(NormalPointerType.Float, mesh.VertexStreams[normalStream].BytesPerVertex, normalData + normalOffset);
                    }


                    //texture coordiantes
                    VertexLayout.Entry.DataTypes texCoord0DataType = VertexLayout.Entry.DataTypes.None;
                    Int32 texCoord0Stream = 0;
                    Int32 texCoord0Offset = 0;
                    bool texCoord0Exists = vertexLayout == null ? false : vertexLayout.GetEntryInfoFromDataUsageAndUsageIndex(VertexLayout.Entry.DataUsages.Texcoord, 0, out texCoord0DataType, out texCoord0Stream, out texCoord0Offset);

                    if (texCoord0Exists)
                    {
                        IntPtr texCoord0Data = streamDataGCHandles[texCoord0Stream].AddrOfPinnedObject();

                        GL.EnableClientState(ArrayCap.TextureCoordArray);

                        TexCoordPointerType texCoord0PointerType = TexCoordPointerType.Float;

                        switch (texCoord0DataType)
                        {
                            case VertexLayout.Entry.DataTypes.Float2:
                                texCoord0PointerType = TexCoordPointerType.Float;
                                break;
                            case VertexLayout.Entry.DataTypes.float16_2:
                                texCoord0PointerType = TexCoordPointerType.HalfFloat;
                                break;
                            default:
                                break;
                        }

                        GL.TexCoordPointer(2, texCoord0PointerType, mesh.VertexStreams[texCoord0Stream].BytesPerVertex, texCoord0Data + texCoord0Offset);
                    }



                    //indices
                    GCHandle indexDataHandle = GCHandle.Alloc(mesh.IndexData, GCHandleType.Pinned);
                    IntPtr indexData = indexDataHandle.AddrOfPinnedObject();

                    DrawElementsType drawElementsType;
                    switch (mesh.IndexSize)
                    {
                        default:
                        case 2:
                            drawElementsType = DrawElementsType.UnsignedShort;
                            break;
                        case 4:
                            drawElementsType = DrawElementsType.UnsignedInt;
                            break;
                    }
                    GL.DrawElements(PrimitiveType.Triangles, (Int32)mesh.IndexCount, drawElementsType, indexData);

                    indexDataHandle.Free();

                    GL.DisableClientState(ArrayCap.VertexArray);
                    GL.DisableClientState(ArrayCap.NormalArray);
                    GL.DisableClientState(ArrayCap.TextureCoordArray);

                    //free stream data handles
                    for (Int32 j = 0; j < streamDataGCHandles.Length; ++j)
                    {
                        streamDataGCHandles[j].Free();
                    }
                }

                GL.UseProgram(0);

                GL.PopAttrib();
                #region boundingBox
                ////bounding box
                //if (showBoundingBoxButton.Checked)
                //{
                //    GL.PushAttrib(AttribMask.CurrentBit | AttribMask.EnableBit);

                //    GL.Color3(Color.Red);

                //    GL.Enable(EnableCap.DepthTest);

                //    Vector3 min = model.Min;
                //    Vector3 max = model.Max;
                //    Vector3[] vertices = new Vector3[8];
                //    UInt32[] indices = { 0, 1, 1, 2, 2, 3, 3, 0, 0, 4, 1, 5, 2, 6, 3, 7, 4, 5, 5, 6, 6, 7, 7, 4 };

                //    vertices[0] = min;
                //    vertices[1] = new Vector3(max.X, min.Y, min.Z);
                //    vertices[2] = new Vector3(max.X, min.Y, max.Z);
                //    vertices[3] = new Vector3(min.X, min.Y, max.Z);
                //    vertices[4] = new Vector3(min.X, max.Y, min.Z);
                //    vertices[5] = new Vector3(max.X, max.Y, min.Z);
                //    vertices[6] = max;
                //    vertices[7] = new Vector3(min.X, max.Y, max.Z);

                //    GL.EnableClientState(ArrayCap.VertexArray);
                //    GL.VertexPointer(3, VertexPointerType.Float, 0, vertices);
                //    GL.DrawRangeElements(BeginMode.Lines, 0, 23, 24, DrawElementsType.UnsignedInt, indices);

                //    GL.PopAttrib();
                //}
                #endregion


                GL.Clear(ClearBufferMask.DepthBufferBit);//clear depth buffer to draw lines on top

                if (false)//TODO "show bones" button
                {

                    /*
                     * This entire section straight up doesn't work, Its not the code here,
                     * model.bonePositions just contains completely the wrong data
                     */

                    GL.Begin(PrimitiveType.Lines);
                    for (int i = 0; i < model.BoneDrawCalls.Length; i++)
                    {
                        BoneDrawCall bdc = model.BoneDrawCalls[i];
                        //VertexLayout vertexLayout = IO.ModelExporterStatic.getVertexLayoutFromMaterialHash(model.Materials[i].MaterialDefinitionHash);

                        for (int j = 0; j < bdc.BoneCount; j++)
                        {
                            GL.Color3(ColorFromHSV(360f * j / bdc.BoneCount, 0.8, 0.8));
                            BoneMapEntry bme = model.BoneMapEntries[j + bdc.BoneStart];
                            GL.Vertex3(model.bonePositions[bme.BoneIndex]);
                            GL.Vertex3(model.bonePositions[bme.BoneIndex + 1]);
                        }
                        //  Console.WriteLine("Bone:" + i + " / " + model.Materials.Count + " = " + vertexLayout.Name);
                        // Vector3[] boneBuffer = IO.ModelExporterStatic.getBoneBuffer(model.Meshes[i], bdc, vertexLayout, sampleOptions);
                        //for(int j = 
                    }
                    GL.End();
                }
            }





            glControl1.SwapBuffers();
        }

        private void ModelBrowserControl_Load(object sender, EventArgs e)
        {
            glControl1.CreateGraphics();

            createShaderProgram();

            Application.Idle += applicationIdle;
        }

        private void applicationIdle(object sender, EventArgs e)
        {
            while (glControl1.Context != null && glControl1.IsIdle)
            {
                update();
                render();
            }
        }

        private void glControl1_Resize(object sender, EventArgs e)
        {
            OpenTK.GLControl glControl = sender as OpenTK.GLControl;

            if (glControl.Height == 0)
            {
                glControl.ClientSize = new System.Drawing.Size(glControl.ClientSize.Width, 1);
            }
        }

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            render();
        }

        public override void Refresh()
        {
            base.Refresh();

            refreshModelsListBox();
        }

        private int pageNumber = 0;
        private int pageSize = 1000;
        private void refreshModelsListBox()
        {
            modelsListBox.FilterBySearch(searchModelsText.Text ?? "");
            if (!showAutoLODModelsButton.Checked) modelsListBox.excludeFromFilter("Auto.dme");
            if (!showCollisionModelsButton.Checked)
            {
                modelsListBox.excludeFromFilter("Collision");
                modelsListBox.excludeFromFilter("Occluder");
            }

            if (!showMultipleLODButton.Checked)//for names that contain LOD's, keep only the largest
            {
                Dictionary<string, ImageBrowser.AssetSearchParam> nameToAsset = new Dictionary<string, ImageBrowser.AssetSearchParam>();
                for (int i = modelsListBox.filteredAssets.Count - 1; i >= 0; i--)
                {
                    Asset a = modelsListBox.filteredAssets[i];
                    int LODLevel = doesNameContainLOD(a.Name);
                    if (LODLevel == -1) continue;
                    string safeName = a.Name.Replace("LOD" + LODLevel, "N");
                    if (nameToAsset.ContainsKey(safeName))//if a pair. remove the lower and save the higher
                    {
                        ImageBrowser.AssetSearchParam searchParam = nameToAsset[safeName];
                        if (LODLevel < searchParam.value)//if new is lower, keep it discard old
                        {
                            searchParam.value = LODLevel;

                            modelsListBox.filteredAssets.Remove(searchParam.asset);
                            searchParam.asset = a;
                        }
                        else//if new is higher, discard it
                        {
                            modelsListBox.filteredAssets.RemoveAt(i);
                        }
                    }
                    else
                    {
                        nameToAsset.Add(safeName, new ImageBrowser.AssetSearchParam(LODLevel, a));
                    }
                }
                modelsListBox.updateFilteredCount();
            }

            int filtered = modelsListBox.MaxFilteredCount;

            int populateStart = pageNumber * pageSize;
            int populateEnd = populateStart + pageSize;
            if (populateEnd > filtered) populateEnd = filtered;
            modelsListBox.PopulateBox(populateStart, populateEnd);

            filesListedLabel.Text = "Page " + (pageNumber + 1)
                + ": " + populateStart + " - " + populateEnd + " / " + filtered;
        }

        //returns -1 if no LOD, else the LOD
        private int doesNameContainLOD(string name)
        {
            if (!name.Contains("LOD")) return -1;
            if (name.Contains("Auto.dme")) return -1;
            string[] temp = name.Split('D');
            temp = temp[temp.Length - 1].Split('.');//isolate what is after d and before . should be the LOD number
            if (int.TryParse(temp[0], out int result)) return result;
            return -1;
        }

        private void nextPageButton_Click(object sender, EventArgs e)
        {
            int maxPageIndex = modelsListBox.MaxFilteredCount / pageSize;
            if (++pageNumber > maxPageIndex) pageNumber = maxPageIndex;
            refreshModelsListBox();
        }
        private void lastPageButton_Click(object sender, EventArgs e)
        {
            if (--pageNumber < 0) pageNumber = 0;
            refreshModelsListBox();
        }

        private void searchModelsText_TextChanged(object sender, EventArgs e)
        {
            searchModelsTimer.Stop();
            searchModelsTimer.Start();
        }

        private void searchModelsTimer_Tick(object sender, EventArgs e)
        {
            if (searchModelsText.Text.Length > 0)
            {
                searchModelsText.BackColor = Color.Yellow;
                clearSearchModelsText.Enabled = true;
            }
            else
            {
                searchModelsText.BackColor = Color.White;
                clearSearchModelsText.Enabled = false;
            }

            searchModelsTimer.Stop();

            refreshModelsListBox();
        }

        private void clearSearchModelsText_Click(object sender, EventArgs e)
        {
            searchModelsText.Clear();
        }

        private void modelsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Asset asset = null;

            exportSelectedModelsToolStripButton.Enabled = modelsListBox.SelectedItems.Count > 0;

            try
            {
                asset = (Asset)modelsListBox.SelectedItem;
            }
            catch (InvalidCastException) { return; }

            System.IO.MemoryStream memoryStream = asset.Pack.CreateAssetMemoryStreamByName(asset.Name);

            try
            {
                model = Model.LoadFromStream(asset.Name, memoryStream);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
            if (model == null)
            {
                Console.WriteLine("Unable to load " + asset.Name + " from memorystream.");
                return;
            }



            ModelBrowserModelStats1.Model = model;
            textures.Clear();

            for (int i = 0; i < model.Meshes.Length; i++)
            {
                textures.Add(gray);
            }

            materialSelectionComboBox.Items.Clear();
            if (model.TextureStrings.Count == 0)
            {
                currentShader = untexturedShader;
            }
            else
            {

                foreach (string textureName in model.TextureStrings)
                {
                    materialSelectionComboBox.Items.Add(textureName);
                }
                currentShader = texturedShader;
            }
            materialSelectionComboBox.SelectedIndex = materialSelectionComboBox.Items.Count > 0 ? 0 : -1; //if no material found


            snapCameraToModel();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            List<String> fileNames = new List<string>();

            foreach (object selectedItem in modelsListBox.SelectedItems)
            {
                Asset asset = null;

                try
                {
                    asset = (Asset)selectedItem;
                }
                catch (InvalidCastException) { continue; }

                fileNames.Add(asset.Name);
            }

            ModelExportForm modelExportForm = new ModelExportForm();
            modelExportForm.FileNames = fileNames;
            modelExportForm.ShowDialog();
        }

        private void snapCameraToModel()
        {
            if (model == null)
            {
                return;
            }

            Vector3 center = (model.Max + model.Min) / 2.0f;
            Vector3 extents = (model.Max - model.Min) / 2.0f;

            glControl1.Camera.DesiredTarget = center;
            glControl1.Camera.DesiredDistance = extents.Length * 1.75f;
        }

        private void showAxesButton_Click(object sender, EventArgs e)
        {
            glControl1.Invalidate();
        }

        private void showWireframeButton_Click(object sender, EventArgs e)
        {
            glControl1.Invalidate();
        }

        private void showAABBButton_Click(object sender, EventArgs e)
        {
            glControl1.Invalidate();
        }

        private void renderModeWireframeButton_Click(object sender, EventArgs e)
        {
            foreach (ToolStripButton button in renderModeButtons)
            {
                button.Checked = (sender == button);
            }
        }

        private void showBoundingBoxButton_Click(object sender, EventArgs e)
        {
            glControl1.Invalidate();
        }

        private void glControl1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F5:
                    renderModeWireframeButton.Checked = true;
                    break;
                case Keys.F6:
                    renderModeSmoothButton.Checked = true;
                    break;
            }
        }

        private void renderModeWireframeButton_CheckedChanged(object sender, EventArgs e)
        {
            if (renderModeWireframeButton.Checked)
            {
                foreach (ToolStripButton button in renderModeButtons)
                {
                    if (sender != button)
                    {
                        button.Checked = false;
                    }
                }
            }
        }

        private void renderModeSmoothButton_CheckedChanged(object sender, EventArgs e)
        {
            if (renderModeSmoothButton.Checked)
            {
                foreach (ToolStripButton button in renderModeButtons)
                {
                    if (sender != button)
                    {
                        button.Checked = false;
                    }
                }
            }
        }

        private void glControl1_MouseEnter(object sender, EventArgs e)
        {
            glControl1.Focus();
        }

        private void showAutoLODModelsButton_CheckedChanged(object sender, EventArgs e)
        {
            refreshModelsListBox();
        }

        private void showMultipleLODsButton_CheckedChanged(object sender, EventArgs e)
        {
            refreshModelsListBox();
        }

        private void showCollisionModelsButton_CheckedChanged(object sender, EventArgs e)
        {
            refreshModelsListBox();
        }

        Int32 currentTexture = 0;
        private void materialSelectionComboBox_Changed(object sender, EventArgs e)
        {
            // Set the new texture
            currentTexture = LoadTexture(materialSelectionComboBox.Text);
        }

        private int LoadTexture(string name)
        {
            MemoryStream textureMemoryStream = AssetManager.Instance.CreateAssetMemoryStreamByName(name);
            return TextureManager.LoadFromStream(textureMemoryStream);
        }

        public void SetTextureForMesh(int meshID, string name)
        {
            //if (textures[meshID] != gray) GL.DeleteTexture(textures[meshID]);
            textures[meshID] = LoadTexture(name);
        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {

        }

        private void toolStripButton1_Click_1(object sender, EventArgs e)
        {
            backgroundColorDialog.ShowDialog();
        }
    }
}
