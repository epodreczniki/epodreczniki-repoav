using System;
using System.Reflection;
using PSNC.Multimedia;
using System.ComponentModel;

namespace PSNC.Multimedia
{

    public class MimeType : Attribute
    {
        public string Value;
        public MimeType(string value)
        {
            this.Value = value;
        }
    }

    public class IsLive : Attribute
    {
        public bool Value = false;
        public IsLive(bool value)
        {
            this.Value = value;
        }
    }

    public static class FileFormatExtensions
    {
        public static string GetMimeType(this MediaParser.FileFormat ff)
        {
            Type type = ff.GetType();
            MemberInfo[] memInfo = type.GetMember(ff.ToString());
            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(MimeType), false);
                if (attrs != null && attrs.Length > 0)
                    return ((MimeType)attrs[0]).Value;
            }
            return String.Empty;
        }

        public static bool IsLive(this MediaParser.FileFormat ff)
        {
            Type type = ff.GetType();
            MemberInfo[] memInfo = type.GetMember(ff.ToString());
            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(IsLive), false);
                if (attrs != null && attrs.Length > 0)
                    return ((IsLive)attrs[0]).Value;
            }
            return false;
        }

        public static string ToProperString(this MediaParser.FileFormat ff)
        {
            Type type = ff.GetType();
            MemberInfo[] memInfo = type.GetMember(ff.ToString());
            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attrs != null && attrs.Length > 0)
                    return ((DescriptionAttribute)attrs[0]).Description;
            }
            return ff.ToString();
        }

    }
}
