
namespace PSNC.Multimedia.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;

    public class MediaStreamReader : BinaryReader
    {
        string fileName;

        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }

        public MediaStreamReader(string fileName)
            : base(new FileStream(fileName, FileMode.Open))
        {
            this.fileName = fileName;
        }

        public MediaStreamReader(Stream stream, string fileName)
            : base(stream)
        {
            this.fileName = fileName;
        }

        public byte[] GetBytes(int count)
        {
            if (BaseStream == null) return null;
            if (count <= 0) return null;
            byte[] result = null;
            long savedPosition = BaseStream.Position;
            result = ReadBytes(count);
            BaseStream.Position = savedPosition;
            return result;
        }

        public byte[] GetBytes(int position, int count)
        {
            if (BaseStream == null) return null;
            if (position < 0) return null;
            if (count <= 0) return null;
            byte[] result = null;
            long savedPosition = BaseStream.Position;
            BaseStream.Position = position;
            result = ReadBytes(count);
            BaseStream.Position = savedPosition;
            return result;
        }

        public byte[] GetChunk(int start, int stop)
        {
            if (BaseStream == null) return null;
            int count = stop - start;
            if (count <= 0) return null;
            byte[] result = null;
            long savedPosition = BaseStream.Position;
            BaseStream.Position = start;
            result = ReadBytes(count);
            BaseStream.Position = savedPosition;
            return result;
        }

        public uint SafeReadUInt32()
        {
            uint result = 0;
            try
            {
                result = this.ReadUInt32();
            }
            catch { }
            return result;
        }

        public char[] GetSafeChars(int count)
        {
            if (BaseStream == null) return null;
            if (count <= 0) return null;
            char[] result = null;
            try { result = this.ReadChars(count); }
            catch { }
            return result;
        }

        public String GetString(int position, int count)
        {
            System.Text.Encoding enc = System.Text.Encoding.Default;
            byte[] result = GetBytes(position, count);
            if (result == null) return null;
            return enc.GetString(result);
        }

        public String GetString(int count)
        {
            System.Text.Encoding enc = System.Text.Encoding.Default;
            byte[] result = GetBytes((int)BaseStream.Position, count);
            if (result == null) return null;
            return enc.GetString(result);
        }

        public String GetStringChunk(int start, int stop)
        {
            System.Text.Encoding enc = System.Text.Encoding.Default;
            byte[] result = GetChunk(start, stop);
            if (result == null) return null;
            return enc.GetString(result);
        }

        public string ReadLine()
        {
            int pos = 0;
            const int bufferSize = 1024;
            char[] _LineBuffer = new char[bufferSize];

            char[] buf = new char[2];

            StringBuilder stringBuffer = null;
            bool lineEndFound = false;
            try
            {
                while (base.Read(buf, 0, 2) > 0)
                {
                    if (buf[1] == '\r')
                    {

                        _LineBuffer[pos++] = buf[0];

                        char ch = base.ReadChar();
                        lineEndFound = true;
                    }
                    else if (buf[0] == '\r')
                    {
                        lineEndFound = true;
                    }
                    else
                    {
                        _LineBuffer[pos] = buf[0];
                        _LineBuffer[pos + 1] = buf[1];
                        pos += 2;

                        if (pos >= bufferSize)
                        {
                            stringBuffer = new StringBuilder(bufferSize + 80);
                            stringBuffer.Append(_LineBuffer, 0, bufferSize);
                            return stringBuffer.ToString();
                        }
                    }

                    if (lineEndFound)
                    {
                        if (stringBuffer == null)
                        {
                            if (pos > 0)
                                return new string(_LineBuffer, 0, pos);
                            else
                                return string.Empty;
                        }
                        else
                        {
                            if (pos > 0)
                                stringBuffer.Append(_LineBuffer, 0, pos);
                            return stringBuffer.ToString();
                        }
                    }
                }
            }
            catch { return String.Empty; }

            if (stringBuffer != null)
            {
                if (pos > 0)
                    stringBuffer.Append(_LineBuffer, 0, pos);
                return stringBuffer.ToString();
            }
            else
            {
                if (pos > 0)
                    return new string(_LineBuffer, 0, pos);
                else
                    return String.Empty;
            }
        }

        public KeyValuePair<string, string> GetPair(String key, int start, int stop)
        {
            String value = GetStringChunk(start, stop);
            value = value.Trim("\0".ToCharArray());
            KeyValuePair<string, string> result = new KeyValuePair<string, string>(key, value.Trim());
            return result;
        }

        public void Skip(int count)
        {
            Debug.Assert(count >= 0);

            this.BaseStream.Seek(count, SeekOrigin.Current);
        }

        public long Length
        {
            get { return BaseStream.Length; }
        }

        public long Seek(long pos, SeekOrigin o)
        {
            return this.BaseStream.Seek(pos, o);
        }

        public long Position
        {
            get { return BaseStream.Position; }
            set { BaseStream.Position = value; }
        }

    }

}

