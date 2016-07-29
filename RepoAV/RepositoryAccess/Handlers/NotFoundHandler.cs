using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;

namespace PSNC.RepoAV.Services.RepositoryAccess.Handlers
{
    public class NotFoundHandler : Handler
    {
        public override void HandleRequest(RequestContext context)
        {
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
            context.AddLog("Nie znaleziono materiału '{0}' i zakończono {1}.", context.FormatId, context.HttpContext.Response.StatusCode.ToString());
            context.HttpContext.ApplicationInstance.CompleteRequest();
        }
    }
}