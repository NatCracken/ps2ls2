using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.XPath;
using ps2ls.Cryptography;

namespace ps2ls.Graphics.Materials
{
    public class MaterialDefinition
    {
        public string Name { get; private set; }
        public uint NameHash { get; private set; }
        public string Type { get; private set; }
        public uint TypeHash { get; private set; }
        public List<DrawStyle> DrawStyles { get; private set; }

        private MaterialDefinition()
        {
            Name = string.Empty;
            NameHash = 0;
            Type = string.Empty;
            TypeHash = 0;
            DrawStyles = new List<DrawStyle>();
        }

        public static MaterialDefinition LoadFromXPathNavigator(XPathNavigator navigator)
        {
            if (navigator == null) return null;

            MaterialDefinition materialDefinition = new MaterialDefinition();

            //name
            materialDefinition.Name = navigator.GetAttribute("Name", string.Empty);
            materialDefinition.NameHash = Jenkins.OneAtATime(materialDefinition.Name);

            //type
            materialDefinition.Type = navigator.GetAttribute("Type", string.Empty);
            materialDefinition.TypeHash = Jenkins.OneAtATime(materialDefinition.Type);

            //draw styles
            XPathNodeIterator entries = navigator.Select("./Array[@Name='DrawStyles']/Object[@Class='DrawStyle']");

            while (entries.MoveNext())
            {
                DrawStyle drawStyle = DrawStyle.LoadFromXPathNavigator(entries.Current);
                if (drawStyle != null) materialDefinition.DrawStyles.Add(drawStyle);
            }

            return materialDefinition;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
