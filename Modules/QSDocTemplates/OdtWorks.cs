using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip;
using NLog;
using QSProjectsLib;
using System.Globalization;

namespace LeaseAgreement
{

	public class OdtWorks
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		public DocPattern DocInfo;
		private ZipFile odtZip;
		private MemoryStream OdtStream;
		private const string contentFileName = "content.xml";

		public OdtWorks (byte[] odtfile)
		{
			ZipConstants.DefaultCodePage = System.Text.Encoding.UTF8.CodePage;
			using (MemoryStream odtInStream = new MemoryStream (odtfile)) {
				byte[] buffer = new byte[4096];
				OdtStream = new MemoryStream ();
				ICSharpCode.SharpZipLib.Core.StreamUtils.Copy(odtInStream, OdtStream, buffer);
			}
			odtZip = new ZipFile (OdtStream);
		}

		public OdtWorks (Stream odtInStream)
		{
			ZipConstants.DefaultCodePage = System.Text.Encoding.UTF8.CodePage;
			byte[] buffer = new byte[4096];
			OdtStream = new MemoryStream ();
			ICSharpCode.SharpZipLib.Core.StreamUtils.Copy(odtInStream, OdtStream, buffer);
		
			odtZip = new ZipFile (OdtStream);
		}

		public OdtWorks (string odtFileName)
		{
			ZipConstants.DefaultCodePage = System.Text.Encoding.UTF8.CodePage;
			using (FileStream fs = new FileStream (odtFileName, FileMode.Open, FileAccess.Read)) {
				byte[] buffer = new byte[4096];
				OdtStream = new MemoryStream ();
				ICSharpCode.SharpZipLib.Core.StreamUtils.Copy(fs, OdtStream, buffer);
			}
			odtZip = new ZipFile (OdtStream);
		}

