using System;
using System.Collections.Generic;
using System.Configuration;

namespace PSNC.Proca3
{
    public class ConfigSection : ConfigurationSection  
    {
        public static ConfigSection GetConfiguration()
        {
            ConfigSection configuration = ConfigurationManager.GetSection("serviceConfiguration") as ConfigSection;

            if (configuration != null)
                return configuration;

            return new ConfigSection();
        }

        [ConfigurationProperty("port", DefaultValue="0", IsRequired = false)]
        public string Port
        {
            get
            {
                return this["port"] as string;
            }
            set
            {
                this["port"] = value;
            }
        }

        [ConfigurationProperty("name", DefaultValue = "Proca3", IsRequired = false)]
        public string Name
        {
            get
            {
                return this["name"] as string;
            }
            set
            {
                this["name"] = value;
            }
        }

    }
}
