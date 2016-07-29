using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PSNC.Multimedia.Tools;
using xml = PSNC.Multimedia.Tools.XmlTools;
using System.Xml.Linq;
using System.Xml;
using MediaInfoWrapper;
using PSNC.Multimedia.Instances;

namespace PSNC.Multimedia
{
    class UnknownParser : AbstractParser, IMediaParserInstance
    {
        private bool _valid_file = true;
        private Encoding _encoding = Encoding.ASCII;
        ulong _filelength = 0;
        string _mime = "www/mime";
        MediaParser.FileFormat fileFormat = MediaParser.FileFormat.UNKNOWN;
        bool isXML = false;
        private ulong _duration = 0;
        List<String> warnings = new List<string>();

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
            get { return 0; }
        }

        String IMediaParserInstance.MimeType
        {
            get { return _mime; }
        }

        AudioStreamProperties[] IMediaParserInstance.AudioStreams
        {
            get { return null; }
        }

        VideoStreamProperties[] IMediaParserInstance.VideoStreams
        {
            get { return null; }
        }

        Dictionary<string, string> IMediaParserInstance.Metadata
        {
            get { return null; }
        }

        IEnumerable<string> IMediaParserInstance.Warnings
        {
            get { return warnings; }
        }

        public static Encoding GetFileEncoding(byte[] buffer)
        {

            if (buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
            {
                return Encoding.UTF8;
            }

            if (buffer[0] == 0xFF && buffer[1] == 0xFE)
            {
                return buffer[2] == 0x00 && buffer[3] == 0x00 ? Encoding.UTF32 : Encoding.Unicode;
            }

            if (buffer[0] == 0xFE && buffer[1] == 0xFF)
            {
                return Encoding.BigEndianUnicode;
            }

            if (buffer[0] == 0x2B && buffer[1] == 0x2F && buffer[2] == 0x76)
            {
                return Encoding.UTF7;
            }

            return Encoding.Default;
        }

        private bool IsBinary(IEnumerable<string> lines)
        {
            foreach (var content in lines)
            {
                if (content.Any(ch => char.IsControl(ch) && ch != '\r' && ch != '\n'))
                    return true;
            }
            return false;
        }

        bool IMediaParserInstance.Parse(MediaStreamReader br)
        {
            _valid_file = false;
            _filelength = (uint)br.BaseStream.Length;
            br.Position = 0;
            if (br.BaseStream.Length > 4)
            {
                byte[] header = br.ReadBytes(4);
                string strHeader = BitConverter.ToString(header);
                if (strHeader == "50-4B-03-04")
                {
                    fileFormat = MediaParser.FileFormat.ZIP;
                    _mime = "application/zip";
                    _valid_file = true;
                    return _valid_file;
                }

                if (strHeader == "25-50-44-46")
                {
                    _mime = "application/pdf";
                    return true;
                }

                _encoding = GetFileEncoding(header);
            }
            br.Position = 0;
            try
            {
                var lines = System.IO.File.ReadAllLines(br.FileName).ToList();
                if (IsBinary(lines))
                    return _valid_file;
                else
                    _mime = "text/plain";
                var firstLine = String.Empty;
                var firstNonEmptyLine = String.Empty;
                var firstLines = String.Empty;
                if (lines.Count > 1)
                {
                    firstLine = lines[0];
                    firstNonEmptyLine = firstLine;
                    if (String.IsNullOrWhiteSpace(firstNonEmptyLine))
                    {
                        for (var l = 0; l < lines.Count; l++)
                        {
                            if (!String.IsNullOrWhiteSpace(lines[l]))
                            {
                                firstNonEmptyLine = lines[l];
                                break;
                            }
                        }
                    }
                    firstLines = lines[0] + lines[1];
                }
                if (firstNonEmptyLine.Contains(Chunk.VTT))
                {
                    TimeSpan latestTimeInSubtitles = TimeSpan.Zero;
                    fileFormat = MediaParser.FileFormat.VTT;
                    _mime = "text/vtt";
                    _valid_file = VttParser.LoadSubtitle(lines, warnings, ref latestTimeInSubtitles);
                    _duration = (ulong)latestTimeInSubtitles.TotalMilliseconds;
                    return _valid_file;
                }
                var ass = firstLines.Contains("[Script Info]");
                if (ass)
                {
                    TimeSpan latestTimeInSubtitles = TimeSpan.Zero;
                    fileFormat = MediaParser.FileFormat.ASS;
                    _mime = "text/x-ssa";
                    _valid_file = AssParser.LoadSubtitle(lines, warnings, ref latestTimeInSubtitles);
                    _duration = (ulong)latestTimeInSubtitles.TotalMilliseconds;
                    return _valid_file;
                }
                var srt = firstLines.Contains("-->");
                if (srt)
                {
                    TimeSpan latestTimeInSubtitles = TimeSpan.Zero;
                    fileFormat = MediaParser.FileFormat.SRT;
                    _mime = "text/plain";
                    _valid_file = SrtParser.LoadSubtitle(lines, warnings, ref latestTimeInSubtitles);
                    _duration = (ulong)latestTimeInSubtitles.TotalMilliseconds;
                    return _valid_file;
                }
            }
            catch { }

            try
            {
                XDocument xd1 = new XDocument();
                xd1 = XDocument.Load(br.FileName);
                isXML = true;
            }
            catch (XmlException exception)
            {
            }
            if (isXML)
            {
                fileFormat = MediaParser.FileFormat.XML;
                _mime = "text/xml";
                _valid_file = true;
                return _valid_file;
            }
            return _valid_file;
        }

        bool IMediaParserInstance.Test(MediaStreamReader br)
        {
            return true;
        }

        string IMediaParserInstance.ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("FileFormat: {0}", fileFormat));
            sb.Append(string.Format("\nRozmiar pliku : {0}", _filelength));
            sb.Append(string.Format("\nKodowanie : {0}", _encoding.EncodingName));
            return sb.ToString();
        }

        string IMediaParserInstance.ToShortString()
        {
            return "Nieznany format pliku";
        }

        string IMediaParserInstance.ToXML(DictionaryEx<string, object> dict)
        {
            if (!_valid_file)
            {
                fileFormat = MediaParser.FileFormat.UNKNOWN;
                _mime = "www/mime";
                return String.Empty;
            }
            DateTime now = DateTime.Now;
            XmlDocument doc = new XmlDocument();
            XmlNode docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(docNode);

            XmlNode rootNode = doc.CreateElement(XmlTools.Format);
            doc.AppendChild(rootNode);

            xml.AddElement(rootNode, XmlTools.FileFormat, fileFormat.ToString().ToLower());
            xml.AddElement(rootNode, XmlTools.FileSize, this._filelength);
            xml.AddElementConditionally(rootNode, dict, XmlTools.Publication, now.ToXmlDate());

            #region przepisanie_właściwości_ze_słownika
            if (dict != null)
                foreach (KeyValuePair<string, object> entry in dict)
                {
                    rootNode.AddOrUpdateElement(entry.Key, entry.Value);
                }
            #endregion

            xml.AddElementConditionally(rootNode, dict, XmlTools.MimeType, this._mime);
            return xml.FormatXmlString(rootNode.OuterXml);
        }

        #endregion
    }
}

