using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using MediaInfoWrapper;
using tools = PSNC.Multimedia.Tools.MediaParserTools;
using xml = PSNC.Multimedia.Tools.XmlTools;
using PSNC.Multimedia.Tools;

namespace PSNC.Multimedia.Instances
{
    class FlacAudioParser : AbstractParser, IMediaParserInstance
    {
        ulong _duration;
        ulong _filelength;
        uint _bitrate;
        int _sample;
        int _samplerate;
        string _mime = "application/x-flac";
        xml.AudioCoding.Values _coding = xml.AudioCoding.Values.Unknown;
        MediaParser.FileFormat _fileformat = MediaParser.FileFormat.FLAC;
        AudioStreamProperties _audiostream = null;
        Dictionary<string, string> _metadata;
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

        string IMediaParserInstance.MimeType
        {
            get { return _mime; }
        }

        AudioStreamProperties[] IMediaParserInstance.AudioStreams
        {
            get
            {
                if (_audiostream == null)
                    return null;
                else
                    return new AudioStreamProperties[] { _audiostream };
            }
        }

        VideoStreamProperties[] IMediaParserInstance.VideoStreams
        {
            get { return null; }
        }

        Dictionary<string, string> IMediaParserInstance.Metadata
        {
            get { return _metadata; }
        }

        IEnumerable<string> IMediaParserInstance.Warnings
        {
            get { return null; }
        }

        bool IMediaParserInstance.Parse(PSNC.Multimedia.Tools.MediaStreamReader br)
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
                        _audiostream.Coding = this._coding;
                    }
                }
            }
            return true;
        }

        bool IMediaParserInstance.Test(PSNC.Multimedia.Tools.MediaStreamReader br)
        {
            if (br.BaseStream.Length > 4)
            {
                string flac = br.GetString(4);
                if (flac == Chunk.FLAC)
                    return true;

            }
            return false;
        }

        string IMediaParserInstance.ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("FileFormat: {0}", this._fileformat));
            sb.Append("\nCzas trwania:\t" + Tools.MediaParserTools.GetHumanReadableDuration((long)this._duration) + " ms.");

            return sb.ToString();
        }

        string IMediaParserInstance.ToShortString()
        {
            return this.GetType().ToString();
        }

        string IMediaParserInstance.ToXML(PSNC.Multimedia.Tools.DictionaryEx<string, object> dict)
        {
            DateTime now = DateTime.Now;
            XmlDocument doc = new XmlDocument();
            XmlNode docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(docNode);

            XmlNode rootNode = doc.CreateElement(XmlTools.Format);
            doc.AppendChild(rootNode);

            xml.AddElement(rootNode, XmlTools.FileFormat, this._fileformat.ToString().ToLower());
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
            xml.AddElement(codec, XmlTools.Sample.Name, XmlTools.Sample.FindClosestValue(_sample));
            xml.AddElement(codec, XmlTools.SampleRate.Name, XmlTools.SampleRate.FindClosestValue(_samplerate));

            xml.AddElementConditionally(rootNode, dict, XmlTools.MimeType, this._mime);
            return xml.FormatXmlString(rootNode.OuterXml);
        }

        #endregion

        private void CreateAudioStream()
        {
            string codec = "Free Lossless Audio Codec";
            _audiostream = new AudioStreamProperties(0, (int)this._bitrate, (int)this._samplerate, (int)this._sample, WaveFormatTag.UNKNOWN, MPEG_LAYER.Unknown, false, codec);
        }

    }
}

