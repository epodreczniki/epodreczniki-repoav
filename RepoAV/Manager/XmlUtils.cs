using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Diagnostics;
using PSNC.Util;

namespace PSNC.RepoAV.Manager
{
    public static class XmlUtils
    {
        public static string GetXmlNode(string xml, string nodeName, bool outer = false)
        {
            Dictionary<string, string> results = GetXmlNodes(xml, new string[] { nodeName }, outer);
            if (results.ContainsKey(nodeName))
                return results[nodeName];
            else
                return null;
        }

        public static Dictionary<string, string> GetXmlNodes(string xml, string[] nodeNames, bool outer = false)
        {
            Dictionary<string, string> results = new Dictionary<string, string>();

            NameTable nt = new NameTable();
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(nt);


            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.LoadXml(xml);

                string defaultNamespace = xmlDoc.DocumentElement.NamespaceURI;
                if(!string.IsNullOrEmpty(defaultNamespace))
                    nsMgr.AddNamespace("n", defaultNamespace);

                XmlNode node;
                foreach (string nodeName in nodeNames)
                {
                    if (!string.IsNullOrEmpty(defaultNamespace))
                        node = xmlDoc.SelectSingleNode("//n:" + nodeName, nsMgr);
                    else
                        node = xmlDoc.SelectSingleNode("//" + nodeName, nsMgr);
                    if(node != null)
                        results.Add(nodeName, outer ? node.OuterXml : node.InnerText);
                }
            }
            catch
            {
                return null;               
            }
            return results;
        }

        public static string[] GetXmlNodes(string xml, string nodeName)
        {
            List<string> results = new List<string>();

            NameTable nt = new NameTable();
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(nt);         

            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.LoadXml(xml);

                string defaultNamespace = xmlDoc.DocumentElement.NamespaceURI;
                if (!string.IsNullOrEmpty(defaultNamespace))
                    nsMgr.AddNamespace("n", defaultNamespace);

                XmlNodeList nodes;
                if (!string.IsNullOrEmpty(defaultNamespace))
                    nodes = xmlDoc.SelectNodes("//n:" + nodeName, nsMgr);
                else
                    nodes = xmlDoc.SelectNodes("//" + nodeName, nsMgr);
                foreach (XmlNode node in nodes)
                    results.Add(node.InnerText);
            }
            catch
            {
                return null;
            }
            return results.ToArray();
        }

        public static int GetXmlNodeCount(string metadata, string nodeName)
        {
            string[] results = GetXmlNodes(metadata, nodeName);
            if (results != null)
                return results.Length;
            else
                return -1;           
        }
    }
}
