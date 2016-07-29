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
using PSNC.RepoAV.Services.RepApi;

namespace PSNC.RepoAV.Services.RepApi.Controllers
{
    public class SetMaterialDistributionController : ApiController
    {
        public void Put([FromBody]SetMaterialDistributionReq req)
        {
            string cnnString = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            RepDBAccess.RepDBAccess db = new RepDBAccess.RepDBAccess(cnnString, false);

            try
            {
                if (req == null)
                {
                    Log.TraceMessage("RepApiController.SetMaterialDistribution: brak parametrów wywołania / błąd parsowania");
                    Helper.ThrowResponseException(this.ControllerContext.Request, HttpStatusCode.BadRequest, "SetMaterialDistribution: no input parameters or parse error (5)");
                }

                Log.TraceMessage("RepApiController.SetMaterialDistribution(" + req.materialId + ", " + req.enable + ", " + Helper.GetClientIp(Request) + ")");

                bool enable = bool.Parse(req.enable);



                db.SetMaterialAllowDistribution(req.materialId, enable);

            }
            catch (Exception ex)
            {
                Log.TraceMessage(ex, "SetMaterialDistribution");
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }

        }
    }
}
