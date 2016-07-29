using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using PSNC.RepoAV;
using PSNC.Util;

namespace PSNC.RepoAV.Services.RepositoryAccess
{
    public class RepositoryConfiguration
    {
        public RepositoryConfiguration()
        {
            LoadConfig();
        }

        public bool Enabled
        {
            get
            {
                if (m_Config != null && m_Config.RedirectionSettings != null)
                {
                    return m_Config.RedirectionSettings.IsEnabled;
                }
                return false;
            }
        }

        public string PublicRequestHeader
        {
            get
            {
                if (m_Config != null && m_Config.RedirectionSettings != null)
                {
                    return m_Config.RedirectionSettings.SmartRedirectForRequestHeader;
                }
                return string.Empty;
            }
        }

        public string ThisNodeId
        {
            get
            {
                if (m_Config != null && m_Config.Nodes != null)
                {
                    return m_Config.Nodes.ThisNodeId;
                }
                return string.Empty;
            }
        }

        public List<RepositoryNode> EnabledNodes
        {
            get
            {
                if (m_EnabeldNodes == null)
                {
                    if (m_Config != null && m_Config.ActiveNodes != null)
                    {
                        m_EnabeldNodes = m_Config.ActiveNodes;
                    }
                    else
                    {
                        m_EnabeldNodes = new List<RepositoryNode>();
                    }
                }
                return m_EnabeldNodes;
            }
        }

        private void LoadConfig()
        {
            try
            {
                m_Config = (RepositoryNodesConfigurationSection)ConfigurationManager.GetSection("repositoryNodesConfiguration");
            }
            catch (Exception ex)
            {
                Log.TraceMessage(ex, "Nie zdefiniowano konfiguracji węzłów repozytorium do przekazywania żądań.");
            }
        }

        public RepositoryNode GetNextRepoNode(string visitedNodes, out string visitedNodesWithThisOne)
        {
            RepositoryNode nextNode = null;
            string nextId = string.Empty;
            visitedNodesWithThisOne = string.Empty;

            if (Enabled)
            {
                nextId = GetNextRepoNode(visitedNodes, EnabledNodes.ConvertAll(n => n.Id), ThisNodeId, out visitedNodesWithThisOne);
                if (!string.IsNullOrEmpty(nextId))
                {
                    nextNode = EnabledNodes.Where(n => n.Id == nextId).FirstOrDefault();
                }
            }

            return nextNode;
        }

        private static string GetVisitedNodesInfo(string[] visited, string add)
        {
            string visitedString = string.Empty;

            List<string> nodes = new List<string>(visited.Length);
            nodes.AddRange(visited);

            if (!nodes.Contains(add))
            {
                nodes.Add(add);
            }

            visitedString = string.Join(VisitedNodesSeparator.ToString(), nodes);
            return visitedString;
        }

        private static string GetNextRepoNode(string visitedNodes, List<string> activeNodesIds, string myId, out string visitedNodesWithThisOne)
        {
            string nextId = string.Empty;

            if (string.IsNullOrEmpty(visitedNodes))
            {
                visitedNodesWithThisOne = myId;
            }
            else
            {
                string[] visited = visitedNodes.Split(VisitedNodesSeparator);
                foreach (var item in visited)
                {
                    activeNodesIds.Remove(item);
                }

                visitedNodesWithThisOne = GetVisitedNodesInfo(visited, myId);
            }


            if (activeNodesIds.Count > 0)
            {
                if (activeNodesIds.Count > 1)
                {
                    int myIndex = activeNodesIds.IndexOf(myId);
                    if (myIndex == activeNodesIds.Count - 1)
                    {
                        nextId = activeNodesIds[0];
                    }
                    else
                    {
                        nextId = activeNodesIds[myIndex + 1];
                    }
                }
                else
                {
                    if (activeNodesIds[0] != myId)
                    {
                        nextId = activeNodesIds[0];
                    }
                }
            }

            return nextId;
        }
        public const char VisitedNodesSeparator = ',';
        private RepositoryNodesConfigurationSection m_Config;
        private List<RepositoryNode> m_EnabeldNodes;
    }

    public class RepositoryNode : ConfigurationElement
    {
        public RepositoryNode()
        {
        }

