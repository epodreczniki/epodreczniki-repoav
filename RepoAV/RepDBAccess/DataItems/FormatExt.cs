using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSNC.RepoAV.DBAccess;

namespace PSNC.RepoAV.RepDBAccess
{
	public class FormatExt : Format
	{
		public string FileFormat {get; internal set;}

		public int VideoBitrate {get; internal set;}

		public int VideoFramerate {get; internal set;}

		public string VideoCoding {get; internal set;}

		public int VideoFrameHeight {get; internal set;}

		public int VideoFrameWidth {get; internal set;}

        public string VideoProfile { get; internal set; }

        public float VideoLevel {get; internal set;}

		public int AudioBitrate {get; internal set;}

		public string AudioCoding {get; internal set;}

		public short AudioSample {get; internal set;}

		public int AudioSampleRate {get; internal set;}

		public FormatExt()
			:base()
		{
			VideoBitrate = -1;
			VideoFramerate = -1;
			VideoFrameHeight = -1;
			VideoFrameWidth = -1;
			VideoLevel = -1.0f;
			AudioBitrate = -1;
			AudioSample = -1;
			AudioSampleRate = -1;
		}
	}
}
