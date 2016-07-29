using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Runtime.ExceptionServices;
using PSNC.Proca3.Subsystem;
using PSNC.Util;
using PSNC.RepoAV;
using PSNC.RepoAV.Common;
using PSNC.RepoAV.MaterialFormatDBAccess;
using PSNC.RepoAV.RepDBAccess;
using PSNC.RepoAV.TaskQueue;
using System.Management;
using System.IO;
using PSNC.Multimedia;

namespace PSNC.RepoAV.SNode
{
	

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
	public class SNodeSubsystem : Subsystem, ISubsystemService, ISNodeSubsystem
    {
		[SubsystemParameter("Czas w minutach, jaki może max być wykonywane zadanie synchronizacji repozytorium z FSC.",
                        DefaultValue = 60,
                        MaxValue = int.MaxValue,
                        MinValue = 1,
                        IsReadOnly = false)]
        private Parameter<int> m_MaxTime4SyncWithFSCTaskExecution;


		[SubsystemParameter("Czas w sekundach, co jaki SNode sprawdza bazę RepDB pod kątem nowych zadań do wykonania.",
						DefaultValue = 30,
						MaxValue = int.MaxValue,
						MinValue = 3,
						IsReadOnly = false)]
		private Parameter<int> m_Checking4NewTasksInterval;

		[SubsystemParameter("Czas w sekundach, co jaki SNode uspójnia informacje o swoim repozytorium z bazą RepDB (-1 wyłącza funkcję).",
						DefaultValue = 120*60,
						MaxValue = int.MaxValue,
						MinValue = 300,
						IsReadOnly = false)]
		private Parameter<int> m_SyncWithFSCInterval;

		[SubsystemParameter("Czas w sekundach, co jaki SNode sprawdza i czyści swoje lokalne repozytorium (-1 wyłącza funkcję).",
						DefaultValue = 600,
						MaxValue = int.MaxValue,
						MinValue = 10,
						IsReadOnly = false)]
		private Parameter<int> m_ValidateInterval;

		[SubsystemParameter("Max. liczba zadań pobieranych przy każdorazowaym zapytaniu o zadania do wykonania.",
						DefaultValue = 5,
						MaxValue = int.MaxValue,
						MinValue = 1,
						IsReadOnly = false)]
		private Parameter<int> m_MaxNewTasksCount;

		[SubsystemParameter("Liczba wątków wykonujących równolegle zadania.",
						DefaultValue = 5,
						MaxValue = 1000,
						MinValue = 1,
						IsReadOnly = false)]
		private Parameter<int> m_ExecutingThreadsCount;


		[SubsystemParameter("Max czas, jaki może pojedynczy wątek wykonawczy spędzić na aktywnym wykonaniu zadania (w sekundach).",
						DefaultValue = 60,
						MaxValue = int.MaxValue,
						MinValue = 1,
						IsReadOnly = false)]
		private Parameter<int> m_MaxTimeOfWorkingThreadExecution;

		[SubsystemParameter("Max czas, jaki może pojedynczy wątek wykonawczy spędzić na aktywnym wykonaniu zadania wstawienia nowego formatu (w sekundach).",
						DefaultValue = 3600,
						MaxValue = int.MaxValue,
						MinValue = 1,
						IsReadOnly = false)]
		private Parameter<int> m_InsertMaterialTaskRunningTimeout;

		[SubsystemParameter("Liczba dni okresu ochronnego przed usunięciem automatycznym dla formatów. Dla wartości -1 pobierana jest aktualna globalna wartość z bazy RepoDB.",
						DefaultValue = (short)-1,
						MaxValue = short.MaxValue,
						MinValue = (short)-1,
						IsReadOnly = false)]
		private Parameter<short> m_RemoveFormatsOlderThanDays;

		[SubsystemParameter("Flaga określająca, czy SNode ma traktować swój storage jako temporalny czy permanentny. W pierwszym przypadku nie będzie honorował parametru 'RemoveFormatsOlderThanDays'.",
						DefaultValue = false,
						IsReadOnly = false)]
		private Parameter<bool> m_IgnoreRemoveFormatsOlderThanDays;

