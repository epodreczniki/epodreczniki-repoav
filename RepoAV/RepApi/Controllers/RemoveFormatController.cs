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
using PSNC.RepoAV.Services.RepApi.Models;

namespace PSNC.RepoAV.Services.RepApi.Controllers
{
    public class RemoveFormatController : ApiController
    {
        [HttpDelete]
        public HttpResponseMessage Delete([FromBody]SetFormatReq rmReq)
        {
            bool res = true;
            string cnnString = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            RepDBAccess.RepDBAccess db = new RepDBAccess.RepDBAccess(cnnString, false);

            try
            {
                Log.TraceMessage(string.Format("RemoveFormat dla formatu typu '{0}' materiału '{1}'", rmReq.formatType, rmReq.materialId));


                TaskAdd task = new TaskAdd();
                task.PublicId = rmReq.materialId;
                task.UniqueId = string.Format("{0}(,,{1})", task.PublicId, rmReq.formatType);
                task.Type = TaskType.RemoveFormat;


                task.Content.Add(RemoveKeywords.ForceDlete.ToString(), "true");

                res = db.AddTask(task);

            }
            catch (DBAccess.DBAccessException ex)
            {
                Log.TraceMessage(ex, "RemoveFormat");
                switch (ex.Error)
                {
                    case ErrorType.MaterialNotFound:                       
                    case ErrorType.FormatNotFound:
                        throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message));
                    case ErrorType.AlreadyExists:
                        throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Conflict, ex.Message));
                    default:
                        throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
                }
            }
            catch (Exception ex)
            {
                Log.TraceMessage(ex, "RemoveFormat");
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }

            if (res == false)
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Błąd dodania zadania"));

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
