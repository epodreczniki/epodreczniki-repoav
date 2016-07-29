using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using PSNC.Util;
using PSNC.RepoAV.MaterialFormatDBAccess;

namespace PSNC.RepoAV.Services.RepositoryAccess.Cache
{
    public class MaterialFormatCache
    {
        private int m_ResponseCachingTime = int.MinValue;
        private static MaterialFormatCache s_Instance = new MaterialFormatCache();

        private ICacheProvider Cache { get; set; }

        public static MaterialFormatCache Instance
        {
            get { return s_Instance; }
        }

        public FormatAccess GetMaterialFormatInfo(string formatId)
        {
            return this.Cache.Get(formatId) as FormatAccess;
        }

        public void SetMaterialFormatInfo(FormatAccess data)
        {
            try
            {
                this.Cache.Set(data.UniqueId, data, ResponseCachingTime);                             
            }
            catch (Exception ex)
            {
                Log.TraceMessage(ex, "Błąd w trakcie wstawiania info o materiale '{0}' do cache'u.");
            }

        }



        private MaterialFormatCache()
        {
            this.Cache = new DefaultCacheProvider();
        }

        private int ResponseCachingTime
        {
            get
            {
                if (m_ResponseCachingTime == int.MinValue)
                {
                    string ti = WebConfigurationManager.AppSettings.Get("ResponseCachingTime");
                    m_ResponseCachingTime = 5;
                    if (string.IsNullOrEmpty(ti) || !int.TryParse(ti, out m_ResponseCachingTime))
                    {
                        m_ResponseCachingTime = 5;
                    }
                }
                return m_ResponseCachingTime;
            }
        }


    }
}