using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using ICSharpCode.SharpZipLib.Zip;
using NLog;
using QS.Utilities;

namespace QS.DocTemplates
{

	public class OdtWorks
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		public IDocParser DocParser;
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

			//Добавляем пользовательские поля
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
				
			foreach(PatternField field in DocParser.FieldsList)
			{
				AddFieldDecl(field, VariableType.User, content, fieldsDels, existFilds);
			}

			existFilds = new List<string> ();
			foreach(XmlNode node in content.SelectNodes ("/office:document-content/office:body/office:text/text:variable-decls/text:variable-decl", nsMgr))
			{
				existFilds.Add (node.Attributes["text:name"].Value);
			}

			fieldsDels = (XmlElement)content.SelectSingleNode ("/office:document-content/office:body/office:text/text:variable-decls", nsMgr);

			if(fieldsDels == null)
			{
				fieldsDels = content.CreateElement ("text", "variable-decls", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
				XmlElement officeText = (XmlElement)content.SelectSingleNode ("/office:document-content/office:body/office:text", nsMgr);
				XmlElement userFieldDecls = (XmlElement)content.SelectSingleNode ("/office:document-content/office:body/office:text/text:user-field-decls", nsMgr);

				officeText.InsertAfter(fieldsDels, userFieldDecls);
			}

			//Добавляем поля таблиц
			foreach(PatternField field in DocParser.TablesFields)
			{
				AddFieldDecl(field, VariableType.Simple, content, fieldsDels, existFilds);
			}

			UpdateXmlDocument (content, contentFileName);
		}

