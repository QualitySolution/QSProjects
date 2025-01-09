using QS.Banks.Contracts;
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace QSBanks.CBRSource
{
	public class BIC
	{
		private ED807 ed807;

		public BICDirectoryEntryType[] BICDirectoryEntry => ed807.BICDirectoryEntry;
		public ReasonCodeType CreationReason => ed807.CreationReason;
		public DateTime CreationDateTime => ed807.CreationDateTime;
		public RequestCodeType InfoTypeCode => ed807.InfoTypeCode;
		public DateTime BusinessDay => ed807.BusinessDay;
		public string DirectoryVersion => ed807.DirectoryVersion;

		private BIC(ED807 ed)
		{
			ed807 = ed ?? throw new ArgumentNullException();
		}

		public static BIC GetBICDocument(Stream xmlData)
		{
			XmlSerializer ser = new XmlSerializer(typeof(ED807));
			XmlReader reader = XmlReader.Create(xmlData);
			ED807 ed = ser.Deserialize(reader) as ED807;
			return new BIC(ed);
		}
	}
}
