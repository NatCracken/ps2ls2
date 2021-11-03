using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.XPath;

namespace ps2ls.Graphics.Materials
{
    public class MaterialDefinitionManager
    {
        #region Singleton
        private static MaterialDefinitionManager instance = null;

        public static void CreateInstance()
        {
            instance = new MaterialDefinitionManager();

            StringReader stringReader = new StringReader(Properties.Resources.materials_3);
            instance.loadFromStringReader(stringReader);
        }

        public static void DeleteInstance()
        {
            instance = null;
        }

        public static MaterialDefinitionManager Instance { get { return instance; } }
        #endregion

        public Dictionary<uint, MaterialDefinition> MaterialDefinitions { get; private set; }
        public Dictionary<uint, ParameterGroup> ParameterGroups { get; private set; }
        public Dictionary<uint, VertexLayout> VertexLayouts { get; private set; }

        MaterialDefinitionManager()
        {
            MaterialDefinitions = new Dictionary<uint, MaterialDefinition>();
            ParameterGroups = new Dictionary<uint, ParameterGroup>();
            VertexLayouts = new Dictionary<uint, VertexLayout>();
        }

        private void loadFromStringReader(StringReader stringReader)
        {
            if (stringReader == null)
                return;

            XPathDocument document = null;

            try
            {
                document = new XPathDocument(stringReader);
            }
            catch (Exception)
            {
                return;
            }

            XPathNavigator navigator = document.CreateNavigator();

            bool loadedSuccesfully = true;

            //vertex layouts
            loadedSuccesfully = loadVertexLayoutsByXPathNavigator(navigator.Clone()) && loadedSuccesfully;

            //parameter groups
            loadedSuccesfully = loadParameterGroupsByXPathNavigator(navigator.Clone()) && loadedSuccesfully;

            //material definitions
            loadedSuccesfully = loadMaterialDefinitionsByXPathNavigator(navigator.Clone()) && loadedSuccesfully;


            if (loadedSuccesfully)
            {
                Console.WriteLine("Material Data Loaded");
            }
            else
            {
                Console.WriteLine("Error Loading Material Data");
            }
        }

        private bool loadMaterialDefinitionsByXPathNavigator(XPathNavigator navigator)
        {
            XPathNodeIterator materialDefinitions = null;

            try
            {
                materialDefinitions = navigator.Select("/Object/Array[@Name='MaterialDefinitions']/Object[@Class='MaterialDefinition']");
            }
            catch (Exception)
            {
                return false;
            }

            while (materialDefinitions.MoveNext())
            {
                MaterialDefinition materialDefinition = MaterialDefinition.LoadFromXPathNavigator(materialDefinitions.Current);

                if (materialDefinition != null && !hasMaterialHash(materialDefinition.NameHash))
                {
                    MaterialDefinitions.Add(materialDefinition.NameHash, materialDefinition);
                }
            }
            return true;
        }


        private bool loadParameterGroupsByXPathNavigator(XPathNavigator navigator)
        {
            XPathNodeIterator parameterGroups = null;

            try
            {
                parameterGroups = navigator.Select("/Object/Array[@Name='ParameterGroups']/Object[@Class='ParameterGroup']");
            }
            catch (Exception)
            {
                return false;
            }

            while (parameterGroups.MoveNext())
            {
                ParameterGroup parameterGroup = ParameterGroup.LoadFromXPathNavigator(parameterGroups.Current);

                if (parameterGroup != null && !hasParameterGroupHash(parameterGroup.NameHash))
                {
                    ParameterGroups.Add(parameterGroup.NameHash, parameterGroup);
                }
            }
            return true;
        }

        private bool loadVertexLayoutsByXPathNavigator(XPathNavigator navigator)
        {
            //material definitions
            XPathNodeIterator vertexLayouts = null;

            try
            {
                vertexLayouts = navigator.Select("/Object/Array[@Name='InputLayouts']/Object[@Class='InputLayout']");
            }
            catch (Exception)
            {
                return false;
            }

            while (vertexLayouts.MoveNext())
            {
                VertexLayout vertexLayout = VertexLayout.LoadFromXPathNavigator(vertexLayouts.Current);

                if (vertexLayout != null && false == VertexLayouts.ContainsKey(vertexLayout.NameHash))
                {
                    VertexLayouts.Add(vertexLayout.NameHash, vertexLayout);
                }
            }
            return true;
        }

        public MaterialDefinition GetMaterialDefinitionFromHash(UInt32 materialDefinitionHash)
        {
            MaterialDefinition materialDefinition = null;

            try
            {
                MaterialDefinitions.TryGetValue(materialDefinitionHash, out materialDefinition);
            }
            catch (Exception)
            {
                throw new Exception("Material definition could not be found.");
            }

            return materialDefinition;
        }

        public bool hasMaterialHash(uint definitionHash)
        {
            return MaterialDefinitions.ContainsKey(definitionHash);
        }
        public bool hasParameterGroupHash(uint definitionHash)
        {
            return ParameterGroups.ContainsKey(definitionHash);
        }
        public bool hasVertexHash(uint definitionHash)
        {
            return VertexLayouts.ContainsKey(definitionHash);
        }
    }
}
