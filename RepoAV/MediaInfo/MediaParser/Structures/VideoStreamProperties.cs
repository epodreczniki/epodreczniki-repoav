using System;
using PSNC.Multimedia.Tools;

namespace PSNC.Multimedia
{
    public class VideoStreamProperties
    {
        private uint _height;

        private uint _width;

        private uint _size;

        private string _codecName = String.Empty;
        XmlTools.VideoCoding.Values coding = XmlTools.VideoCoding.Values.Unknown;

        public XmlTools.VideoCoding.Values Coding
        {
            get { return coding; }
            set { coding = value; }
        }

        private int _index;
        private int _bitrate;
        private int _framerate;

        public VideoStreamProperties(uint height, uint width)
        {
            _height = height;
            _width = width;
        }


        public VideoStreamProperties(uint height, uint width, uint size)
        {
            _height = height;
            _width = width;
            _size = size;
        }


        public String CodecName
        {
            get { return _codecName; }
            set { _codecName = value; }
        }

        public int Bitrate
        {
            get { return _bitrate; }
            set { _bitrate = value; }
        }


        public int Index
        {
            get { return _index; }
            set { _index = value; }
        }

        public uint Height
        {
            get { return _height; }
            set { _height = value; }
        }

        public uint Width
        {
            get { return _width; }
            set { _width = value; }
        }

        public int Framerate
        {
            get { return _framerate; }
            set { _framerate = value; }
        }


        public override string ToString()
        {
            return "" + _height + "/" + _width;
        }

    }
}
