using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSNC.Multimedia
{
    public class SubtitleStreamProperties
    {
          public string Id;
          public int Index;
          public string Description;
          public PSNC.Multimedia.MediaParser.FileFormat FileFormat = MediaParser.FileFormat.UNKNOWN;
          public string MediaURI;
          public bool ClosedCaptions = true;
          public bool VideoEmbedded = false;
          public string Language;
    }
}
