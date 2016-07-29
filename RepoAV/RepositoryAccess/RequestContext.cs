using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Diagnostics;
using PSNC.Util;

namespace PSNC.RepoAV.Services.RepositoryAccess
{
    public class RequestContext
    {
        public RequestContext()
        {
            m_Stopwatch.Start();
        }

        public bool PublicRequest { get; set; }

        public HttpContext HttpContext { get; set; }

        public string FormatId { get; set; }

        public void AddLog(string log, params string[] param)
        {
            m_Log.AppendFormat(log, param);
            m_Log.AppendLine();
        }

        public void SaveLog()
        {
            if (m_Log.Length > 0)
            {
                m_Stopwatch.Stop();
                m_Log.AppendFormat("Czas obsługi {0:00}:{1:00}:{2:000} ms", m_Stopwatch.Elapsed.Minutes, m_Stopwatch.Elapsed.Seconds, m_Stopwatch.Elapsed.Milliseconds);
                Log.TraceMessage(TraceEventType.Verbose, m_Log.ToString());
            }
        }

        private StringBuilder m_Log = new StringBuilder(50);
        private Stopwatch m_Stopwatch = new Stopwatch();
    }
}