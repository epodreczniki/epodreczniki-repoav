
namespace PSNC.Multimedia.Instances
{
    internal class Chunk
    {
        public const string Empty = "";
        public const string Riff = "RIFF";
        public const string Fmt = "fmt ";
        public const string Data = "data";
        public const string Pad = "PAD ";
        public const string WAVE = "WAVE";
        public const string Musi = "Musi";
        public const string File = "File";
        public const string ID3 = "ID3";
        public const string revID3 = "3DI";
        public const string Tag = "TAG";
        public const string extTag = "TAG+";
        public const string Xing = "Xing";
        public const string Info = "Info";
        public const string VBRI = "VBRI";
        public const string FLAC = "fLaC";
        public const string MP4 = "ftyp";
        public const string VTT = "WEBVTT";


        internal Chunk(string name, uint len)
        {
            Name = name;
            Legth = len;
        }

        internal string Name;
        internal uint Legth;

        public override string ToString()
        {
            return "Subchunk: " + Name + " " + Legth.ToString();
        }

    }
}
