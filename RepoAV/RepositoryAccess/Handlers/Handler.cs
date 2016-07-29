using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Hosting;
using System.Diagnostics;
using System.Web.Configuration;
using PSNC.Util;
using PSNC.RepoAV.Services.RepositoryAccess;

namespace PSNC.RepoAV.Services.RepositoryAccess.Handlers
{
    public abstract class Handler
    {
        public void SetSuccessor(Handler successor)
        {
            this.m_Successor = successor;
        }

        public abstract void HandleRequest(RequestContext context);

        public static string GetUrlInfo(HttpRequest request, out string processingHint)
        {
            string path = request.Path;
            if(path.StartsWith(request.ApplicationPath))
            {
                path = path.Substring(request.ApplicationPath.Length);
            }
            processingHint = string.Empty;
            string resourceIdentifier = string.Empty;// format id, file name

            string[] parts = path.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length > 0)
            {
                if ((string.Equals(parts[0], ProcessingKeyMetadata, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(parts[0], ProcessingKeyHealthTest, StringComparison.OrdinalIgnoreCase))
                    && parts.Length > 1)
                {
                    processingHint = parts[0].ToLower();
                    resourceIdentifier = parts[1];
                }
                else
                {
                    resourceIdentifier = parts[0];
                }
            }
            else
            {
                Log.TraceMessage(TraceEventType.Verbose, "Błędnie zbudowany URL '" + request.RawUrl + "'. Nie można pobrać id formatu z URL.");
            }

            return resourceIdentifier;
        }

        public static bool IsUrlForDownload(HttpRequest request)
        {
            return string.Equals(Boolean.FalseString, request.QueryString[QueryStringParamPlay], StringComparison.InvariantCultureIgnoreCase);
        }

        protected static void AddContentDisposition(string name, HttpResponse response)
        {
            response.AddHeader("content-disposition", "attachment;filename=" + name + ";");
        }

 

        protected static void SpeciallyTransmitIsmFile(RequestContext context, string fullPath)
        {
            FileInfo fi = new FileInfo(fullPath);
            if (fi.Exists)
            {
                context.HttpContext.Response.ContentType = "text/xml";
                context.HttpContext.Response.TransmitFile(fi.FullName, 0, fi.Length);
            }
        }

        protected static string CreateRewriteUrl(string virtualDirectory, string filePath)
        {
            string newUrl = HostingEnvironment.ApplicationVirtualPath + "/" + virtualDirectory + "/" + filePath.Replace('\\', '/');

            return newUrl;
        }

        protected Handler m_Successor;


        public const string QueryStringParamPlay = "play";
        public const string ProcessingKeyHealthTest = "health-test";
        public const string ProcessingKeyMetadata = "meta";

        public static char[] splitChars = new char[] { '/', '?' };
    }
}