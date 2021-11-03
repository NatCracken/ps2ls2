using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.XPath;
using ps2ls.Cryptography;

namespace ps2ls.Graphics.Materials
{
    public class DrawStyle
    {
        public string Name { get; private set; }
        public uint NameHash { get; private set; }
        public string Effect { get; private set; }
        public uint VertexLayoutNameHash { get; private set; }

        private DrawStyle()
        {
            Name = string.Empty;
            NameHash = 0;
            Effect = string.Empty;
            VertexLayoutNameHash = 0;
        }

        public static DrawStyle LoadFromXPathNavigator(XPathNavigator navigator)
        {
            if (navigator == null)
            {
                return null;
            }

            DrawStyle drawStyle = new DrawStyle();

            //name
            drawStyle.Name = navigator.GetAttribute("Name", string.Empty);
            drawStyle.NameHash = Jenkins.OneAtATime(drawStyle.Name);

            //effect
            drawStyle.Effect = navigator.GetAttribute("Effect", String.Empty);

            //input layout
            String vertexLayout = navigator.GetAttribute("InputLayout", String.Empty);
            drawStyle.VertexLayoutNameHash = Jenkins.OneAtATime(vertexLayout);

            return drawStyle;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
