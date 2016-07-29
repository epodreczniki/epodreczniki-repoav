using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections.Specialized;
using System.Web.Configuration;
using System.Net;

namespace PSNC.RepoAV.Services.RepositoryAccess.Handlers
{
    public class RedirectHandler : Handler
    {
        public RedirectHandler(RepositoryConfiguration repositoryConfiguration)
        {
            m_RepositoryConfiguration = repositoryConfiguration;
        }

        public override void HandleRequest(RequestContext context)
        {
            string newUrl = string.Empty;

            UriBuilder ub = new UriBuilder(context.HttpContext.Request.Url);
            NameValueCollection queryString = HttpUtility.ParseQueryString(ub.Query);
            string visitedNodesWithMe = string.Empty;
            RepositoryNode nextNode = m_RepositoryConfiguration.GetNextRepoNode(queryString[QueryStringParamVisitedNodes], out visitedNodesWithMe);

            if (nextNode != null)
            {
                if (!string.IsNullOrEmpty(nextNode.Address) && !context.PublicRequest)
                {
                    ub.Host = nextNode.Address;
                }

                queryString[QueryStringParamVisitedNodes] = visitedNodesWithMe;
                queryString[QueryStringParamNextNode] = nextNode.Id;
                ub.Query = queryString.ToString();
                newUrl = ub.ToString();
                context.AddLog("Przekieruje do partnera na adres '{0}'.", newUrl);
                context.HttpContext.Response.Redirect(newUrl, false);
                context.HttpContext.ApplicationInstance.CompleteRequest();
            }
            else
            {
                context.AddLog("Partner '{0}' nie ma materiału '{1}', ale i ja go nie mam.", queryString[QueryStringParamVisitedNodes], context.FormatId);
                m_Successor.HandleRequest(context);
            }
        }

        private static string QueryStringParamVisitedNodes = "vn";
        private static string QueryStringParamNextNode = "sn";
        private RepositoryConfiguration m_RepositoryConfiguration;
    }
}