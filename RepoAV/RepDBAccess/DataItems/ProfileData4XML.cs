using PSNC.RepoAV.DBAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSNC.RepoAV.RepDBAccess
{
	public class ProfileData4XML : BaseObject
	{
		public int Id {get; set;}
		public string Name {get; set;}
		public string FileFormat { get; set; }
		public bool AudioOnly { get; set; }
		public int FrameWidth { get; set; }
		public int FrameHeight { get; set; }
		public int VideoBitrate { get; set; }
		public int Framerate { get; set; }
		public string VideoCoding { get; set; }
		public int AudioBitrate { get; set; }
		public string AudioCoding { get; set; }
		public short Sample { get; set; }
		public int SampleRate { get; set; }

	}
}
