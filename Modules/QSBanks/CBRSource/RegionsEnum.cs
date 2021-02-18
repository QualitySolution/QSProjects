using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

[Serializable()]
[XmlType(AnonymousType=true)]
[XmlRoot(Namespace="", IsNullable=false)]
public class RegionsEnum
{
	[XmlElement("RGID", Form = XmlSchemaForm.Unqualified)]
	public RegionsEnumRGID[] Items { get; set; }

	public static RegionsEnum GetRegions()
	{
		CreditOrgInfo coi = new CreditOrgInfo();
		XmlNode regionsXML = coi.RegionsEnumXML();
		XmlSerializer ser = new XmlSerializer(typeof(RegionsEnum));
		XmlNodeReader reader = new XmlNodeReader(regionsXML);
		return ser.Deserialize(reader) as RegionsEnum;
	}
}

[Serializable()]
[XmlType(AnonymousType=true)]
public class RegionsEnumRGID
{
	[XmlElement(Form = XmlSchemaForm.Unqualified)]
	public decimal RegCode { get; set; }

	[XmlIgnore()]
	public bool RegCodeSpecified { get; set; }

	[XmlElement(Form = XmlSchemaForm.Unqualified)]
	public string CNAME { get; set; }
}
