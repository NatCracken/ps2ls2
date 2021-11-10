using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Runtime.InteropServices;
using System.IO;
using FMOD;

namespace ps2ls.IO
{
    class SoundExporterStatic
    {
        public class SoundFormatInfo
        {
            public string Name { get; internal set; }
            public string Extension { get; internal set; }
            public string ImageFormat { get; internal set; }

            internal SoundFormatInfo()
            {
            }

            public override string ToString()
            {
                return Name + @" (*." + Extension + @")";
            }
        }

        public static SoundFormatInfo[] SoundFormats;

        static SoundExporterStatic()
        {
            createSoundFormats();
        }

        private static void createSoundFormats()
        {
            List<SoundFormatInfo> soundFormats = new List<SoundFormatInfo>();

            //Waveform Audio (*.wav)
            SoundFormatInfo soundFormat = new SoundFormatInfo();
            soundFormat.Name = "Waveform Audio";
            soundFormat.Extension = "wav";
            soundFormat.ImageFormat = ".wav";
            soundFormats.Add(soundFormat);

            SoundFormats = soundFormats.ToArray();
        }

        public static bool exportSound(Sound sound, string name, string directory, SoundFormatInfo soundFormat)
        {
            short[] shorts = SoundToShortArray(sound, out int channels);
            byte[] buffer = new byte[shorts.Length * 2];
            Buffer.BlockCopy(shorts, 0, buffer, 0, buffer.Length);

            sound.getDefaults(out float frequency, out int priority);

            string path = directory + @"\" + Path.GetFileNameWithoutExtension(name) + @"." + soundFormat.Extension;

            if (File.Exists(path)) File.Delete(path);
            FileStream fs = File.Create(path);
            BinaryWriter bw = new BinaryWriter(fs);

            WriteWAVHeader(bw, buffer.Length, (int)frequency, 16u, (uint)channels);
            bw.Write(buffer, 0, buffer.Length);

            bw.Dispose();
            fs.Dispose();

            return true;
        }

        public static void WriteWAVHeader(BinaryWriter bw, int length, int audioSampleRate, uint bitsPerSample, uint audioChannels)
        {
            bw.Write(new char[4] { 'R', 'I', 'F', 'F' });
            int fileSize = 36 + length;
            bw.Write(fileSize);
            bw.Write(new char[8] { 'W', 'A', 'V', 'E', 'f', 'm', 't', ' ' });
            bw.Write((int)16);
            bw.Write((ushort)1);
            bw.Write((ushort)audioChannels);
            bw.Write(audioSampleRate);
            uint bytesPerSample = (bitsPerSample >> 3) * audioChannels;
            bw.Write((int)(audioSampleRate * bytesPerSample));
            bw.Write((ushort)bytesPerSample);
            bw.Write((ushort)bitsPerSample);
            bw.Write(new char[4] { 'd', 'a', 't', 'a' });
            bw.Write(length);
        }

        public static short[] SoundToShortArray(Sound sound, out int channels)
        {
            sound.getLength(out uint sampleCount, TIMEUNIT.PCM);
            return ReadSampleToShortArray(0, (int)sampleCount, sound, out channels);
        }

