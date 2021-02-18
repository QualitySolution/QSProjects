// 
// Schema XSD source https://www.cbr.ru/StaticHtml/File/50736/UFEBS_v2019_1_1.zip
//

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
		public bool BusinessDaySpecified => ed807.BusinessDaySpecified;
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

	[Serializable]
	[XmlType(Namespace = "urn:cbr-ru:ed:v2.0")]
	[XmlRoot(Namespace = "urn:cbr-ru:ed:v2.0", IsNullable = false)]
	public class ED807 : ESIDWithPartInfo
	{
		[XmlElement("BICDirectoryEntry")]
		public BICDirectoryEntryType[] BICDirectoryEntry { get; set; }
		[XmlAttribute]
		public ReasonCodeType CreationReason { get; set; }
		[XmlAttribute]
		public DateTime CreationDateTime { get; set; }
		[XmlAttribute]
		public RequestCodeType InfoTypeCode { get; set; }
		[XmlAttribute(DataType = "date")]
		public DateTime BusinessDay { get; set; }
		[XmlIgnore]
		public bool BusinessDaySpecified { get; set; }
		[XmlAttribute(DataType = "integer")]
		public string DirectoryVersion { get; set; }
	}

	[Serializable]
	[XmlType(Namespace = "urn:cbr-ru:ed:v2.0")]
	public class BICDirectoryEntryType
	{
		public ParticipantInfoType ParticipantInfo { get; set; }
		[XmlElement("SWBICS")]
		public SWBICList[] SWBICS { get; set; }
		[XmlElement("Accounts")]
		public AccountsType[] Accounts { get; set; }
		[XmlAttribute]
		public string BIC { get; set; }
		[XmlAttribute]
		public ChangeType ChangeType { get; set; }
		[XmlIgnore]
		public bool ChangeTypeSpecified { get; set; }
	}

	[Serializable]
	[XmlType(Namespace = "urn:cbr-ru:ed:v2.0")]
	public class ParticipantInfoType
	{
		public ParticipantInfoType()
		{
			this.NPSParticipant = true;
		}
		[XmlElement("RstrList")]
		public RstrListType[] RstrList { get; set; }
		[XmlAttribute]
		public string NameP { get; set; }
		[XmlAttribute]
		public string EnglName { get; set; }
		[XmlAttribute]
		public string RegN { get; set; }
		[XmlAttribute]
		public string CntrCd { get; set; }
		[XmlAttribute]
		public string Rgn { get; set; }
		[XmlAttribute]
		public string Ind { get; set; }
		[XmlAttribute]
		public string Tnp { get; set; }
		[XmlAttribute]
		public string Nnp { get; set; }
		[XmlAttribute]
		public string Adr { get; set; }
		[XmlAttribute]
		public string PrntBIC { get; set; }
		[XmlAttribute(DataType = "date")]
		public DateTime DateIn { get; set; }
		[XmlAttribute(DataType = "date")]
		public DateTime DateOut { get; set; }
		[XmlIgnore]
		public bool DateOutSpecified { get; set; }
		[XmlAttribute]
		public string PtType { get; set; }
		[XmlAttribute]
		public string Srvcs { get; set; }
		[XmlAttribute]
		public string XchType { get; set; }
		[XmlAttribute]
		public string UID { get; set; }
		[XmlAttribute]
		public bool NPSParticipant { get; set; }
		[XmlAttribute(DataType = "date")]
		public DateTime ToNPSDate { get; set; }
		[XmlIgnore]
		public bool ToNPSDateSpecified { get; set; }
		[XmlAttribute]
		public ParticipantStatusType ParticipantStatus { get; set; }
		[XmlIgnore]
		public bool ParticipantStatusSpecified { get; set; }
	}

	[Serializable]
	[XmlType(Namespace = "urn:cbr-ru:ed:v2.0")]
	public class RstrListType
	{
		[XmlAttribute]
		public RstrType Rstr { get; set; }
		[XmlAttribute(DataType = "date")]
		public DateTime RstrDate { get; set; }
	}

	[Serializable]
	[XmlType(Namespace = "urn:cbr-ru:ed:leaftypes:v2.0")]
	public enum RstrType
	{
		NORS,
		URRS,
		LWRS,
		LMRS,
		CLRS,
		FPRS,
		MRTR,
	}

	[Serializable]
	[XmlType(Namespace = "urn:cbr-ru:ed:v2.0")]
	public class InitialEDInfo
	{
		public EDRefID EDRefID { get; set; }
		[XmlAttribute]
		public string PacketStatus { get; set; }
	}

	[XmlInclude(typeof(ED))]
	[XmlInclude(typeof(PacketEDWithPartInfo))]
	[XmlInclude(typeof(ESWithMandatoryEDReceiver))]
	[XmlInclude(typeof(ESWithEDSender))]
	[XmlInclude(typeof(ESID))]
	[XmlInclude(typeof(ESIDResponse))]
	[XmlInclude(typeof(ESIDWithPartInfo))]
	[XmlInclude(typeof(ED807))]
	[XmlInclude(typeof(EPDComplete))]
	[Serializable]
	[XmlType(Namespace = "urn:cbr-ru:ed:v2.0")]
	public class EDRefID
	{
		[XmlAttribute(DataType = "integer")]
		public string EDNo { get; set; }
		[XmlAttribute(DataType = "date")]
		public DateTime EDDate { get; set; }
		[XmlAttribute]
		public string EDAuthor { get; set; }
	}

	[XmlInclude(typeof(PacketEDWithPartInfo))]
	[XmlInclude(typeof(ESWithMandatoryEDReceiver))]
	[XmlInclude(typeof(ESWithEDSender))]
	[XmlInclude(typeof(ESID))]
	[XmlInclude(typeof(ESIDResponse))]
	[XmlInclude(typeof(ESIDWithPartInfo))]
	[XmlInclude(typeof(ED807))]
	[XmlInclude(typeof(EPDComplete))]
	[Serializable]
	[XmlType(Namespace = "urn:cbr-ru:ed:v2.0")]
	public class ED : EDRefID
	{
		[XmlAnyElement]
		public XmlElement[] Any { get; set; }
	}

	[Serializable]
	[XmlType(Namespace = "urn:cbr-ru:ed:v2.0")]
	public class PacketEDWithPartInfo : ED
	{
		public PartInfo PartInfo { get; set; }
		public EDRefID InitialED { get; set; }
	}

	[Serializable]
	[XmlType(Namespace = "urn:cbr-ru:ed:v2.0")]
	public class PartInfo
	{
		[XmlAttribute(DataType = "integer")]
		public string PartNo { get; set; }
		[XmlAttribute(DataType = "integer")]
		public string PartQuantity { get; set; }
		[XmlAttribute]
		public string PartAggregateID { get; set; }
	}

	[XmlInclude(typeof(ESWithEDSender))]
	[Serializable]
	[XmlType(Namespace = "urn:cbr-ru:ed:v2.0")]
	public class ESWithMandatoryEDReceiver : ED
	{
		[XmlAttribute]
		public string EDReceiver { get; set; }
	}

	[Serializable]
	[XmlType(Namespace = "urn:cbr-ru:ed:v2.0")]
	public class ESWithEDSender : ESWithMandatoryEDReceiver
	{
		[XmlAttribute]
		public string EDSender { get; set; }
	}

	[XmlInclude(typeof(ESIDResponse))]
	[XmlInclude(typeof(ESIDWithPartInfo))]
	[XmlInclude(typeof(ED807))]
	[Serializable]
	[XmlType(Namespace = "urn:cbr-ru:ed:v2.0")]
	public class ESID : ED
	{
		[XmlAttribute]
		public string EDReceiver { get; set; }
	}

	[Serializable]
	[XmlType(Namespace = "urn:cbr-ru:ed:v2.0")]
	public class ESIDResponse : ESID
	{
		public InitialEDInfo InitialEDInfo { get; set; }
		public EDRefID InitialED { get; set; }
	}

	[XmlInclude(typeof(ED807))]
	[Serializable]
	[XmlType(Namespace = "urn:cbr-ru:ed:v2.0")]
	public class ESIDWithPartInfo : ESID
	{
		public PartInfo PartInfo { get; set; }
		public EDRefID InitialED { get; set; }
	}

	[Serializable]
	[XmlType(Namespace = "urn:cbr-ru:ed:v2.0")]
	public class EPDComplete : ED
	{
		public SettleNotEarlier SettleNotEarlier { get; set; }
		public SettleNotLater SettleNotLater { get; set; }
		public AccDocRefID AccDoc { get; set; }
		public PayerRU Payer { get; set; }
		public PayeeRU Payee { get; set; }
		[XmlAttribute]
		public string EDReceiver { get; set; }
		[XmlAttribute]
		public string PaytKind { get; set; }
		[XmlAttribute(DataType = "integer")]
		public string Sum { get; set; }
		[XmlAttribute]
		public string PaymentPrecedence { get; set; }
		[XmlAttribute(DataType = "date")]
		public DateTime ReqSettlementDate { get; set; }
		[XmlIgnore]
		public bool ReqSettlementDateSpecified { get; set; }
	}

	[Serializable]
	[XmlType(Namespace = "urn:cbr-ru:ed:v2.0")]
	public class SettleNotEarlier
	{
		[XmlElement("SessionID", typeof(string))]
		[XmlElement("SettlementTime", typeof(DateTime), DataType = "time")]
		public object Item { get; set; }
	}

	[Serializable]
	[XmlType(Namespace = "urn:cbr-ru:ed:v2.0")]
	public class SettleNotLater
	{
		[XmlElement(DataType = "time")]
		public DateTime SettlementTime { get; set; }
	}

	[Serializable]
	[XmlType(Namespace = "urn:cbr-ru:ed:v2.0")]
	public class AccDocRefID
	{
		[XmlAttribute]
		public string AccDocNo { get; set; }
		[XmlAttribute(DataType = "date")]
		public DateTime AccDocDate { get; set; }
	}

	[Serializable]
	[XmlType(Namespace = "urn:cbr-ru:ed:v2.0")]
	public class PayerRU
	{
		public string Name { get; set; }
		public BankRU Bank { get; set; }
		[XmlAttribute]
		public string PersonalAcc { get; set; }
		[XmlAttribute]
		public string INN { get; set; }
		[XmlAttribute]
		public string KPP { get; set; }
	}

	[Serializable]
	[XmlType(Namespace = "urn:cbr-ru:ed:v2.0")]
	public class BankRU
	{
		[XmlAttribute]
		public string BIC { get; set; }
		[XmlAttribute]
		public string CorrespAcc { get; set; }
	}

	[Serializable]
	[XmlType(Namespace = "urn:cbr-ru:ed:v2.0")]
	public class PayeeRU
	{
		public string Name { get; set; }
		public BankRU Bank { get; set; }
		[XmlAttribute]
		public string PersonalAcc { get; set; }
		[XmlAttribute]
		public string INN { get; set; }
		[XmlAttribute]
		public string KPP { get; set; }
	}

	[Serializable]
	[XmlType(Namespace = "urn:cbr-ru:ed:v2.0")]
	public class AccRstrListType
	{
		[XmlAttribute]
		public RstrType AccRstr { get; set; }
		[XmlAttribute(DataType = "date")]
		public DateTime AccRstrDate { get; set; }
	}

	[Serializable]
	[XmlType(Namespace = "urn:cbr-ru:ed:v2.0")]
	public class AccountsType
	{
		[XmlElement("AccRstrList")]
		public AccRstrListType[] AccRstrList { get; set; }
		[XmlAttribute]
		public string Account { get; set; }
		[XmlAttribute]
		public AccountType RegulationAccountType { get; set; }
		[XmlAttribute]
		public string CK { get; set; }
		[XmlAttribute]
		public string AccountCBRBIC { get; set; }
		[XmlAttribute(DataType = "date")]
		public DateTime DateIn { get; set; }
		[XmlAttribute(DataType = "date")]
		public DateTime DateOut { get; set; }
		[XmlIgnore]
		public bool DateOutSpecified { get; set; }
		[XmlAttribute]
		public AccountStatusType AccountStatus { get; set; }
		[XmlIgnore]
		public bool AccountStatusSpecified { get; set; }
	}

	[Serializable]
	[XmlType(Namespace = "urn:cbr-ru:ed:leaftypes:v2.0")]
	public enum AccountType
	{
		BANA,
		CBRA,
		CRSA,
		TRSA,
		CLRA,
		EPGA,
		EPSA,
		GARA,
		TRUA,
		UTRA,
		CLAC,
	}

	[Serializable]
	[XmlType(Namespace = "urn:cbr-ru:ed:leaftypes:v2.0")]
	public enum AccountStatusType
	{
		ACAC,
		ACDL,
	}

	[Serializable]
	[XmlType(Namespace = "urn:cbr-ru:ed:v2.0")]
	public class SWBICList
	{
		[XmlAttribute]
		public string SWBIC { get; set; }
		[XmlAttribute]
		public bool DefaultSWBIC { get; set; }
	}

	[Serializable]
	[XmlType(Namespace = "urn:cbr-ru:ed:leaftypes:v2.0")]
	public enum ParticipantStatusType
	{
		PSAC,
		PSDL,
	}

	[Serializable]
	[XmlType(Namespace = "urn:cbr-ru:ed:leaftypes:v2.0")]
	public enum ChangeType
	{
		ADDD,
		CHGD,
		NCNG,
		DLTD,
	}

	[Serializable]
	[XmlType(Namespace = "urn:cbr-ru:ed:leaftypes:v2.0")]
	public enum ReasonCodeType
	{
		ACCH,
		AICH,
		ALCH,
		ANTC,
		APPA,
		RIRA,
		RIRP,
		RMAA,
		RMVA,
		RQST,
		SMON,
		SOBD,
		SPOF,
		UIRA,
		UIRP,
		ARRD,
		ARRM,
		ARRS,
		EOBD,
		EOCC,
		ICLD,
		ICLM,
		ICLS,
		PCHD,
		CSCH,
		NSCH,
		FCBD,
		CIBD,
		PPAD,
	}

	[Serializable]
	[XmlType(Namespace = "urn:cbr-ru:ed:leaftypes:v2.0")]
	public enum RequestCodeType
	{
		FIRR,
		SIRR,
		PROF,
	}
}