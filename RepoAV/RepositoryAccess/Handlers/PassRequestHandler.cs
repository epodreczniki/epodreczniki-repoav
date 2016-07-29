using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PSNC.RepoAV.Services.RepositoryAccess.Handlers
{
    public class PassRequestHandler : Handler
    {
        public override void HandleRequest(RequestContext context)
        {
            context.AddLog("Przepuszczam żądanie dla id '{0}' bez zmian.", context.FormatId);
        }
    }
}