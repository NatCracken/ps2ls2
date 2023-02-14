using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections.Concurrent;
using System.Threading;

namespace ps2ls.Assets.Pack
{

    class NamelistGenerator
    {
        //This is a reimplimentation of the work done in https://github.com/brhumphe/dbg-pack and https://github.com/RhettVX/forgelight-toolbox/tree/master/FLUtils

        /*static readonly string testString = @"DMOD?????DMAT???0??purple.dds?Shared_Rock_Large_DN.dds?Hossin_Props_TreeStumps_OC.dds?grey.dds?Indar_Flora_Trees_Highlands_Bark_S.dds?Indar_Flora_Trees_Highlands_Bark_N.dds?Indar_Flora_Trees_Highlands_Bark_C.dds?Esamir_Props_TreeBranch_NoSnow_S.dds?Esamir_Props_TreeBranch_NoSnow_N.dds?Esamir_Props_TreeBranch_NoSnow_C.dds?????wQh??]d???????X????????????>?????????????n??????????????????????????????????fNf?????????P_,_?w?[???????????????????????????????@K?????????????|?""?1???????????????%??????????""?|sA?????????9?\%??
                            ??????????????P??????????????j/?????????o?9?!???????????,X?\??[??????????????? t??????????????O????????????????l?6P??EWD??????X?????????????????????????u?c???????????????=???????????????=???=???=????v???????????????????????????V+??????????????????????????A?Y??????????????N?>???????????????????????????????;M???????????????P????????????N?j/??????????w?v!???????????
                            Li\??[??????????????O???????????????????«??????\??@Ma?A??@??????????????????????????????????y?V°A????µ[??J?A?lG1?>YN?A?Z??G1?>YN?A?Z??µ[??J?A?l?
                            >??A?Ng?p9_?w???E?pCa?;??>?????k????????p9_?w???E?°?1?"">????pCa?;??>?????r=???
                            ??{??g?>????1??9V??p???1??9V??p??{??g?>???R???@RTw??$???[k?p7???z??[k?pD?R?:??_u=???R?:??_u=?????z??[k?pD?3??D?=?M??
                            ?\?w?t>?v?$???[k?p7?R?:??_u=??????kk@;?G??$???[k?p7?
                            ";*/

        private static NamelistGenerator Instance;
        private GenericLoadingForm loadingForm;
        private BackgroundWorker buildNamelistBackgroundWorker;
        private NamelistGenerator()
        {
            buildNamelistBackgroundWorker = new BackgroundWorker();
            buildNamelistBackgroundWorker.WorkerReportsProgress = true;
            buildNamelistBackgroundWorker.ProgressChanged += new ProgressChangedEventHandler(BuildNamelistProgressChanged);
            buildNamelistBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BuildNamelistWorkerCompleted);
            buildNamelistBackgroundWorker.DoWork += new DoWorkEventHandler(BuildNamelistDoWork);
        }


        private void BuildNamelistProgressChanged(object sender, ProgressChangedEventArgs args)
        {
            loadingForm.SetProgressBarPercent(args.ProgressPercentage);
            loadingForm.SetLabelText((string)args.UserState);
        }

        private void BuildNamelistDoWork(object sender, DoWorkEventArgs args)
        {
            BuildNamelist(sender, args.Argument);
        }

        int threadCount;
        ConcurrentDictionary<ulong, string> nameDict;
        ConcurrentDictionary<string, byte> processedNames;
        BackgroundWorker currentWorker;
        string packName;
        string nameListDirectory;

        private void BuildNamelist(object sender, object arg)
        {
            currentWorker = (BackgroundWorker)sender;

            threadCount = Environment.ProcessorCount;
            nameDict = new ConcurrentDictionary<ulong, string>(threadCount * 2, 262144);
            //nameDict.TryAdd(0x4137cc65bd97fd30, @"{NAMELIST}");
            processedNames = new ConcurrentDictionary<string, byte>(threadCount * 2, 262144);
            int packCount = targetPacks.Length;
            for (int i = 0; i < packCount; i++)
            {
                ExtractFromPack(targetPacks[i], i + 1 + "/" + packCount);
            }

            nameListDirectory = Path.GetDirectoryName(targetPacks[0]) + @"\NameList" + (useRegex ? "_RegEx" : "") + ".txt";
            StreamWriter writer = new StreamWriter(nameListDirectory, false);
            int count = nameDict.Count;
            int j = 0;
            foreach (ulong key in nameDict.Keys)
            {
                writer.WriteLine(key + ":" + nameDict[key]);
                if (++j % 1000 != 0) continue;
                currentWorker.ReportProgress((int)(100f * (j + 1f) / count), nameDict[key] + " | " + j + " / " + count);
                //Thread.Sleep(0);
            }
            writer.Close();
        }

