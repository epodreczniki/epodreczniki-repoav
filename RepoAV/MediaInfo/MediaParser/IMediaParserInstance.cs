using System;
using System.Collections.Generic;
using MediaInfoWrapper;
using PSNC.Multimedia.Tools;
using System.Xml.Linq;

namespace PSNC.Multimedia
{
    public interface IMediaParserInstance
    {
        #region własności

        IMediaInfo MediaInfo { get; set; }

        // FormatException pliku 
        MediaParser.FileFormat FileFormat { get; }

        // czas trwania w ms
        ulong Duration { get; }

        // długość pliku w bajtach
        ulong Filelength { get; }

        // sumaryczna przepływność w bitach na sekundę
        uint Bitrate { get; }

        String MimeType { get; }

        AudioStreamProperties[] AudioStreams { get; }

        VideoStreamProperties[] VideoStreams { get; }

        Dictionary<string, string> Metadata { get; }

        IEnumerable<String> Warnings { get; }

        #endregion

        // czy to jest moj format
        bool Test(MediaStreamReader br);


        // analiza danych
        bool Parse(MediaStreamReader br);

        // prezentacja
        string ToString();
        string ToShortString();
        string ToXML(DictionaryEx<string, object> dict);
    }
}
