using System;
using System.Collections.Generic;

namespace QSCustomFields
{
	public class CFMain
	{
		public static List<CFTable> Tables;
		public static string FieldsInfoTable = "custom_fields";

		public CFMain()
		{
		}
	}

	public class CFTable
	{
		public string Title;
		public string DBName;

		public CFTable(string dbname, string title)
		{
			Title = title;
			DBName = dbname;
		}
	}
}

