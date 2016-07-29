using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.ServiceModel.Channels;
using System.Net.Http;
using System.Web.Http;
using System.Web.Configuration;
using System.Xml;
using System.ComponentModel.DataAnnotations;
using PSNC.RepoAV.Services.RepApi.Models;

namespace PSNC.RepoAV.Services.RepApi.Controllers
{
    public class RepApiController : ApiController
    {


        [HttpGet]
        public string About()
        {
            return "Repository AV RepApi";
        }

        [HttpPost]
        public string AddMaterial([FromBody]SetMaterialReq addReq)
        {
            return "AddMaterial";

            //try
            //{
            //    if (addReq == null)
            //    {
            //        ErrorsHandler.TraceMessage("RepApiController.AddMaterial: brak parametrów wywołania / błąd parsowania");
            //        Helper.ThrowResponseException(this.ControllerContext.Request, HttpStatusCode.BadRequest, "RepApiController.AddMaterial: no input parameters or parse error (5)");
            //    }

            //    ErrorsHandler.TraceMessage("RepApiController.AddMaterial(" + (addReq.materialId ?? "(null)")
            //                                        + ", " + (addReq.materialURL ?? "(null)")
            //                                        + ", " + (addReq.mime ?? "(null)")
            //                                        + ", " + (addReq.userId ?? "(null)")
            //                                        + ", " + (addReq.materialMD5 ?? "(null)")
            //                                        + ", " + (addReq.metadata ?? "(null)")
            //                                        + ", " + Helper.GetClientIp(Request)+")");

            //    string sourceId = string.Empty;

            //    if (string.IsNullOrEmpty(addReq.materialURL))
            //        Helper.ThrowResponseException(this.ControllerContext.Request, HttpStatusCode.BadRequest, "RepApiController.AddMaterial: materialURL cannot be empty (0)");


            //    if (!RepApi.Properties.Settings.Default.MaterialIdHasProviderId)
            //    {
            //        if (string.IsNullOrEmpty(addReq.materialId))
            //            addReq.materialId = Guid.NewGuid().ToString().Replace("-", ""); //generowanie id
            //    }
            //    else
            //    {
            //        if (string.IsNullOrEmpty(addReq.materialId))
            //            sourceId = Helper.GetDefaultPrefix();
            //        else
            //            if (!Helper.CheckPrefix(addReq.materialId))
            //            {
            //                Helper.ThrowResponseException(this.ControllerContext.Request, HttpStatusCode.BadRequest, string.Format("RepApiController.AddMaterial: prefix {0} is unknown or not authorized (1)", addReq.materialId));
            //                throw new HttpResponseException(HttpStatusCode.BadRequest);
            //            }
            //            else
            //                sourceId = addReq.materialId;
            //        sourceId = sourceId + ".";
            //    }

            //    string publicID = addReq.materialId;
            //    if (string.IsNullOrEmpty(publicID))
            //        publicID = sourceId + Guid.NewGuid().ToString().Replace("-", "");

            //    using (RepositoryDB repDB = new RepositoryDB())
            //    {
            //        // jeśli w tabeli importu jest już wpis z danym ID to nie można zlecać następnych zadań
            //        Helper.CheckMaterialLockState(publicID, TransactionType.Add, ControllerContext);

            //        MediaType mtype = MediaType.Unknown;
            //        if (!string.IsNullOrEmpty(addReq.mime) && addReq.mime.ToLower().StartsWith("audio"))
            //            mtype = MediaType.SimpleAudio;
            //        if (!string.IsNullOrEmpty(addReq.mime) && addReq.mime.ToLower().StartsWith("video"))
            //            mtype = MediaType.SimpleVideo;

            //        XmlDocument xml = XmlUtils.BuildXml(mtype, publicID, publicID + "-" + DateTime.Now.ToShortDateString(), string.Empty, string.Empty, ContentType.StreamOrFile, addReq.mime, addReq.materialURL, null);

            //        UpdateMetadataFromExternalMetadata(xml, addReq.metadata);

            //        using (var stringWriter = new StringWriter())
            //        using (var xmlTextWriter = XmlWriter.Create(stringWriter))
            //        {
            //            xml.WriteTo(xmlTextWriter);
            //            xmlTextWriter.Flush();
            //            repDB.AddTransactionExt(publicID, addReq.materialURL, "", addReq.materialMD5, 0, 0, stringWriter.GetStringBuilder().ToString(), false, TransactionType.Add);
            //            Helper.ImportTaskAddedNotification();
            //        }

            //        return publicID;
            //    }
            //}
            //catch (HttpResponseException)
            //{
            //    throw;
            //}
            //catch (Exception ex)
            //{
            //    ErrorsHandler.TraceMessage(ex, "RepApiController.AddMaterial: Unexpected error (4)");
            //    throw new HttpResponseException(this.ControllerContext.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            //}
        }

