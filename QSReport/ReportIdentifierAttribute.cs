using System;

namespace QSReport
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