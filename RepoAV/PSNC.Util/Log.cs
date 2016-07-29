using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.Logging.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;


namespace PSNC.Util
{
    public class Log
    {
        static LogWriter logWriter = null;
        static Log()
        {
            var logWriterFactory = new LogWriterFactory();
            logWriter = logWriterFactory.Create();

        }

        public static void TraceMessage(string msg)
        {
            TraceMessage(TraceEventType.Information, "General", msg, 50, -1, false);
        }


        public static void TraceMessage(string msg, int eventId)
        {
            TraceMessage(TraceEventType.Information, "General", msg, 50, eventId, false);
        }

        public static void TraceMessage(string msg, string category)
        {
            TraceMessage(TraceEventType.Information, category, msg, 50, -1, false);
        }

        public static void TraceMessage(string msg, string category, int eventId)
        {
            TraceMessage(TraceEventType.Information, category, msg, 50, eventId, false);
        }

        public static void TraceMessage(TraceEventType type, string strMsg)
        {
            TraceMessage(type, "General", strMsg);
        }

        public static void TraceMessage(TraceEventType type, string category, string strMsg)
        {
            TraceMessage(type, category, strMsg, 50, -1, false);
        }

        public static void TraceMessage(TraceEventType type, string category, string strMsg, int priority)
        {
            TraceMessage(type, category, strMsg, priority, -1, false);
        }

        public static void TraceMessage(TraceEventType type, string category, string strMsg, int priority, int eventId)
        {
            TraceMessage(type, category, strMsg, priority, eventId, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strSource"></param>
        /// <param name="mostTopException"></param>
        public static void TraceMessage(Exception mostTopException)
        {
            TraceMessage(mostTopException, "General", null);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strSource"></param>
        /// <param name="mostTopException"></param>
        public static void TraceMessage(Exception mostTopException, string strAddNote)
        {
            TraceMessage(mostTopException, "General", strAddNote);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strSource"></param>
        /// <param name="mostTopException"></param>
        public static void TraceMessage(Exception mostTopException, string category, string strAddNote)
        {
            string strMsg = "";
            if (strAddNote != null)
                strMsg += strAddNote + ": ";

            strMsg += GetExceptionDescription(mostTopException);

            TraceMessage(TraceEventType.Error, category, strMsg, 1, -1, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
        /// <param name="strSource"></param>
        /// <param name="strMessage"></param>
        public static void TraceMessage(TraceEventType type, string category, string strMsg, int priority, int eventID, bool showStack)
        {
            try
            {
                LogEntry log = new LogEntry();
                log.Message = strMsg;
                log.Categories.Add(category);
                log.Severity = type;
                log.Priority = priority;
                log.EventId = eventID;

                System.Security.Principal.IIdentity identity = Thread.CurrentPrincipal != null ? Thread.CurrentPrincipal.Identity : null;
                log.ManagedThreadName = identity != null ? identity.Name : "(unknown)";
                log.Title = "";


                if (showStack)
                {
                    Dictionary<string, object> dictionary = new Dictionary<string, object>();
                    Microsoft.Practices.EnterpriseLibrary.Logging.ExtraInformation.DebugInformationProvider informationHelper = new Microsoft.Practices.EnterpriseLibrary.Logging.ExtraInformation.DebugInformationProvider();
                    informationHelper.PopulateDictionary(dictionary);
                    log.ExtendedProperties = dictionary;
                }


                TraceMessage(log);
            }
            catch
            {
            }
        }


        private static void TraceMessage(LogEntry log)
        {

            if (logWriter.IsLoggingEnabled())
            {
                logWriter.Write(log);
            }
        }


        private static string GetExceptionDescription(Exception mostTopException)
        {
            StringBuilder strDescription = new StringBuilder();
            const string strEndLineFormatter = "\r\n";
            Exception tempException = null;
            Exception mostInnerException = null;

            if (mostTopException != null)
            {
                tempException = mostTopException;

                strDescription.Append(strEndLineFormatter);
                strDescription.Append("Exception Description:");
                do
                {
                    strDescription.Append(strEndLineFormatter);
                    strDescription.Append("\t");
                    strDescription.Append(tempException.Message);
                    mostInnerException = tempException;
                    tempException = tempException.InnerException;
                }
                while (tempException != null);

                strDescription.Append(strEndLineFormatter);
                strDescription.Append("StackTrace:\r\n");
                strDescription.Append(mostInnerException.StackTrace);
            }

            return strDescription.ToString();
        }
    }
}
