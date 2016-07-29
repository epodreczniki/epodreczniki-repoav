using System;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Linq;
using System.Linq;
using System.Text;

namespace PSNC.Multimedia.Tools
{
    public static class XmlTools
    {

        public class IntValue : Attribute
        {
            public int Value;
            public IntValue(int value)
            {
                this.Value = value;
            }
        }

        public class StringValue : Attribute
        {
            public string Value;
            public StringValue(string value)
            {
                this.Value = value;
            }
        }

        public class NamedValue
        {
            public String Name;
            public int Value;

            public NamedValue(String name, int value)
            {
                Name = name;
                Value = value;
            }

            public override string ToString()
            {
                return Name;
            }
        }

        public struct SampleRate
        {

            public const String Name = "SampleRate";

            [IntValue(8000)]
            public const String SampleRate8 = "8 000";
            [IntValue(22050)]
            public const String SampleRate22 = "22 050";
            [IntValue(32000)]
            public const String SampleRate32 = "32 000";
            [IntValue(44056)]
            public const String SampleRate44 = "44 056";
            [IntValue(44100)]
            public const String SampleRate44100 = "44 100";
            [IntValue(47250)]
            public const String SampleRate47 = "47 250";
            [IntValue(48000)]
            public const String SampleRate48 = "48 000";
            [IntValue(50000)]
            public const String SampleRate50 = "50 000";
            [IntValue(88200)]
            public const String SampleRate88 = "88 200";
            [IntValue(96000)]
            public const String SampleRate96 = "96 000";
            [IntValue(176400)]
            public const String SampleRate176 = "176 400";
            [IntValue(192000)]
            public const String SampleRate192 = "192 000";
            [IntValue(2822400)]
            public const String SampleRate2822 = "2 822 400";

            public static String FindClosestValue(int value)
            {
                object zaslepka = new object();
                int min = int.MaxValue;
                String result = "";
                Type type = typeof(SampleRate);
                IntValue intval;
                foreach (FieldInfo field in type.GetFields())
                {
                    foreach (Attribute attr in field.GetCustomAttributes(true))
                    {
                        intval = attr as IntValue;
                        if (intval != null)
                        {
                            int current = intval.Value;
                            int diff = System.Math.Abs(current - value);
                            if (diff < min)
                            {
                                min = diff;
                                result = (String)field.GetValue(zaslepka);
                            }
                        }
                    }
                }

                return result;
            }

        }

        public struct AudioCoding
        {
            public const String Name = "AudioCoding";

            public enum Values
            {
                [StringValue("unknown")]
                Unknown = 0,
                [StringValue("PCM")]
                PCM = 1,
                [StringValue("unknown")]
                Layer1 = 2,
                [StringValue("MPEG 1.0 layer 2")]
                Layer2 = 3,
                [StringValue("MPEG 1.0 layer 3")]
                Layer3 = 4,
                [StringValue("Windows Media Audio 10 Professional")]
                WMA10 = 5,
                [StringValue("Windows Media Audio 9")]
                WMA9 = 6,
                [StringValue("Windows Media Audio")]
                WMA = 7,
                [StringValue("Advanced Audio Coding")]
                MP4 = 8,
                [StringValue("Advanced Audio Coding")]
                AAC = 8,
                [StringValue("High Efficiency Advanced Audio Coding")]
                HE_AAC = 9,
                [StringValue("Free Lossless Audio Codec")]
                FLAC = 10,
                [StringValue("Monkey Audio")]
                APE = 11,
                [StringValue("Real Media Audio")]
                RMA = 12,
                [StringValue("Vorbis")]
                OGG = 13,
                [StringValue("Dolby Digital Advanced Codec 3")]
                AC3 = 14,
                [StringValue("Digital Theatre Systems")]
                DTS = 15
            }

        }

        public struct VideoCoding
        {
            public const String Name = "VideoCoding";

            public enum Values
            {
                [StringValue("unknown")]
                Unknown = 0,
                [StringValue("wmv10")]
                WMV10 = 1,
                [StringValue("wmv9")]
                WMV9 = 2,
                [StringValue("wmv")]
                WMV = 3,
                [StringValue("VC-1")]
                VC1 = 4,
                [StringValue("rm10")]
                RM = 5,
                [StringValue("TrueMotion VP6")]
                FLV = 6,
                [StringValue("TrueMotion VP6")]
                VP6 = 6,
                [StringValue("TrueMotion VP7")]
                VP7 = 7,
                [StringValue("On2 VP8")]
                VP8 = 8,
                [StringValue("H.263 Video Codec")]
                H263 = 9,
                [StringValue("H.264/MPEG-4 AVC")]
                H264 = 10
            }

        }

