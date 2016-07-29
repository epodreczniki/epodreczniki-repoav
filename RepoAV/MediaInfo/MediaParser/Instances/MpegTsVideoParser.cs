using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using xml = PSNC.Multimedia.Tools.XmlTools;
using System.Xml;
using PSNC.Multimedia.Tools;
using MediaInfoWrapper;


namespace PSNC.Multimedia.Instances
{
    class MpegTsVideoParser : AbstractParser, IMediaParserInstance
    {
        Dictionary<string, string> _metadata = new Dictionary<string, string>();
        MediaParser.FileFormat _fileFormat = MediaParser.FileFormat.M2TS;
        xml.AudioCoding.Values _coding = xml.AudioCoding.Values.Unknown;
        xml.VideoCoding.Values _video = xml.VideoCoding.Values.Unknown;
        xml.ColorDomain.Values _domain = xml.ColorDomain.Values.Unknown;
        ulong _filelength = 0;
        ulong _medialength = 0;
        ulong _duration = 0;
        uint _bitrate = 0;
        int _sample = 0;
        int _samplerate = 0;
        string _mime = "video/mpeg";
        AudioStreamProperties _audioStream = new AudioStreamProperties();
        VideoStreamProperties _videoStream = new VideoStreamProperties(0, 0);
        bool hasVideo = false;
        bool hasAudio = false;
        string profile = "";
        string level = "";
        IMediaInfo file = null;

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

        public bool Test(PSNC.Multimedia.Tools.MediaStreamReader br)
        {
            bool result = false;
            try
            {

                file = MediaInfoFactory.GetInstance();
                file.Open(br.FileName);
                var format = file.Get<String>(Generalinfo.Format);
                result = format == "MPEG-TS" || format == "BDAV";
                if (!result) file.Close();
            }
            catch (Exception ex)
            {
                return false;
            }

            return result;
        }

        public bool Parse(PSNC.Multimedia.Tools.MediaStreamReader br)
        {


            if (this.MediaInfo != null)
            {
                file = MediaInfo;
                this._mime = "video/m2ts";
                this._duration = (ulong)file.Get<double>(Generalinfo.PlayTime);
                hasVideo = file.Get<int>(Generalinfo.VideoCount) > 0;
                hasAudio = file.Get<int>(Generalinfo.AudioCount) > 0;
                this._bitrate = file.Get<uint>(Generalinfo.BitRate);
                this._medialength = (ulong)br.BaseStream.Length;
                this._filelength = (ulong)br.BaseStream.Length;
                if (hasAudio)
                {
                    MPEG_LAYER mpeglayer = MPEG_LAYER.Unknown;
                    WaveFormatTag wft = WaveFormatTag.PCM;
                    var audiocodec = file.Get<String>(Audioinfo.Codec);
                    var audiobitrate = file.Get<int>(Audioinfo.BitRate);
                    this._samplerate = file.Get<int>(Audioinfo.SamplingRate);
                    _audioStream = new AudioStreamProperties(0, (int)audiobitrate, (int)this._samplerate,
                    (int)this._sample, wft, mpeglayer, false, audiocodec);
                    if (audiocodec.ToLower().Contains("ac3"))
                    {
                        this._coding = xml.AudioCoding.Values.AAC;
                        _audioStream.Coding = this._coding;
                    }
                }
                if (hasVideo)
                {
                    var height = file.Get<int>(Videoinfo.Height);
                    var width = file.Get<int>(Videoinfo.Width);
                    _videoStream = new VideoStreamProperties((uint)height, (uint)width);
                    _videoStream.Framerate = (int)file.Get<double>(Videoinfo.FrameRate);
                    _videoStream.Bitrate = file.Get<int>(Videoinfo.BitRate);
                    _videoStream.CodecName = file.Get<string>(Videoinfo.Codec);
                    if (_videoStream.CodecName.ToLower().Contains("avc"))
                    {
                        this._video = xml.VideoCoding.Values.H264;
                        _videoStream.Coding = this._video;
                    }
                }
                else this._mime = "audio/m2ts";
            }
            else return false;
            return true;
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

            XmlNode codec = null;
            if (hasAudio)
            {
                codec = xml.AddElement(rootNode, XmlTools.AudioCoding.Name);
                xml.AddElement(codec, XmlTools.Bitrate, this._audioStream.Bitrate);
                xml.AddElement(codec, XmlTools.Coding, _coding.EnumToString());
                xml.AddElement(codec, XmlTools.Sample.Name, XmlTools.Sample.FindClosestValue(_sample));
                xml.AddElement(codec, XmlTools.SampleRate.Name, XmlTools.SampleRate.FindClosestValue(_samplerate));
            }

            if (hasVideo)
            {
                codec = xml.AddElement(rootNode, XmlTools.VideoCoding.Name);
                xml.AddElement(codec, XmlTools.ColorDomain.Name, _domain.EnumToString());
                XmlNode frame = xml.AddElement(codec, "Frame");
                xml.AddElement(frame, "Width", _videoStream.Width);
                xml.AddElement(frame, "Height", _videoStream.Height);
                xml.AddElement(codec, XmlTools.Bitrate, (_videoStream.Bitrate > 0) ? (uint)_videoStream.Bitrate : (uint)this._bitrate);
                xml.AddElement(codec, XmlTools.FrameRate, _videoStream.Framerate);
                xml.AddElement(codec, XmlTools.Coding, _video.EnumToString());
                if (!String.IsNullOrEmpty(this.profile))
                {
                    var x = xml.AddElement(codec, "Profile", this.profile);
                    var a = doc.CreateAttribute("removme");
                    a.Value = "true";
                    x.Attributes.Append(a);
                }
                if (!String.IsNullOrEmpty(this.level))
                {
                    var x = xml.AddElement(codec, "Level", this.level);
                    var a = doc.CreateAttribute("removme");
                    a.Value = "true";
                    x.Attributes.Append(a);
                }
            }

            xml.AddElementConditionally(rootNode, dict, XmlTools.MimeType, this._mime);
            return xml.FormatXmlString(rootNode.OuterXml);
        }

        #endregion


    }
}

