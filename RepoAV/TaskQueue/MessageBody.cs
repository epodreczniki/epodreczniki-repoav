using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSNC.RepoAV.TaskQueue
{
	[Serializable]
	public sealed class MessageBody
	{
		public string m_Type;
		public long m_ReceivingTaskID;
		public long m_SendingTaskID;
		public int m_ErrorType;
		public string m_ErrorDesc;
		public Dictionary<string, object> m_Params = null;
		public bool m_Postpone;
		public DateTime m_PostponeTill;
		public string m_Result;
		public string[] m_AddInfo;


		public MessageBody()
		{
			m_Type = "UnknownMsg";
			m_ReceivingTaskID = -1;
			m_SendingTaskID = -1;
		}
		public MessageBody(string type, long recID, long senderTaskID, int ErrorType, string errorMsg, bool postpone, DateTime postponeTill, string result, string[] addInfo, Dictionary<string, object> ps)
		{
			m_Type = type;
			m_ReceivingTaskID = recID;
			m_SendingTaskID = senderTaskID;
			m_ErrorType = ErrorType;
			m_ErrorDesc = errorMsg;
			m_Params = ps;
			m_Postpone = postpone;
			m_PostponeTill = postponeTill;
			m_Result = result;
			m_AddInfo = addInfo;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			if (m_Params != null)
			{
				foreach (var de in m_Params)
					sb.AppendFormat("Key={0} Value={1} |", de.Key == null ? "NULL" : de.Key, de.Value == null ? "NULL" : de.Value.ToString());
			}
			string ps = sb.ToString();

			string addInfo = "";
			if (m_AddInfo != null)
			{
				sb.Length = 0;
				for (int i = 0; i < m_AddInfo.Length; i++)
					sb.AppendFormat("AddInfo[{0}]={1} | ", i, m_AddInfo[i]);
				addInfo = sb.ToString();
			}

			return string.Format("Treśc wiadomości: ReceiverTaskID={0}, SenderTaskID='{5}', Type={1}, ErrorType={2}, ErrorDesc={3}, Parametry='{4}', Postpone='{6}, PostponeTill='{7}', Result='{8}', AddInfo='{9}'.",
								m_ReceivingTaskID, m_Type, m_ErrorType, m_ErrorDesc, ps, m_SendingTaskID, m_Postpone, m_PostponeTill, m_Result, addInfo);
		}

		public bool DoesParamExist(string name)
		{
			if (m_Params != null)
				return m_Params.ContainsKey(name);

			return false;
		}

		public object GetParam(string name)
		{
			if (m_Params != null)
			{
				if (m_Params.ContainsKey(name))
					return m_Params[name];
			}
			return null;
		}

		public void AddParam(string name, object val)
		{
			m_Params[name] = val;
		}
	}
}