        static short[] ReadSampleToShortArray(int startSample, int sampleCount, Sound sound, out int channels)//for operations
        {
            sound.getFormat(out SOUND_TYPE type, out SOUND_FORMAT format, out channels, out int bitsPerSample);
            int bytesPerSample = (bitsPerSample >> 3) * channels;

            int offset = startSample * bytesPerSample;
            int length = bytesPerSample * sampleCount;

            sound.@lock((uint)offset, (uint)length, out IntPtr ptr1, out IntPtr ptr2, out uint Len1, out uint Len2);
            byte[] buffer = new byte[length];
            Marshal.Copy(ptr1, buffer, 0, length);

            short[] output = new short[sampleCount * channels];
            offset = 0;
            for (int currentSample = 0; currentSample < sampleCount; currentSample++)
            {
                for (int currentAudioChannel = 0; currentAudioChannel < channels; currentAudioChannel++)
                {
                    short amplitude = 0;
                    switch (format)
                    {
                        case SOUND_FORMAT.PCM8:
                            amplitude = buffer[offset + currentAudioChannel];
                            break;
                        case SOUND_FORMAT.PCM16:
                            amplitude = BitConverter.ToInt16(buffer, offset + (currentAudioChannel * 2));
                            break;
                        case SOUND_FORMAT.PCM32:
                            amplitude = Convert.ToInt16(
                                BitConverter.ToInt32(buffer, offset + (currentAudioChannel * 4)) / (float)int.MaxValue * short.MaxValue);
                            break;
                        case SOUND_FORMAT.PCMFLOAT:
                            float clampedFloat = BitConverter.ToSingle(buffer, offset + (currentAudioChannel * 4)) / 1.2f;//+-1.2 is a guess for the ampl range of float sounds
                            clampedFloat = Math.Min(clampedFloat, 1f);
                            clampedFloat = Math.Max(clampedFloat, -1f);
                            amplitude = Convert.ToInt16(clampedFloat * short.MaxValue);
                            break;
                        default:
                        case SOUND_FORMAT.NONE:
                        case SOUND_FORMAT.BITSTREAM:
                        case SOUND_FORMAT.MAX:
                            break;
                    }
                    output[(currentSample * channels) + currentAudioChannel] = amplitude;
                }
                offset += bytesPerSample;
            }

            sound.unlock(ptr1, ptr2, Len1, Len2);
            return output;
        }


        public static float[][] SoundToFloatArray(Sound sound)
        {
            sound.getLength(out uint sampleCount, TIMEUNIT.PCM);
            return ReadSampleToFloatArray(0, (int)sampleCount, sound);
        }

        static float[][] ReadSampleToFloatArray(int startSample, int sampleCount, Sound sound)//for visualization
        {
            RESULT res = sound.getFormat(out SOUND_TYPE type, out SOUND_FORMAT format, out int audioChannels, out int bitsPerSample);
            int bytesPerSample = (bitsPerSample >> 3) * audioChannels;

            int offset = startSample * bytesPerSample;
            int length = bytesPerSample * sampleCount;
            res = sound.@lock((uint)offset, (uint)length, out IntPtr ptr1, out IntPtr ptr2, out uint Len1, out uint Len2);
            byte[] buffer = new byte[length];
            Marshal.Copy(ptr1, buffer, 0, length);

            float[][] output = new float[audioChannels][];
            for (int i = 0; i < audioChannels; i++)
            {
                output[i] = new float[sampleCount];
            }
            offset = 0;
            for (int currentSample = 0; currentSample < sampleCount; currentSample++)
            {
                for (int currentAudioChannel = 0; currentAudioChannel < audioChannels; currentAudioChannel++)
                {
                    float amplitude = 0;
                    switch (format)
                    {
                        case SOUND_FORMAT.PCM8:
                            amplitude = buffer[offset + currentAudioChannel] / (float)sbyte.MaxValue;
                            break;
                        case SOUND_FORMAT.PCM16:
                            amplitude = BitConverter.ToInt16(buffer, offset + (currentAudioChannel * 2)) / (float)short.MaxValue;
                            break;
                        case SOUND_FORMAT.PCM32:
                            amplitude = BitConverter.ToInt32(buffer, offset + (currentAudioChannel * 4)) / (float)int.MaxValue;
                            break;
                        case SOUND_FORMAT.PCMFLOAT:
                            amplitude = BitConverter.ToSingle(buffer, offset + (currentAudioChannel * 4));
                            break;
                        default:
                        case SOUND_FORMAT.NONE:
                        case SOUND_FORMAT.BITSTREAM:
                        case SOUND_FORMAT.MAX:
                            break;
                    }
                    output[currentAudioChannel][currentSample] = amplitude;
                }
                offset += bytesPerSample;
            }

            sound.unlock(ptr1, ptr2, Len1, Len2);
            return output;
        }
    }
}
