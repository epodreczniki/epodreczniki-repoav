using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaInfoWrapper;
using PSNC.Multimedia.Instances;
using xml = PSNC.Multimedia.Tools.XmlTools;
using System.Xml;
using PSNC.Multimedia.Tools;

namespace PSNC.Multimedia
{
    class AviVideoParser : AbstractParser, IMediaParserInstance
    {

        Dictionary<string, string> _metadata = new Dictionary<string, string>();
        MediaParser.FileFormat _fileFormat = MediaParser.FileFormat.AVI;
        xml.AudioCoding.Values _coding = xml.AudioCoding.Values.Unknown;
        xml.VideoCoding.Values _video = xml.VideoCoding.Values.Unknown;
        xml.ColorDomain.Values _domain = xml.ColorDomain.Values.Unknown;
        ulong _filelength = 0;
        ulong _medialength = 0;
        ulong _duration = 0;
        uint _bitrate = 0;
        uint _audiobitrate = 0;
        int _sample = 0;
        int _samplerate = 0;
        string _mime = "video/avi";
        AudioStreamProperties _audioStream = null;
        VideoStreamProperties _videoStream = null;
        private int _framerate;
        private uint videoBitrate;

        #region IMediaParserInstance Members

        MediaParser.FileFormat IMediaParserInstance.FileFormat
        {
            get
            {
                return _fileFormat;
            }
        }

        ulong IMediaParserInstance.Duration
        {
            get { return _duration; }
        }

        ulong IMediaParserInstance.Filelength
        {
            get { return _filelength; }
        }

        public uint Bitrate
        {
            get { return _bitrate; }
        }

        String IMediaParserInstance.MimeType
        {
            get { return _mime; }
        }

        public AudioStreamProperties[] AudioStreams
        {
            get
            {
                if (_audioStream == null)
                    return null;
                else
                    return new AudioStreamProperties[] { _audioStream };
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

        public Dictionary<string, string> Metadata
        {
            get
            {
                return _metadata;
            }
        }

        IEnumerable<string> IMediaParserInstance.Warnings
        {
            get { return null; }
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

                        var kodek = audiocodec.ToLower();
                        if (kodek.Contains("ac3"))
                            this._coding = xml.AudioCoding.Values.AAC;
                        else if (audiocodec.ToLower().Contains("aac"))
                            this._coding = xml.AudioCoding.Values.AAC;
                        else if (kodek.Contains("wma"))
                            _coding = xml.AudioCoding.Values.WMA;
                        else if (kodek.Contains("mp4"))
                            _coding = XmlTools.AudioCoding.Values.MP4;

                        _audioStream = new AudioStreamProperties(i, (int)this._audiobitrate, (int)this._samplerate, (int)this._sample, wft, mpeglayer, false, kodek);
                    }
                }
                if (hasVideo)
                {
                    var height = media.Get<int>(Videoinfo.Height);
                    var width = media.Get<int>(Videoinfo.Width);
                    _videoStream = new VideoStreamProperties((uint)height, (uint)width);
                    _videoStream.Framerate = (int)media.Get<double>(Videoinfo.FrameRate);
                    this._framerate = _videoStream.Framerate;
                    _videoStream.Bitrate = media.Get<int>(Videoinfo.BitRate);
                    this.videoBitrate = (uint)_videoStream.Bitrate;
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
            }
            return true;
        }

        public bool Test(PSNC.Multimedia.Tools.MediaStreamReader br)
        {
            bool result = false;
            try
            {
            }
            catch
            {

            }

            return result;

        }

        string IMediaParserInstance.ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("FileFormat: {0}", this._fileFormat));
            sb.Append("\nRealna ilość danych:\t" + _medialength + " bajtów");
            sb.Append("\nCzas trwania:\t" + Tools.MediaParserTools.GetHumanReadableDuration((long)this._duration) + " ms.");

            return sb.ToString();

        }

        string IMediaParserInstance.ToShortString()
        {
            return this.GetType().ToString();
        }

        string IMediaParserInstance.ToXML(DictionaryEx<string, object> dict)
        {
            DateTime now = DateTime.Now;
            XmlDocument doc = new XmlDocument();
            XmlNode docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(docNode);

            XmlNode rootNode = doc.CreateElement(XmlTools.Format);
            doc.AppendChild(rootNode);

            xml.AddElement(rootNode, XmlTools.FileFormat, this._fileFormat.ToString().ToLower());
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
            xml.AddElement(codec, XmlTools.Bitrate, this._audiobitrate);
            xml.AddElement(codec, XmlTools.Coding, _coding.EnumToString());
            xml.AddElement(codec, XmlTools.Sample.Name, XmlTools.Sample.FindClosestValue(_sample));
            xml.AddElement(codec, XmlTools.SampleRate.Name, XmlTools.SampleRate.FindClosestValue(_samplerate));

            codec = xml.AddElement(rootNode, XmlTools.VideoCoding.Name);
            xml.AddElement(codec, XmlTools.ColorDomain.Name, _domain.EnumToString());
            XmlNode frame = xml.AddElement(codec, "Frame");
            xml.AddElement(frame, "Width", _videoStream.Width);
            xml.AddElement(frame, "Height", _videoStream.Height);
            xml.AddElement(codec, XmlTools.Bitrate, _videoStream.Bitrate);
            xml.AddElement(codec, XmlTools.FrameRate, _videoStream.Framerate);
            xml.AddElement(codec, XmlTools.Coding, _video.EnumToString());

            xml.AddElementConditionally(rootNode, dict, XmlTools.MimeType, this._mime);
            return xml.FormatXmlString(rootNode.OuterXml);
        }

        #endregion

    }
}