		//[SubsystemParameter("Mapowanie typów MIME na rozszerzenia plików.",
		//				DefaultValue = "application/envoy;.evy|application/fractals;.fif|application/futuresplash;.spl|application/hta;.hta|application/internet-property-stream;.acx|application/mac-binhex40;.hqx|application/msword;.doc|application/octet-stream;.bin|application/oda;.oda|application/oleobject;.ods|application/olescript;.axs|application/pdf;.pdf|application/pics-rules;.prf|application/pkcs7-mime;.p7m|application/pkcs7-signature;.p7s|application/pkcs10;.p10|application/pkix-crl;.crl|application/postscript;.ps|application/rtf;.rtf|application/set-payment-initiation;.setpay|application/set-registration-initiation;.setreg|application/winhlp;.hlp|application/vndms-pkicertstore;.sst|application/vndms-pkipko;.pko|application/vndms-pkistl;.stl|application/vndms-pkiseccat;.cat|application/vnd.ms-excel;.xlm|application/vnd.ms-powerpoint;.pps|application/vnd.ms-project;.mpp|application/vnd.ms-works;.wps|application/x-bcpio;.bcpio|application/x-cdf;.cdf|application/x-compressed;.tgz|application/x-cpio;.cpio|application/x-csh;.csh|application/x-dvi;.dvi|application/x-director;.dir|application/x-gzip;.gz|application/x-gtar;.gtar|application/x-hdf;.hdf|application/x-internet-signup;.isp|application/x-iphone;.iii|application/x-javascript;.js|application/x-latex;.latex|application/x-msmediaview;.m13|application/x-msaccess;.mdb|application/x-msclip;.clp|application/x-mscardfile;.crd|application/x-msdownload;.dll|application/x-msmediaview;.mvb|application/x-msmetafile;.wmf|application/x-msmoney;.mny|application/x-mspublisher;.pub|application/x-msschedule;.scd|application/x-msterminal;.trm|application/x-mswrite;.wri|application/x-ms-application;.application|application/x-ms-manifest;.manifest|application/x-netcdf;.nc|application/x-perfmon;.pmc|application/x-pkcs7-certreqresp;.p7r|application/x-pkcs7-certificates;.p7b|application/x-pkcs12;.p12|application/x-sv4cpio;.sv4cpio|application/x-sv4crc;.sv4crc|application/x-shar;.shar|application/x-sh;.sh|application/x-stuffit;.sit|application/x-tar;.tar|application/x-tcl;.tcl|application/x-tex;.tex|application/x-texinfo;.texinfo|application/x-troff;.roff|application/x-troff-man;.man|application/x-troff-me;.me|application/x-troff-ms;.ms|application/x-ustar;.ustar|application/x-wais-source;.src|application/x-x509-ca-cert;.cer|application/x-zip-compressed;.zip|audio/aiff;.aiff|audio/basic;.au|audio/mid;.mid|audio/wav;.wav|audio/x-aiff;.aif|audio/x-mpegurl;.m3u|audio/x-pn-realaudio;.ra|image/bmp;.bmp|image/cis-cod;.cod|image/gif;.gif|image/ief;.ief|image/jpeg;.jpg|image/pjpeg;.jfif|image/tiff;.tif|image/x-cmu-raster;.ras|image/x-cmx;.cmx|image/x-icon;.ico|image/x-portable-anymap;.pnm|image/x-portable-bitmap;.pbm|image/x-portable-graymap;.pgm|image/x-portable-pixmap;.ppm|image/x-rgb;.rgb|image/x-xbitmap;.xbm|image/x-xpixmap;.xpm|image/x-xwindowdump;.xwd|message/rfc822;.eml|text/css;.css|text/html;.html|text/iuls;.uls|text/plain;.txt|text/richtext;.rtx|text/scriptlet;.sct|text/tab-separated-values;.tsv|text/webviewhtml;.htt|text/xml;.xml|text/x-component;.htc|text/x-setext;.etx|text/x-vcard;.vcf|video/mpeg;.mpeg|video/quicktime;.mov|video/x-ivf;.ivf|video/x-la-asf;.lsf|video/x-msvideo.avi|video/x-ms-asf;.asf|video/x-sgi-movie;.movie|x-world/x-vrml;.flr|*;.bin",
		//				IsReadOnly = true)]
		//private Parameter<string> m_Mime2FileExtension;

