using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace PSNC.RepoAV.Common
{
   public enum DownloadKeywords
    {
       FormatURL,
       MimeType,
       FormatMD5,
	   OverwriteIfExists,
	   PreserveFileName,
	   FileExtension
    }

    public enum RecodeKeywords
    {
        SourceFormatIds,
        OutputFormatIds,
        DownloadSourceFiles,
        OperationXML,
        ParameterCount,
        Parameter1,
        Parameter2,
        Parameter3,
        Parameter4
    }

    public enum AddKeywords
    {
        MimeType,
        Metadata,
        FormatURL,
        FormatMD5,
        FormatType,
        User
    }

	public enum RemoveKeywords
	{
		ForceDlete,
        FormatId
	}

}