        public struct ColorDomain
        {
            public const String Name = "ColorDomain";

            public enum Values
            {
                [StringValue("unknown")]
                Unknown,
                [StringValue("binary")]
                Binary,
                [StringValue("color")]
                Color,
                [StringValue("colored")]
                Colored,
                [StringValue("graylevel")]
                GrayLevel,
            }

        }
        public const String Normalization = "Normalization";
        public const String Coding = "Coding";
        public const String Format = "Format";
        public const String FileFormat = "FileFormat";
        public const String Bitrate = "Bitrate";
        public const String FrameRate = "FrameRate";
        public const String FileName = "FileName";
        public const String FileSize = "FileSize";
        public const String MimeType = "MimeType";
        public const String AccessChannel = "AccessChannel";

        public const String FormatIdentifier = "FormatIdentifier";
        public const String DistributionType = "DistributionType";
        public const String Publication = "Publication";

        public const String DistributionTypeValue = "file";

        public enum Distribution
        {
            [StringValue("file")]
            file,
            [StringValue("stream")]
            stream,
            [StringValue("file or stream")]
            file_or_stream,
            [StringValue("live")]
            live,
            [StringValue("live + timeshifting")]
            live__plus__timeshifting
        }

        public struct License
        {
            public const String Name = "Name";
            public const String Value = "License";
        }

        public struct Sample
        {
            public const String Name = "Sample";
            [IntValue(16)]
            public const String Sample16 = "16";
            [IntValue(24)]
            public const String Sample24 = "24";

            public static String FindClosestValue(int value)
            {
                object zaslepka = new object();
                int min = int.MaxValue;
                String result = "";
                Type type = typeof(Sample);
                IntValue intval;
                foreach (FieldInfo field in type.GetFields())
                {
                    foreach (Attribute attr in field.GetCustomAttributes(true))
                    {
                        intval = attr as IntValue;
                        if (intval != null)
                        {
                            int current = intval.Value;
                            int diff = System.Math.Abs(current - value);
                            if (diff < min)
                            {
                                min = diff;
                                result = (String)field.GetValue(zaslepka);
                            }
                        }
                    }
                }

                return result;
            }
        }

        public static XmlNode PSNCNormalize(XmlNode parent)
        {
            return parent;
        }

        public static void ValidateXML(String filename, String schemapath)
        {
            XmlReader reader = XmlReader.Create(new FileStream(schemapath, FileMode.Open));

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            XmlSchemaSet schemas = new XmlSchemaSet();
            XmlSchema schema = schemas.Add(null, reader);
            settings.Schemas = schemas;

            XmlReader validator = XmlReader.Create(filename, settings);

            try
            {
                while (validator.Read()) { }
            }
            catch (XmlException err)
            {
                Console.WriteLine(err.Message);
            }
            finally
            {
                validator.Close();
            }
        }

        public static string GetSchema()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string contents = string.Empty;
            try
            {
                string[] resources = assembly.GetManifestResourceNames();
                string resourcePath = "PSNC.Multimedia.mdv.xsd";
                Stream strm = assembly.GetManifestResourceStream(resourcePath);
                StreamReader reader = new StreamReader(strm);
                contents = reader.ReadToEnd();
                reader.Close();
                return contents;
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
                return string.Empty;
            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine(ex.Message);
                return string.Empty;
            }
        }

        public static void Validate(String xmlString, String schemaString)
        {
            XmlReader reader = XmlReader.Create(new StringReader(schemaString));

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            XmlSchemaSet schemas = new XmlSchemaSet();
            XmlSchema schema = schemas.Add(null, reader);
            settings.Schemas = schemas;

            XmlReader validator = XmlReader.Create(new StringReader(xmlString), settings);

            try
            {
                while (validator.Read()) { }
            }
            catch (XmlException err)
            {
                Console.WriteLine(err.Message);
            }
            finally
            {
                validator.Close();
            }
        }

        public static string FormatXmlString(string xmlString)
        {
            System.Xml.Linq.XElement element = System.Xml.Linq.XElement.Parse(xmlString);
            return element.ToString();
        }

        private static XElement RemoveAllNamespaces(this XElement xmlDocument)
        {
            if (!xmlDocument.HasElements)
            {
                XElement xElement = new XElement(xmlDocument.Name.LocalName);
                xElement.Value = xmlDocument.Value;
                return xElement;
            }
            return new XElement(xmlDocument.Name.LocalName, xmlDocument.Elements().Select(el => RemoveAllNamespaces(el)));
        }

