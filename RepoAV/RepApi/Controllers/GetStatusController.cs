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
    /// <summary>
    /// 
    /// </summary>
    public class GetStatusController : ApiController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TransactionStatusInfo Get(string id)
        {
            bool res = true;
            string cnnString = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            RepDBAccess.RepDBAccess db = new RepDBAccess.RepDBAccess(cnnString, false);
            MaterialStatus status;
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    Log.TraceMessage("GetStatus: brak parametrów wywołania / błąd parsowania");
                    Helper.ThrowResponseException(this.ControllerContext.Request, HttpStatusCode.BadRequest, "GetStatus: no input parameters or parse error (5)");
                }

                Log.TraceMessage("GetStatus dla materiału " + id);
                status = db.GetMaterialStatus(id);

                if (status == MaterialStatus.NotAvailable)
                    status = MaterialStatus.NotFound;

            }
            catch (Exception ex)
            {
                Log.TraceMessage(ex, "GetStatus");
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }

            return new TransactionStatusInfo() { materialId = id, status = status.ToString(), description = "" };



        }
    }
}
