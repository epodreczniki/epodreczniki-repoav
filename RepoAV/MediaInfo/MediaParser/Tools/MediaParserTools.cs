using System;
using System.Collections;

namespace PSNC.Multimedia.Tools
{
    public static class MediaParserTools
    {
        public const int NOT_FOUND = -1;

        public const int KILOBIT = 1000;

        public const int KILOBYTE = 1024;

        public static String GetHumanReadableDuration(long duration)
        {
            DateTime duration_human_readable = new DateTime((long)duration * TimeSpan.TicksPerMillisecond);
            return duration_human_readable.ToString("HH:mm:ss.ffff");
        }

        public static string GetHumanReadableLength(ulong bytes)
        {
            string[] Suffix = { "B", "KB", "MB", "GB", "TB" };
            int i;
            double dblSByte = 0;
            for (i = 0; (int)(bytes / KILOBYTE) > 1000.0; i++, bytes /= KILOBYTE)
                dblSByte = bytes / KILOBYTE;
            return String.Format("{0:0} {1}", dblSByte, Suffix[i]);
        }

        public static string Strip(String s)
        {
            String result = "";
            if (!String.IsNullOrEmpty(s))
            {
                String value = s.Trim("\0".ToCharArray()).Trim();
                foreach (char c in value)
                {
                    if (Char.IsLetterOrDigit(c))
                        result += c;
                }
            }
            return result;
        }

        public static bool IsCorrectFrameHeader(String s)
        {
            foreach (char c in s)
            {
                if (!Char.IsLetterOrDigit(c))
                    return false;
            }

            return true;
        }

        public static bool In(String s, string[] strings)
        {
            foreach (String instr in strings)
            {
                if (instr == s)
                    return true;
            }

            return false;
        }

        public static bool In(Char c, char[] chars)
        {
            foreach (Char chr in chars)
            {
                if (chr == c)
                    return true;
            }

            return false;
        }

        public static bool Contains(string[] strings, String s)
        {
            foreach (String instr in strings)
            {
                if (instr == s)
                    return true;
            }

            return false;
        }

        public static bool Contains(char[] chars, Char c)
        {
            foreach (Char chr in chars)
            {
                if (chr == c)
                    return true;
            }

            return false;
        }

        public static int WhichInArray(string[] strings, String s)
        {
            for (int i = 0; i < strings.Length; i++)
            {
                if (strings[i] == s)
                    return i;
            }

            return NOT_FOUND;
        }

        public static String GetString(byte[] data)
        {
            if (data == null) return null;
            return System.Text.Encoding.Default.GetString(data);
        }

        public static BitArray ToBitArray(byte[] data)
        {
            if (data == null) return null;
            return new BitArray(data);
        }

        public static BitArray ToBitArray(byte b)
        {
            return new BitArray(new byte[] { b });
        }

        public static bool IsEqual(byte[] first, byte[] second)
        {
            if (first.Length != second.Length)
                return false;
            int i = 0;
            while (i < first.Length)
            {
                if (first[i] != second[i])
                    return false;
                i++;
            }

            return true;
        }

        public static bool IsEqual(bool[] first, bool[] second)
        {
            if (first.Length != second.Length)
                return false;
            int i = 0;
            while (i < first.Length)
            {
                if (first[i] != second[i])
                    return false;
                i++;
            }

            return true;
        }

        public static String ToBiteString(BitArray ba)
        {
            String Result = "";
            foreach (bool b in ba)
            {
                Result += b ? "1" : "0";
            }
            return Result;
        }

        public static BitArray Reverse(BitArray ba)
        {
            bool[] tmpArray = new bool[ba.Count];
            ba.CopyTo(tmpArray, 0);
            Array.Reverse(tmpArray);
            BitArray result = new BitArray(tmpArray);
            return result;
        }

        public static void Display(BitArray myBitArray, string arrayListName)
        {
            for (int i = 0; i < myBitArray.Count; i++)
            {
                Console.WriteLine(arrayListName + "[" + i + "]  =  " + myBitArray[i]);
            }
        }

        public static byte[] Range(byte[] source, int startIndex, int length)
        {
            if (startIndex < 0 || startIndex > source.Length)
                throw new ArgumentOutOfRangeException(
                "startIndex");

            if (length < 0 || startIndex + length > source.Length)
                throw new ArgumentOutOfRangeException(
                "length");

            if (length == 0)
                return new byte[0];

            if (startIndex + length > source.Length)
                length = source.Length - startIndex;

            byte[] data = new byte[length];

            source.CopyTo(data, startIndex);

            return data;
        }

        public static byte ToByte(BitArray ba)
        {
            byte value = 0x00;

            for (byte x = 0; x < ba.Count; x++)
            {
                value |= (byte)((ba[x] == true) ? (0x01 << x) : 0x00);
            }
            return value;
        }

        public static int DecodeSyncsafeInt(byte[] data)
        {
            int Result = 0;
            Array.Reverse(data);
            int dwNumByteShifts = 0;

            for (int n = 0; n < data.Length; n++)
            {
                Result |= data[n] << 8 * dwNumByteShifts++;
            }
            return Result;

        }

        public static uint UInt32FromBigEndian(byte[] bytes)
        {
            return (uint)((bytes[0] & 0xff) << 24 | (bytes[1] & 0xff) << 16 | (bytes[2] & 0xff) << 8 | (bytes[3] & 0xff) << 0);
        }

        public static int ToInt(byte[] buffer)
        {
            return (int)(buffer[0] << 21) + (buffer[1] << 14) + (buffer[2] << 7) + (int)(buffer[3]);
        }

        public static string OnlyNumbers(string s)
        {
            string result = String.Empty;
            if (!String.IsNullOrEmpty(s))
                foreach (char c in s.ToCharArray())
                    if (Char.IsDigit(c))
                        result += c;

            return result;
        }

        public static string OnlyAlpha(string s)
        {
            string result = String.Empty;
            if (!String.IsNullOrEmpty(s))
                foreach (char c in s.ToCharArray())
                    if (Char.IsLetterOrDigit(c))
                        result += c;

            return result;
        }

        public static string TrimAll(string s)
        {
            string result = s;
            if (!String.IsNullOrEmpty(s))
            {
                result = result.Replace(" ", String.Empty);
                result = result.Replace("\t", String.Empty);
                result = result.Replace("\n", String.Empty);
            }
            return result;
        }

        public static ulong GetAverageBitrate(IMediaParserInstance instance)
        {
            ulong result = 0;
            if (instance.Duration > 0)
            {
                try
                {
                    ulong size = (instance.Filelength * 8);
                    ulong dur = (instance.Duration / 1000);
                    result = size / dur;
                }
                catch { }
            }
            return result;

        }

    }
}
