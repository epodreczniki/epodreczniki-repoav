using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using MediaInfoWrapper;
using PSNC.Multimedia.Tools;
using xml = PSNC.Multimedia.Tools.XmlTools;
using System.Xml.Linq;

namespace PSNC.Multimedia.Instances
{
    class AsfParser : AbstractParser, IMediaParserInstance
    {
        public readonly Guid ASF_Header_Object = new Guid("75B22630-668E-11CF-A6D9-00AA0062CE6C");

        private Dictionary<int, AudioStreamProperties> audiostreams = new Dictionary<int, AudioStreamProperties>();
        private Dictionary<int, VideoStreamProperties> videostreams = new Dictionary<int, VideoStreamProperties>();

        Dictionary<string, string> _metadata = new Dictionary<string, string>();
        ulong _filelength = 0;
        ulong _duration = 0;
        uint _bitrate = 0;
        xml.AudioCoding.Values _coding = xml.AudioCoding.Values.Unknown;
        xml.VideoCoding.Values _video = xml.VideoCoding.Values.Unknown;
        xml.ColorDomain.Values _domain = xml.ColorDomain.Values.Unknown;
        string _mime = "video/x-ms-asf";
        MediaParser.FileFormat fileFormat = MediaParser.FileFormat.WMA;

        #region IMediaParserInstance Members

        MediaParser.FileFormat IMediaParserInstance.FileFormat
        {
            get { return fileFormat; }
        }

        ulong IMediaParserInstance.Duration
        {
            get { return _duration; }
        }

        ulong IMediaParserInstance.Filelength
        {
            get { return _filelength; }
        }

        uint IMediaParserInstance.Bitrate
        {
            get { return _bitrate; }
        }

        String IMediaParserInstance.MimeType
        {
            get { return _mime; }
        }

        AudioStreamProperties[] IMediaParserInstance.AudioStreams
        {
            get { return audiostreams.Values.ToArray(); }
        }

        VideoStreamProperties[] IMediaParserInstance.VideoStreams
        {
            get { return videostreams.Values.ToArray(); }
        }

        Dictionary<string, string> IMediaParserInstance.Metadata
        {
            get { return _metadata; }
        }

        IEnumerable<string> IMediaParserInstance.Warnings
        {
            get { return null; }
        }

        bool IMediaParserInstance.Parse(MediaStreamReader br)
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

