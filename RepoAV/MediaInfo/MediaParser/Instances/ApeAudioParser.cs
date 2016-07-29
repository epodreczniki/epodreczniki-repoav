using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MediaInfoWrapper;
using PSNC.Multimedia.Instances;
using xml = PSNC.Multimedia.Tools.XmlTools;
using System.Xml;
using PSNC.Multimedia.Tools;

namespace PSNC.Multimedia
{
    public class ApeAudioParser : AbstractParser, IMediaParserInstance
    {

        Dictionary<string, string> _metadata = new Dictionary<string, string>();
        ulong _filelength = 0;
        ulong _duration = 0;
        uint _bitrate = 0;
        int _framerate = 0;
        int _width = 0;
        int _height = 0;

        xml.AudioCoding.Values _coding = xml.AudioCoding.Values.APE;

        MediaParser.FileFormat fileFormat = MediaParser.FileFormat.APE;
        AudioStreamProperties _audiostream = null;
        VideoStreamProperties _videostream = null;

        string _mime = "audio/x-monkeys-audio";


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
                if (_videostream == null)
                    return null;
                else
                    return new VideoStreamProperties[] { _videostream };
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
            }
            return false;
        }

        public bool Test(PSNC.Multimedia.Tools.MediaStreamReader br)
        {
            bool result = false;
            using (var media = MediaInfoWrapper.MediaInfoFactory.GetInstance())
            {
                media.Open(br.FileName);
                var format = media.Get<String>(Generalinfo.Format);
                result = format.ToLowerInvariant().Contains("monkey");
            }
            return result;
        }

        public string ToShortString()
        {
            return this.GetType().ToString();
        }

        string IMediaParserInstance.ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("FileFormat: {0}", MediaParser.FileFormat.APE));
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

            XmlNode codec = xml.AddElement(rootNode, XmlTools.AudioCoding.Name);
            xml.AddElement(codec, XmlTools.Bitrate, _audiostream.Bitrate);
            _coding = XmlTools.AudioCoding.Values.APE;
            xml.AddElement(codec, XmlTools.Coding, _coding.EnumToString());
            xml.AddElement(codec, XmlTools.Sample.Name, XmlTools.Sample.FindClosestValue((int)_audiostream.Bitrate));
            xml.AddElement(codec, XmlTools.SampleRate.Name, XmlTools.SampleRate.FindClosestValue(0));

            xml.AddElementConditionally(rootNode, dict, XmlTools.MimeType, this._mime);
            return xml.FormatXmlString(rootNode.OuterXml);
        }

        #endregion

        #region inne
        private void CreateAudioStream()
        {
            string codec = "Monkey";
            _audiostream = new AudioStreamProperties(0, (int)this._bitrate, (int)0, (int)0, WaveFormatTag.UNKNOWN, MPEG_LAYER.Unknown, false, codec);
        }

        #endregion



    }
}

