using System;
using System.Collections.Generic;
using System.Xml;
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
    public class AddMaterialController : ApiController
    {
        //
        // POST: /AddMaterial/

        public IHttpActionResult Post([FromBody]SetMaterialReq addReq)
        {
            bool res = true;
            string cnnString = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            RepDBAccess.RepDBAccess db = new RepDBAccess.RepDBAccess(cnnString, false);

            try
            {
                Log.TraceMessage("AddMaterial dla materiału " + addReq.materialId);


                if (string.IsNullOrEmpty(addReq.materialId))
                    return BadRequest("Nie podano parametru materialId");

                XmlDocument xml = new XmlDocument();


                if (!string.IsNullOrEmpty(addReq.metadata))
                {
                    try
                    {
                        xml.LoadXml(addReq.metadata);
                    }
                    catch (XmlException xex)
                    {
                        return BadRequest("Niepoprawny XML dla parametru metadata");
                    }
                }

                TaskAdd task = new TaskAdd();
                task.PublicId = addReq.materialId;
                task.Type = TaskType.UpdateMaterial;
                

                task.Content.Add(AddKeywords.MimeType.ToString(), addReq.mime);
                task.Content.Add(AddKeywords.FormatURL.ToString(), addReq.materialURL);
                if(!string.IsNullOrEmpty(addReq.materialMD5))
                    task.Content.Add(AddKeywords.FormatMD5.ToString(), addReq.materialMD5);
                if (!string.IsNullOrEmpty(addReq.metadata))
                    task.Content.Add(AddKeywords.Metadata.ToString(), addReq.metadata);
                if (!string.IsNullOrEmpty(addReq.userId))
                    task.Content.Add(AddKeywords.User.ToString(), addReq.userId);

                res = db.AddTask(task);

            }
            catch(Exception ex)
            {
                Log.TraceMessage(ex, "AddMaterial");
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }

            if(res == false)
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Błąd dodania zadania"));
         
             return Ok(addReq.materialId);
    
        }

    }
}
