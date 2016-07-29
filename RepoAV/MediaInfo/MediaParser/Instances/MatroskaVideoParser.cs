using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using xml = PSNC.Multimedia.Tools.XmlTools;
using System.Text;
using System.Xml;
using PSNC.Multimedia.Tools;

using MediaInfoWrapper;


namespace PSNC.Multimedia.Instances
{
    public class MatroskaVideoParser : AbstractParser, IMediaParserInstance
    {
        Dictionary<string, string> _metadata = new Dictionary<string, string>();
        ulong _filelength = 0;
        ulong _duration = 0;
        uint _bitrate = 0;
        int _framerate = 0;
        int _width = 0;
        int _height = 0;

        xml.AudioCoding.Values _coding = xml.AudioCoding.Values.Unknown;
        xml.VideoCoding.Values _video = xml.VideoCoding.Values.Unknown;
        xml.ColorDomain.Values _domain = xml.ColorDomain.Values.Unknown;

        MediaParser.FileFormat fileFormat = MediaParser.FileFormat.MKV;
        AudioStreamProperties _audiostream = null;
        VideoStreamProperties _videostream = null;
        List<VideoStreamProperties> _videostreams = new List<VideoStreamProperties>();
        List<AudioStreamProperties> _audiostreams = new List<AudioStreamProperties>();

        string _mime = "video/x-matroska";
        bool hasVideo = false;
        bool hasAudio = false;

        IMediaInfo file = null;

        private ulong _medialength;
        private int _samplerate;
        private int _sample;

        public MatroskaVideoParser()
        {

        }

        #region IMediaParserInstance Members

        public MediaParser.FileFormat FileFormat
        {
            get { return fileFormat; }
        }

        public ulong Duration
        {
            get { return _duration; }
        }

        public ulong Filelength
        {
            get { return _filelength; }
        }

        public uint Bitrate
        {
            get { return _bitrate; }
        }

        public string MimeType
        {
            get { return _mime; }
        }

        public AudioStreamProperties[] AudioStreams
        {
            get
            {
                if (_audiostreams == null)
                    return null;
                else
                    return _audiostreams.ToArray();
            }
        }

        public VideoStreamProperties[] VideoStreams
        {
            get
            {
                if (_videostreams == null)
                    return null;
                else
                    return _videostreams.ToArray();
            }
        }

        public System.Collections.Generic.Dictionary<string, string> Metadata
        {
            get { return _metadata; }
        }

        IEnumerable<string> IMediaParserInstance.Warnings
        {
            get { return null; }
        }

        public int Framerate
        {
            get { return _framerate; }
            set { _framerate = value; }
        }

        public int Width
        {
            get { return _width; }
            set { _width = value; }
        }

        public int Height
        {
            get { return _height; }
            set { _height = value; }
        }

        public bool Test(PSNC.Multimedia.Tools.MediaStreamReader br)
        {
            var result = false;
            file = MediaInfoFactory.GetInstance();
            file.Open(br.FileName);
            var format = file.Get<String>(Generalinfo.Format).ToUpperInvariant();
            result = format == "MKV" || format == "WEBM" || format == "MATROSKA";
            if (!result) file.Close();
            return result;
        }