		public void UpdateFields()
		{
			XmlDocument content = GetXMLDocument(contentFileName);
			XmlNamespaceManager nsMgr = new XmlNamespaceManager(content.NameTable);
			nsMgr.AddNamespace("office", "urn:oasis:names:tc:opendocument:xmlns:office:1.0");
			nsMgr.AddNamespace("text", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");

			List<string> existFilds = new List<string> ();
			foreach(XmlNode node in content.SelectNodes ("/office:document-content/office:body/office:text/text:user-field-decls/text:user-field-decl", nsMgr))
			{
				existFilds.Add (node.Attributes["text:name"].Value);
			}
				
			XmlElement fieldsDels = (XmlElement)content.SelectSingleNode ("/office:document-content/office:body/office:text/text:user-field-decls", nsMgr);

			if(fieldsDels == null)
			{
				fieldsDels = content.CreateElement ("text", "user-field-decls", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
				XmlElement officeText = (XmlElement)content.SelectSingleNode ("/office:document-content/office:body/office:text", nsMgr);
				XmlElement sequenceDecls = (XmlElement)content.SelectSingleNode ("/office:document-content/office:body/office:text/text:sequence-decls", nsMgr);

				officeText.InsertAfter(fieldsDels, sequenceDecls);
			}

			foreach(PatternField field in DocInfo.Fields)
			{

				if (field.Type == PatternFieldType.FString) {

					if (existFilds.Contains (field.Name))
						continue;

					XmlElement newFieldNode = content.CreateElement ("text", "user-field-decl", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
					newFieldNode.SetAttribute ("value-type", "urn:oasis:names:tc:opendocument:xmlns:office:1.0", "string");
					newFieldNode.SetAttribute ("string-value", "urn:oasis:names:tc:opendocument:xmlns:office:1.0", "");
					newFieldNode.SetAttribute ("name", "urn:oasis:names:tc:opendocument:xmlns:text:1.0", field.Name);
					fieldsDels.AppendChild (newFieldNode);
				}
				else if (field.Type == PatternFieldType.FDate) {

					if (existFilds.Contains (field.Name))
						continue;

					XmlElement newFieldNode = content.CreateElement ("text", "user-field-decl", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
					newFieldNode.SetAttribute ("value-type", "urn:oasis:names:tc:opendocument:xmlns:office:1.0", "string");
					newFieldNode.SetAttribute ("string-value", "urn:oasis:names:tc:opendocument:xmlns:office:1.0", "");
					//newFieldNode.SetAttribute ("value-type", "urn:oasis:names:tc:opendocument:xmlns:office:1.0", "date");
					//newFieldNode.SetAttribute ("date-value", "urn:oasis:names:tc:opendocument:xmlns:office:1.0", "");
					newFieldNode.SetAttribute ("name", "urn:oasis:names:tc:opendocument:xmlns:text:1.0", field.Name);
					fieldsDels.AppendChild (newFieldNode);
				}
				else if(field.Type == PatternFieldType.FCurrency)
				{
					if (!existFilds.Contains (field.Name + ".Число")) 
					{
						XmlElement newFieldNode = content.CreateElement ("text", "user-field-decl", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
						newFieldNode.SetAttribute ("value-type", "urn:oasis:names:tc:opendocument:xmlns:office:1.0", "currency");
						newFieldNode.SetAttribute ("value", "urn:oasis:names:tc:opendocument:xmlns:office:1.0", "0");
						string curr = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol;
						newFieldNode.SetAttribute ("currency", "urn:oasis:names:tc:opendocument:xmlns:office:1.0", curr);
						newFieldNode.SetAttribute ("name", "urn:oasis:names:tc:opendocument:xmlns:text:1.0", field.Name + ".Число");
						fieldsDels.AppendChild (newFieldNode);
					}
					if (!existFilds.Contains (field.Name + ".Пропись")) 
					{
						XmlElement newFieldNode = content.CreateElement ("text", "user-field-decl", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
						newFieldNode.SetAttribute ("value-type", "urn:oasis:names:tc:opendocument:xmlns:office:1.0", "string");
						newFieldNode.SetAttribute ("string-value", "urn:oasis:names:tc:opendocument:xmlns:office:1.0", "");
						newFieldNode.SetAttribute ("name", "urn:oasis:names:tc:opendocument:xmlns:text:1.0", field.Name + ".Пропись");
						fieldsDels.AppendChild (newFieldNode);
					}
				}
			}
			UpdateXmlDocument (content, contentFileName);
		}

		public void FillValues()
		{
			logger.Info ("Заполняем поля документа...");
			XmlDocument content = GetXMLDocument("content.xml");
			XmlNamespaceManager nsMgr = new XmlNamespaceManager(content.NameTable);
			nsMgr.AddNamespace("office", "urn:oasis:names:tc:opendocument:xmlns:office:1.0");
			nsMgr.AddNamespace("text", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");

			foreach (XmlNode node in content.SelectNodes ("/office:document-content/office:body/office:text/text:user-field-decls/text:user-field-decl", nsMgr)) {
				string fieldName = node.Attributes ["text:name"].Value;
				PatternField field = DocInfo.Fields.Find (f => fieldName.StartsWith (f.Name));
				if (field == null) {
					logger.Warn ("Поле {0} не найдено, поэтому пропущено.", fieldName);
					continue;
				}
				if (field.Type == PatternFieldType.FDate) // && node.Attributes ["office:date-value"] != null)
					node.Attributes ["office:string-value"].Value = field.value != DBNull.Value ? ((DateTime)field.value).ToLongDateString () : "";
					//node.Attributes ["office:date-value"].Value = field.value != DBNull.Value ? XmlConvert.ToString ((DateTime)field.value, XmlDateTimeSerializationMode.Unspecified) : "";
				else if (field.Type == PatternFieldType.FCurrency) {
					decimal value = field.value != DBNull.Value ? (decimal)field.value : Decimal.Zero;
					if (fieldName.Replace (field.Name, "") == ".Число") {						
						((XmlElement)node).SetAttribute ("value-type", "urn:oasis:names:tc:opendocument:xmlns:office:1.0", "currency");
						((XmlElement)node).SetAttribute ("value", "urn:oasis:names:tc:opendocument:xmlns:office:1.0", XmlConvert.ToString (value));
						string curr = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol;
						((XmlElement)node).SetAttribute ("currency", "urn:oasis:names:tc:opendocument:xmlns:office:1.0", curr);
					}
					if (fieldName.Replace (field.Name, "") == ".Пропись") {						
						string val = RusCurrency.Str ((int)value, true, "рубль", "рубля", "рублей", "", "", "");
						node.Attributes ["office:string-value"].Value = val;
					}
				} else
					node.Attributes ["office:string-value"].Value = field.value.ToString ();
			}	
			UpdateXmlDocument (content, contentFileName);
		}

		public LinkedList<XmlElement> GetXmlObjects(string nodeName,string attribute,string value)
		{
			var result = new LinkedList<XmlElement> ();
			XmlDocument content = GetXMLDocument (contentFileName);
			var nodelist = content.GetElementsByTagName (nodeName);
			foreach (XmlElement node in nodelist) {
				if (node.Attributes [attribute].Value.StartsWith (value, true, CultureInfo.InvariantCulture))
					result.AddLast (node);
			}
			return result;
		}

		public void ReplaceFrameContent(string frameName, byte[] image, string path){
			XmlDocument content = GetXMLDocument("content.xml");
			XmlNamespaceManager nsMgr = new XmlNamespaceManager(content.NameTable);
			nsMgr.AddNamespace("office", "urn:oasis:names:tc:opendocument:xmlns:office:1.0");
			nsMgr.AddNamespace("text", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
			nsMgr.AddNamespace ("fo", "urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0");
			nsMgr.AddNamespace ("xlink", "http://www.w3.org/1999/xlink");
			nsMgr.AddNamespace ("draw", "urn:oasis:names:tc:opendocument:xmlns:drawing:1.0");
			nsMgr.AddNamespace ("svg", "urn:oasis:names:tc:opendocument:xmlns:svg-compatible:1.0");
			var nodeList = content.GetElementsByTagName ("draw:frame");
			foreach (XmlElement frameNode in nodeList) {
				if (frameNode.Attributes ["draw:name"].Value == frameName) {
					var x = frameNode.Attributes ["svg:x"];
					var y = frameNode.Attributes ["svg:y"];
					var anchor = frameNode.Attributes ["text:anchor-type"];
					var height = frameNode.Attributes ["svg:height"];
					var width = frameNode.Attributes ["svg:width"];
					for(var frameChild = frameNode.FirstChild;frameChild!=null;frameChild=frameChild.NextSibling){
						if(height==null) height = frameChild.Attributes ["fo:min-height"];
						if(width ==null) width = frameChild.Attributes ["fo:min-width"];
						frameNode.RemoveChild (frameChild);
					}


					if(anchor!=null) 
						frameNode.SetAttribute ("anchor-type", nsMgr.LookupNamespace ("text"), anchor.Value);
					if(x!=null) 
						frameNode.SetAttribute ("x", nsMgr.LookupNamespace ("svg"), x.Value);
					if(y!=null)
						frameNode.SetAttribute ("y", nsMgr.LookupNamespace ("svg"), y.Value);
					if(height!=null)
						frameNode.SetAttribute ("height", nsMgr.LookupNamespace ("svg"), height.Value);
					if(width!=null)
						frameNode.SetAttribute ("width", nsMgr.LookupNamespace ("svg"), width.Value);

					XmlElement drawing = content.CreateElement ("draw:image",nsMgr.LookupNamespace("draw"));
					drawing.SetAttribute ("href", nsMgr.LookupNamespace("xlink"), path);
					drawing.SetAttribute ("show",nsMgr.LookupNamespace("xlink"),"embed");
					drawing.SetAttribute ("type",nsMgr.LookupNamespace("xlink"), "simple");
					drawing.SetAttribute ("actuate",nsMgr.LookupNamespace("xlink"), "onLoad");
					frameNode.AppendChild (drawing);
				}
			}
			UpdateXmlDocument (content, contentFileName);
			AddFile (image, path);
		}

		public void AddFile(byte[] data, string path)
		{
			XmlDocument manifest = GetXMLDocument ("META-INF/manifest.xml");
			XmlNamespaceManager nsManager = new XmlNamespaceManager (manifest.NameTable);
			nsManager.AddNamespace ("manifest", "urn:oasis:names:tc:opendocument:xmlns:manifest:1.0");
			XmlNode manifestNode = manifest.SelectSingleNode("/manifest:manifest",nsManager);
			XmlElement fileEntry = manifest.CreateElement ("manifest", "file-entry", nsManager.LookupNamespace("manifest"));
			fileEntry.SetAttribute ("full-path", nsManager.LookupNamespace ("manifest"),path);
			fileEntry.SetAttribute ("media-type", nsManager.LookupNamespace ("manifest"), "image/png");
			manifestNode.AppendChild(fileEntry);
			using (var fileStream = new MemoryStream (data)) {
				odtZip.BeginUpdate ();
				StreamStaticDataSource sds = new StreamStaticDataSource ();
				sds.SetStream (fileStream);
				odtZip.Add (sds, path);
				odtZip.CommitUpdate ();
			}
			UpdateXmlDocument (manifest, "META-INF/manifest.xml");
		}

		private XmlDocument GetXMLDocument(string path)
		{
			ZipEntry entry = odtZip.GetEntry (path);
			Stream contentStream = odtZip.GetInputStream (entry);
			XmlDocument document = new XmlDocument ();
			document.Load (contentStream);
			return document;
		}

		public void UpdateXmlDocument(XmlDocument document, string path)
		{
			using( MemoryStream outContentStream = new MemoryStream ()) {
				XmlWriterSettings xmlSettings = new XmlWriterSettings();
				xmlSettings.Indent = false;
				xmlSettings.NewLineChars = String.Empty;

				using (XmlWriter xmlWriter = XmlWriter.Create (outContentStream, xmlSettings))
					document.Save (xmlWriter);

				odtZip.BeginUpdate ();

				StreamStaticDataSource sds = new StreamStaticDataSource ();
				sds.SetStream (outContentStream);

				odtZip.Add (sds, path);
				odtZip.CommitUpdate ();
			}
		}

		public byte[] GetArray()
		{
			OdtStream.Position = 0;
			return OdtStream.ToArray ();
		}

		internal class StreamStaticDataSource : IStaticDataSource {
			private Stream _stream;
			// Implement method from IStaticDataSource
			public Stream GetSource() {
				return _stream;
			}

			// Call this to provide the memorystream
			public void SetStream(Stream inputStream) {
				_stream = inputStream;
				_stream.Position = 0;
			}
		}

		public void Close()
		{
			odtZip.Close ();
		}
	}

}

