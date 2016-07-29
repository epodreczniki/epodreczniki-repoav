using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PSNC.RepoAV.Services.RepositoryAccess.Handlers
{
    public class HealthTestHandler : Handler
    {
        public override void HandleRequest(RequestContext context)
        {
            m_testsCounter++;

            if (m_LatestLog == DateTime.MinValue || m_LatestLog.AddMinutes(20) < DateTime.Now)
            {
                context.AddLog("Test dostępności; log co 20 min; liczba testów {0}.", m_testsCounter.ToString());
                m_LatestLog = DateTime.Now;
                m_testsCounter = 0;
            }

            context.HttpContext.Response.ContentType = RespMime;
            context.HttpContext.Response.Output.Write("healthy");
            context.HttpContext.Response.StatusCode = 200;
            context.HttpContext.ApplicationInstance.CompleteRequest();

        }

        private static DateTime m_LatestLog = DateTime.MinValue;
        private static int m_testsCounter;
        private const string RespMime = "text/html";
    }
}