        [HttpPut]
        public string UpdateMaterial([FromBody]SetMaterialReq updReq)
        {
            return "UpdateMaterial";
            //try
            //{
            //    if (updReq == null)
            //    {
            //        ErrorsHandler.TraceMessage("RepApiController.UpdateMaterial: brak parametrów wywołania / błąd parsowania");
            //        Helper.ThrowResponseException(this.ControllerContext.Request, HttpStatusCode.BadRequest, "RepApiController.UpdateMaterial: no input parameters or parse error (5)");
            //    }

            //    ErrorsHandler.TraceMessage("RepApiController.UpdateMaterial(" + (updReq.materialId ?? "(null)")
            //                                        + ", " + (updReq.materialURL ?? "(null)")
            //                                        + ", " + (updReq.mime ?? "(null)")
            //                                        + ", " + (updReq.userId ?? "(null)")
            //                                        + ", " + (updReq.materialMD5 ?? "(null)")
            //                                        + ", " + (updReq.metadata ?? "(null)")
            //                                        + ", " + Helper.GetClientIp(Request)+")");

            //    string sourceId = string.Empty;

            //    if (string.IsNullOrEmpty(updReq.materialURL))
            //        Helper.ThrowResponseException(this.ControllerContext.Request, HttpStatusCode.BadRequest, "RepApiController.UpdateMaterial: materialURL cannot be empty (0)");

            //    using (RepositoryDB repDB = new RepositoryDB())
            //    {

            //        if (!RepApi.Properties.Settings.Default.MaterialIdHasProviderId)
            //        {
            //            if (string.IsNullOrEmpty(updReq.materialId))
            //                updReq.materialId = Guid.NewGuid().ToString().Replace("-", ""); //generowanie id
            //        }
            //        else
            //        {
            //            if (string.IsNullOrEmpty(updReq.materialId))
            //                sourceId = Helper.GetDefaultPrefix();
            //            else
            //                if (!Helper.CheckPrefix(updReq.materialId))
            //                {
            //                    Helper.ThrowResponseException(this.ControllerContext.Request, HttpStatusCode.BadRequest, string.Format("RepApiController.UpdateMaterial: prefix {0} is unknown or not authorized (1)", updReq.materialId));
            //                    throw new HttpResponseException(HttpStatusCode.BadRequest);
            //                }
            //                else
            //                    sourceId = updReq.materialId;
            //            sourceId = sourceId + ".";
            //        }

            //        string publicID = updReq.materialId;
            //        if (string.IsNullOrEmpty(publicID))
            //            publicID = sourceId + Guid.NewGuid().ToString().Replace("-", "");

            //        TransactionType transType = TransactionType.Update;

            //        //Automatyczne przejscie na add, gdy materiału nie ma.. (na życzenie mmatela)
            //        FscDBAccess fscDB = Helper.GetFscDBAccess();
            //        if(Helper.IsMaterialAbsent(fscDB.GetMaterialGroupState(updReq.materialId)) )
            //        {
            //                TransactionType ttype;
            //                TransactionStatus? transStatus;
            //                string md5;
            //                string tid = repDB.GetTransaction(updReq.materialId, out transStatus, out md5, out ttype);

            //                if (!transStatus.HasValue)
            //                {
            //                    transType = TransactionType.Add;
            //                }
            //        }

            //        Helper.CheckMaterialLockState(updReq.materialId, transType, ControllerContext);


            //        MediaType mtype = MediaType.SimpleVideo;
            //        if (!string.IsNullOrEmpty(updReq.mime) && updReq.mime.ToLower().StartsWith("audio"))
            //            mtype = MediaType.SimpleAudio;
            //        XmlDocument xml = XmlUtils.BuildXml(mtype, publicID, publicID + "-" + DateTime.Now.ToShortDateString(), string.Empty, string.Empty, ContentType.StreamOrFile, updReq.mime, updReq.materialURL, null);

            //        UpdateMetadataFromExternalMetadata(xml, updReq.metadata);

            //        using (var stringWriter = new StringWriter())
            //        using (var xmlTextWriter = XmlWriter.Create(stringWriter))
            //        {
            //            xml.WriteTo(xmlTextWriter);
            //            xmlTextWriter.Flush();
            //            repDB.AddTransactionExt(publicID, updReq.materialURL, "", updReq.materialMD5, 0, 0, stringWriter.GetStringBuilder().ToString(), false, transType);
            //            Helper.ImportTaskAddedNotification();
            //        }

            //        return publicID;
            //    }
            //}
            //catch (HttpResponseException)
            //{
            //    throw;
            //}
            //catch (Exception ex)
            //{
            //    ErrorsHandler.TraceMessage(ex, "RepApiController.UpdateMaterial: Unexpected error (3)");
            //    throw new HttpResponseException(this.ControllerContext.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            //}
        }

