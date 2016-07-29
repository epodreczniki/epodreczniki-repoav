using System;
using System.Collections.Generic;
using System.IO;
using MediaInfoWrapper;
using PSNC.Multimedia.Instances;
using PSNC.Multimedia.Tools;
using System.Reflection;
using System.ComponentModel;

namespace PSNC.Multimedia
{

    public partial class MediaParser : IDisposable
    {
        public enum FileFormat
        {
            [MimeType("www/mime")]
            UNKNOWN,
            [MimeType("application/octet-stream")]
            Archive,
            [MimeType("application/octet-stream")]
            Document,
            [MimeType("application/octet-stream")]
            Multimedia,
            [MimeType("application/zip")]
            ZIP,
            [Description("3gp")]
            [MimeType("video/3gpp")]
            _3GP,
            [MimeType("audio/mp4")]
            AAC,
            [MimeType("audio/x-monkeys-audio")]
            APE,
            [MimeType("video/avi")]
            AVI,
            [MimeType("audio/ogg")]
            OGG,
            [MimeType("video/x-matroska")]
            MKV,
            [MimeType("audio/wav")]
            WAV,
            [MimeType("audio/mpeg")]
            MUS,
            [MimeType("audio/mpeg")]
            MP2,
            [MimeType("audio/mpeg")]
            S48,
            [MimeType("audio/mpeg3")]
            MP3,
            [MimeType("video/x-ms-asf")]
            ASF,
            [MimeType("video/x-ms-asf")]
            [IsLive(true)]
            WMV,
            [MimeType("audio/x-ms-asf")]
            WMA,
            [MimeType("video/mp4")]
            MP4,
            [MimeType("application/x-flac")]
            FLAC,
            [MimeType("video/x-flv")]
            FLV,
            [MimeType("audio/vnd.rn-realaudio")]
            RMA,
            [MimeType("video/vnd.rn-realvideo")]
            RMV,
            [MimeType("text/x-ssa")]
            ASS,
            [MimeType("text/plain")]
            SRT,
            [MimeType("text/vtt")]
            VTT,
            [MimeType("text/xml")]
            XML,
            [MimeType("video/m2ts")]
            M2TS,
            [MimeType("video/webm")]
            WEBM,
            [MimeType("text/ism")]
            ISM,
            [MimeType("text/ismc")]
            ISMC,
            [MimeType("text/isml")]
            [IsLive(true)]
            ISMV

        }
        private string path;

        private bool disposed = false;
        List<IMediaParserInstance> _instances = null;
        MediaStreamReader _br = null;

        public MediaParser(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
                throw new ArgumentException("Plik " + filePath + " nie został znaleziony!");
            path = filePath;
            Open(System.IO.File.OpenRead(filePath));
        }

        public MediaParser(Stream stream)
        {
            Open(stream);
        }

        private void Open(Stream stream)
        {
            Initialize();

            stream.Position = 0;
            _br = new MediaStreamReader(stream, path);
        }

        public bool UsesAdditionalLibraries()
        {
            return true;
        }

        public string GetCompilationType()
        {
            return Assembly.GetExecutingAssembly().GetName().ProcessorArchitecture.ToString();
        }

        ~MediaParser()
        {

            Dispose(true);
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        public IMediaParserInstance Parse()
        {
            IMediaParserInstance correctInstance = null;
            if (_br.BaseStream.Length != 0)
            {
                using (var media = MediaInfoWrapper.MediaInfoFactory.GetInstance())
                {
                    media.Open(_br.FileName);
                    var format = media.Get<String>(Generalinfo.Format);
                    switch (format)
                    {
                        case "Monkey's Audio": correctInstance = new ApeAudioParser(); break;
                        case "Windows Media": correctInstance = new AsfParser(); break;
                        case "Wave": correctInstance = new MpegAudioParser();
                            break;
                        case "AVI": correctInstance = new AviVideoParser(); break;
                        case "FLAC": correctInstance = new FlacAudioParser(); break;
                        case "Flash Video": correctInstance = new FlvVideoParser(); break;
                        case "WEBM":
                        case "MKV":
                        case "Matroska": correctInstance = new MatroskaVideoParser(); break;
                        case "MPEG Audio":
                            {
                                var profile = media.Get<String>(Audioinfo.Codec);
                                if (profile != "MPA1L3")
                                    correctInstance = new MpegAudioParser();
                                else
                                    correctInstance = new Mp3AudioParser();
                            }
                            break;
                        case "MPEG-4": correctInstance = new Mp4AudioParser(); break;
                        case "MPEG-TS": correctInstance = new MpegTsVideoParser(); break;
                        case "MPEG-PS": correctInstance = new MpegVideoParser(); break;
                        case "OGG": correctInstance = new OggAudioParser(); break;
                        case "RealMedia": correctInstance = new RealVideoParser(); break;
                        default:
                            correctInstance = new UnknownParser();
                            break;
                    }
                    if (correctInstance != null)
                    {
                        correctInstance.MediaInfo = media;
                        correctInstance.Parse(_br);
                    }
                }
            }
            return correctInstance;
        }

        private void Initialize()
        {
            if (_instances == null)
            {
                _instances = new List<IMediaParserInstance>();
                _instances.Add(new Mp4AudioParser());
                _instances.Add(new MatroskaVideoParser());
                _instances.Add(new AsfParser());
                _instances.Add(new MpegTsVideoParser());
                _instances.Add(new AviVideoParser());
                _instances.Add(new MpegVideoParser());

                _instances.Add(new FlacAudioParser());
                _instances.Add(new FlvVideoParser());
                _instances.Add(new RealVideoParser());
                _instances.Add(new ApeAudioParser());
                _instances.Add(new OggAudioParser());

                _instances.Add(new MusiAudioParser());
                _instances.Add(new MpegAudioParser());
                _instances.Add(new Mp3AudioParser());

                _instances.Add(new UnknownParser());
            }
        }

        private void Dispose(bool disposing)
        {

            if (!this.disposed)
            {

                if (disposing)
                {
                    if (_br != null)
                    {
                        if (_br.BaseStream != null)
                            _br.BaseStream.Close();
                        _br.Close();
                        _br = null;
                    }
                }

                disposed = true;

            }
        }

    }
}