        private void BuildNamelistWorkerCompleted(object sender, RunWorkerCompletedEventArgs args)
        {
            loadingForm.Close();
            MessageBox.Show("Found " + processedNames.Count + " names in: " + DateTime.Now.Subtract(startTime).ToString(@"hh\:mm\:ss") + ".\rNamelist created at:\r" + nameListDirectory, "Namelist Created", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void GenerateNameList(string[] getPacks, bool useRegex)
        {
            if (Instance == null) Instance = new NamelistGenerator();
            Instance.GenerateNameListInternal(getPacks, useRegex);
        }

        string[] targetPacks;
        bool useRegex;
        DateTime startTime;
        private void GenerateNameListInternal(string[] getPacks, bool useRegex)
        {
            if (buildNamelistBackgroundWorker.IsBusy) return;
            targetPacks = getPacks;
            this.useRegex = useRegex;
            startTime = DateTime.Now;
            loadingForm = new GenericLoadingForm();
            loadingForm.SetWindowTitle("Generating Namelist...");
            loadingForm.Show();

            buildNamelistBackgroundWorker.RunWorkerAsync();
        }

        private struct AssetLite
        {
            public ulong offset;
            public ulong dataLength;
            public bool isZipped;
        }

        ConcurrentQueue<int> remainingProcess;
        private void ExtractFromPack(string path, string progress)
        {
            AssetLite[] assets = IsolateAssets(path);
            int assetCount = assets.Length;

            remainingProcess = new ConcurrentQueue<int>();
            for (int i = 0; i < assetCount; i++) remainingProcess.Enqueue(i);

            packName = Path.GetFileName(path);
            FileStream[] fileStreams = new FileStream[threadCount];
            Thread[] threads = new Thread[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
                int index = i;
                fileStreams[index] = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                threads[index] = new Thread(() => ExtractNamesThread(assets, fileStreams[index])) { Name = "Search_" + packName + index };
                threads[index].Start();
            }

            bool cont = true;
            while (cont)
            {
                cont = false;
                foreach (Thread thread in threads)
                {
                    if (!thread.IsAlive) continue;
                    cont = true;
                    break;
                }
                currentWorker.ReportProgress((int)(100f * (1f - remainingProcess.Count / (float)assetCount)), packName + " | " + progress + " | " + processedNames.Count + " Names");
                Thread.Sleep(100);
            }

            foreach (FileStream fs in fileStreams) fs.Dispose();
        }


        private AssetLite[] IsolateAssets(string path)
        {
            FileStream fileStream = File.OpenRead(path);
            BinaryReader reader = new BinaryReader(fileStream);

            fileStream.Seek(4, SeekOrigin.Begin);
            uint assetCount = reader.ReadUInt32();
            fileStream.Seek(8, SeekOrigin.Current);
            ulong mapOffset = reader.ReadUInt64();
            fileStream.Seek(Convert.ToInt64(mapOffset), SeekOrigin.Begin);

            AssetLite[] toReturn = new AssetLite[assetCount];
            for (int i = 0; i < assetCount; i++)
            {
                fileStream.Seek(8, SeekOrigin.Current);
                ulong offset = reader.ReadUInt64();
                ulong dataLength = reader.ReadUInt64();
                uint zippedflag = reader.ReadUInt32();
                fileStream.Seek(4, SeekOrigin.Current);

                toReturn[i] = new AssetLite()
                {
                    offset = offset,
                    dataLength = dataLength,
                    isZipped = Asset.TestZipped(zippedflag) && dataLength > 0,
                };

            }

            fileStream.Dispose();
            return toReturn;
        }

        private void ExtractNamesThread(AssetLite[] assets, FileStream fileStream)
        {
            while (remainingProcess.TryDequeue(out int todo))
            {
                string[] foundNames = ExtractNames(CreateBufferFromAsset(assets[todo], fileStream), useRegex);
                if (foundNames.Length == 0) continue;
                foundNames = ProcessNames(foundNames);
                foreach (string name in foundNames) SaveName(name);
            }
        }

        private byte[] CreateBufferFromAsset(AssetLite asset, FileStream fileStream)
        {
            byte[] buffer = new byte[asset.dataLength];

            long offset = Convert.ToInt64(asset.offset) + (asset.isZipped ? 8 : 0);//zipped assets need another offset of 8 bytes
            fileStream.Seek(offset, SeekOrigin.Begin);
            fileStream.Read(buffer, 0, (int)asset.dataLength);

            if (asset.isZipped) buffer = Pack.Decompress(buffer);

            return buffer;
        }

        static readonly byte[] bFsb5 = Encoding.UTF8.GetBytes("FSB5");
        static readonly byte[] bActorRuntime = Encoding.UTF8.GetBytes("<ActorRuntime>");
        private static string[] ExtractNames(byte[] source, bool useRegex)
        {
            if (source.Length == 0) return new string[0];
            if (MatchBytes(source, bFsb5))
            {
                int readHead = (int)BitConverter.ToUInt32(source, 12) + 64;
                return new string[] { ReadTerminatedString(source, readHead) + ".fsb" };
            }
            if (MatchBytes(source, bActorRuntime))
            {
                return SearchADR(source);
            }

            if (useRegex) return PatternMatchExtractNames(source);
            return ByteMatchExtractNames(source);
        }

        private static string ReadTerminatedString(byte[] source, int offset = 0, byte terminator = 0x0)
        {
            return ReadByteString(source, offset, CountTerminatedString(source, offset, terminator));
        }

        private static int CountTerminatedString(byte[] source, int offset = 0, byte terminator = 0x0)
        {
            int size = 0;
            while (source[offset + size] != terminator) { size++; }
            return size;
        }

        private static string ReadByteString(byte[] source, int offset, int length)
        {
            return Encoding.UTF8.GetString(source, offset, length);
        }

        static readonly byte[] bName = Encoding.UTF8.GetBytes("Name=");
        static readonly byte[] bModel = Encoding.UTF8.GetBytes("Model=");
        static readonly byte[] bDmeL = Encoding.UTF8.GetBytes("_LOD0.dme");
        static readonly byte[] bAdr = Encoding.UTF8.GetBytes(".adr");
        private static string[] SearchADR(byte[] source)
        {
            List<string> names = new List<string>();
            for (int i = 0; i < source.Length; i++)
            {
                if (MatchBytes(source, bName, i))
                {
                    i += 6;
                    int size = CountTerminatedString(source, i, 34);//34 = ` " `
                    if (size == 0 || source[i + size - 4] != 46) continue;//check for 3 leter filetype, 46 = ' . '
                    names.Add(ReadByteString(source, i, size));

                    int extentionIndex = i + size - 9;
                    if (MatchBytes(source, bDmeL, extentionIndex)) //|| MatchBytes(source, bDmeU, extentionIndex))
                    {
                        for (int j = 0; j < bAdr.Length; j++) source[extentionIndex + j] = bAdr[j];
                        names.Add(ReadByteString(source, i, size - 5));
                    }

                    i += size + 1;
                    continue;
                }
                if (MatchBytes(source, bModel, i))
                {
                    i += 7;
                    int size = CountTerminatedString(source, i, 34);
                    if (size == 0 || source[i + size - 4] != 46) continue;
                    names.Add(ReadByteString(source, i, size));
                    i += size + 1;
                    continue;
                }
            }

            //Console.WriteLine(Encoding.UTF8.GetString(source));

            return names.ToArray();
            /*
            MatchCollection matches = filePattern.Matches(Encoding.UTF8.GetString(source));
            foreach (Match m in matches) names.Add(m.Value + ".adr");
            return names.ToArray();*/
        }

        private static bool MatchBytes(byte[] source, byte[] match, int sourceOffset = 0)
        {
            if (match.Length + sourceOffset > source.Length) return false;
            for (int i = 0; i < match.Length; i++) if (source[sourceOffset + i] != match[i]) return false;
            return true;
        }

        private static void SkipLine(byte[] source, ref int offset)
        {
            while (source[offset] != 10) { offset++; } //adr definition use 10-13 (newline-carrage return)
            offset += 2;
        }

        //static readonly Regex removePattern = new Regex(@"[^\u002D-\u007A]", RegexOptions.Compiled);
        static readonly Regex filePattern = new Regex(@"([><\w-]+\.(" +
            @"adr|agr|ags|apb|apx|bat|bin|cdt|cnk0|cnk1|cnk2|cnk3|cnk4|cnk5|
            crc|crt|cso|cur|dat|db|dds|def|dir|dll|
            dma|dme|dmv|dsk|dx11efb|dx11rsb|dx11ssb|eco|efb|exe|
            fsb|fxd|fxo|gfx|gnf|i64|ini|jpg|lst|lua|mrn|pak|
            pem|playerstudio|png|prsb|psd|pssb|tga|thm|tome|ttf|
            txt|vnfo|wav|xlsx|xml|xrsb|xssb|zone" +
            @"))", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static string[] PatternMatchExtractNames(byte[] source)
        {
            List<string> names = new List<string>();
            MatchCollection matches = filePattern.Matches(Encoding.UTF8.GetString(source));
            foreach (Match m in matches) names.Add(m.Value);
            return names.ToArray();
        }

        static readonly List<byte[]> fileExtentions = new List<byte[]>() {
            Encoding.UTF8.GetBytes("adr"),
            Encoding.UTF8.GetBytes("agr"),
            Encoding.UTF8.GetBytes("Agr"),
            Encoding.UTF8.GetBytes("ags"),
            Encoding.UTF8.GetBytes("apx"),
            Encoding.UTF8.GetBytes("bat"),
            Encoding.UTF8.GetBytes("bin"),
            Encoding.UTF8.GetBytes("cdt"),
            Encoding.UTF8.GetBytes("cnk0"),
            Encoding.UTF8.GetBytes("cnk1"),
            Encoding.UTF8.GetBytes("cnk2"),
            Encoding.UTF8.GetBytes("cnk3"),
            Encoding.UTF8.GetBytes("cnk4"),
            Encoding.UTF8.GetBytes("cnk5"),
            Encoding.UTF8.GetBytes("crc"),
            Encoding.UTF8.GetBytes("crt"),
            Encoding.UTF8.GetBytes("cso"),
            Encoding.UTF8.GetBytes("cur"),
            Encoding.UTF8.GetBytes("Cur"),
            Encoding.UTF8.GetBytes("dat"),
            Encoding.UTF8.GetBytes("Dat"),
            Encoding.UTF8.GetBytes("db"),
            Encoding.UTF8.GetBytes("dds"),
            Encoding.UTF8.GetBytes("DDS"),
            Encoding.UTF8.GetBytes("def"),
            Encoding.UTF8.GetBytes("Def"),
            Encoding.UTF8.GetBytes("dir"),
            Encoding.UTF8.GetBytes("Dir"),
            Encoding.UTF8.GetBytes("dll"),
            Encoding.UTF8.GetBytes("DLL"),
            Encoding.UTF8.GetBytes("dma"),
            Encoding.UTF8.GetBytes("dme"),
            Encoding.UTF8.GetBytes("DME"),
            Encoding.UTF8.GetBytes("dmv"),
            Encoding.UTF8.GetBytes("dsk"),
            Encoding.UTF8.GetBytes("dx11efb"),
            Encoding.UTF8.GetBytes("dx11rsb"),
            Encoding.UTF8.GetBytes("dx11ssb"),
            Encoding.UTF8.GetBytes("eco"),
            Encoding.UTF8.GetBytes("efb"),
            Encoding.UTF8.GetBytes("exe"),
            Encoding.UTF8.GetBytes("fsb"),
            Encoding.UTF8.GetBytes("fxd"),
            Encoding.UTF8.GetBytes("fxo"),
            Encoding.UTF8.GetBytes("gfx"),
            Encoding.UTF8.GetBytes("gnf"),
            Encoding.UTF8.GetBytes("i64"),
            Encoding.UTF8.GetBytes("ini"),
            Encoding.UTF8.GetBytes("INI"),
            Encoding.UTF8.GetBytes("Ini"),
            Encoding.UTF8.GetBytes("jpg"),
            Encoding.UTF8.GetBytes("JPG"),
            Encoding.UTF8.GetBytes("lst"),
            Encoding.UTF8.GetBytes("lua"),
            Encoding.UTF8.GetBytes("mrn"),
            Encoding.UTF8.GetBytes("pak"),
            Encoding.UTF8.GetBytes("pem"),
            Encoding.UTF8.GetBytes("playerstudio"),
            Encoding.UTF8.GetBytes("PlayerStudio"),
            Encoding.UTF8.GetBytes("png"),
            Encoding.UTF8.GetBytes("prsb"),
            Encoding.UTF8.GetBytes("psd"),
            Encoding.UTF8.GetBytes("pssb"),
            Encoding.UTF8.GetBytes("tga"),
            Encoding.UTF8.GetBytes("TGA"),
            Encoding.UTF8.GetBytes("thm"),
            Encoding.UTF8.GetBytes("tome"),
            Encoding.UTF8.GetBytes("ttf"),
            Encoding.UTF8.GetBytes("txt"),
            Encoding.UTF8.GetBytes("vnfo"),
            Encoding.UTF8.GetBytes("wav"),
            Encoding.UTF8.GetBytes("xlsx"),
            Encoding.UTF8.GetBytes("xml"),
            Encoding.UTF8.GetBytes("xrsb"),
            Encoding.UTF8.GetBytes("xssb"),
            Encoding.UTF8.GetBytes("zone"),
            Encoding.UTF8.GetBytes("Zone"),};
        private static string[] ByteMatchExtractNames(byte[] source)
        {
            List<string> names = new List<string>();
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] != 46) continue;//search for periods
                if (!MatchBytesList(source, fileExtentions, out int fileExtentionLength, i + 1)) continue;//match known file extentions
                if (!FindWordsInverse(source, i - 1, out int nameLength)) continue;
                names.Add(Encoding.UTF8.GetString(source, i - nameLength, nameLength + fileExtentionLength + 1));
            }
            return names.ToArray();
        }