                        var _audioStream = new AudioStreamProperties(index, (int)audioBitrate, (int)_samplerate, (int)_sample, wft, mpeglayer, false, audiocodec);
                        var kodek = audiocodec.ToLower();
                        if (kodek.Contains("ac3"))
                            this._coding = xml.AudioCoding.Values.AAC;
                        else if (audiocodec.ToLower().Contains("aac"))
                            this._coding = xml.AudioCoding.Values.AAC;
                        else if (kodek.Contains("wma"))
                            _coding = xml.AudioCoding.Values.WMA;
                        else if (kodek.Contains("mp4"))
                            _coding = XmlTools.AudioCoding.Values.MP4;
                        _audioStream.Coding = this._coding;
                        this.audiostreams.Add(i, _audioStream);
                    }
                }
                if (hasVideo)
                {
                    var height = media.Get<int>(Videoinfo.Height);
                    var width = media.Get<int>(Videoinfo.Width);
                    var _videoStream = new VideoStreamProperties((uint)height, (uint)width);
                    _videoStream.Framerate = (int)media.Get<double>(Videoinfo.FrameRate);
                    var _framerate = _videoStream.Framerate;
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
                    this.videostreams.Add(0, _videoStream);
                }
                media.Close();

            }
            return true;
        }

        bool IMediaParserInstance.Test(MediaStreamReader br)
        {
            if (br.BaseStream.Length < 24)
            {
                return false;
            }

            byte[] buf = new byte[16];

            br.Read(buf, 0, 16);

            Guid guid = new Guid(buf);

            return guid == ASF_Header_Object;
        }

        string IMediaParserInstance.ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("FileFormat: {0}", MediaParser.FileFormat.ASF));
            sb.Append("\nWielkość pliku:\t" + _filelength + " bajtów");
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

            AudioStreamProperties asp = null;
            VideoStreamProperties vsp = null;

            foreach (AudioStreamProperties a in audiostreams.Values)
            {
                if (asp == null || asp.Bitrate < a.Bitrate)
                    asp = a;
            }

            foreach (VideoStreamProperties v in videostreams.Values)
            {
                if (vsp == null || vsp.Bitrate < v.Bitrate)
                    vsp = v;
            }

            if (asp != null)
            {
                XmlNode codec = xml.AddElement(rootNode, XmlTools.AudioCoding.Name);
                xml.AddElement(codec, XmlTools.Bitrate, asp.Bitrate);

                _coding = RecognizeAudioCoding(asp);

                xml.AddElement(codec, XmlTools.Coding, _coding.EnumToString());
                xml.AddElement(codec, XmlTools.Sample.Name, XmlTools.Sample.FindClosestValue(asp.BitsPerSample));
                xml.AddElement(codec, XmlTools.SampleRate.Name, XmlTools.SampleRate.FindClosestValue(asp.SamplesPerSec));
            }

            if (vsp != null)
            {
                XmlNode codec = xml.AddElement(rootNode, XmlTools.VideoCoding.Name);
                xml.AddElement(codec, XmlTools.ColorDomain.Name, _domain.EnumToString());
                XmlNode frame = xml.AddElement(codec, "Frame");
                xml.AddElement(frame, "Width", vsp.Width);
                xml.AddElement(frame, "Height", vsp.Height);

                _video = RecognizeVideoCoding(vsp);

                xml.AddElement(codec, XmlTools.Bitrate, vsp.Bitrate);
                xml.AddElement(codec, XmlTools.FrameRate, vsp.Framerate);
                xml.AddElement(codec, XmlTools.Coding, _video.EnumToString());
            }
            xml.AddElementConditionally(rootNode, dict, XmlTools.MimeType, this._mime);
            return xml.FormatXmlString(rootNode.OuterXml);
        }

        private XmlTools.VideoCoding.Values RecognizeVideoCoding(VideoStreamProperties vsp)
        {
            XmlTools.VideoCoding.Values result = XmlTools.VideoCoding.Values.Unknown;
            string nazwa = MediaParserTools.OnlyAlpha(vsp.CodecName);
            string wersja = MediaParserTools.OnlyNumbers(vsp.CodecName);

            if (nazwa.ToLower().StartsWith("windowsmediavideo"))
            {
                if (wersja.StartsWith("10"))
                    result = XmlTools.VideoCoding.Values.WMV10;
                else
                    if (wersja.StartsWith("9"))
                        result = XmlTools.VideoCoding.Values.WMV9;
                    else
                        result = XmlTools.VideoCoding.Values.WMV;
            }
            else if (nazwa.ToLower().Contains("vc1"))
                result = XmlTools.VideoCoding.Values.VC1;
            return result;
        }

        private XmlTools.AudioCoding.Values RecognizeAudioCoding(AudioStreamProperties asp)
        {
            XmlTools.AudioCoding.Values result = XmlTools.AudioCoding.Values.Unknown;
            string nazwa = MediaParserTools.OnlyAlpha(asp.Codec);
            string wersja = MediaParserTools.OnlyNumbers(asp.Codec);
            if (nazwa.ToLower().StartsWith("windowsmediaaudio"))
            {
                if (wersja.StartsWith("10"))
                    result = XmlTools.AudioCoding.Values.WMA10;
                else
                    if (wersja.StartsWith("9"))
                        result = XmlTools.AudioCoding.Values.WMA9;
                    else
                        result = XmlTools.AudioCoding.Values.WMA;
            } if (nazwa.ToLower().Contains("aac"))
                result = XmlTools.AudioCoding.Values.MP4;
            return result;
        }

        #endregion
    }
}

