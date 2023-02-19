using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ps2ls.Assets;

namespace ps2ls.Forms
{
    public partial class ModelBrowserModelStats : UserControl
    {
        private Model model;

        public ModelBrowserModelStats()
        {
            InitializeComponent();
        }

        public Model Model
        {
            get { return model; }
            set
            {
                model = value;

                nameLabel.Text = model != null ? model.name : "";
                meshCountLabel.Text = model != null ? model.meshes.Length.ToString() : "0";
                modelVertexCountLabel.Text = model != null ? model.vertexCount.ToString() : "0";
                modelTriangleCountLabel.Text = model != null ? (model.indexCount / 3).ToString() : "0";
                materialCount.Text = model != null ? model.dma.materials.Length.ToString() : "0";
                modelUnknown0Label.Text = model != null ? model.unknown0.ToString() : "0";
                modelUnknown1Label.Text = model != null ? model.unknown1.ToString() : "0";
                mdoelUnknown2Label.Text = model != null ? model.unknown2.ToString() : "0";
                BoneDrawCallsLabel.Text = model != null ? model.boneDrawCalls.Length.ToString() : "0";
                BoneMapCountLabel.Text = model != null ? model.boneMapEntries.Length.ToString() : "0";
                modelVersionLabel.Text = model != null ? model.version.ToString() : "0";

                meshesComboBox.Items.Clear();
                textureComboBox1.Items.Clear();
                texturesComboBox2.Items.Clear();

                texturesComboBox2.Visible = false;
                label18.Visible = false;

                if (model != null)
                {
                    for (int i = 0; i < model.meshes.Length; ++i)
                    {
                        meshesComboBox.Items.Add("Mesh " + i);
                        textureComboBox1.Items.Add("" + i);
                    }

                    foreach(string s in model.dma.textureStrings)
                    {
                        texturesComboBox2.Items.Add(s);
                    }
                }

                if (meshesComboBox.Items.Count > 0)
                {
                    meshesComboBox.SelectedIndex = 0;
                }


            }
        }

        private void meshesComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Mesh mesh = null;

            if (model != null && meshesComboBox.SelectedIndex >= 0)
            {
                mesh = model.meshes[meshesComboBox.SelectedIndex];
            }

            meshVertexCountLabel.Text = mesh != null ? mesh.vertexCount.ToString() : "0";
            meshTriangleCountLabel.Text = mesh != null ? (mesh.indexCount / 3).ToString() : "0";
            //meshBytesPerVertexLabel.Text = mesh != null ? mesh.BytesPerVertex.ToString() : "0";
            meshIndexLabel.Text = mesh != null ? mesh.drawCallOffset.ToString() : "0";
            meshUnknown1Label.Text = mesh != null ? mesh.drawCallCount.ToString() : "0";
            meshUnknown2Label.Text = mesh != null ? mesh.boneTransformCount.ToString() : "0";
            meshUnknown3Label.Text = mesh != null ? mesh.unknown3.ToString() : "0";
            meshVertexBlockCountLabel.Text = mesh != null ? mesh.vertexStreams.Length.ToString() : "0";
        }

        private void textureComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            texturesComboBox2.Visible = true;
            texturesComboBox2.SelectedItem = null;
            label18.Visible = true;
        }

        private void texturesComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            ModelBrowser.Instance.SetTextureForMesh(int.Parse((string)textureComboBox1.SelectedItem), (string)texturesComboBox2.SelectedItem);
        }
    }
}