        private static bool MatchBytesList(byte[] source, List<byte[]> matches, out int length, int sourceOffset = 0)
        {
            foreach (byte[] match in matches)
            {
                if (MatchBytes(source, match, sourceOffset))
                {
                    length = match.Length;
                    return true;
                }
            }
            length = 0;
            return false;
        }

        private static bool FindWordsInverse(byte[] source, int sourceOffset, out int length)
        {
            length = 0;
            if (!IsWordCharacter(source[sourceOffset - length])) return false;
            do
            {
                length++;
                if (length > sourceOffset) return true;
            } while (IsWordCharacter(source[sourceOffset - length]));
            return true;
        }

        private static bool IsWordCharacter(byte toTest)
        {
            return toTest >= 48 & toTest <= 57 ||//numbers
                toTest >= 65 & toTest <= 90 ||//uppercase
                toTest >= 97 & toTest <= 122 ||//lowercase
                toTest == 95 || toTest == 45 || toTest == 60 || toTest == 62;// matches _, -, <, or >
        }

        private static string[] ProcessNames(string[] foundNames)
        {
            List<string> altNames = new List<string>();
            foreach (string name in foundNames)
            {
                if (name.Contains("<gender>"))
                {
                    altNames.Add(name.Replace("<gender>", "Male"));
                    altNames.Add(name.Replace("<gender>", "Female"));
                    continue;
                }
                if (name.Contains(".efb"))
                {
                    altNames.Add(name);
                    altNames.Add(name.Replace(".efb", ".dx11efb"));
                    continue;
                }
                if (name.Contains('<') || name.Contains('>'))
                {
                    altNames.Add(name.Replace("<", "").Replace("<", ""));
                    continue;
                }
                altNames.Add(name);
            }
            return altNames.ToArray();
        }

