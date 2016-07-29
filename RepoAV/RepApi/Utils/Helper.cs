using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.ServiceModel.Channels;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using PSNC.Util;



namespace PSNC.RepoAV.Services.RepApi
{
    public class Helper
    {
        public static bool NoOngoingNotification = true;
        private static Dictionary<string, object> Cache = new Dictionary<string, object>(5);

        public static string GetClientIp(HttpRequestMessage request)
        {
            if (request.Properties.ContainsKey("MS_HttpContext"))
            {
                return ((HttpContextWrapper)request.Properties["MS_HttpContext"]).Request.UserHostAddress;
            }
            else if (request.Properties.ContainsKey(RemoteEndpointMessageProperty.Name))
            {
                RemoteEndpointMessageProperty prop;
                prop = (RemoteEndpointMessageProperty)request.Properties[RemoteEndpointMessageProperty.Name];
                return prop.Address;
            }
            else
            {
                return null;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subsystemName"></param>
        /// <param name="interfaceName"></param>
        /// <returns></returns>
    

        //internal static bool CheckPrefix(string prefix)
        //{
        //    try
        //    {
        //        using (RepositoryDB db = new RepositoryDB())
        //        {
        //            return db.CheckPrefix(prefix);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorsHandler.TraceMessage(ex);
        //    }
        //    return false;
        //}


        internal static bool IsSharedDir(string location)
        {
            return (location.StartsWith(@"\\"));
        }



        internal static HttpResponseMessage ThrowResponseException(HttpRequestMessage request, HttpStatusCode code, string message)
        {
            Log.TraceMessage(message);
            throw new HttpResponseException(request.CreateErrorResponse(code, message));
        }




        //internal static bool IsFinalMaterialState(MaterialStatus? materialStatus)
        //{
        //    if (!materialStatus.HasValue)
        //        return true;
        //    switch(materialStatus.Value)
        //    {
        //        case MaterialStatus.AddError:
        //        case MaterialStatus.InvalidFile:
        //        case MaterialStatus.NotFound:
        //        case MaterialStatus.RecError:
        //        case MaterialStatus.Recoded:
        //        case MaterialStatus.Removed:
        //            return true;
        //        default:
        //            return false;
        //    }
        //}


       public static string GetChecksumForText(string id)
        {
            int hash = 238;

            for (int i = 0; i < id.Length; i++)
            {
                hash = hash ^ id[i];
            }

            hash = 65 + hash % 25;
            char letter = (char)hash;

            return letter.ToString();
        }
    }
}