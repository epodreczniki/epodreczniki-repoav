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
    public class ManagerController : ApiController
    {
        [HttpGet]
        public bool ReplicateMaterial(string publicId)
        {
            try
            {
                string cnnString = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                RepDBAccess.RepDBAccess db = new RepDBAccess.RepDBAccess(cnnString, false);
                string url = db.GetGlobalData("ManagerAPINLB");
                if (!url.EndsWith("/"))
                    url = url + "/";
                Log.TraceMessage("ReplicateMaterial dla materiału " + publicId);

                WebClient client = new WebClient();
                return bool.Parse(client.DownloadString(url + "ReplicateMaterial/" + publicId));
            }
            catch (Exception ex)
            {
                Log.TraceMessage(ex, "ReplicateMaterial");
            }

            return false;
        }

        [HttpGet]
        public bool RecodeMaterial(string publicId)
        {
            try
            {
                string cnnString = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                RepDBAccess.RepDBAccess db = new RepDBAccess.RepDBAccess(cnnString, false);
                string url = db.GetGlobalData("ManagerAPINLB");
                if (!url.EndsWith("/"))
                    url = url + "/";
                Log.TraceMessage("RecodeMaterial dla materiału " + publicId);

                WebClient client = new WebClient();
                return bool.Parse(client.DownloadString(url + "RecodeMaterial/" + publicId));
            }
            catch (Exception ex)
            {
                Log.TraceMessage(ex, "RecodeMaterial");
            }

            return false;
        }

        [HttpGet]
        public bool RemoveMaterial(string publicId)
        {
            try
            {
                string cnnString = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                RepDBAccess.RepDBAccess db = new RepDBAccess.RepDBAccess(cnnString, false);
                string url = db.GetGlobalData("ManagerAPINLB");
                if (!url.EndsWith("/"))
                    url = url + "/";
                Log.TraceMessage("RemoveMaterial dla materiału " + publicId);

                WebClient client = new WebClient();
                return bool.Parse(client.DownloadString(url + "RemoveMaterial/" + publicId));
            }
            catch (Exception ex)
            {
                Log.TraceMessage(ex, "RemoveMaterial");
            }

            return false;
        }

        [HttpGet]
        public bool ReplicateFormat(string id)
        {
            try
            {
                string cnnString = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                RepDBAccess.RepDBAccess db = new RepDBAccess.RepDBAccess(cnnString, false);
                string url = db.GetGlobalData("ManagerAPINLB");
                if (!url.EndsWith("/"))
                    url = url + "/";
                Log.TraceMessage("ReplicateFormat dla formatu " + id);

                WebClient client = new WebClient();
                return bool.Parse(client.DownloadString(url + "ReplicateFormat/" + id));
            }
            catch (Exception ex)
            {
                Log.TraceMessage(ex, "ReplicateFormat");
            }

            return false;
        }

        [HttpGet]
        public bool RemoveFormat(string id)
        {
            try
            {
                string cnnString = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                RepDBAccess.RepDBAccess db = new RepDBAccess.RepDBAccess(cnnString, false);
                string url = db.GetGlobalData("ManagerAPINLB");
                if (!url.EndsWith("/"))
                    url = url + "/";
                Log.TraceMessage("RemoveFormat dla formatu " + id);

                WebClient client = new WebClient();
                return bool.Parse(client.DownloadString(url + "RemoveFormat/" + id));
            }
            catch (Exception ex)
            {
                Log.TraceMessage(ex, "RemoveFormat");
            }

            return false;
        }

        [HttpGet]
        public bool RepairFormats()
        {
            try
            {
                string cnnString = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                RepDBAccess.RepDBAccess db = new RepDBAccess.RepDBAccess(cnnString, false);
                string url = db.GetGlobalData("ManagerAPINLB");
                if (!url.EndsWith("/"))
                    url = url + "/";
                Log.TraceMessage("RepairFormats");

                WebClient client = new WebClient();
                return bool.Parse(client.DownloadString(url + "RepairFormats"));
            }
            catch (Exception ex)
            {
                Log.TraceMessage(ex, "RepairFormats");
            }

            return false;
        }

        [HttpGet]
        public bool RepairReplicas()
        {
            try
            {
                string cnnString = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                RepDBAccess.RepDBAccess db = new RepDBAccess.RepDBAccess(cnnString, false);
                string url = db.GetGlobalData("ManagerAPINLB");
                if (!url.EndsWith("/"))
                    url = url + "/";
                Log.TraceMessage("RepairReplicas");

                WebClient client = new WebClient();
                return bool.Parse(client.DownloadString(url + "RepairReplicas"));
            }
            catch (Exception ex)
            {
                Log.TraceMessage(ex, "RepairReplicas");
            }

            return false;
        }

    }
}