		protected TaskQueueManager m_TaskQueue;
		protected MaterialFormatDBAccess.MaterialFormatDBAccess m_DBAccess;
		protected RepDBAccess.RepDBAccess m_RepoDBAccess;
		protected DateTime m_LastChecking4NewTasksDate;
		protected DateTime m_LastSyncWithFSCDate;
		protected DateTime m_LastValidateDate;
		protected bool m_DuringTaskOrdering;
        protected RepoAV.RepDBAccess.TaskTypeComplex[] m_myTaskTypes;
		internal int InsertMaterialTaskRunningTimeout
		{
			get { return m_InsertMaterialTaskRunningTimeout.Value; }
		}
		internal int MaxTime4SyncWithFSCTaskExecution
		{
			get { return m_MaxTime4SyncWithFSCTaskExecution.Value; }
		}
		protected HashSet<long> m_TasksInSystem;
		protected Repository m_Repo;
		protected Dictionary<string, string> m_Mime2FileExtensionMap;
		protected short m_RemoveFormatsOlderThanDaysGlobal = 720;//2 lata

		internal short RemoveFormatsOlderThanDays
		{
			get 
			{
				if (m_RemoveFormatsOlderThanDays.Value > -1)
					return m_RemoveFormatsOlderThanDays.Value;
				else
					return m_RemoveFormatsOlderThanDaysGlobal;
			}
		}
		internal NodeRole m_MyRole;
		internal bool IgnoreRemoveFormatsOlderThanDays
		{
			get { return m_IgnoreRemoveFormatsOlderThanDays.Value; }
		}

		public event OnTaskFinishedDel OnTaskFinished;

		internal MaterialFormatDBAccess.MaterialFormatDBAccess DBAccess
		{
			get { return m_DBAccess; }
		}

		internal RepDBAccess.RepDBAccess RepoDBAccess
		{
			get { return m_RepoDBAccess; }
		}

		internal Repository Repository
		{
			get { return m_Repo; }
		}


		public SNodeSubsystem()
			:base()
		{
			
			m_LastChecking4NewTasksDate = DateTime.MinValue;
			m_LastSyncWithFSCDate = DateTime.MinValue;
			m_LastValidateDate = DateTime.MinValue;
			m_DuringTaskOrdering = false;
			m_TasksInSystem = new HashSet<long>();
		}

		void m_TaskQueue_ReportErrorEvent(string message, Exception exc)
		{
			Log.TraceMessage(exc, "SNode", message);
		}

		void m_TaskQueue_ShowTextEvent(string message, System.Diagnostics.TraceEventType level)
		{
			Log.TraceMessage(level, "SNode", message);
		}


        public string GetId()
        {
            return Guid.NewGuid().ToString();
        }

        public override string GetName()
        {
            return "SNode";
        }

