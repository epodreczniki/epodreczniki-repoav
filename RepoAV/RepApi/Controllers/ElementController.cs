using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Configuration;
using PSNC.RepoAV.RepDBAccess;
using PSNC.RepoAV.Common;
using PSNC.Util;

namespace PSNC.RepoAV.Services.RepApi.Controllers
{
    public class ElementController : ApiController
    {
        [ActionName("Get")]
        public HttpResponseMessage Get(string id)
         {
             if (string.IsNullOrEmpty(id))
                 throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Nie podano identyfikatora materiału"));


             string cnnString = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
             RepDBAccess.RepDBAccess db = new RepDBAccess.RepDBAccess(cnnString, false);


             try
             {
                 Log.TraceMessage("Element/Get dla materiału " + id);

                 string url = db.GetGlobalData("RepositoryAccessNLB");

                 if (url != null)
                     url += id;

                 var response = Request.CreateResponse(HttpStatusCode.Moved);
                 response.Headers.Location = new Uri(url);
                 return response;

             }
             catch (Exception ex)
             {
                 Log.TraceMessage(ex, "Element/Get");
                 throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
             }

              
         }


         [HttpGet]
         [ActionName("Metadata")]
         public string Metadata(string id)
         {
             if(string.IsNullOrEmpty(id))
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Nie podano identyfikatora materiału"));


             string xml = "";
             string cnnString = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
             RepDBAccess.RepDBAccess db = new RepDBAccess.RepDBAccess(cnnString, false);




             try
             {
                 Log.TraceMessage("Element/Metadata dla materiału " + id);

                 Material material = db.GetMaterial(id);
                 if (material == null)
                     throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, "Nie znaleziono materiału"));


                 xml = material.Metadata.Value;
             }
             catch (Exception ex)
             {
                 Log.TraceMessage(ex, "Element/Metadata");
                 throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
             }

             return xml;
         }
    }
}
