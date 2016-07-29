using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PSNC.RepoAV.Services.RepApi.Models;
using System.Web.Configuration;
using PSNC.RepoAV.RepDBAccess;
using PSNC.RepoAV.Common;
using PSNC.Util;

namespace PSNC.RepoAV.Services.RepApi.Controllers
{
    public class UpdateFormatController : ApiController
    {
        public IHttpActionResult Put([FromBody]SetFormatReq addReq)
        {
            bool res = true;
            string cnnString = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            RepDBAccess.RepDBAccess db = new RepDBAccess.RepDBAccess(cnnString, false);

            try
            {
                Log.TraceMessage("UpdateFormat dla materiału " + addReq.materialId);

                TaskAdd task = new TaskAdd();
                task.PublicId = addReq.materialId;
                task.UniqueId = string.Format("{0}(,,{1})", task.PublicId, addReq.formatType);
                task.Type = TaskType.UpdateFormat;


                task.Content.Add(AddKeywords.FormatType.ToString(), addReq.formatType);
                task.Content.Add(AddKeywords.MimeType.ToString(), addReq.mime);
                task.Content.Add(AddKeywords.FormatURL.ToString(), addReq.formatURL);
                if (!string.IsNullOrEmpty(addReq.formatMD5))
                    task.Content.Add(AddKeywords.FormatMD5.ToString(), addReq.formatMD5);
                if (!string.IsNullOrEmpty(addReq.userId))
                    task.Content.Add(AddKeywords.User.ToString(), addReq.userId);

                res = db.AddTask(task);

            }
            catch (DBAccess.DBAccessException ex)
            {
                Log.TraceMessage(ex, "UpdateFormat");
                switch (ex.Error)
                {
                    case ErrorType.MaterialNotFound:
                        throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message));
                    case ErrorType.AlreadyExists:
                        throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Conflict, ex.Message));
                    default:
                        throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
                }
            }
            catch (Exception ex)
            {
                Log.TraceMessage(ex, "UpdateFormat");
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }

            if (res == false)
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Błąd dodania zadania"));

            return Ok(string.Format("{0}(,,{1})", addReq.materialId, addReq.formatType));
        }
    }
}
