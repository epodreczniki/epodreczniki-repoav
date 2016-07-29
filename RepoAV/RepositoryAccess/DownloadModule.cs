using System;
using System.Web;
using System.Net;
using System.Diagnostics;
using System.Web.Configuration;
using PSNC.Util;
using PSNC.RepoAV.Services.RepositoryAccess.Handlers;

namespace PSNC.RepoAV.Services.RepositoryAccess
{
    public class DownloadModule : IHttpModule
    {
        private string m_MaterialFormatDbConnectionString;
        private string m_RepoDbConnectionString;
		private string m_RepoVirtualPath;

        private RepositoryConfiguration m_RepositoryConfiguration;
        private NotFoundHandler m_NotFoundHandler;
        private PassRequestHandler m_PassRequestHandler = new PassRequestHandler();
        private HealthTestHandler m_HealthTestHandler = new HealthTestHandler();

        /// <summary>
        /// You will need to configure this module in the Web.config file of your
        /// web and register it with IIS before being able to use it. For more information
        /// see the following link: http://go.microsoft.com/?linkid=8101007
        /// </summary>
        #region IHttpModule Members

        public void Dispose()
        {
            //clean-up code here.
        }

        public void Init(HttpApplication context)
        {
            m_RepositoryConfiguration = new RepositoryConfiguration();
            m_NotFoundHandler = new NotFoundHandler();

            context.LogRequest += new EventHandler(OnLogRequest);
            context.BeginRequest += new EventHandler(context_BeginRequest);



            try
            {
                m_MaterialFormatDbConnectionString = WebConfigurationManager.ConnectionStrings["ContentDB"].ConnectionString;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                m_MaterialFormatDbConnectionString = string.Empty;
            }

            try
            {
                m_RepoDbConnectionString = WebConfigurationManager.ConnectionStrings["RepoDB"].ConnectionString;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                m_RepoDbConnectionString = string.Empty;
            }

			try
			{
				PSNC.RepoAV.MaterialFormatDBAccess.MaterialFormatDBAccess dbAccess = new PSNC.RepoAV.MaterialFormatDBAccess.MaterialFormatDBAccess(m_MaterialFormatDbConnectionString);
				m_RepoVirtualPath = dbAccess.GetGlobalData("RepositoryVirtualDir");
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
				m_RepoVirtualPath = string.Empty;
			}
        }


        #endregion


        void context_BeginRequest(object sender, EventArgs e)
        {
            HttpApplication app = sender as HttpApplication;
            HttpContext context = app.Context;

            try
            {
                string processingHint = string.Empty;
                string formatId = Handler.GetUrlInfo(context.Request, out processingHint);
                if (string.IsNullOrEmpty(formatId))
                {
                    Log.TraceMessage(TraceEventType.Verbose, "Nie znaleziono formatu w urlu '" + context.Request.RawUrl + "'.");
                    context.Response.StatusCode = 404;
                    app.CompleteRequest();
                }
                else
                {
					if (string.Compare(formatId, m_RepoVirtualPath, true) == 0)// jest to juz przekierowanie, a nie pierwsze żądanie
						return;

                    string formatIdWithoutChecksum = string.Empty;
                    if (CheckSum.IsValid(false, formatId, out formatIdWithoutChecksum))
                    {
                        formatId = formatIdWithoutChecksum;
                        Handler first = CreateHandlingChain(context, processingHint);

                        // wstawianie naglowka jest opisane tutaj
                        //http://www.iis.net/learn/extensions/url-rewrite-module/url-rewrite-module-20-configuration-reference#Setting_Server_Variables
                        RequestContext rc = new RequestContext();
                        rc.PublicRequest = !string.IsNullOrEmpty(m_RepositoryConfiguration.PublicRequestHeader) // i nie jest to zadanie przez farme
                                            && !string.IsNullOrEmpty(context.Request.Headers[m_RepositoryConfiguration.PublicRequestHeader]); // bo farma wstawia nagłowek
                        rc.HttpContext = context;
                        rc.FormatId = formatId;

                        first.HandleRequest(rc);
                        rc.SaveLog();
                    }
                    else
                    {
                        Log.TraceMessage(TraceEventType.Verbose, string.Format("Suma kontrolna błędna dla url '" + context.Request.RawUrl + "' oraz formatu '" + formatId + "', zakończono {0}.", (int)HttpStatusCode.Forbidden));
                        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        app.CompleteRequest();
                    }
                }
                
            }
            catch (Exception ex)
            {
                Log.TraceMessage(ex, string.Format("Błąd w BeginRequest dla url {1} zakończono {0}.", (int)HttpStatusCode.InternalServerError, context.Request.RawUrl));
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                app.CompleteRequest();
            }
        }

        private Handler CreateHandlingChain(HttpContext context, string processingHint)
        {
            Handler predecessor = null;
            Handler first = null;

            switch (processingHint)
            {
                case Handler.ProcessingKeyHealthTest:
                    first = m_HealthTestHandler;
                    predecessor = first;
                    break;
                case Handler.ProcessingKeyMetadata:
                    if (!string.IsNullOrEmpty(m_RepoDbConnectionString))
                    {
                        predecessor = new MetadataHandler(m_RepoDbConnectionString);
                        predecessor.SetSuccessor(m_NotFoundHandler);
                        first = predecessor;
                    }
                    break;
                default:
                    if (!string.IsNullOrEmpty(m_MaterialFormatDbConnectionString))
                    {
                        MaterialFormatDbHandler meta = new MaterialFormatDbHandler(m_MaterialFormatDbConnectionString);
                        if (predecessor != null)
                        {
                            predecessor.SetSuccessor(meta);
                        }
                        predecessor = meta;
                        if (first == null)
                        {
                            first = meta;
                        }
                    }
                    else
                    {
                        Log.TraceMessage(TraceEventType.Error, "Żądanie do katalogu MaterialFormatDB, connection string do bazy nie jest podany.");
                    }

                    if (m_RepositoryConfiguration.Enabled)
                    {
                        RedirectHandler redirect = new RedirectHandler(m_RepositoryConfiguration);
                        if (predecessor != null)
                        {
                            predecessor.SetSuccessor(redirect);
                        }
                        predecessor = redirect;
                        if (first == null)
                        {
                            first = redirect;
                        }
                    }

                    if (string.IsNullOrEmpty(processingHint))
                    {
                        if (predecessor == null)
                        {
                            predecessor = m_NotFoundHandler;
                        }
                        else
                        {
                            predecessor.SetSuccessor(m_NotFoundHandler);
                        }
                    }
                    else
                    {
                        if (predecessor == null)
                        {
                            predecessor = m_PassRequestHandler;
                        }
                        else
                        {
                            predecessor.SetSuccessor(m_PassRequestHandler);
                        }
                    }

                    break;
            }

            if (first == null)
            {
                first = m_PassRequestHandler;
            }

            return first;
        }

        public void OnLogRequest(Object source, EventArgs e)
        {
            Log.TraceMessage("Request: " + ((HttpApplication)source).Request.Url);
        }
    }
}
