using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.XPath;

namespace ps2ls.Assets
{
    public class ActorDefinition
    {

        #region Animation
        public string animationNetworkFileName { get; private set; }
        public string animationNetworkSetName { get; private set; }
        public AnimationEnumeration[] animationEnumerations { get; private set; }
        public class AnimationEnumeration
        {
            public int hash { get; private set; }
            public int value { get; private set; }
        }
        public string animationValues { get; private set; }
        #endregion

        #region Base
        public string modelName { get; private set; }
        public string fileName { get; private set; }
        public string paletteName { get; private set; }
        public string materialName
        {
            get { return paletteName; }
            private set { paletteName = value; }
        }
        #endregion
        //updateRadius
        //waterDisplacementHeight
        //objectTerrainDataId

        public TextureAlias[] textureAliases { get; private set; }
        public class TextureAlias
        {
            public string aliasName { get; private set; }
            public string textureName { get; private set; }
            public int modelType { get; private set; }
            public int materialIndex { get; private set; }
            public int hash { get; private set; }
            public int defaultAlias { get; private set; }
            public int occlusionSlotBitmask { get; private set; }
        }
        public string tintAliases { get; private set; }
        public TintInheritance tintInheritance { get; private set; }
        public class TintInheritance
        {
            public int camo { get; private set; }
            public int decal { get; private set; }
            public int empire { get; private set; }
            public int extra { get; private set; }
            public int export { get; private set; }
        }
        public LOD[] lods { get; private set; }
        public class LOD
        {
            public string filename { get; private set; }
            public string palletName { get; private set; }
            public string materialName
            {
                get { return palletName; }
                private set { palletName = value; }
            }
            public int distance { get; private set; }
        }
        public LOD lodAuto { get; private set; }
        public Effect[] effectList { get; private set; }
        public class Effect
        {
            public string toolName { get; private set; }
            public string type { get; private set; }
            public string name { get; private set; }
            public int id { get; private set; }
        }

        #region Collision
        public int collisionType { get; private set; }
        public string collisionDataFile { get; private set; }
        public string simpleCollisionDataFile { get; private set; }
        #endregion

        #region Usage
        public int actorUsage { get; private set; }
        public bool borrowSkelleton { get; private set; }
        public string boneAttachmentName { get; private set; }
        public bool validatePcNpc { get; private set; }
        #endregion

        public string equippedSlotName { get; private set; }
        public string[] childAttachSLots { get; private set; }
        public Mountable mountable { get; private set; }
        public class Mountable
        {
            public int minOccupancy { get; private set; }
            public string animSlotPrifix { get; private set; }
            public string idleAnim { get; private set; }
            public string runAnim { get; private set; }
            public string runToIdleAnim { get; private set; }
            public Seat[] seatList { get; private set; }
            public class Seat
            {
                public string bone { get; private set; }
                public string animation { get; private set; }
                public int controller { get; private set; }
                public bool riderMimicsSeatAnim { get; private set; }
                public Bone entranceBone { get; private set; }
                public Bone exitBone { get; private set; }
                public class Bone
                {
                    public string name { get; private set; }
                    public string animation { get; private set; }
                    public string location { get; private set; }
                }
            }
        }
        public string collusionFile { get; private set; }
        public int occlusionVisibility { get; private set; }
        public string materialType { get; private set; }
        public int invisibleValue { get; private set; }
        public bool loadedSuccessfully { get; private set; }
        public ActorDefinition(MemoryStream memoryStream, out bool loaded)
        {
            loadedSuccessfully = false;
            loaded = loadedSuccessfully;
            if (memoryStream == null)
                return;

            XPathDocument document;

            try
            {
                document = new XPathDocument(memoryStream);
            }
            catch (Exception)
            {
                return;
            }

            loadFromXPathNavigator(document.CreateNavigator());

            loadedSuccessfully = true;
            loaded = loadedSuccessfully;
        }

        private void loadFromXPathNavigator(XPathNavigator baseNavigator)
        {
            XPathNavigator navigator = baseNavigator.Clone();
            Console.WriteLine(navigator.LocalName);
        }


    }

}
