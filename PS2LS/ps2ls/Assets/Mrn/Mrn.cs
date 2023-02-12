using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ps2ls.Assets.Mrn
{
    public class MrnData
    {
        public static MrnData LoadFromStream(string name, Stream stream)
        {
            BinaryReader binaryReader = new BinaryReader(stream);
            Console.WriteLine("Reading " + name + "; Not Yet Implimented.");
            MrnData mrn = new MrnData();
            return mrn;
        }
    }
}