        public override void OnStart()
        {
			base.OnStart();

            string cnnString = this.LocalNode.GetGlobalParameter("MFDBConnection");
            if (string.IsNullOrEmpty(cnnString))
				throw new ApplicationException("Zła konfiguracja - brak connection string dla bazy MaterialFormat.");

            m_DBAccess = new MaterialFormatDBAccess.MaterialFormatDBAccess(cnnString);


            cnnString = this.LocalNode.GetGlobalParameter("RepoDBConnection");
            if (string.IsNullOrEmpty(cnnString))
                throw new ApplicationException("Zła konfiguracja - brak connection string dla bazy RepoDB.");

            m_RepoDBAccess = new RepDBAccess.RepDBAccess(cnnString, false);

			int nodeId = LocalNode.NodeIdAsInt;
			Node me = m_RepoDBAccess.GetNode(nodeId);
			m_MyRole = me.Role;

			if (m_MyRole == NodeRole.Snode || m_MyRole == NodeRole.SNodeRecoder)
			{
				m_myTaskTypes = new TaskTypeComplex[] { new TaskTypeComplex() { Type = TaskType.Download }, new TaskTypeComplex() { Type = TaskType.Remove }, new TaskTypeComplex() { Type = TaskType.SyncFormatMetadata } };
			}
			else if (m_MyRole == NodeRole.Recoder)
			{
				m_myTaskTypes = new TaskTypeComplex[] { new TaskTypeComplex() { Type = TaskType.Remove }, new TaskTypeComplex() { Type = TaskType.SyncFormatMetadata } };
			}
            

			long totalLoad;
			byte minFreeSpace;
			int repoSize;
			string repositoryPath = DBAccess.GetRepositoryPath(out repoSize, out totalLoad, out minFreeSpace);

			if (!string.IsNullOrEmpty(repositoryPath))
			{
				m_Repo = new Repository(repositoryPath, repoSize * 1024, minFreeSpace, DBAccess);
			}
			else
			{
				m_Repo = null;
				throw new ApplicationException("Niepoprawna konfiguracja repozytorium plikowego.");
			}

			m_Mime2FileExtensionMap = new Dictionary<string, string>();

			if (m_Repo != null)
			{
				Log.TraceMessage(System.Diagnostics.TraceEventType.Information, GetName(), string.Format("Repozytorium: ścieżka '{0}', rozmiar {1} MB, ClusterSize = {2}B, MinFreeSpace={3}%.", m_Repo.Path, m_Repo.Size, m_Repo.ClusterSize, m_Repo.MinFreeSpace));

				try
				{
					m_RepoDBAccess.SetNodeRepositorySpace(nodeId, m_Repo.Size, m_Repo.Size - (totalLoad / (1024L * 1024L)));//w MB

					string olderThenDaysStr = RepoDBAccess.GetGlobalData("MaxMaterialAge");
					if (!string.IsNullOrEmpty(olderThenDaysStr))
						m_RemoveFormatsOlderThanDaysGlobal = short.Parse(olderThenDaysStr);
				}
				catch (Exception ex)
				{
					Log.TraceMessage(ex, GetName(), string.Format("Błąd podczas powiadamiania RepDB o stanie repozytoriu plików. [{0}].", ex.Message));
				}

				try
				{
					Mime2Extension[] arr = m_RepoDBAccess.GetAllMime2FileExtension();
					if (arr != null)
					{ 
						foreach (var p in arr)
							m_Mime2FileExtensionMap[p.Mime] = p.FileExtension;
					}
				}
				catch (Exception ex)
				{
					Log.TraceMessage(ex, GetName(), string.Format("Błąd podczas pobrania mapowania typu MIME na rozszerzenie z RepDB. [{0}].", ex.Message));
				}


			}
			//else
			//{
			//	if (string.IsNullOrEmpty(m_Mime2FileExtension.Value))
			//	{
			//		string[] parts = m_Mime2FileExtension.Value.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
			//		foreach (string p in parts)
			//		{
			//			string[] inps = p.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			//			if (inps.Length > 1)
			//			{
			//				m_Mime2FileExtensionMap[inps[0]] = inps[1];
			//			}
			//		}
			//	}
			//}

			
			m_TaskQueue = new TaskQueueManager(1000, 10000, (uint)m_ExecutingThreadsCount.Value, "SNode", this);
			m_TaskQueue.CheckIfRunningTasksAreLate = true;
			m_TaskQueue.MaxTimeOfWorkingThreadExecution = m_MaxTimeOfWorkingThreadExecution.Value;
			m_TaskQueue.QueueCheckingInterval = 2000;
			m_TaskQueue.TaskWaitingForAnswerTimeout = InsertMaterialTaskRunningTimeout;

			m_TaskQueue.ShowTextEvent += m_TaskQueue_ShowTextEvent;
			m_TaskQueue.ReportErrorEvent += m_TaskQueue_ReportErrorEvent;
			m_TaskQueue.TaskFinished += m_TaskQueue_TaskFinished;
			m_TaskQueue.Start();
        }

