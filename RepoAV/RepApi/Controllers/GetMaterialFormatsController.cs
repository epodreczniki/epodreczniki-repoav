using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Configuration;
using System.Xml;
using System.Text;
using System.Linq;
using System.IO;
using System.Xml.Serialization;
using PSNC.RepoAV.RepDBAccess;
using PSNC.RepoAV.Common;
using PSNC.Util;


namespace PSNC.RepoAV.Services.RepApi.Controllers
{
    public class Utf8StringWriter : StringWriter
    {
        public Utf8StringWriter()
        {
        }

        public override Encoding Encoding 
        {   get { return Encoding.UTF8; }
        }

    }


    public class GetMaterialFormatsController : ApiController
    {
        public HttpResponseMessage Get(string id)
        {
            const string SecialString = "@@@|@@@";
            string output;
            Dictionary<string, string> xmls = new Dictionary<string, string>();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = false;
            settings.NewLineHandling = NewLineHandling.None;

            Format[] formats = null;

            string cnnString = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            RepDBAccess.RepDBAccess db = new RepDBAccess.RepDBAccess(cnnString, false);
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    Log.TraceMessage("GetMaterialFormats: brak parametrów wywołania / błąd parsowania");
                    Helper.ThrowResponseException(this.ControllerContext.Request, HttpStatusCode.BadRequest, "GetMaterialFormats: no input parameters or parse error (5)");
                }

                Log.TraceMessage("GetMaterialFormats dla materiału " + id);


                formats = db.GetFormats4Material(id);

                foreach(Format f in formats)
                {
                    string xmlmetadata = f.XmlMetadata;

                    
                    XmlDocument doc = new XmlDocument();

                    doc.LoadXml("<XmlMetadata>" + xmlmetadata + "</XmlMetadata>");
                    xmlmetadata = doc.DocumentElement.InnerXml;

                    string key = SecialString + f.Id.ToString();
                    xmls.Add(key, xmlmetadata);
                    f.XmlMetadata = key;
                }
            }
            catch (Exception ex)
            {
                Log.TraceMessage(ex, "GetMaterialFormats");
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
            if (formats == null)
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, "Nie odnaleziono materiału of id=" + id));


            string c;
            XmlSerializer s = new XmlSerializer(typeof(Format[]));
    

            using (Utf8StringWriter textWriter = new Utf8StringWriter())
            {
                using (XmlWriter xtw = XmlWriter.Create(textWriter, settings))
                {
                    s.Serialize(xtw, formats);
                    output = textWriter.ToString();
                }
            }

            foreach (KeyValuePair<string, string> kvp in xmls)
            {
                output = output.Replace(kvp.Key, kvp.Value);
            }

            Log.TraceMessage(output);

            //StringBuilder sb = new StringBuilder(output.Length);
            //for (int i = 0; i < output.Length; i++)
            //{
            //    char cc = output[i];
            //    switch (cc)
            //    {
            //        case '\r':
            //        case '\n':
            //        case '\t':
            //            continue;
            //        default:
            //            sb.Append(cc);
            //            break;
            //    }
            //}
            //output = sb.ToString();

            return new HttpResponseMessage()
            {
                Content = new StringContent(
                    output,
                    System.Text.Encoding.UTF8,
                    "application/xml"
                )
            };
          

        }

    }
}