        public bool Parse(PSNC.Multimedia.Tools.MediaStreamReader br)
        {

            if (this.MediaInfo != null)
            {
                file = MediaInfo;
                this._duration = (ulong)file.Get<double>(Generalinfo.PlayTime);
                hasVideo = file.Get<int>(Generalinfo.VideoCount) > 0;
                hasAudio = file.Get<int>(Generalinfo.AudioCount) > 0;
                var format = file.Get<String>(Generalinfo.Format).ToUpperInvariant();
                if (format == "WEBM")
                {
                    this.fileFormat = MediaParser.FileFormat.WEBM;
                    if (hasVideo)
                        this._mime = "video/webm";
                    else
                        this._mime = "audio/webm";
                }

                this._bitrate = file.Get<uint>(Generalinfo.BitRate);
                this._medialength = (ulong)br.BaseStream.Length;
                this._filelength = (ulong)br.BaseStream.Length;
                if (hasAudio)
                {
                    this._coding = xml.AudioCoding.Values.Layer3;
                    MPEG_LAYER mpeglayer = MPEG_LAYER.Unknown;
                    WaveFormatTag wft = WaveFormatTag.PCM;
                    mpeglayer = MPEG_LAYER.LAYER3;
                    wft = WaveFormatTag.MPEGLAYER3;
                    var audiocodec = file.Get<String>(Audioinfo.Codec);
                    var audiobitrate = file.Get<int>(Audioinfo.BitRate);
                    this._samplerate = file.Get<int>(Audioinfo.SamplingRate);
                    _audiostream = new AudioStreamProperties(0, (int)audiobitrate, (int)this._samplerate, (int)0, wft, mpeglayer, false, audiocodec);
                    if (audiocodec.ToLower().Contains("aac"))
                    {
                        this._coding = xml.AudioCoding.Values.AAC;
                        _audiostream.Coding = this._coding;
                    }
                    if (audiocodec.ToLower().Contains("ogg"))
                    {
                        this._coding = xml.AudioCoding.Values.OGG;
                        _audiostream.Coding = this._coding;
                    }
                    if (audiocodec.ToLower().Contains("vorbis"))
                    {
                        this._coding = xml.AudioCoding.Values.OGG;
                        _audiostream.Coding = this._coding;
                    }
                    this._audiostreams.Add(_audiostream);
                }
                if (hasVideo)
                {
                    var height = file.Get<int>(Videoinfo.Height);
                    var width = file.Get<int>(Videoinfo.Width);
                    _videostream = new VideoStreamProperties((uint)height, (uint)width);
                    _videostream.Framerate = (int)file.Get<double>(Videoinfo.FrameRate);
                    _videostream.Bitrate = file.Get<int>(Videoinfo.BitRate);
                    _videostream.CodecName = file.Get<string>(Videoinfo.Codec);
                    if (_videostream.CodecName.ToLower().Contains("avc"))
                    {
                        this._video = xml.VideoCoding.Values.H264;
                        _videostream.Coding = this._video;
                    }
                    if (_videostream.CodecName.ToLower().Contains("vp8"))
                    {
                        this._video = xml.VideoCoding.Values.VP8;
                        _videostream.Coding = this._video;
                    }
                    if (_videostream.CodecName.ToLower().Contains("vp6"))
                    {
                        this._video = xml.VideoCoding.Values.VP6;
                        _videostream.Coding = this._video;
                    }
                    if (_videostream.CodecName.ToLower().Contains("vp7"))
                    {
                        this._video = xml.VideoCoding.Values.VP7;
                        _videostream.Coding = this._video;
                    }
                    this._videostreams.Add(_videostream);
                }
            }

            return true;
        }

        public string ToShortString()
        {
            return this.GetType().ToString();
        }

        string IMediaParserInstance.ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("FileFormat: {0}", this.FileFormat));
            sb.Append("\nWielkość pliku:\t" + MediaParserTools.GetHumanReadableLength(_filelength));
            sb.Append("\nCzas trwania:\t" + Tools.MediaParserTools.GetHumanReadableDuration((long)this._duration) + " ms.");

            return sb.ToString();
        }

        public string ToXML(PSNC.Multimedia.Tools.DictionaryEx<string, object> dict)
        {
            DateTime now = DateTime.Now;
            XmlDocument doc = new XmlDocument();
            XmlNode docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(docNode);

            XmlNode rootNode = doc.CreateElement(XmlTools.Format);
            doc.AppendChild(rootNode);

            xml.AddElement(rootNode, XmlTools.FileFormat, fileFormat.ToString().ToLower());
            xml.AddElement(rootNode, XmlTools.Bitrate, this._bitrate);
            xml.AddElement(rootNode, XmlTools.FileSize, this._filelength);
            xml.AddElementConditionally(rootNode, dict, XmlTools.Publication, now.ToXmlDate());

            #region przepisanie_właściwości_ze_słownika
            if (dict != null)
                foreach (KeyValuePair<string, object> entry in dict)
                {
                    rootNode.AddOrUpdateElement(entry.Key, entry.Value);
                }
            #endregion
            foreach (AudioStreamProperties asp in AudioStreams)
            {
                XmlNode codec = xml.AddElement(rootNode, XmlTools.AudioCoding.Name);
                xml.AddElement(codec, XmlTools.Bitrate, asp.Bitrate);
                xml.AddElement(codec, XmlTools.Coding, _coding.EnumToString());
                xml.AddElement(codec, XmlTools.Sample.Name, XmlTools.Sample.FindClosestValue(asp.BitsPerSample));
                xml.AddElement(codec, XmlTools.SampleRate.Name, XmlTools.SampleRate.FindClosestValue(asp.Bitrate));
            }

            foreach (VideoStreamProperties vsp in VideoStreams)
            {
                XmlNode codec = xml.AddElement(rootNode, XmlTools.VideoCoding.Name);
                xml.AddElement(codec, XmlTools.ColorDomain.Name, _domain.EnumToString());
                XmlNode frame = xml.AddElement(codec, "Frame");
                xml.AddElement(frame, "Width", vsp.Width);
                xml.AddElement(frame, "Height", vsp.Height);
                xml.AddElement(codec, XmlTools.Bitrate, vsp.Bitrate);
                xml.AddElement(codec, XmlTools.FrameRate, vsp.Framerate);
                xml.AddElement(codec, XmlTools.Coding, _video.EnumToString());
            }
            xml.AddElementConditionally(rootNode, dict, XmlTools.MimeType, this._mime);
            return xml.FormatXmlString(rootNode.OuterXml);

        }

        #endregion

        #region inne

        #endregion
    }

}