        private void SaveName(string name)
        {
            string upperName = name.Trim().ToUpper();
            if (processedNames.ContainsKey(upperName)) return;//this aught to be faster than processing the key and checking for that
            processedNames.TryAdd(upperName, 0);
            nameDict.TryAdd(CRC64Encode(upperName), name);
        }

        #region CRC64

        static readonly ulong[] crc_table = new ulong[] {
            0x0000000000000000, 0x7ad870c830358979,
            0xf5b0e190606b12f2, 0x8f689158505e9b8b,
            0xc038e5739841b68f, 0xbae095bba8743ff6,
            0x358804e3f82aa47d, 0x4f50742bc81f2d04,
            0xab28ecb46814fe75, 0xd1f09c7c5821770c,
            0x5e980d24087fec87, 0x24407dec384a65fe,
            0x6b1009c7f05548fa, 0x11c8790fc060c183,
            0x9ea0e857903e5a08, 0xe478989fa00bd371,
            0x7d08ff3b88be6f81, 0x07d08ff3b88be6f8,
            0x88b81eabe8d57d73, 0xf2606e63d8e0f40a,
            0xbd301a4810ffd90e, 0xc7e86a8020ca5077,
            0x4880fbd87094cbfc, 0x32588b1040a14285,
            0xd620138fe0aa91f4, 0xacf86347d09f188d,
            0x2390f21f80c18306, 0x594882d7b0f40a7f,
            0x1618f6fc78eb277b, 0x6cc0863448deae02,
            0xe3a8176c18803589, 0x997067a428b5bcf0,
            0xfa11fe77117cdf02, 0x80c98ebf2149567b,
            0x0fa11fe77117cdf0, 0x75796f2f41224489,
            0x3a291b04893d698d, 0x40f16bccb908e0f4,
            0xcf99fa94e9567b7f, 0xb5418a5cd963f206,
            0x513912c379682177, 0x2be1620b495da80e,
            0xa489f35319033385, 0xde51839b2936bafc,
            0x9101f7b0e12997f8, 0xebd98778d11c1e81,
            0x64b116208142850a, 0x1e6966e8b1770c73,
            0x8719014c99c2b083, 0xfdc17184a9f739fa,
            0x72a9e0dcf9a9a271, 0x08719014c99c2b08,
            0x4721e43f0183060c, 0x3df994f731b68f75,
            0xb29105af61e814fe, 0xc849756751dd9d87,
            0x2c31edf8f1d64ef6, 0x56e99d30c1e3c78f,
            0xd9810c6891bd5c04, 0xa3597ca0a188d57d,
            0xec09088b6997f879, 0x96d1784359a27100,
            0x19b9e91b09fcea8b, 0x636199d339c963f2,
            0xdf7adabd7a6e2d6f, 0xa5a2aa754a5ba416,
            0x2aca3b2d1a053f9d, 0x50124be52a30b6e4,
            0x1f423fcee22f9be0, 0x659a4f06d21a1299,
            0xeaf2de5e82448912, 0x902aae96b271006b,
            0x74523609127ad31a, 0x0e8a46c1224f5a63,
            0x81e2d7997211c1e8, 0xfb3aa75142244891,
            0xb46ad37a8a3b6595, 0xceb2a3b2ba0eecec,
            0x41da32eaea507767, 0x3b024222da65fe1e,
            0xa2722586f2d042ee, 0xd8aa554ec2e5cb97,
            0x57c2c41692bb501c, 0x2d1ab4dea28ed965,
            0x624ac0f56a91f461, 0x1892b03d5aa47d18,
            0x97fa21650afae693, 0xed2251ad3acf6fea,
            0x095ac9329ac4bc9b, 0x7382b9faaaf135e2,
            0xfcea28a2faafae69, 0x8632586aca9a2710,
            0xc9622c4102850a14, 0xb3ba5c8932b0836d,
            0x3cd2cdd162ee18e6, 0x460abd1952db919f,
            0x256b24ca6b12f26d, 0x5fb354025b277b14,
            0xd0dbc55a0b79e09f, 0xaa03b5923b4c69e6,
            0xe553c1b9f35344e2, 0x9f8bb171c366cd9b,
            0x10e3202993385610, 0x6a3b50e1a30ddf69,
            0x8e43c87e03060c18, 0xf49bb8b633338561,
            0x7bf329ee636d1eea, 0x012b592653589793,
            0x4e7b2d0d9b47ba97, 0x34a35dc5ab7233ee,
            0xbbcbcc9dfb2ca865, 0xc113bc55cb19211c,
            0x5863dbf1e3ac9dec, 0x22bbab39d3991495,
            0xadd33a6183c78f1e, 0xd70b4aa9b3f20667,
            0x985b3e827bed2b63, 0xe2834e4a4bd8a21a,
            0x6debdf121b863991, 0x1733afda2bb3b0e8,
            0xf34b37458bb86399, 0x8993478dbb8deae0,
            0x06fbd6d5ebd3716b, 0x7c23a61ddbe6f812,
            0x3373d23613f9d516, 0x49aba2fe23cc5c6f,
            0xc6c333a67392c7e4, 0xbc1b436e43a74e9d,
            0x95ac9329ac4bc9b5, 0xef74e3e19c7e40cc,
            0x601c72b9cc20db47, 0x1ac40271fc15523e,
            0x5594765a340a7f3a, 0x2f4c0692043ff643,
            0xa02497ca54616dc8, 0xdafce7026454e4b1,
            0x3e847f9dc45f37c0, 0x445c0f55f46abeb9,
            0xcb349e0da4342532, 0xb1eceec59401ac4b,
            0xfebc9aee5c1e814f, 0x8464ea266c2b0836,
            0x0b0c7b7e3c7593bd, 0x71d40bb60c401ac4,
            0xe8a46c1224f5a634, 0x927c1cda14c02f4d,
            0x1d148d82449eb4c6, 0x67ccfd4a74ab3dbf,
            0x289c8961bcb410bb, 0x5244f9a98c8199c2,
            0xdd2c68f1dcdf0249, 0xa7f41839ecea8b30,
            0x438c80a64ce15841, 0x3954f06e7cd4d138,
            0xb63c61362c8a4ab3, 0xcce411fe1cbfc3ca,
            0x83b465d5d4a0eece, 0xf96c151de49567b7,
            0x76048445b4cbfc3c, 0x0cdcf48d84fe7545,
            0x6fbd6d5ebd3716b7, 0x15651d968d029fce,
            0x9a0d8ccedd5c0445, 0xe0d5fc06ed698d3c,
            0xaf85882d2576a038, 0xd55df8e515432941,
            0x5a3569bd451db2ca, 0x20ed197575283bb3,
            0xc49581ead523e8c2, 0xbe4df122e51661bb,
            0x3125607ab548fa30, 0x4bfd10b2857d7349,
            0x04ad64994d625e4d, 0x7e7514517d57d734,
            0xf11d85092d094cbf, 0x8bc5f5c11d3cc5c6,
            0x12b5926535897936, 0x686de2ad05bcf04f,
            0xe70573f555e26bc4, 0x9ddd033d65d7e2bd,
            0xd28d7716adc8cfb9, 0xa85507de9dfd46c0,
            0x273d9686cda3dd4b, 0x5de5e64efd965432,
            0xb99d7ed15d9d8743, 0xc3450e196da80e3a,
            0x4c2d9f413df695b1, 0x36f5ef890dc31cc8,
            0x79a59ba2c5dc31cc, 0x037deb6af5e9b8b5,
            0x8c157a32a5b7233e, 0xf6cd0afa9582aa47,
            0x4ad64994d625e4da, 0x300e395ce6106da3,
            0xbf66a804b64ef628, 0xc5bed8cc867b7f51,
            0x8aeeace74e645255, 0xf036dc2f7e51db2c,
            0x7f5e4d772e0f40a7, 0x05863dbf1e3ac9de,
            0xe1fea520be311aaf, 0x9b26d5e88e0493d6,
            0x144e44b0de5a085d, 0x6e963478ee6f8124,
            0x21c640532670ac20, 0x5b1e309b16452559,
            0xd476a1c3461bbed2, 0xaeaed10b762e37ab,
            0x37deb6af5e9b8b5b, 0x4d06c6676eae0222,
            0xc26e573f3ef099a9, 0xb8b627f70ec510d0,
            0xf7e653dcc6da3dd4, 0x8d3e2314f6efb4ad,
            0x0256b24ca6b12f26, 0x788ec2849684a65f,
            0x9cf65a1b368f752e, 0xe62e2ad306bafc57,
            0x6946bb8b56e467dc, 0x139ecb4366d1eea5,
            0x5ccebf68aecec3a1, 0x2616cfa09efb4ad8,
            0xa97e5ef8cea5d153, 0xd3a62e30fe90582a,
            0xb0c7b7e3c7593bd8, 0xca1fc72bf76cb2a1,
            0x45775673a732292a, 0x3faf26bb9707a053,
            0x70ff52905f188d57, 0x0a2722586f2d042e,
            0x854fb3003f739fa5, 0xff97c3c80f4616dc,
            0x1bef5b57af4dc5ad, 0x61372b9f9f784cd4,
            0xee5fbac7cf26d75f, 0x9487ca0fff135e26,
            0xdbd7be24370c7322, 0xa10fceec0739fa5b,
            0x2e675fb4576761d0, 0x54bf2f7c6752e8a9,
            0xcdcf48d84fe75459, 0xb71738107fd2dd20,
            0x387fa9482f8c46ab, 0x42a7d9801fb9cfd2,
            0x0df7adabd7a6e2d6, 0x772fdd63e7936baf,
            0xf8474c3bb7cdf024, 0x829f3cf387f8795d,
            0x66e7a46c27f3aa2c, 0x1c3fd4a417c62355,
            0x935745fc4798b8de, 0xe98f353477ad31a7,
            0xa6df411fbfb21ca3, 0xdc0731d78f8795da,
            0x536fa08fdfd90e51, 0x29b7d047efec8728
        };

