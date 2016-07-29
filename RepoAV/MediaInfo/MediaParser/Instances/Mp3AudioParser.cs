using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using MediaInfoWrapper;
using PSNC.Multimedia.Tools;
using tools = PSNC.Multimedia.Tools.MediaParserTools;
using xml = PSNC.Multimedia.Tools.XmlTools;
using System.Xml.Linq;

namespace PSNC.Multimedia.Instances
{
    class Mp3AudioParser : AbstractParser, IMediaParserInstance
    {
        public const int MAX_SEARCH_BYTES = 100000;

        MediaParser.FileFormat _fileformat = MediaParser.FileFormat.MP3;
        uint _filelength = 0;
        ulong _medialength = 0;
        ulong _duration = 0;
        uint _bitrate = 0;
        int _sample = 0;
        int _samplerate = 0;
        xml.AudioCoding.Values _coding = xml.AudioCoding.Values.Unknown;
        string _mime = "audio/mpeg3";
        Dictionary<string, string> _metadata = new Dictionary<string, string>();

        AudioStreamProperties _audioStream = null;

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
            get
            {
                if (_bitrate < MediaParserTools.KILOBIT)
                    return _bitrate * MediaParserTools.KILOBIT;
                else return _bitrate;
            }
        }

        String IMediaParserInstance.MimeType
        {
            get { return _mime; }
        }

        AudioStreamProperties[] IMediaParserInstance.AudioStreams
        {
            get
            {
                if (_audioStream == null)
                    return null;
                else
                    return new AudioStreamProperties[] { _audioStream };
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
                this._filelength = (uint)br.BaseStream.Length;
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

                        _audioStream = new AudioStreamProperties(index, (int)audioBitrate, (int)_samplerate,
                            (int)_sample, wft, mpeglayer, false, audiocodec);
                        var kodek = audiocodec.ToLower();
                        _audioStream.Coding = this._coding;
                    }
                }
            }

            return true;
        }

        bool IMediaParserInstance.Test(MediaStreamReader br)
        {
            try
            {
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
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

            xml.AddElement(rootNode, XmlTools.FileFormat, this._fileformat.ToString().ToLower());
            xml.AddElement(rootNode, XmlTools.Bitrate, ((IMediaParserInstance)this).Bitrate);
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
            xml.AddElement(codec, XmlTools.Bitrate, ((IMediaParserInstance)this).Bitrate);
            xml.AddElement(codec, XmlTools.Coding, _coding.EnumToString());
            xml.AddElement(codec, XmlTools.Sample.Name, XmlTools.Sample.FindClosestValue(_sample));
            xml.AddElement(codec, XmlTools.SampleRate.Name, XmlTools.SampleRate.FindClosestValue(_samplerate));

            xml.AddElementConditionally(rootNode, dict, XmlTools.MimeType, this._mime);
            return xml.FormatXmlString(rootNode.OuterXml);
        }

        #endregion

        private void CreateAudioStream()
        {
            string codec = "MPEG-1 Layer 3";
            _audioStream = new AudioStreamProperties(0, (int)((IMediaParserInstance)this).Bitrate, (int)this._samplerate, (int)this._sample, WaveFormatTag.MPEGLAYER3, MPEG_LAYER.LAYER3, false, codec);
        }

    }
}

