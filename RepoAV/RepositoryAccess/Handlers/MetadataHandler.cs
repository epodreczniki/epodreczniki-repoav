using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using PSNC.Util;

namespace PSNC.RepoAV.Services.RepositoryAccess.Handlers
{
    public class MetadataHandler : Handler
    {
        public MetadataHandler(string fscConnectionString)
        {
            m_FscConnectionString = fscConnectionString;
        }

        public override void HandleRequest(RequestContext context)
        {
            try
            {
                Log.TraceMessage("meta: " + context.FormatId);


                context.HttpContext.Response.AddHeader("Access-Control-Allow-Origin", "*");

                Stopwatch s = new Stopwatch();
                s.Start();
                MaterialInfo o = GetMetadataFromDb(context.FormatId);
                s.Stop();
                if (o != null)
                {
                    if (o.Ready)
                        context.HttpContext.Response.AddHeader("X-MaterialReady", "1");
                    else
                        context.HttpContext.Response.AddHeader("X-MaterialReady", "0");

                    if (o.AllowDistribution)
                    {
                        context.HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;

                        string json = m_Serializer.Serialize(o);
                        byte[] data = Encoding.UTF8.GetBytes(json);

                        context.HttpContext.Response.OutputStream.Write(data, 0, data.Length);
                        context.HttpContext.Response.ContentEncoding = Encoding.UTF8;
                        context.HttpContext.Response.ContentType = "application/json";


                    }
                    else
                    {
                        context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    }
                    Log.TraceMessage(TraceEventType.Verbose, string.Format("Pobranie metadanych dla Oryginalny url:'{0}', materialId:'{1}', allowDistrib {2}; czas obsługi {3} ms.", context.HttpContext.Request.Path, context.FormatId, o.AllowDistribution, (int)s.Elapsed.TotalMilliseconds));

                    context.HttpContext.ApplicationInstance.CompleteRequest();
                }
                else
                {
                    Log.TraceMessage(TraceEventType.Verbose, string.Format("Nie znaleziono metadanych dla Oryginalny url:'{0}', materialId:'{1}'; czas obsługi {2} ms.", context.HttpContext.Request.Path, context.FormatId, (int)s.Elapsed.TotalMilliseconds));
                    m_Successor.HandleRequest(context);
                }
            }
            catch (Exception ex)
            {
                Log.TraceMessage(ex);

                Log.TraceMessage(TraceEventType.Error, string.Format("Błąd podczas obsługi pobrania metadanych. Oryginalny url:'{0}', format:'{1}'.", context.HttpContext.Request.Path, context.FormatId));
                m_Successor.HandleRequest(context);
            }
        }

        private MaterialInfo GetMetadataFromDb(string materialId)
        {
            MaterialInfo info = null;

            using (SqlConnection connection = new SqlConnection(m_FscConnectionString))
            {
                using (SqlCommand command = new SqlCommand("[dbo].[GetMaterialInfo]", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add("publicId", System.Data.SqlDbType.VarChar).Value = materialId;
                    SqlParameter outDistribution = command.Parameters.Add("allowDistribution", System.Data.SqlDbType.Bit);
                    SqlParameter outSubs = command.Parameters.Add("subtitles", System.Data.SqlDbType.VarChar);
                    SqlParameter outProfiles = command.Parameters.Add("profiles", System.Data.SqlDbType.VarChar);
                    SqlParameter outAdditionalAudioStreams = command.Parameters.Add("addAudioCount", System.Data.SqlDbType.Int);
                    SqlParameter outDuration = command.Parameters.Add("duration", System.Data.SqlDbType.BigInt);
                    SqlParameter outReady = command.Parameters.Add("ready", System.Data.SqlDbType.Bit);

                    outDistribution.Direction = System.Data.ParameterDirection.Output;
                    outSubs.Direction = System.Data.ParameterDirection.Output;
                    outSubs.Size = 255;
                    outProfiles.Direction = System.Data.ParameterDirection.Output;
                    outProfiles.Size = 255;
                    outAdditionalAudioStreams.Direction = System.Data.ParameterDirection.Output;
                    outDuration.Direction = System.Data.ParameterDirection.Output;
                    outReady.Direction = System.Data.ParameterDirection.Output;

                    connection.Open();
                    command.ExecuteNonQuery();

                    info = new MaterialInfo();
                    info.MaterialId = materialId;

                    if (outDistribution.Value != DBNull.Value)
                    {
                        info.AllowDistribution = (bool)outDistribution.Value;
                    }
                    else
                        info.AllowDistribution = false;

                    if (info.AllowDistribution)
                    {
                        if (outProfiles.Value != DBNull.Value)
                        {
                            info.Profiles = outProfiles.Value.ToString().Split(ValuesSeparator, StringSplitOptions.RemoveEmptyEntries);
                        }
                        if (outSubs.Value != DBNull.Value)
                        {
                            info.Subtitles = outSubs.Value.ToString().Split(ValuesSeparator, StringSplitOptions.RemoveEmptyEntries);
                        }
                        else
                        {
                            info.Subtitles = new string[] { };
                        }

                        if (outAdditionalAudioStreams.Value != DBNull.Value)
                        {
                            info.AltAudio = (int)outAdditionalAudioStreams.Value;
                        }
                        if (outDuration.Value != DBNull.Value)
                        {
                            info.Duration = (long)outDuration.Value;
                        }
                        else
                        {
                            info.Duration = -1;
                        }

                        if (outReady.Value != DBNull.Value)
                        {
                            info.Ready = (bool)outReady.Value;
                        }
                    }
                    else
                    {
                        //using (RepositoryDB rep = new RepositoryDB(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["REPDB"].ConnectionString))
                        //{
                            //    TransactionStatus? t;
                            //    TransactionType type;
                            //    string md5;
                            //    string id = rep.GetTransaction(materialId, out t, out md5, out type);
                            //    if (!string.IsNullOrEmpty(id) && type == TransactionType.Add)
                            //        info = new MaterialInfo() { AllowDistribution = true, MaterialId = materialId };
                            //    Trace.Verbose("Pobranie metadanych dla materialId:'{0}' z REPDB: trId: {3} status: {1}, type :{2}", materialId ?? "(null)", t.HasValue ? t.Value.ToString() : "(null)", type, id);

                        //}
                    }
                }
            }

            return info;
        }

        private static JavaScriptSerializer m_Serializer = new JavaScriptSerializer();
        private static string[] ValuesSeparator = new string[] { "," };
        private string m_FscConnectionString;

        private class MaterialInfo
        {
            public string[] Profiles { get; set; }

            public string[] Subtitles { get; set; }

            public int AltAudio { get; set; }

            public string MaterialId { get; set; }

            public bool AllowDistribution { get; set; }

            public long Duration { get; set; }

            public bool Ready { get; set; }

            public MaterialInfo()
            {
                this.Profiles = new string[0];
                this.Profiles = new string[0];
            }
        }
    }
}