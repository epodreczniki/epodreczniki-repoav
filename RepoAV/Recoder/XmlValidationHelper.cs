using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Reflection;
using System.Xml;

namespace PSNC.RepoAV.Recoder
{
	public static class XmlValidationHelper
	{
		public static TRootElement DeserializeXml<TRootElement>(string xml, string xsdResourceName, string schemaUri, Assembly curAssembly, string nspace)
		{
			XmlSerializer serializer = new XmlSerializer(typeof(TRootElement));

			using (XmlReader reader = GetValidatingReader(xml, xsdResourceName, schemaUri, curAssembly, nspace))
				return (TRootElement)serializer.Deserialize(reader);
		}

		public static string SerializeXml<TRootElement>(TRootElement data, string xsdResourceName, string schemaUri)
		{
			XmlSerializer serializer = new XmlSerializer(typeof(TRootElement));
			StringWriter xmlStringWriter = new StringWriter();
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.NewLineOnAttributes = true;


			using (XmlWriter writer = XmlWriter.Create(xmlStringWriter, settings))
			{
				serializer.Serialize(writer, data);

				return xmlStringWriter.ToString();
			}
		}


		public static XmlReader GetValidatingReader(string xml, string xsdResourceName, string schemaUri, Assembly curAssembly, string nspace)
		{
			//typeof(XmlValidationHelper).Namespace
			//using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(String.Format("{1}.{0}", xsdResourceName, typeof(XmlValidationHelper).Namespace)))
			using (Stream stream = curAssembly.GetManifestResourceStream(String.Format("{1}.{0}", xsdResourceName, nspace)))
			{
				XmlTextReader schemaReader = new XmlTextReader(stream);

				XmlReaderSettings settings = new XmlReaderSettings();
				settings.Schemas.Add(schemaUri, schemaReader);
				StringReader xmlStringReader = new StringReader(xml);
				XmlReader catalogReader = XmlReader.Create(xmlStringReader, settings);

				return catalogReader;
			}
		}

	}

}
