using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Configuration;

namespace PSNC.RepoAV.Services.RepApi.Controllers
{
    public class ElementsController : ApiController
    {
        public string[] Get(string from)
        {

            DateTime fromDate;



            if(from == null || DateTime.TryParse(from, out fromDate) == false)
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Niepoprawny parametr wejściowy 'from'"));

            string[] ar = null;
            try
            {
                //GetPublicIds4ChangedMaterialsSince
                string cnnString = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                RepDBAccess.RepDBAccess db = new RepDBAccess.RepDBAccess(cnnString, false);


                ar = db.GetPublicIds4ChangedMaterialsSince(fromDate, DateTime.Now);
            }
            catch(Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
            return ar;
        }

    }
}
