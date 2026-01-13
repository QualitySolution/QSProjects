using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

[Serializable]
[XmlType(AnonymousType=true)]
[XmlRoot("EnumRegions", Namespace = "", IsNullable = false)]
public class RegionsEnum
{
	[XmlAttribute("TotalRegions")]
	public int TotalRegions { get; set; }

	[XmlElement("ER", Form = XmlSchemaForm.Unqualified)]
	public RegionsEnumRGID[] Items { get; set; }

	public static RegionsEnum GetRegions()
	{
		using(var coi = new CreditOrgInfo())
		{
			XmlNode regionsXML = coi.EnumRegionsXML();
			XmlSerializer ser = new XmlSerializer(typeof(RegionsEnum));

			using(var reader = new XmlNodeReader(regionsXML))
			{
				return ser.Deserialize(reader) as RegionsEnum;
			}
		}
	}
}

[Serializable]
[XmlType(AnonymousType = true)]
public class RegionsEnumRGID 
{
	private string _cname;

	[XmlElement("rgn", Form = XmlSchemaForm.Unqualified)]
	public decimal RegCode { get; set; }

	[XmlIgnore]
	public bool RegCodeSpecified { get; set; }

	[XmlElement("Name", Form = XmlSchemaForm.Unqualified)]
	public string CNAME
	{
		get => _cname;
		set => _cname = value?.Trim();
	}
}