		void m_TaskQueue_TaskFinished(BaseTask task)
		{
			if (task == null || !(task is BaseDemanTask))
				return;

			lock (m_TasksInSystem)
				m_TasksInSystem.Remove(((BaseDemanTask)task).RepoTaskId);

			if (OnTaskFinished != null)
			{
				if (task is InsertFormatTask)
					OnTaskFinished.BeginInvoke(task.ID, (ErrorType)task.CodeOfError, task.ErrorDesc, task.Result, task.AddInfo, ((BaseDemanTask)task).RepoTaskId, ((InsertFormatTask)task).RecoderTaskId, null, null);
				else
					OnTaskFinished.BeginInvoke(task.ID, (ErrorType)task.CodeOfError, task.ErrorDesc, task.Result, task.AddInfo, ((BaseDemanTask)task).RepoTaskId, -1, null, null);
			}
		}

        public override void OnStop()
        {
            base.OnStop();

            if (m_TaskQueue != null)
            {
                m_TaskQueue.StopWorkingNow();
                m_TaskQueue.Clear();
                m_TaskQueue.ShowTextEvent -= m_TaskQueue_ShowTextEvent;
                m_TaskQueue.ReportErrorEvent -= m_TaskQueue_ReportErrorEvent;
                m_TaskQueue = null;
            }
        }

        public override void OnTimer(long tick)
        {
			//Log.TraceMessage(System.Diagnostics.TraceEventType.Verbose, GetName(), "####  SNode - OnTimer #####");

            //base.OnTimer(tick);
			if (m_Checking4NewTasksInterval.Value > -1 && (DateTime.Now - m_LastChecking4NewTasksDate).TotalSeconds >= m_Checking4NewTasksInterval.Value)
			{
				Log.TraceMessage(System.Diagnostics.TraceEventType.Verbose, GetName(), "Okresowe pobranie zadań do wykonania.");
				GetAndOrderTasks(); // asynchronizm zapewnia mechanizm bazowy wołający OnTimer w osobnych zadaniach
				//(new System.Threading.Tasks.Task(delegate() { GetAndOrderTasks(); })).Start();				
			}

			if (m_SyncWithFSCInterval.Value > -1 && (DateTime.Now - m_LastSyncWithFSCDate).TotalSeconds >= m_SyncWithFSCInterval.Value)
			{
				string errorMsg;
				int errorId;
				Log.TraceMessage(System.Diagnostics.TraceEventType.Verbose, GetName(), "Okresowa synchronizacja z RepDB.");
				BaseTask bt = new SyncWithFSCTask(-1);
				m_TaskQueue.AddTask(bt, out errorMsg, out errorId);
                m_LastSyncWithFSCDate = DateTime.Now;
			}

			if (m_ValidateInterval.Value > -1 && (DateTime.Now - m_LastValidateDate).TotalSeconds >= m_ValidateInterval.Value)
			{
				string errorMsg;
				int errorId;
				Log.TraceMessage(System.Diagnostics.TraceEventType.Verbose, GetName(), "Okresowa walidacja lokalnego repozytorium.");
				BaseTask bt = new ValidateTask(-1);
				m_TaskQueue.AddTask(bt, out errorMsg, out errorId);
                m_LastValidateDate = DateTime.Now;
			}
        }

		public void CheckTasks()
		{
			Log.TraceMessage(System.Diagnostics.TraceEventType.Verbose, GetName(), "Wymuszone pobranie zadań do wykonania.");
			GetAndOrderTasks();
		}

