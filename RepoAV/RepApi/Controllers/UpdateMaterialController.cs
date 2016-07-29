using System;
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
    /// <summary>
    /// 
    /// </summary>
    public class UpdateMaterialController : ApiController
    {
        //
        // GET: /UpdateMaterial/

        /// <summary>
        /// 
        /// </summary>
        public void Put([FromBody]SetMaterialReq addReq)
        {
            bool res = true;
            string cnnString = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            RepDBAccess.RepDBAccess db = new RepDBAccess.RepDBAccess(cnnString, false);

            try
            {
                Log.TraceMessage("UpdateMaterial dla materiału " + addReq.materialId);

                TaskAdd task = new TaskAdd();
                task.PublicId = addReq.materialId;
                task.Type = TaskType.UpdateMaterial;


                task.Content.Add(AddKeywords.MimeType.ToString(), addReq.mime);
                task.Content.Add(AddKeywords.FormatURL.ToString(), addReq.materialURL);             
                if (!string.IsNullOrEmpty(addReq.materialMD5))
                    task.Content.Add(AddKeywords.FormatMD5.ToString(), addReq.materialMD5);
                if (!string.IsNullOrEmpty(addReq.metadata))
                    task.Content.Add(AddKeywords.Metadata.ToString(), addReq.metadata);
                if (!string.IsNullOrEmpty(addReq.userId))
                    task.Content.Add(AddKeywords.User.ToString(), addReq.userId);

                res = db.AddTask(task);

            }
            catch (Exception ex)
            {
                Log.TraceMessage(ex, "RemoveMaterial");
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }

            if (res == false)
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Błąd dodania zadania"));
        }

    }
}
