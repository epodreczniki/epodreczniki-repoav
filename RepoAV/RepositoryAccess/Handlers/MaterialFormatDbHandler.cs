using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.IO;
using System.Collections.Specialized;
using System.Web.Hosting;
using System.Net;
using PSNC.Util;
using PSNC.RepoAV.MaterialFormatDBAccess;
using PSNC.RepoAV.Services.RepositoryAccess.Cache;

namespace PSNC.RepoAV.Services.RepositoryAccess.Handlers
{
    public class MaterialFormatDbHandler : Handler
    {
        public MaterialFormatDbHandler(string materialFormatDbConnectionString)
        {
            m_MaterialFormatDbConnectionString = materialFormatDbConnectionString;
        }

        public override void HandleRequest(RequestContext context)
        {
            string newUrl = string.Empty;

            FormatAccess formatAccess = null;
            try
            {
                formatAccess = GetMaterialInfo(context.FormatId);
            }
            catch (Exception ex)
            {
                Log.TraceMessage(ex, string.Format("Błąd podczas obsługi żądania. Oryginalny url:'{0}'", context.HttpContext.Request.Path));
            }

            if (formatAccess == null)
            {
                context.AddLog("Nie ma w MaterialFormatDB formatu '{0}'.", context.FormatId);
                m_Successor.HandleRequest(context);
            }
            else
            {
                if (formatAccess.AllowDistribution || !context.PublicRequest)
                {
                    context.HttpContext.Response.AddHeader("Access-Control-Allow-Origin", "*");

                    if (IsUrlForDownload(context.HttpContext.Request))
                    {
                        AddContentDisposition(Path.GetFileName(formatAccess.Location), context.HttpContext.Response);

                        newUrl = CreateRewriteUrl(formatAccess.VirtualDir, formatAccess.Location);
                        context.AddLog("Jest w MaterialFormatDB; rewrite (pobierz) na lokalny plik; url '{0}' na '{1}'.", context.HttpContext.Request.Path, newUrl);
                        Log.TraceMessage(string.Format("Jest w MaterialFormatDB; rewrite (pobierz) na lokalny plik; url '{0}' na '{1}'.", context.HttpContext.Request.Path, newUrl));
                        context.HttpContext.RewritePath(newUrl);
                    }
                    else
                    {
                        newUrl = CreateRewriteUrl(formatAccess.VirtualDir, formatAccess.Location);
                        context.AddLog("Jest w MaterialFormatDB; rewrite (graj) na lokalny plik; url '{0}' na '{1}'.", context.HttpContext.Request.Path, newUrl);
                        Log.TraceMessage(string.Format("Jest w MaterialFormatDB; rewrite (graj) na lokalny plik; url '{0}' na '{1}'.", context.HttpContext.Request.Path, newUrl));
                        context.HttpContext.RewritePath(newUrl);
                    }
                }
                else
                {
                    context.AddLog("Materiał '{0}' nie jest dopuszczony do dystrybucji.", context.FormatId);
                    context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    context.HttpContext.ApplicationInstance.CompleteRequest();
                }
            }
        }

        private FormatAccess GetMaterialInfo(string materialId)
        {
            FormatAccess data = MaterialFormatCache.Instance.GetMaterialFormatInfo(materialId);
            if (data != null)
            {
                Log.TraceMessage(string.Format("Info o formacie wydane z cache'u."));
            }
            else
            {
                PSNC.RepoAV.MaterialFormatDBAccess.MaterialFormatDBAccess dbAccess = new PSNC.RepoAV.MaterialFormatDBAccess.MaterialFormatDBAccess(m_MaterialFormatDbConnectionString);
                data = dbAccess.GetFormatAccess(materialId);
                if (data != null) // dodanie do lokalnego cache'a
                {
                    MaterialFormatCache.Instance.SetMaterialFormatInfo(data);
                }
                else
                {
                    Log.TraceMessage(string.Format("Materiał '{0}' jest, ale jeszcze nie gotowy do wydania, lub go nie ma.", materialId));
                }
            }

            return data;
        }

        private string m_MaterialFormatDbConnectionString;
    }
}