        public static ulong CRC64Encode(byte[] buffer)
        {
            return CRC64Encode(Encoding.UTF8.GetString(buffer).Trim().ToUpper());
        }

        public static ulong CRC64Encode(string buffer)//buffer should be trimmed and uppered beforehand
        {
            ulong value = 0xffffffffffffffff;

            foreach (char c in buffer)
            {
                int tableIndex = ((int)(value & 0xff) ^ c) & 0xFF;
                value = crc_table[tableIndex] ^ (value >> 8);
            }

            return value ^ 0xffffffffffffffff;
        }
        #endregion


        #region DiffTool
        public static void DiffNameLists(string[] getNameLists)
        {
            if (getNameLists.Length != 2)
            {
                MessageBox.Show("You must select 2 files.", "Invalid Selection", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Dictionary<ulong, string> fileOne = AssetManager.ReadNameList(getNameLists[0]);
            Dictionary<ulong, string> fileTwo = AssetManager.ReadNameList(getNameLists[1]);

            List<ulong> fileOneOnly = new List<ulong>();
            foreach (ulong key in fileOne.Keys)
            {
                if (fileTwo.ContainsKey(key)) continue;
                fileOneOnly.Add(key);
            }


            List<ulong> fileTwoOnly = new List<ulong>();
            foreach (ulong key in fileTwo.Keys)
            {
                if (fileOne.ContainsKey(key)) continue;
                fileTwoOnly.Add(key);
            }

            string resultsDirectory = Path.GetDirectoryName(getNameLists[0]) + @"\NameListDiff.txt";
            StreamWriter writer = new StreamWriter(resultsDirectory, false);

            writer.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            writer.WriteLine("~~~~~~~~~~~~~ " + (fileOneOnly.Count > 0 ? "Only in " : "There are no names found only in ") + getNameLists[0]);
            writer.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            foreach (ulong key in fileOneOnly) writer.WriteLine(key + ":" + fileOne[key]);
            writer.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            writer.WriteLine("~~~~~~~~~~~~~ " + (fileTwoOnly.Count > 0 ? "Only in " : "There are no names found only in ") + getNameLists[1]);
            writer.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            foreach (ulong key in fileTwoOnly) writer.WriteLine(key + ":" + fileTwo[key]);
            writer.Close();

            MessageBox.Show("Diff results created at:\r" + resultsDirectory, "Diff Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion

    }
}
