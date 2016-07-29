using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using MediaInfoWrapper;
using PSNC.Multimedia.Tools;
using xml = PSNC.Multimedia.Tools.XmlTools;
using System.Xml.Linq;

namespace PSNC.Multimedia.Instances
{
    class MpegAudioParser : AbstractParser, IMediaParserInstance
    {
        Chunk _mainchunk;
        List<Chunk> _subchunks = new List<Chunk>();
        Dictionary<string, string> _metadata = new Dictionary<string, string>();
        MediaParser.FileFormat _fileFormat = MediaParser.FileFormat.UNKNOWN;
        xml.AudioCoding.Values _coding = xml.AudioCoding.Values.Unknown;
        ulong _filelength = 0;
        ulong _duration = 0;
        uint _bitrate = 0;
        int _sample = 0;
        int _samplerate = 0;
        string _mime = "audio/mpeg";
        AudioStreamProperties _audioStream = null;

        internal MpegAudioParser()
        {
        }

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
                return null;
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

        bool IMediaParserInstance.Parse(MediaStreamReader br)
        {
            if (this.MediaInfo != null)
            {
                var media = MediaInfo;
                this._bitrate = media.Get<uint>(Generalinfo.BitRate);
                var format = media.Get<String>(Generalinfo.Format);
                _fileFormat = MediaParser.FileFormat.WAV;
                _mime = "audio/wav";
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
                        if (audiocodec == "MPA1L2")
                            _fileFormat = MediaParser.FileFormat.MP2;
                        var audioBitrate = (uint)media.Get<int>(Audioinfo.BitRate, i);
                        var _sample = media.Get<int>(Audioinfo.SamplingCount, i);
                        var _samplerate = media.Get<int>(Audioinfo.SamplingRate, i);

                        _audioStream = new AudioStreamProperties(index, (int)audioBitrate, (int)_samplerate,
                            (int)_sample, wft, mpeglayer, false, audiocodec);
                        var kodek = audiocodec.ToLower();
                        _audioStream.Coding = this._coding;
                    }
                }
            }
            return true;

        }

        private void CreateAudioStream()
        {
            var codec = "";
            _coding = xml.AudioCoding.Values.Unknown;
            _audioStream = new AudioStreamProperties(0, (int)this._bitrate, (int)_samplerate, 0, WaveFormatTag.UNKNOWN, MPEG_LAYER.Unknown, false, codec);
        }

        bool IMediaParserInstance.Test(MediaStreamReader br)
        {
            string str;

            byte[] startBytes = br.ReadBytes(4);
            str = Encoding.ASCII.GetString(startBytes);

            if (startBytes[0] == 0xff && startBytes[1] == 0xfd)
            {
                _fileFormat = MediaParser.FileFormat.MP2;
                return true;
            }

            if (str != Chunk.Riff)
                return false;

            br.ReadUInt32();
            str = new String(br.ReadChars(4));

            if (str != Chunk.WAVE)
                return false;

            str = new String(br.ReadChars(4));

            if (str == Chunk.Pad)
                _fileFormat = MediaParser.FileFormat.S48;
            else if (str == Chunk.Fmt)
                _fileFormat = MediaParser.FileFormat.WAV;
            else
                _fileFormat = MediaParser.FileFormat.MP2;

            return true;

        }

        public string ToShortString()
        {
            return this.GetType().ToString();
        }

        string IMediaParserInstance.ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(string.Format("FileFormat: {0}", _fileFormat));
            sb.Append(string.Format("\nRozmiar pliku : {0}", _filelength));
            sb.Append("\nCzas trwania:\t" + _duration.ToString() + " ms");

            return sb.ToString();

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
            xml.AddElement(codec, XmlTools.Bitrate, this._bitrate);
            xml.AddElement(codec, XmlTools.Coding, _coding.EnumToString());
            xml.AddElement(codec, XmlTools.Sample.Name, XmlTools.Sample.FindClosestValue(this._sample));
            xml.AddElement(codec, XmlTools.SampleRate.Name, XmlTools.SampleRate.FindClosestValue(this._samplerate));
            xml.AddElementConditionally(rootNode, dict, XmlTools.MimeType, this._mime);
            return xml.FormatXmlString(rootNode.OuterXml);
        }

        #endregion

    }
}

