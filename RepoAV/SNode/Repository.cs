using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using PSNC.RepoAV.DBAccess;
using System.IO;
using PSNC.RepoAV.MaterialFormatDBAccess;
using PSNC.Util;

namespace PSNC.RepoAV.SNode
{
	public class Repository
	{
		internal string Path {get; set;}
		internal int ClusterSize { get; set; } //w B
		internal int Size  {get; set;}// w MB
		internal byte MinFreeSpace  {get; set;}

		protected int m_LastSubdirIndex;
		protected MaterialFormatDBAccess.MaterialFormatDBAccess m_DBAccess;

		public Repository(string path, int sizeInMB, byte minFreeSpace, MaterialFormatDBAccess.MaterialFormatDBAccess dba)
		{
			Path = path;
			Size = sizeInMB;
			MinFreeSpace = minFreeSpace;
			m_DBAccess = dba;
			m_LastSubdirIndex = 1;

			GetClusterSizeFromWMI();
		}

		private void GetClusterSizeFromWMI()
		{
			long realDiskSizeInMB;
			ClusterSize = -1;

			int pos = Path.IndexOf(":");
			string drive = Path.Substring(0, pos + 1);
			ManagementObject mo = new ManagementObject("Win32_LogicalDisk.DeviceID=\"" + drive + "\"");
			if (mo != null)
			{
				foreach (ManagementObject partition in mo.GetRelated("Win32_DiskPartition"))
				{
					if (partition == null)
						continue;

					if (partition.Properties["BlockSize"] != null && partition.Properties["Size"] != null)
					{
						try
						{
							ClusterSize = int.Parse(partition["BlockSize"].ToString());
							realDiskSizeInMB = (long)(ulong.Parse(partition["Size"].ToString()) / (1024L * 1024L));

							if (realDiskSizeInMB < Size)
								Size = (int)realDiskSizeInMB;
						}
						catch (Exception ex)
						{
                            Log.TraceMessage(ex, string.Format("Nie udało się pobrać z WMI rozmiaru bloku dyskowego dla dysku '{0}'.", drive));
						}
						return;
					}
				}
			}
		}

		internal long GetRepositoryFreeSpace()//w MB
		{
			long totalLoad = m_DBAccess.GetTotalLoad() / (1024L * 1024L);
			long free = Size - totalLoad;
			return free < 0 ? 0 : free;
		}

		internal string GetSubdirName4Material()
		{
			lock (this)
			{
				DirectoryInfo di = new DirectoryInfo(System.IO.Path.Combine(Path, m_LastSubdirIndex.ToString()));
				if (!di.Exists)
					di.Create();
				else
				{
					var list = di.GetFiles();
					if (list.Length >= 3000)
					{
						m_LastSubdirIndex++;
						if (!Directory.Exists(System.IO.Path.Combine(Path, m_LastSubdirIndex.ToString())))
							Directory.CreateDirectory(System.IO.Path.Combine(Path, m_LastSubdirIndex.ToString()));
					}
				}
			}

			return m_LastSubdirIndex.ToString();
		}

		internal string BuildLocationForNewMaterial(string uniqueId, string defaultExt, bool preserveFileName, out string location)
		//zwraca full path dla nowej lokalizacji
		{
			if (defaultExt == null)
				defaultExt = "";

			string fileName = uniqueId.Replace(".", "");
			if (!string.IsNullOrEmpty(defaultExt))
				fileName = fileName + defaultExt;

			string subDir = GetSubdirName4Material();
			location = System.IO.Path.Combine(subDir, fileName);

			int idx = 1;
			string newLoc = System.IO.Path.Combine(Path, location);
			if (!preserveFileName)
			{
				while (File.Exists(newLoc))
				{
					fileName = System.IO.Path.GetFileNameWithoutExtension(fileName) + "_" + idx.ToString() + defaultExt;
					location = System.IO.Path.Combine(subDir, fileName);
					newLoc = System.IO.Path.Combine(Path, location);
					idx++;
				}
			}
			else
			{
				if (File.Exists(newLoc))
					return null;
			}

			return System.IO.Path.Combine(Path, location);
		}

		public long CalculateRealFileSize(long fileSize)
		{
			long div = fileSize / (long)ClusterSize;
			long rest = fileSize % (long)ClusterSize;
			if (rest > 0)
			{
				div += 1;
			}
			return div * (long)ClusterSize;
		}

		public string GetFormatFullLocation(FormatMetadata fm)
		{
			return System.IO.Path.Combine(Path, fm.Location);
		}
	}
}
