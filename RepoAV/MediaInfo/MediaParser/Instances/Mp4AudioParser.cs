using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using MediaInfoWrapper;
using PSNC.Multimedia.Tools;
using xml = PSNC.Multimedia.Tools.XmlTools;
using System.Xml.Linq;
using System.Linq;
using MediaInfoWrapper;
using System.Xml.XPath;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PSNC.Multimedia.Instances
{
    class Mp4AudioParser : AbstractParser, IMediaParserInstance
    {
        MediaParser.FileFormat _fileformat = MediaParser.FileFormat.MP4;
        xml.VideoCoding.Values _video = xml.VideoCoding.Values.Unknown;
        xml.ColorDomain.Values _domain = xml.ColorDomain.Values.Unknown;
        bool hasVideo = false;
        bool hasAudio = false;
        bool hasSubtitles = false;
        ulong _filelength = 0;
        ulong _medialength = 0;
        int _xingmedialength = 0;
        int _xingframes = 0;
        ulong _duration = 0;
        uint _bitrate = 0;
        uint audioBitrate = 0;
        uint videoBitrate = 0;
        int _sample = 0;
        int _samplerate = 0;
        int _framerate = 0;
        String _coding = "Unknown";
        string _mime_video = "video/mp4";
        string _mime_audio = "audio/mp4";
        string _mime = "audio/mp4";
        string profile = "";
        string level = "";
        const string video = "video";
        const string audio = "audio";
        Dictionary<string, string> _metadata = new Dictionary<string, string>();
        AudioStreamProperties _audioStream = null;
        AudioStreamProperties[] audioStreams = null;
        VideoStreamProperties _videoStream = null;
        List<SubtitleStreamProperties> subtitleStreams = new List<SubtitleStreamProperties>();

        #region IMediaParserInstance Members

        MediaParser.FileFormat IMediaParserInstance.FileFormat
        {
            get { return _fileformat; }
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
            get
            {
                if (audioStreams != null)
                    return audioStreams;
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

        Dictionary<string, string> IMediaParserInstance.Metadata
        {
            get { return _metadata; }
        }

        IEnumerable<string> IMediaParserInstance.Warnings
        {
            get { return null; }
        }

        private static string DecimalSeparator
        {
            get { return CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator; }
        }

        bool IMediaParserInstance.Test(MediaStreamReader br)
        {
            if (br.BaseStream.Length > 8)
            {
                byte[] header = br.ReadBytes(4);
                byte[] bAtomName = br.ReadBytes(4);
                string atomName = Encoding.ASCII.GetString(bAtomName);
                if (atomName == Chunk.MP4)
                    return true;
            }
            return false;
        }

        bool IMediaParserInstance.Parse(MediaStreamReader br)
        {
            if (this.MediaInfo != null)
            {
                var media = MediaInfo;
                var format = media.Get<String>(Generalinfo.Format);
                if (!String.IsNullOrEmpty(format))
                {
                    if (format.ToLowerInvariant().Contains("3gp"))
                    {
                        this._fileformat = MediaParser.FileFormat._3GP;
                        this._mime = "video/3gpp";
                        this._mime_audio = "audio/3gpp";
                        this._mime_video = "video/3gpp";
                    }
                }

                this._duration = (ulong)media.Get<double>(Generalinfo.PlayTime);
                hasVideo = media.Get<int>(Generalinfo.VideoCount) > 0;
                var audioCount = media.Get<int>(Generalinfo.AudioCount);
                hasAudio = audioCount > 0;
                this._bitrate = media.Get<uint>(Generalinfo.BitRate);
                this._medialength = (ulong)br.BaseStream.Length;
                this._filelength = (ulong)br.BaseStream.Length;
                var subtitleCount = media.Get<int>(Generalinfo.TextCount);
                hasSubtitles = subtitleCount > 0;
                if (hasAudio)
                {
                    List<AudioStreamProperties> lasp = new List<AudioStreamProperties>();
                    for (int i = 0; i < audioCount; i++)
                    {
                        MPEG_LAYER mpeglayer = MPEG_LAYER.Unknown;
                        WaveFormatTag wft = WaveFormatTag.PCM;
                        var index = media.Get<int>(Audioinfo.ID, i);
                        var audiocodec = media.Get<String>(Audioinfo.Codec, i);
                        this.audioBitrate = (uint)media.Get<int>(Audioinfo.BitRate, i);
                        this._samplerate = media.Get<int>(Audioinfo.SamplingRate, i);
                        _audioStream = new AudioStreamProperties(index, (int)audioBitrate, (int)this._samplerate, (int)this._sample, wft, mpeglayer, false, audiocodec);
                        this._coding = audiocodec;
                        _audioStream.CodecName = this._coding;
                        lasp.Add(_audioStream);
                    }
                    audioStreams = lasp.ToArray();
                }
                if (hasSubtitles)
                {
                    for (int i = 0; i < subtitleCount; i++)
                    {
                        var _subtitleStream = new SubtitleStreamProperties();
                        _subtitleStream.Id = (i + 1).ToString();
                        _subtitleStream.Index = media.Get<int>(Textinfo.ID, i);
                        _subtitleStream.Description = media.Get<string>(Textinfo.Codec, i);
                        _subtitleStream.Language = media.Get<string>(Textinfo.Language, i);
                        subtitleStreams.Add(_subtitleStream);
                    }
                }

                if (hasVideo)
                {
                    _mime = _mime_video;
                    var height = media.Get<int>(Videoinfo.Height);
                    var width = media.Get<int>(Videoinfo.Width);
                    _videoStream = new VideoStreamProperties((uint)height, (uint)width);
                    _videoStream.Framerate = (int)media.Get<double>(Videoinfo.FrameRate);
                    this._framerate = _videoStream.Framerate;
                    _videoStream.Bitrate = media.Get<int>(Videoinfo.BitRate);
                    this.videoBitrate = (uint)_videoStream.Bitrate;
                    _videoStream.CodecName = media.Get<string>(Videoinfo.Codec);
                    var codec = _videoStream.CodecName;
                    _videoStream.CodecName = codec;
                }
                var profil = media.Get<string>(Videoinfo.Codec_Profile);

                char[] byApe = { '@' };
                var values = profil.Split(byApe);
                if (values.Length > 0)
                {
                    this.profile = values[0];
                    if (values.Length > 1)
                    {
                        this.level = values[1];
                        if (Char.IsLetter(this.level[0]))
                        {
                            this.level = level.Substring(1);
                        }
                    }
                }
                media.Close();
            }
            return false;
        }


        private static DictionaryEx<string, DictionaryEx<string, string>> GetTextInfo(IMediaInfo mi)
        {
            var inform = new DictionaryEx<String, DictionaryEx<String, String>>();
            try
            {
                string[] lines = Regex.Split(mi.Inform(), "\r\n");
                var marker = String.Empty;
                foreach (var line in lines)
                {
                    if (!inform.Keys.Contains(marker))
                    {
                        inform.Add(marker, new DictionaryEx<string, string>());
                    }
                    var split = line.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    if (split.Length > 1)
                    {
                        inform[marker].Add(split[0].Trim(), split[1].Trim());
                    }
                    else
                    {
                        marker = line;
                    }
                }
            }
            catch
            {

            }

            return inform;
        }

        private uint GetBitrateFromIsmFile(string filename, string bitrate, out uint videoBitrate, out uint audioBitrate)
        {
            uint result = 0; videoBitrate = 0; audioBitrate = 0;

            try
            {
                XmlDocument xmldoc = new XmlDocument();
                xmldoc.Load(filename);
                XDocument xdoc = XDocument.Parse(xmldoc.OuterXml);
                XElement xe = xdoc.Root;
                xe = xe.RemoveNamespacesByPrefix("xmlns");
                XPathDocument doc = new XPathDocument(new StringReader(xe.ToString()));
                XPathNavigator nav = doc.CreateNavigator();
                XPathExpression exprVideo, exprAudio;
                exprVideo = nav.Compile("/smil/body/switch/video");
                exprAudio = nav.Compile("/smil/body/switch/audio");
                XPathNodeIterator iteratorVideo = nav.Select(exprVideo);
                while (iteratorVideo.MoveNext())
                {
                    XPathNavigator current = iteratorVideo.Current.Clone();
                    var systemBitrate = current.GetAttribute("systemBitrate", "");
                    if (systemBitrate.StartsWith(bitrate))
                    {
                        videoBitrate = uint.Parse(systemBitrate);
                        result += videoBitrate;
                        break;
                    }
                }
                XPathNodeIterator iteratorAudio = nav.Select(exprAudio);
                while (iteratorAudio.MoveNext())
                {
                    XPathNavigator current = iteratorAudio.Current.Clone();
                    var systemBitrate = current.GetAttribute("systemBitrate", "");
                    audioBitrate = uint.Parse(systemBitrate);
                    result += audioBitrate;
                }
            }
            catch (Exception ex)
            {
            }

            return result;
        }

        public static string GetPart(string s, string separator, int part)
        {
            if (s.Contains(separator) && (part != 0))
            {
                char[] seps = new char[] { separator.ToCharArray()[0] };
                string[] parts = s.Split(seps, StringSplitOptions.None);

                if (part < 0)
                {
                    if (parts.Length >= Math.Abs(part))
                        return parts[parts.Length + part];
                }
                else if (part > 0)
                {
                    if (parts.Length >= part)
                        return parts[part - 1];
                }
                return String.Empty;
            }
            return String.Empty;
        }

        private string GetBitrateFromFilename(string p)
        {
            string result = String.Empty;
            result = Path.GetFileNameWithoutExtension(p);
            result = GetPart(result, "_", -1);
            return result;
        }

        private string GetIsmPath(string p)
        {
            string result = String.Empty;
            string path = Path.GetDirectoryName(p);
            result = Path.GetFileNameWithoutExtension(p);
            result = GetPart(result, "_", 1);
            if (result != String.Empty)
            {
                result += ".ism";
                result = Path.Combine(path, result);
            }
            return result;
        }

        string IMediaParserInstance.ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("FileFormat: {0}", this._fileformat));
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

            rootNode.AddElement(XmlTools.FileFormat, this._fileformat.ToProperString().ToLower());
            rootNode.AddElement(XmlTools.Bitrate, this._bitrate);
            rootNode.AddElement(XmlTools.FileSize, this._filelength);
            rootNode.AddElementConditionally(dict, XmlTools.Publication, now.ToXmlDate());

            #region przepisanie_właściwości_ze_słownika
            if (dict != null)
                foreach (KeyValuePair<string, object> entry in dict)
                {
                    rootNode.AddOrUpdateElement(entry.Key, entry.Value);
                }
            #endregion
            XmlNode secstream = null;
            if (hasAudio)
            {
                XmlNode codec = null;
                for (var i = 0; i < audioStreams.Length; i++)
                {
                    var _audioStream = audioStreams[i];
                    if (i == 0)
                    {
                        codec = xml.AddElement(rootNode, XmlTools.AudioCoding.Name);
                    }
                    else
                    {
                        if (secstream == null)
                            secstream = xml.AddElement(rootNode, "SecondaryStreams");
                        var stream = xml.AddElement(secstream, "Stream");
                        var streamid = xml.AddElement(stream, "StreamId", _audioStream.Index);
                        codec = xml.AddElement(stream, XmlTools.AudioCoding.Name);
                    }
                    xml.AddElement(codec, XmlTools.Bitrate, _audioStream.Bitrate);
                    xml.AddElement(codec, XmlTools.Coding, _coding);
                    xml.AddElement(codec, XmlTools.Sample.Name, _sample);
                    xml.AddElement(codec, XmlTools.SampleRate.Name, _samplerate);
                }
            }

            if (hasSubtitles)
            {
                foreach (var subtitle in subtitleStreams)
                {
                    if (secstream == null)
                        secstream = xml.AddElement(rootNode, "SecondaryStreams");
                    var stream = xml.AddElement(secstream, "Stream");
                    stream.AddElement("StreamId", subtitle.Index);
                    var set = stream.AddElement("SubtitlesSet");
                    set.AddElement("Id", subtitle.Id);
                    if (!String.IsNullOrWhiteSpace(subtitle.Description))
                        set.AddElement("Description", subtitle.Description);
                }
            }

            if (hasVideo)
            {

                XmlNode codec = xml.AddElement(rootNode, XmlTools.VideoCoding.Name);
                xml.AddElement(codec, XmlTools.ColorDomain.Name, _domain.EnumToString());
                XmlNode frame = xml.AddElement(codec, "Frame");
                xml.AddElement(frame, "Width", _videoStream.Width);
                xml.AddElement(frame, "Height", _videoStream.Height);
                xml.AddElement(codec, XmlTools.Bitrate, _videoStream.Bitrate);
                xml.AddElement(codec, XmlTools.FrameRate, _videoStream.Framerate);
                xml.AddElement(codec, XmlTools.Coding, _videoStream.CodecName);
                if (!String.IsNullOrEmpty(this.profile))
                {
                    var x = xml.AddElement(codec, "Profile", this.profile);
                }
                if (!String.IsNullOrEmpty(this.level))
                {
                    var x = xml.AddElement(codec, "Level", this.level);
                }
            }
            if (hasVideo) _mime = _mime_video;
            xml.AddElementConditionally(rootNode, dict, XmlTools.MimeType, this._mime);
            return xml.FormatXmlString(rootNode.OuterXml);
        }

        #endregion


    }

}