        private void UpdateMetadataFromExternalMetadata(XmlDocument xml, string metadata)
        {
            //if (String.IsNullOrWhiteSpace(metadata))
            //    return;
            //try
            //{
            //    XmlDocument addMeta = new XmlDocument();
            //    addMeta.LoadXml(metadata);

            //    if (addMeta.DocumentElement.NamespaceURI == XmlUtils.Namespace && addMeta.DocumentElement.Name == "Subtitles")
            //    {
            //        NameTable nt = new NameTable();
            //        XmlNamespaceManager nsMgr = new XmlNamespaceManager(nt);
            //        nsMgr.AddNamespace("n", XmlUtils.Namespace);
            //        XmlNode subtitlesElem = xml.SelectSingleNode("/n:MediaInformation/n:Subtitles", nsMgr);
            //        if (subtitlesElem == null)
            //            xml.SelectSingleNode("/n:MediaInformation", nsMgr).AppendChild(xml.ImportNode(addMeta.DocumentElement, true));
            //        else
            //            subtitlesElem.InnerXml = addMeta.DocumentElement.InnerXml;
            //    }

            //    if (addMeta.DocumentElement.NamespaceURI == "http://epodreczniki.pl/" && addMeta.DocumentElement.Name == "metadata")
            //    {
            //        NameTable nt = new NameTable();
            //        XmlNamespaceManager nsMgr = new XmlNamespaceManager(nt);
            //        nsMgr.AddNamespace("n", XmlUtils.Namespace);
            //        nsMgr.AddNamespace("e", "http://epodreczniki.pl/");
            //        XmlNode titleElem = xml.SelectSingleNode("/n:MediaInformation/n:Title", nsMgr);
            //        if (titleElem != null && addMeta.SelectSingleNode("/e:metadata/e:title", nsMgr) != null)
            //            titleElem.InnerText = addMeta.SelectSingleNode("/e:metadata/e:title", nsMgr).InnerText;
                                                
            //        XmlNode subtitlesElem = addMeta.SelectSingleNode("/e:metadata/e:subtitlesURL", nsMgr);
            //        if (subtitlesElem != null && !string.IsNullOrWhiteSpace(subtitlesElem.InnerText))
            //        {
            //            XmlNode node = xml.SelectSingleNode("/n:MediaInformation/n:Subtitles", nsMgr);
            //            if (node == null)
            //            {
            //                node = xml.CreateElement("Subtitles", nsMgr.LookupNamespace("n"));
            //                xml.DocumentElement.AppendChild(node);
            //            }
            //            XmlNode set = xml.CreateElement("Set", nsMgr.LookupNamespace("n"));
            //            set.AppendChild(xml.CreateElement("Id", nsMgr.LookupNamespace("n"))).InnerText="subtitles";
            //            set.AppendChild(xml.CreateElement("FileFormat", nsMgr.LookupNamespace("n"))).InnerText = "vtt";
            //            set.AppendChild(xml.CreateElement("ClosedCaptions", nsMgr.LookupNamespace("n"))).InnerText = "false";
            //            set.AppendChild(xml.CreateElement("MediaURI", nsMgr.LookupNamespace("n"))).InnerText = subtitlesElem.InnerText;
            //            set.AppendChild(xml.CreateElement("VideoEmbedded", nsMgr.LookupNamespace("n"))).InnerText = "true";
            //            node.AppendChild(set);
            //        }

            //        subtitlesElem = addMeta.SelectSingleNode("/e:metadata/e:captionsURL", nsMgr);
            //        if (subtitlesElem != null && !string.IsNullOrWhiteSpace(subtitlesElem.InnerText))
            //        {
            //            XmlNode node = xml.SelectSingleNode("/n:MediaInformation/n:Subtitles", nsMgr);
            //            if (node == null)
            //            {
            //                node = xml.CreateElement("Subtitles", nsMgr.LookupNamespace("n"));
            //                xml.DocumentElement.AppendChild(node);
            //            }
            //            XmlNode set = xml.CreateElement("Set", nsMgr.LookupNamespace("n"));
            //            set.AppendChild(xml.CreateElement("Id", nsMgr.LookupNamespace("n"))).InnerText = "captions";
            //            set.AppendChild(xml.CreateElement("FileFormat", nsMgr.LookupNamespace("n"))).InnerText = "vtt";
            //            set.AppendChild(xml.CreateElement("ClosedCaptions", nsMgr.LookupNamespace("n"))).InnerText = "true";
            //            set.AppendChild(xml.CreateElement("MediaURI", nsMgr.LookupNamespace("n"))).InnerText = subtitlesElem.InnerText;
            //            set.AppendChild(xml.CreateElement("VideoEmbedded", nsMgr.LookupNamespace("n"))).InnerText = "true";
            //            node.AppendChild(set);
            //        }

            //    }
            //}
            //catch (Exception ex)
            //{
            //    throw new HttpResponseException(this.ControllerContext.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Błąd parsowania przekazanych metadanych: " + ex.Message, ex));
            //}
        }