		private void AddFieldDecl(PatternField field, VariableType type, XmlDocument content, XmlElement fieldsDels, List<string> existFilds)
		{
			var declNode = type == VariableType.User ? "user-field-decl" : "variable-decl";
			if (field.Type == PatternFieldType.FString) {

				if (existFilds.Contains (field.Name))
					return;

				XmlElement newFieldNode = content.CreateElement ("text", declNode, "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
				newFieldNode.SetAttribute ("value-type", "urn:oasis:names:tc:opendocument:xmlns:office:1.0", "string");
				newFieldNode.SetAttribute ("name", "urn:oasis:names:tc:opendocument:xmlns:text:1.0", field.Name);
				if(type == VariableType.User)
					newFieldNode.SetAttribute ("string-value", "urn:oasis:names:tc:opendocument:xmlns:office:1.0", "");
				fieldsDels.AppendChild (newFieldNode);
			}
			else if (field.Type == PatternFieldType.FDate) {

				if (existFilds.Contains (field.Name))
					return;

				XmlElement newFieldNode = content.CreateElement ("text", declNode, "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
				newFieldNode.SetAttribute ("value-type", "urn:oasis:names:tc:opendocument:xmlns:office:1.0", "string");
				//newFieldNode.SetAttribute ("value-type", "urn:oasis:names:tc:opendocument:xmlns:office:1.0", "date");
				//newFieldNode.SetAttribute ("date-value", "urn:oasis:names:tc:opendocument:xmlns:office:1.0", "");
				newFieldNode.SetAttribute ("name", "urn:oasis:names:tc:opendocument:xmlns:text:1.0", field.Name);
				if(type == VariableType.User)
					newFieldNode.SetAttribute ("string-value", "urn:oasis:names:tc:opendocument:xmlns:office:1.0", "");
				
				fieldsDels.AppendChild (newFieldNode);
			}
			else if(field.Type == PatternFieldType.FCurrency)
			{
				if (!existFilds.Contains (field.Name + ".Число")) 
				{
					XmlElement newFieldNode = content.CreateElement ("text", declNode, "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
					newFieldNode.SetAttribute ("value-type", "urn:oasis:names:tc:opendocument:xmlns:office:1.0", "currency");
					string curr = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol;
					newFieldNode.SetAttribute ("currency", "urn:oasis:names:tc:opendocument:xmlns:office:1.0", curr);
					newFieldNode.SetAttribute ("name", "urn:oasis:names:tc:opendocument:xmlns:text:1.0", field.Name + ".Число");
					if(type == VariableType.User)
						newFieldNode.SetAttribute ("value", "urn:oasis:names:tc:opendocument:xmlns:office:1.0", "0");
					
					fieldsDels.AppendChild (newFieldNode);
				}
				if (!existFilds.Contains (field.Name + ".Пропись")) 
				{
					XmlElement newFieldNode = content.CreateElement ("text", declNode, "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
					newFieldNode.SetAttribute ("value-type", "urn:oasis:names:tc:opendocument:xmlns:office:1.0", "string");
					newFieldNode.SetAttribute ("name", "urn:oasis:names:tc:opendocument:xmlns:text:1.0", field.Name + ".Пропись");
					if(type == VariableType.User)
						newFieldNode.SetAttribute ("string-value", "urn:oasis:names:tc:opendocument:xmlns:office:1.0", "");
					
					fieldsDels.AppendChild (newFieldNode);
				}
			}
			else if (field.Type == PatternFieldType.FAutoRowNumber || field.Type == PatternFieldType.FNumber) {

				if (existFilds.Contains (field.Name))
					return;

				XmlElement newFieldNode = content.CreateElement ("text", declNode, "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
				newFieldNode.SetAttribute ("value-type", "urn:oasis:names:tc:opendocument:xmlns:office:1.0", "float");
				newFieldNode.SetAttribute ("name", "urn:oasis:names:tc:opendocument:xmlns:text:1.0", field.Name);
				fieldsDels.AppendChild (newFieldNode);
			}
		}

		public void FillValues()
		{
			logger.Info ("Заполняем поля документа...");
			XmlDocument content = GetXMLDocument("content.xml");
			XmlNamespaceManager nsMgr = new XmlNamespaceManager(content.NameTable);
			nsMgr.AddNamespace("office", "urn:oasis:names:tc:opendocument:xmlns:office:1.0");
			nsMgr.AddNamespace("text", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
			nsMgr.AddNamespace("table", "urn:oasis:names:tc:opendocument:xmlns:table:1.0");

			foreach (XmlNode node in content.SelectNodes ("/office:document-content/office:body/office:text/text:user-field-decls/text:user-field-decl", nsMgr)) {
				string fieldName = node.Attributes ["text:name"].Value;
				var field = DocParser.FieldsList.FirstOrDefault(f => fieldName == f.Name) ?? DocParser.FieldsList.Find (f => fieldName.StartsWith (f.Name));
				if (field == null) {
					logger.Warn ("Поле {0} не найдено, поэтому пропущено.", fieldName);
					continue;
				}
				SetFieldValue(node, field.Type, field.Value);
			}

			//Строка и поле
			var tableFields = new List<odtFieldSet>();

			foreach (XmlNode node in content.SelectNodes ("//table:table-row//text:variable-set", nsMgr))
			{
				string fieldName = node.Attributes["text:name"].Value;
				//Ищем в таблицах
				var field = DocParser.TablesFields.FirstOrDefault(f => fieldName.StartsWith(f.Name));
				if (field != null)
				{
					var fieldSet = new odtFieldSet();
					fieldSet.Field = field;
					fieldSet.FieldNode = node;
					fieldSet.RowNode = FineParent(node, "table:table-row");
					fieldSet.TableNode = FineParent(node, "table:table");
					tableFields.Add(fieldSet);
				}
			}

			if(tableFields.Count > 0)
			{
				//Удаляем уже заполненные строки.
				var toDelete = new List<odtFieldSet>();
				foreach(var table in tableFields.GroupBy(x => x.TableNode))
				{
					XmlNode firstRow = null;
					foreach(var byTable in table)
					{
						if (firstRow == null)
							firstRow = byTable.RowNode;
						else if(firstRow != byTable.RowNode)
						{
							toDelete.Add(byTable);
						}
					}
				}
				foreach(var delete in toDelete)
				{
					tableFields.Remove(delete);
				}

				foreach(var row in toDelete.GroupBy(x => x.RowNode))
				{
					row.First().TableNode.RemoveChild(row.Key);
				}
				//Заполняем поля добавляя строки
				foreach(var table in tableFields.GroupBy(x => x.TableNode))
				{
					XmlNode curentRow = table.First().RowNode;
					var rowCounts = table.First().Field.DataTable.DataRowsCount;

					for(int i = 0; i < rowCounts; i++)
					{
						foreach(XmlNode fieldNode in curentRow.SelectNodes(".//text:variable-set", nsMgr))
						{
							string fieldName = fieldNode.Attributes["text:name"].Value;
							var info = table.FirstOrDefault(x => fieldName.StartsWith(x.Field.Name));
							if(info != null)
							{
								SetFieldVariableValue(fieldNode, info.Field.Type, info.Field.GetValue(i));
							}
						}
						if(i + 1 < rowCounts)
						{//Создаем новую строку
							var newRow = curentRow.CloneNode(true);
							var tableNode = table.First().TableNode;
							tableNode.InsertAfter(newRow, curentRow);
							curentRow = newRow;
						}
					}
				}

			}

			UpdateXmlDocument (content, contentFileName);
		}

		void SetFieldValue(XmlNode node, PatternFieldType type, object value)
		{
			var element = (XmlElement)node;
			if (type == PatternFieldType.FDate) // && node.Attributes ["office:date-value"] != null)
				node.Attributes["office:string-value"].Value = value != null ? ((DateTime)value).ToLongDateString() : String.Empty;
			//node.Attributes ["office:date-value"].Value = field.value != DBNull.Value ? XmlConvert.ToString ((DateTime)field.value, XmlDateTimeSerializationMode.Unspecified) : "";
			else if (type == PatternFieldType.FCurrency)
			{
				decimal valueDec = value != null ? (decimal)value : Decimal.Zero;
				string fieldName = node.Attributes["text:name"].Value;
				if (fieldName.EndsWith(".Число"))
				{						
					((XmlElement)node).SetAttribute("value-type", "urn:oasis:names:tc:opendocument:xmlns:office:1.0", "currency");
					((XmlElement)node).SetAttribute("value", "urn:oasis:names:tc:opendocument:xmlns:office:1.0", XmlConvert.ToString(valueDec));
					string curr = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol;
					((XmlElement)node).SetAttribute("currency", "urn:oasis:names:tc:opendocument:xmlns:office:1.0", curr);
				}
				if (fieldName.EndsWith(".Пропись"))
				{						
					string val = NumberToTextRus.Str((int)valueDec, true, "рубль", "рубля", "рублей");
					node.Attributes["office:string-value"].Value = val;
				}
			}
			else if(type == PatternFieldType.FNumber)
			{
				element.SetAttribute("value", "urn:oasis:names:tc:opendocument:xmlns:office:1.0", value?.ToString());
			}
			else
				element.SetAttribute("string-value", "urn:oasis:names:tc:opendocument:xmlns:office:1.0", value?.ToString()); //value != null ? value.ToString () : String.Empty);
		}

		void SetFieldVariableValue(XmlNode node, PatternFieldType type, object value)
		{
			var element = (XmlElement)node;

			if (type == PatternFieldType.FDate)
				throw new NotImplementedException();
				//node.Attributes["office:string-value"].Value = value != null ? ((DateTime)value).ToLongDateString() : String.Empty;
			//node.Attributes ["office:date-value"].Value = field.value != DBNull.Value ? XmlConvert.ToString ((DateTime)field.value, XmlDateTimeSerializationMode.Unspecified) : "";
			else if (type == PatternFieldType.FCurrency)
			{
				decimal valueDec = value != null ? (decimal)value : Decimal.Zero;
				string fieldName = node.Attributes["text:name"].Value;
				if (fieldName.EndsWith(".Число"))
				{						
					((XmlElement)node).SetAttribute("value-type", "urn:oasis:names:tc:opendocument:xmlns:office:1.0", "currency");
					((XmlElement)node).SetAttribute("value", "urn:oasis:names:tc:opendocument:xmlns:office:1.0", XmlConvert.ToString(valueDec));
					string curr = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol;
					((XmlElement)node).SetAttribute("currency", "urn:oasis:names:tc:opendocument:xmlns:office:1.0", curr);
					element.InnerText = valueDec.ToString("C");
				}
				if (fieldName.EndsWith(".Пропись"))
				{						
					string val = NumberToTextRus.Str((int)valueDec, true, "рубль", "рубля", "рублей");
					node.Attributes["office:string-value"].Value = val;
					element.InnerText = val;
				}
			}
			else if(type == PatternFieldType.FAutoRowNumber || type == PatternFieldType.FNumber)
			{
				element.SetAttribute("value", "urn:oasis:names:tc:opendocument:xmlns:office:1.0", value?.ToString()); //value != null ? value.ToString () : String.Empty);
				element.InnerText = value?.ToString();
			}
			else
			{
				element.SetAttribute("string-value", "urn:oasis:names:tc:opendocument:xmlns:office:1.0", value?.ToString()); //value != null ? value.ToString () : String.Empty);
				element.InnerText = value?.ToString();
			}
				
		}

		private XmlNode FineParent(XmlNode node, string element)
		{
			if (node.ParentNode == null)
				return null;

			if (node.ParentNode.Name == element)
				return node.ParentNode;
			else
				return FineParent(node.ParentNode, element);
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

		enum VariableType{
			Simple,
			User
		}

		class odtFieldSet{
			public XmlNode RowNode;
			public XmlNode TableNode;
			public XmlNode FieldNode;
			public IPatternTableField Field;
		}
	}

}

