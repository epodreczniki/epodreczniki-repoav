using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace PSNC.RepoAV.Services.RepositoryAccess
{
    public class CheckSum
    {
        public static bool IsValid(bool checksumRequired, string formatId, out string formatIdWithoutChecksum)
        {
            bool valid = false;
            formatIdWithoutChecksum = string.Empty;

            if (!string.IsNullOrEmpty(formatId))
            {
                if (formatId[0] == '!')
                {
                    if (formatId.Length > 2)
                    {
                        formatIdWithoutChecksum = formatId.Substring(2, formatId.Length - 2);
                        char checkSum = formatId[1];

                        int hash = 238;
                        for (int i = 0; i < formatIdWithoutChecksum.Length; i++)
                        {
                            hash = hash ^ formatIdWithoutChecksum[i];
                        }
                        hash = 65 + hash % 25;

                        valid = (char)hash == checkSum;
                    }
                }
                else
                {
                    formatIdWithoutChecksum = formatId;
                    valid = !checksumRequired;
                }
            }

            return valid;
        }

        public static string GetForText(string id)
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