        public static XElement RemoveNamespacesByPrefix(this XElement xmlDocument, string prefix)
        {
            if (!xmlDocument.HasElements)
            {
                XElement xElement = new XElement(xmlDocument.Name.LocalName);
                xElement.Value = xmlDocument.Value;
                if (xmlDocument.HasAttributes)
                {
                    foreach (XAttribute xatr in xmlDocument.Attributes())
                    {

                        if (!xatr.Name.ToString().StartsWith(prefix))
                        {
                            xElement.SetAttributeValue(xatr.Name, xatr.Value);
                        }

                    }
                }
                return xElement;
            }
            XElement newXElement = new XElement(xmlDocument.Name.LocalName, xmlDocument.Elements().Select(el => RemoveNamespacesByPrefix(el, prefix)));
            if (xmlDocument.HasAttributes)
            {
                foreach (XAttribute xatr in xmlDocument.Attributes())
                {

                    if (!xatr.Name.ToString().StartsWith(prefix))
                    {
                        newXElement.SetAttributeValue(xatr.Name, xatr.Value);
                    }

                }
            }
            return newXElement;
        }

        public static XElement RemoveNamespacesByUri(this XElement xmlDocument, string uri)
        {
            if (!xmlDocument.HasElements)
            {
                XElement xElement = new XElement(xmlDocument.Name.LocalName);
                xElement.Value = xmlDocument.Value;
                if (xmlDocument.HasAttributes)
                {
                    foreach (XAttribute xatr in xmlDocument.Attributes())
                    {

                        if (!(xatr.Value.ToString() == uri))
                        {
                            xElement.SetAttributeValue(xatr.Name, xatr.Value);
                        }

                    }
                }
                return xElement;
            }
            XElement newXElement = new XElement(xmlDocument.Name.LocalName, xmlDocument.Elements().Select(el => RemoveNamespacesByUri(el, uri)));
            if (xmlDocument.HasAttributes)
            {
                foreach (XAttribute xatr in xmlDocument.Attributes())
                {

                    if (!(xatr.Value.ToString() == uri))
                    {
                        newXElement.SetAttributeValue(xatr.Name, xatr.Value);
                    }

                }
            }
            return newXElement;
        }

        public static string InnerXml(this XElement element)
        {
            StringBuilder innerXml = new StringBuilder();
            element = element.RemoveAllNamespaces();
            foreach (XNode node in element.Nodes())
            {
                innerXml.Append(node.ToString());
            }
            return innerXml.ToString();
        }

        public static XmlNode AddElementConditionally(this XmlNode parent, DictionaryEx<string, object> dict, String name, Object value)
        {
            if ((dict == null) || (!dict.ContainsKey(name)))
            {
                return AddElement(parent, name, value);
            }
            else return null;
        }

        private static XmlNode FindChildNodeByName(this XmlNode parent, String name)
        {
            XmlNode result = null;
            if (parent.HasChildNodes)
            {
                foreach (XmlNode node in parent.ChildNodes)
                {
                    if (node.Name == name)
                    {
                        result = node;
                    }
                }
            }
            return result;
        }

        public static XmlNode AddElement(this XmlNode parent, String name, Object value)
        {
            XmlElement newNode = parent.OwnerDocument.CreateElement(name);
            if (value != null)
            {
                if (value is String)
                    newNode.AppendChild(parent.OwnerDocument.CreateTextNode(value.ToString()));
                else if (value is XElement)
                    newNode.InnerXml = (value as XElement).InnerXml();
                else
                    newNode.InnerText = value.ToString();
            }
            parent.AppendChild(newNode);

            return newNode;
        }

        public static XmlNode AddOrUpdateElement(this XmlNode parent, String name, Object value)
        {
            XmlNode node = parent.FindChildNodeByName(name);
            if (node == null)
                return parent.AddElement(name, value);

            if (value != null)
            {
                if (value is String)
                    node.FirstChild.Value = value as String;
                else if (value is XElement)
                    node.InnerXml = (value as XElement).InnerXml();
                else
                    node.InnerText = value.ToString();
            }

            return node;
        }

        public static XmlNode AddElementIfNotExists(this XmlNode parent, String name, Object value)
        {
            XmlNode node = parent.FindChildNodeByName(name);
            if (node != null)
                return node;
            return parent.AddElement(name, value);
        }

        public static XmlNode AddElement(this XmlNode parent, String name)
        {
            XmlElement newNode = parent.OwnerDocument.CreateElement(name);
            parent.AppendChild(newNode);
            return newNode;
        }

        public static String ToXmlDate(this DateTime date)
        {
            return date.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffffzzz");

        }

        public static string EnumToString(this Enum en)
        {
            Type type = en.GetType();
            MemberInfo[] memInfo = type.GetMember(en.ToString());
            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(StringValue), false);
                if (attrs != null && attrs.Length > 0)
                    return ((StringValue)attrs[0]).Value;
            }
            return en.ToString();
        }

    }
}