		protected void GetAndOrderTasks()
		{
			try
			{
				lock (this)
				{
					if (m_DuringTaskOrdering)
						return;

					m_DuringTaskOrdering = true;
				}
				m_LastChecking4NewTasksDate = DateTime.Now;

				if (m_RepoDBAccess == null)
					throw new ApplicationException("Podsystem nie został poprawnie zainicjalizowany - brak dostępu do bazy RepoDB.");

				int numOfTasks2Get = Math.Min((int)m_TaskQueue.NumOfWorkingThreads - m_TaskQueue.NumberOfRunningTasks, m_MaxNewTasksCount.Value);
				if (numOfTasks2Get < 1)
					return;

				TaskShort[] tss = m_RepoDBAccess.GetTasks2Execute(LocalNode.NodeIdAsInt, m_myTaskTypes, numOfTasks2Get);

				if (tss != null && tss.Length > 0)
				{
					string overKey = DownloadKeywords.OverwriteIfExists.ToString();
					string preserveKey = DownloadKeywords.PreserveFileName.ToString();
					string forceKey = RemoveKeywords.ForceDlete.ToString();
					string mimeKey = DownloadKeywords.MimeType.ToString();
					string extKey = DownloadKeywords.FileExtension.ToString();
					string hashKey = DownloadKeywords.FormatMD5.ToString();
					string urlKey = DownloadKeywords.FormatURL.ToString();

					string errorMsg;
					int errorId;

					foreach (TaskShort ts in tss)
					{
						if (ts == null)
							continue;

						lock (m_TasksInSystem)
						{
							if (m_TasksInSystem.Contains(ts.Id))
								continue;
						}
						BaseTask bt = null;
						string mime = null;
						string fileExt = null;
						string fileHash = null;
						string downloadUrl = null;
						if (ts.Type == TaskType.Download)
						{
							bool overwrite = false;
							bool preserve = false;

							if (ts.Content != null)
							{
								if (ts.Content.ContainsKey(overKey))
									overwrite = bool.Parse(ts.Content[overKey]);

								if (ts.Content.ContainsKey(preserveKey))
									preserve = bool.Parse(ts.Content[preserveKey]);

								if (ts.Content.ContainsKey(mimeKey))
									mime = ts.Content[mimeKey];

								if (ts.Content.ContainsKey(extKey))
									fileExt = ts.Content[extKey];

								if (ts.Content.ContainsKey(hashKey))
									fileHash = ts.Content[hashKey];

								if (ts.Content.ContainsKey(urlKey))
									downloadUrl = ts.Content[urlKey];
							}

							if (string.IsNullOrEmpty(fileExt))
								fileExt = GetExtension4MimeType(mime);

							bt = new InsertFormatTask(ts.Id, ts.UniqueId, ts.PublicId, mime, fileExt, fileHash, downloadUrl, -1, preserve, overwrite);
						}
						else if (ts.Type == TaskType.Remove)
						{
							bool force = false;
							if (ts.Content.ContainsKey(forceKey))
								force = bool.Parse(ts.Content[forceKey]);

							bt = new RemoveFormatTask(ts.Id, ts.UniqueId, force);
						}
						else if (ts.Type == TaskType.SyncFormatMetadata)
						{
							bt = new SyncWithFSCTask(ts.Id);
						}
						else if (ts.Type == TaskType.ValidateRepository)
						{
							bt = new ValidateTask(ts.Id);
						}

						m_TaskQueue.AddTask(bt, out errorMsg, out errorId);


						if (errorId != (int)StandardErrors.Success)
						{
							Log.TraceMessage(System.Diagnostics.TraceEventType.Warning, "Nie udało sie dodanie nowego zadania dla zadania z Repo o Id=" + ts.Id.ToString());
						}
						else
						{
							lock (m_TasksInSystem)
							{
								m_TasksInSystem.Add(ts.Id);
							}

							Log.TraceMessage(System.Diagnostics.TraceEventType.Verbose, string.Format("Dodano do kolejki zadanie o Id={1} dla zadania z Repo o Id={0}.", ts.Id, bt.ID));
						}

						StringBuilder sbContent = new StringBuilder();
						if (ts.Content != null)
						{
							foreach (var de in ts.Content)
								sbContent.AppendFormat("{0}={1};", de.Key, de.Value ?? "NULL");
						}
						else
							sbContent.Append("NULL");

						Log.TraceMessage(System.Diagnostics.TraceEventType.Verbose, string.Format("Parametry zadania: typ={0}, UniqueId={1}, RepoTaskId={2}, PublicId={3}, Content={4}.", ts.Type, ts.UniqueId ?? "NULL", ts.Id, ts.PublicId ?? "NULL", sbContent.ToString()));
					}
				}
			}
			catch (Exception ex)
			{
				Log.TraceMessage(ex, "Błąd podczas pobierania nowych zadań do wykonania.");
			}
			finally
			{
				lock (this)
				{
					m_DuringTaskOrdering = false;
				}
				Log.TraceMessage(System.Diagnostics.TraceEventType.Verbose, GetName(), "Zakończono okresowe pobieranie zadań do wykonania.");
			}
		}

