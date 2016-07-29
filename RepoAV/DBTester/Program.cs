using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using PSNC.RepoAV.RepDBAccess;
using PSNC.RepoAV.DBAccess;
using System.Management;

namespace DBTester
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				bool res = true;
				PSNC.RepoAV.RepDBAccess.RepDBAccess dba = new PSNC.RepoAV.RepDBAccess.RepDBAccess(@"Data Source=(LocalDB)\v11.0;Persist Security Info=True;Integrated Security = SSPI;Initial Catalog=RepDB", false);
				object resObj = null;
				int total = -1;

				Console.WriteLine("Server Name = " + dba.ServerName);


				//-----------------------------------------------------------------------------------
				//resObj = dba.GetTasksCount(null, TaskStatus.New);
				//-----------------------------------------------------------------------------------
				//resObj = dba.GetManagerTasks2Execute(1);
				//-----------------------------------------------------------------------------------
				//resObj = dba.GetTasks2Execute(3, TaskType.Download);
				//-----------------------------------------------------------------------------------
				//TaskAdd task = new TaskAdd() { ExecutingNodeId = 1, PublicId = "abc", Type = TaskType.AddMaterial };
				//task.Content.Add("MaterialId", "abc");
				//task.Content.Add("MimeType", "video/mp4");
				//task.Content.Add("MaterialURL", "http://10.11.111.151/throw_it_away.mp3");
				//res = dba.AddTask(task);
				//-----------------------------------------------------------------------------------
				//Task t = dba.GetTask(10);
				//-----------------------------------------------------------------------------------
				//resObj = dba.GetMaterial("abc");
				//-----------------------------------------------------------------------------------
				//resObj = dba.GetFormat(1);
				//-----------------------------------------------------------------------------------
				//resObj = dba.GetTasksCount(null, new TaskStatus[] { TaskStatus.Executing });
				//-----------------------------------------------------------------------------------
				//resObj = dba.GetFormatLocations("1e63fd48-9558-43ef-96be-1afa504bd5c0");
				//-----------------------------------------------------------------------------------
				//res = dba.RemoveFormat(12);
				//-----------------------------------------------------------------------------------
				//resObj = dba.GetTasksOfType(TaskType.AddMaterial);
				//-----------------------------------------------------------------------------------
				//resObj = dba.GetOperationXml4Profile(38);
				//-----------------------------------------------------------------------------------
				//resObj = dba.GetEncodingProfilesXml();
				//-----------------------------------------------------------------------------------
				//resObj = dba.GetMaterialStatus("xxx.publi_2");
				//-----------------------------------------------------------------------------------
				//-----------------------------------------------------------------------------------

				//int pid;

				//Process m_RunningProcessHandle = new Process();
				//m_RunningProcessHandle.StartInfo = new ProcessStartInfo("cmd.exe", "/C run.bat");
				//m_RunningProcessHandle.StartInfo.UseShellExecute = true;
				//m_RunningProcessHandle.StartInfo.CreateNoWindow = true;
				//m_RunningProcessHandle.StartInfo.WorkingDirectory = @"e:\Temp";

				//m_RunningProcessHandle.Start();
				//pid = m_RunningProcessHandle.Id;
				//m_RunningProcessHandle.PriorityClass = ProcessPriorityClass.BelowNormal;

				////KillProcessAndChildren(pid);
				//try
				//{
				//	if (!m_RunningProcessHandle.HasExited)
				//		m_RunningProcessHandle.WaitForExit();
				//}
				//catch (SystemException)
				//{
				//}

				//Console.WriteLine("Exit code = " + m_RunningProcessHandle.ExitCode.ToString());
				//-----------------------------------------------------------------------------------
				//MaterialSearch ms = new MaterialSearch();
				//ms.AllowDistribution = true;
				//ms.Offset = 0;
				//ms.SortOrder = MaterialSortKind.DurationASC;
				//ms.Count = 10;
				//Material[] mss = dba.FindMaterials(ms);
				//-----------------------------------------------------------------------------------

				long repoSizeB = 34 * 1024 * 1024;//w B
				byte freeSpaceProc = 10;
				long wantedMaxLoad = (long)(((double)(100 - freeSpaceProc)) / 100.0 * (double)repoSizeB);//w B

				//-----------------------------------------------------------------------------------

				if (res)
					Console.WriteLine("Sukces");
				else
					Console.WriteLine("ERROR ");

				if (total > -1)
					Console.WriteLine("Total= " + total.ToString());

				if (resObj != null)
				{
					StringBuilder sb = new StringBuilder();
					BaseObject.Print(resObj, sb, "  ");
					Console.WriteLine(sb.ToString());
				}
			}
			catch (DBAccessException dbe)
			{
				Console.WriteLine(string.Format("Wyjątek DBAccess: EC={0}, Msg={1}, [{2}]", dbe.Error, dbe.Message, dbe.InnerException == null ? dbe.StackTrace : dbe.InnerException.ToString()));
			}
			catch (Exception ex)
			{
				Console.WriteLine("Wyjątek: " + ex.ToString());
			}
			Console.ReadLine();
		}



		/// <summary>
		/// Kill a process, and all of its children, grandchildren, etc.
		/// </summary>
		/// <param name="pid">Process ID.</param>
		private static void KillProcessAndChildren(int pid)
		{
			ManagementObjectSearcher searcher = new ManagementObjectSearcher ("Select * From Win32_Process Where ParentProcessID=" + pid);
			ManagementObjectCollection moc = searcher.Get();
			foreach (ManagementObject mo in moc)
			{
				KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
			}
			try
			{
				Process proc = Process.GetProcessById(pid);
				proc.Kill();
			}
			catch (ArgumentException)
			{
				// Process already exited.
			}
		}
	}
}