        [HttpDelete]
        public HttpStatusCode RemoveMaterial(string id)
        {
            return HttpStatusCode.OK;
            //try
            //{
            //    if (string.IsNullOrWhiteSpace( id))
            //    {
            //        ErrorsHandler.TraceMessage("RepApiController.RemoveMaterial: brak parametrów wywołania / błąd parsowania");
            //        Helper.ThrowResponseException(this.ControllerContext.Request, HttpStatusCode.BadRequest, "RepApiController.RemoveMaterial: no input parameters or parse error (5)");
            //    }

            //    ErrorsHandler.TraceMessage("RepApiController.RemoveMaterial(" + id ?? "(null)" 
            //                                                            + ", " + Helper.GetClientIp(Request)+")");

            //    Helper.CheckMaterialLockState(id, TransactionType.Remove, ControllerContext);


            //    using (RepositoryDB repDB = new RepositoryDB())
            //    {
            //        repDB.AddTransactionExt(id, null, null, null, 0, 0, null, false, TransactionType.Remove);
            //        Helper.ImportTaskAddedNotification();
            //    }

            //    return HttpStatusCode.OK;
            //}
            //catch (HttpResponseException)
            //{
            //    throw;
            //}
            //catch (Exception ex)
            //{
            //    ErrorsHandler.TraceMessage(ex, "RepApiController.RemoveMaterial: Unexpected error (1)");
            //    throw new HttpResponseException(this.ControllerContext.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            //}

        }



        [HttpGet]
        public string GetMaterialFormats([FromUri]string id)
        {
            return "GetMaterialFormats";
//            try
//            {
//                if (string.IsNullOrWhiteSpace(id))
//                {
//                    ErrorsHandler.TraceMessage("RepApiController.GetMaterialFormats: brak parametrów wywołania / błąd parsowania");
//                    Helper.ThrowResponseException(this.ControllerContext.Request, HttpStatusCode.BadRequest, "RepApiController.GetMaterialFormats: no input parameters or parse error (5)");
//                }

//                FscDBAccess fscDB = Helper.GetFscDBAccess();

//                MaterialStatus? status = fscDB.GetMaterialStatus(id);
//                if (!status.HasValue || status.Value != MaterialStatus.Recoded)
//                    Helper.ThrowResponseException(this.ControllerContext.Request, HttpStatusCode.Conflict,
//                        string.Format("RepApiController.SetMaterialDistribution: material {0} is not ready for distribution (state {1})", id, status != null ? status.Value.ToString() : "material_not_found"));

//                MaterialXmlMetadata metadata = fscDB.GetMaterialXmlMetadata(id);
//                return metadata.Xml;
//#warning TODO - zmienić schemat XML
//            }
//            catch (HttpResponseException)
//            {
//                throw;
//            }
//            catch (Exception ex)
//            {
//                ErrorsHandler.TraceMessage(ex, "RepApiController.GetMaterialFormats: Unexpected error");
//                throw new HttpResponseException(this.ControllerContext.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
//            }
        }

    }

}