		internal FormatMetadata TranslateMetadata(Format format, string mime)
		{
			FormatMetadata fm = new FormatMetadata()
			{
				AllowDistribution = format.AllowDistribution,
				Id = format.Id,
				Size = format.Size,
				Status = MaterialFormatDBAccess.FormatStatus.Full,
				UniqueId = format.UniqueId,
				Mime = format.Mime ?? mime,
                RealSize = format.Size
			};
			return fm;
		}

		public ErrorType OrderInsertFormatTask(long orderingTaskId, string uniqueId, string publicId, string mime, string fileExt, string fileHash, string downloadUrl, out long taskId)
		{
			taskId = -1;
			try
			{
				string errorMsg;
				int errorId;

				InsertFormatTask bt = new InsertFormatTask(-1, uniqueId, publicId, mime, fileExt, fileHash, downloadUrl, orderingTaskId);

				m_TaskQueue.AddTask(bt, out errorMsg, out errorId);

				if (errorId != (int)StandardErrors.Success)
				{
					Log.TraceMessage(System.Diagnostics.TraceEventType.Warning, string.Format("Nie udało się dodanie nowego zadania wstawienia formatu o UId={0} do repozytorium - zlecenie od zadania rekodera o Id={1}.", uniqueId, orderingTaskId));
					return (ErrorType)errorId;
				}
				else
				{
					Log.TraceMessage(System.Diagnostics.TraceEventType.Verbose, string.Format("Dodano do kolejki zadanie wstawienia formatu '{1}' do repozytorium - zlecenie od zadania rekodera o Id={2}. Id={0}.", bt.ID, uniqueId, orderingTaskId));

					taskId = bt.ID;

					return ErrorType.Success;
				}
			}
			catch (Exception ex)
			{
				Log.TraceMessage(ex, "Błąd podczas dodawania nowego zadania wstawienia formatu do repozytorium.");
			}
			return ErrorType.Runtime;
		}

		internal string GetMD5Hashtable(FileStream fs)
		{
			// Create a new instance of the MD5CryptoServiceProvider object.
			System.Security.Cryptography.MD5 md5Hasher = System.Security.Cryptography.MD5.Create();

			// Compute the hash.
			fs.Seek(0, SeekOrigin.Begin);
			byte[] data = md5Hasher.ComputeHash(fs);

			// Create a new Stringbuilder to collect the bytes
			// and create a string.
			StringBuilder sBuilder = new StringBuilder();

			// Loop through each byte of the hashed data 
			// and format each one as a hexadecimal string.
			for (int i = 0; i < data.Length; i++)
			{
				sBuilder.Append(data[i].ToString("x2"));
			}

			// Return the hexadecimal string.
			return sBuilder.ToString();
		}

