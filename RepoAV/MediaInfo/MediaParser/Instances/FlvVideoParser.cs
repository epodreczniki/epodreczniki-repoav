using System;
using System.IO;
using System.Collections.Generic;
using MediaInfoWrapper;
using xml = PSNC.Multimedia.Tools.XmlTools;
using System.Text;
using System.Xml;
using PSNC.Multimedia.Tools;
namespace PSNC.Multimedia.Instances
{
    public class FlvVideoParser : AbstractParser, IMediaParserInstance
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
        string _mime = "video/x-flv";
        MediaParser.FileFormat fileFormat = MediaParser.FileFormat.FLV;
        AudioStreamProperties _audiostream = null;
        VideoStreamProperties _videoStream = null;


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
                if (_audiostream == null)
                    return null;
                else
                    return new AudioStreamProperties[] { _audiostream };
            }
        }

        public VideoStreamProperties[] VideoStreams
        {
            get
            {
                if (_videoStream == null)
                    return null;
                else
                    return new VideoStreamProperties[] { _videoStream };
            }
        }

        public System.Collections.Generic.Dictionary<string, string> Metadata
        {
            get { return null; }
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

        public bool Parse(PSNC.Multimedia.Tools.MediaStreamReader br)
        {
            if (this.MediaInfo != null)
            {
                var media = MediaInfo;
                this._bitrate = media.Get<uint>(Generalinfo.BitRate);
                var format = media.Get<String>(Generalinfo.Format);
                this._duration = (ulong)media.Get<double>(Generalinfo.PlayTime);
                var hasVideo = media.Get<int>(Generalinfo.VideoCount) > 0;
                var audioCount = media.Get<int>(Generalinfo.AudioCount);
                var hasAudio = audioCount > 0;
                this._bitrate = media.Get<uint>(Generalinfo.BitRate);
                this._filelength = (ulong)br.BaseStream.Length;
                var subtitleCount = media.Get<int>(Generalinfo.TextCount);
                var hasSubtitles = subtitleCount > 0;
                if (hasAudio)
                {
                    for (int i = 0; i < audioCount; i++)
                    {
                        MPEG_LAYER mpeglayer = MPEG_LAYER.Unknown;
                        WaveFormatTag wft = WaveFormatTag.PCM;
                        var index = media.Get<int>(Audioinfo.ID, i);
                        var audiocodec = media.Get<String>(Audioinfo.Codec, i);
                        var audioBitrate = (uint)media.Get<int>(Audioinfo.BitRate, i);
                        var _sample = media.Get<int>(Audioinfo.SamplingCount, i);
                        var _samplerate = media.Get<int>(Audioinfo.SamplingRate, i);

                        _audiostream = new AudioStreamProperties(index, (int)audioBitrate, (int)_samplerate,
                            (int)_sample, wft, mpeglayer, false, audiocodec);
                        var kodek = audiocodec.ToLower();
                        if (kodek.Contains("ac3"))
                            this._coding = xml.AudioCoding.Values.AAC;
                        else if (audiocodec.ToLower().Contains("aac"))
                            this._coding = xml.AudioCoding.Values.AAC;
                        else if (kodek.Contains("wma"))
                            _coding = xml.AudioCoding.Values.WMA;
                        else if (kodek.Contains("mp4"))
                            _coding = XmlTools.AudioCoding.Values.MP4;
                        _audiostream.Coding = this._coding;
                    }
                }
                if (hasVideo)
                {
                    _height = media.Get<int>(Videoinfo.Height);
                    _width = media.Get<int>(Videoinfo.Width);
                    _videoStream = new VideoStreamProperties((uint)_height, (uint)_width);
                    _videoStream.Framerate = (int)media.Get<double>(Videoinfo.FrameRate);
                    _framerate = _videoStream.Framerate;
                    _videoStream.Bitrate = media.Get<int>(Videoinfo.BitRate);
                    var videoBitrate = (uint)_videoStream.Bitrate;
                    _videoStream.CodecName = media.Get<string>(Videoinfo.Codec);
                    var codec = _videoStream.CodecName.ToLower();
                    if (codec.Contains("avc"))
                        _video = xml.VideoCoding.Values.H264;
                    else if (codec.Contains("vc1"))
                        _video = xml.VideoCoding.Values.VC1;
                    else if (codec.Contains("264"))
                        _video = xml.VideoCoding.Values.H264;
                    else if (codec.Contains("263"))
                        _video = xml.VideoCoding.Values.H263;
                    _videoStream.Coding = this._video;
                }
                media.Close();

            }
            return false;
        }

        public bool Test(PSNC.Multimedia.Tools.MediaStreamReader br)
        {
            using (var media = MediaInfoWrapper.MediaInfoFactory.GetInstance())
            {
                media.Open(br.FileName);
                var format = media.Get<String>(Generalinfo.Format);
                return format.ToLowerInvariant().Contains("flash");
            }
            return false;
        }

        public string ToShortString()
        {
            return this.GetType().ToString();
        }

        string IMediaParserInstance.ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("FileFormat: {0}", MediaParser.FileFormat.FLV));
            sb.Append("\nWielkość pliku:\t" + _filelength + " bajtów");
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

            XmlNode codec = xml.AddElement(rootNode, XmlTools.AudioCoding.Name);
            xml.AddElement(codec, XmlTools.Bitrate, _audiostream.Bitrate);
            _coding = XmlTools.AudioCoding.Values.Unknown;
            xml.AddElement(codec, XmlTools.Coding, _coding.EnumToString());
            xml.AddElement(codec, XmlTools.Sample.Name, XmlTools.Sample.FindClosestValue((int)this.Bitrate));
            xml.AddElement(codec, XmlTools.SampleRate.Name, XmlTools.SampleRate.FindClosestValue(0));

            codec = xml.AddElement(rootNode, XmlTools.VideoCoding.Name);
            xml.AddElement(codec, XmlTools.ColorDomain.Name, _domain.EnumToString());
            XmlNode frame = xml.AddElement(codec, "Frame");
            xml.AddElement(frame, "Width", this.Width);
            xml.AddElement(frame, "Height", this.Height);

            _video = XmlTools.VideoCoding.Values.Unknown;

            xml.AddElement(codec, XmlTools.Bitrate, _videoStream.Bitrate);
            xml.AddElement(codec, XmlTools.FrameRate, this.Framerate);
            xml.AddElement(codec, XmlTools.Coding, _video.EnumToString());

            xml.AddElementConditionally(rootNode, dict, XmlTools.MimeType, this._mime);
            return xml.FormatXmlString(rootNode.OuterXml);
        }


    }

        #endregion
}