        public bool IsEnabled
        {
            get
            {
                return string.Equals(Boolean.TrueString, Enabled, StringComparison.InvariantCultureIgnoreCase);
            }
        }

        [ConfigurationProperty("id", IsRequired = false)]
        public string Id
        {
            get { return (string)this["id"]; }
            set { this["id"] = value; }
        }

        [ConfigurationProperty("address", IsRequired = false)]
        public string Address
        {
            get { return (string)this["address"]; }
            set { this["address"] = value; }
        }

        [ConfigurationProperty("enabled", IsRequired = false)]
        public string Enabled
        {
            get
            {
                object o = base["enabled"];
                if (o != null)
                {
                    return o.ToString();
                }
                return string.Empty;
            }
        }
    }

    public class RepositoryRedirection : ConfigurationElement
    {
        public RepositoryRedirection()
        {
        }

        public bool IsEnabled
        {
            get
            {
                return string.Equals(Boolean.TrueString, Enabled, StringComparison.InvariantCultureIgnoreCase);
            }
        }

        [ConfigurationProperty("enabled", IsRequired = false)]
        public string Enabled
        {
            get
            {
                object o = base["enabled"];
                if (o != null)
                {
                    return o.ToString();
                }
                return string.Empty;
            }
        }

        [ConfigurationProperty("smartRedirectForRequestHeader", IsRequired = false)]
        public string SmartRedirectForRequestHeader
        {
            get { return (string)this["smartRedirectForRequestHeader"]; }
            set { this["smartRedirectForRequestHeader"] = value; }
        }
    }

    public class RepositoryNodesCollection : ConfigurationElementCollection
    {
        public RepositoryNodesCollection()
        {
        }

        [ConfigurationProperty("thisNodeId", IsRequired = false)]
        public string ThisNodeId
        {
            get { return (string)base["thisNodeId"]; }
        }

        public void Add(RepositoryNode customElement)
        {
            BaseAdd(customElement);
        }

        protected override void BaseAdd(ConfigurationElement element)
        {
            base.BaseAdd(element, false);
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.AddRemoveClearMap;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new RepositoryNode();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((RepositoryNode)element).Id;
        }

        public RepositoryNode this[int Index]
        {
            get
            {
                return (RepositoryNode)BaseGet(Index);
            }
            set
            {
                if (BaseGet(Index) != null)
                {
                    BaseRemoveAt(Index);
                }
                BaseAdd(Index, value);
            }
        }

        new public RepositoryNode this[string Name]
        {
            get
            {
                return (RepositoryNode)BaseGet(Name);
            }
        }

        public int indexof(RepositoryNode element)
        {
            return BaseIndexOf(element);
        }

        public void Remove(RepositoryNode node)
        {
            if (BaseIndexOf(node) >= 0)
                BaseRemove(node.Id);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        public void Remove(string name)
        {
            BaseRemove(name);
        }

        public void Clear()
        {
            BaseClear();
        }
    }

    public class RepositoryNodesConfigurationSection : ConfigurationSection
    {
        public RepositoryNodesConfigurationSection()
        {

        }

        public List<RepositoryNode> ActiveNodes
        {
            get
            {
                if (m_ActiveNodes == null)
                {
                    m_ActiveNodes = new List<RepositoryNode>();

                    if (RedirectionSettings != null && RedirectionSettings.IsEnabled)
                    {
                        foreach (RepositoryNode element in Nodes)
                        {
                            if (!string.IsNullOrEmpty(element.Id) && element.IsEnabled)
                            {
                                m_ActiveNodes.Add(element);
                            }
                        }
                    }

                }
                return m_ActiveNodes;
            }
        }

        [ConfigurationProperty("repositoryNodes", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(RepositoryNodesCollection), AddItemName = "add", ClearItemsName = "clear", RemoveItemName = "remove")]
        public RepositoryNodesCollection Nodes
        {
            get
            {
                return (RepositoryNodesCollection)base["repositoryNodes"];
            }
        }

        [ConfigurationProperty("redirection", IsRequired = false)]
        public RepositoryRedirection RedirectionSettings
        {
            get
            {
                return (RepositoryRedirection)this["redirection"];
            }
        }

        private List<RepositoryNode> m_ActiveNodes;
    }
}