        [HandleProcessCorruptedStateExceptions]
		// uzyskuje xml z opisem formatu tworzony na podstawie pliku
		public string GetFormatXmlForFile(string publicId, string location, string defaultAccessChannel, out ulong duration, out string mime)
		{
			duration = 0;
			mime = null;
            try
            {
			using (MediaParser mediaParser = new MediaParser(location))
			{
				IMediaParserInstance _mparser = mediaParser.Parse();
				string formatXml;

				if (_mparser != null && _mparser.FileFormat != MediaParser.FileFormat.UNKNOWN)
				{
					mime = _mparser.MimeType;
					PSNC.Multimedia.Tools.DictionaryEx<string, object> dict = new PSNC.Multimedia.Tools.DictionaryEx<string, object>();

					dict.Add(PSNC.Multimedia.Tools.XmlTools.FileName, Path.GetFileName(location));
					dict.Add(PSNC.Multimedia.Tools.XmlTools.FormatIdentifier, publicId);
					if (defaultAccessChannel != null)
						dict.Add(PSNC.Multimedia.Tools.XmlTools.AccessChannel, defaultAccessChannel);

					dict.Add(PSNC.Multimedia.Tools.XmlTools.DistributionType, "file or stream");

					formatXml = _mparser.ToXML(dict);
					duration = _mparser.Duration;
					return formatXml;
				}
                }
			}
            catch(Exception ex)
            {
                Log.TraceMessage(ex, "Błąd podczas parsowania pliku: " + location);
            }
			return null;
		}

		internal bool RemoveFormat(string uniqueId, bool forceDelete, out string errorDesc)
		{
			bool res = true;
			errorDesc = String.Empty;
			Log.TraceMessage(System.Diagnostics.TraceEventType.Information, GetName(), string.Format("Usunięcie formatu o UniqueId={0}.", uniqueId));
			
			FormatMetadata fm = DBAccess.GetFormat(uniqueId);
			if (fm != null)
			{
				string fullMaterialLocation = System.IO.Path.Combine(Repository.Path, fm.Location);

				DBAccess.RemoveFormat(uniqueId);

				try
				{
					System.IO.File.Delete(fullMaterialLocation);
				}
				catch
				{
					if (forceDelete)
					{
						errorDesc = string.Format("Nie udało się usunąć pliku '{0}' odpowiadającego formatowi o UniqueId={1} przy ustawione fladze ForceDelete.", fullMaterialLocation, uniqueId);
						Log.TraceMessage(System.Diagnostics.TraceEventType.Error, GetName(), errorDesc);
						res = false;
					}
				}
				if (res)
				{
					long freeSpace = Repository.GetRepositoryFreeSpace();
					RepoDBAccess.RemoveFormatLocation(uniqueId, LocalNode.NodeIdAsInt, freeSpace);
				}
			}
			else
			{
				Log.TraceMessage(System.Diagnostics.TraceEventType.Warning, GetName(), string.Format("Formatu o UniqueId={0} nie znajduje się w repozytorium - usunięcie niemożliwe.", uniqueId));
			}

			return res;
		}

		public string GetFormatLocation(string uniqueId)
		{
			FormatMetadata fm = DBAccess.GetFormat(uniqueId);
			if (fm != null)
				return System.IO.Path.Combine(Repository.Path, fm.Location);

			return null;
		}

        public bool MimeAreEquivalent(string mime1, string mime2)
        {
            if (m_Mime2FileExtensionMap.ContainsKey(mime1) && m_Mime2FileExtensionMap.ContainsKey(mime2))
                return m_Mime2FileExtensionMap[mime1] == m_Mime2FileExtensionMap[mime2];
            else
                if (!m_Mime2FileExtensionMap.ContainsKey(mime1) && !m_Mime2FileExtensionMap.ContainsKey(mime2))
                    return mime1 == mime2;
                else
                    return false;
        }

		public string GetExtension4MimeType(string mime)
		{
			if (string.IsNullOrEmpty(mime))
				return m_Mime2FileExtensionMap["*"];

			if (m_Mime2FileExtensionMap.ContainsKey(mime))
				return m_Mime2FileExtensionMap[mime];
			else
				return m_Mime2FileExtensionMap["*"];
		}

		public bool IsTaskStillRunning(long taskId, out bool isKnown)
		{
			isKnown = false;

			TaskStateInfo info = m_TaskQueue.GetCurrentTaskState(taskId);
			if (info != null)
			{
				isKnown = true;
				return !info.IsFinished;
			}
			return false;
		}
    }
}
