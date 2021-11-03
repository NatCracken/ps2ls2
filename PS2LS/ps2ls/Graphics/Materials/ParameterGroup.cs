using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;
using ps2ls.Cryptography;

namespace ps2ls.Graphics.Materials
{
    public class ParameterGroup
    {
        public string Name { get; private set; }
        public uint NameHash { get; private set; }
        public List<Parameter> Parameters { get; private set; }
        private ParameterGroup()
        {
            Name = string.Empty;
            NameHash = 0;
            Parameters = new List<Parameter>();
        }

        public static ParameterGroup LoadFromXPathNavigator(XPathNavigator navigator)
        {
            if (navigator == null) return null;

            ParameterGroup parameterGroup = new ParameterGroup();

            //name
            parameterGroup.Name = navigator.GetAttribute("Name", string.Empty);
            parameterGroup.NameHash = Jenkins.OneAtATime(parameterGroup.Name);

            //parameters
            XPathNodeIterator entries = navigator.Select("./Array[@Name='Parameters']/Object[@Class='IntParameter']"); //TODO, match with other parameters, floatparemeter, float4perameter etc...

            while (entries.MoveNext())
            {
                Parameter parameter = Parameter.LoadFromXPathNavigator(entries.Current);
                if (parameter != null) parameterGroup.Parameters.Add(parameter);
            }

            return parameterGroup;
        }

        public class Parameter
        {
            public string Name { get; private set; }
            public uint NameHash { get; private set; }
            public string Variable { get; private set; }
            public uint VariableHash { get; private set; }

            public object defaultValue;

            private Parameter()
            {
                Name = string.Empty;
                NameHash = 0;
                Variable = string.Empty;
                VariableHash = 0;
                defaultValue = 0;
            }

            public static Parameter LoadFromXPathNavigator(XPathNavigator navigator)
            { 
                if (navigator == null) return null;

                Parameter parameter = new Parameter();

                //name
                parameter.Name = navigator.GetAttribute("Name", string.Empty);
                parameter.NameHash = Jenkins.OneAtATime(parameter.Name);

                //variable
                parameter.Variable = navigator.GetAttribute("Variable", string.Empty);
                parameter.VariableHash = Jenkins.OneAtATime(parameter.Variable);

                //value
                parameter.defaultValue = navigator.GetAttribute("Default", string.Empty); //TODO, parse to number

                return parameter;
            }
        }
    }
}
