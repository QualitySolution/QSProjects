using System;

namespace QS.Report
{
	[AttributeUsage(AttributeTargets.Field)]
	public class ReportIdentifierAttribute : Attribute
	{
		public string Identifier { get; set; }

		public ReportIdentifierAttribute(string identifier)
		{
			Identifier = identifier;
		}
	}
}
