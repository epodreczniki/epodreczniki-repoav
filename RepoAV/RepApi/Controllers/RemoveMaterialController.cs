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
    /// <summary>
    /// 
    /// </summary>
    public class RemoveMaterialController : ApiController
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="materialId"></param>
        public void Delete(string id)
        {
            bool res = true;
            string cnnString = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            RepDBAccess.RepDBAccess db = new RepDBAccess.RepDBAccess(cnnString, false);

            try
            {
                Log.TraceMessage("RemoveMaterial dla materiału " + id);


                TaskAdd task = new TaskAdd();
                task.PublicId = id;
                task.Type = TaskType.RemoveMaterial;


                task.Content.Add(RemoveKeywords.ForceDlete.ToString(), "true");

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
