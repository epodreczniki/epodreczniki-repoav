using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Configuration;
using PSNC.RepoAV.RepDBAccess;
using PSNC.Util;
using System.Xml;
using System.Xml.Schema;

namespace PSNC.RepoAV.Services.RepApi.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class GetEncodingProfilesController : ApiController
    {
        /// <summary>
        /// Pobiera profile kodowania materiałów.
        /// </summary>
        /// <returns>XML zgodny ze schematem http://schemas.psnc.pl/media-metadata/Repository/1.0 zawierający listę profili.</returns>
        public string Get()
        {
            string res = null;
            try
            {
                string cnnString = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                RepDBAccess.RepDBAccess db = new RepDBAccess.RepDBAccess(cnnString, false);

                string profiles = db.GetEncodingProfilesXml();
                res = "<Profiles xmlns=\"http://schemas.psnc.pl/media-metadata/Repository/1.0\">" + profiles + "</Profiles>";
            }
            catch(Exception ex)
            {
                Log.TraceMessage(ex, "GetEncodingProfilesController");
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));

            }


            return res;
        }
    }
}
