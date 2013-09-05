using System;
using System.Collections.Generic;

namespace QSProjectsLib
{
	public class TableInfo
	{
		public string ObjectsName;
		public string ObjectName;
		public string SqlSelect;
		public string DisplayString;
		public PrimaryKeys PrimaryKey;
		public Dictionary<string, DeleteDependenceItem> DeleteItems;
		public Dictionary<string, ClearDependenceItem> ClearItems;

		public TableInfo()
		{
			DeleteItems = new Dictionary<string, DeleteDependenceItem>();
			ClearItems = new Dictionary<string, ClearDependenceItem>();
		}
	
		public class DeleteDependenceItem
		{
			public string sqlwhere;
			public PrimaryKeys SqlParam;

			public DeleteDependenceItem(string sqlwhere , string StrParamName, string IntParamName)
			{
				this.sqlwhere = sqlwhere;
				SqlParam = new PrimaryKeys(IntParamName, StrParamName);
			}
		}

		public class ClearDependenceItem
		{
			public string[] ClearFields;
			public string sqlwhere;
			public PrimaryKeys SqlParam;

			public ClearDependenceItem(string sqlwhere , string StrParamName, string IntParamName, string ClearField1, string ClearField2)
			{
				this.sqlwhere = sqlwhere;
				SqlParam = new PrimaryKeys(IntParamName, StrParamName);
				ClearFields = new string[]{ClearField1, ClearField2};
			}
			public ClearDependenceItem(string sqlwhere, string StrParamName, string IntParamName, string ClearField1)
			{
				this.sqlwhere = sqlwhere;
				SqlParam = new PrimaryKeys(IntParamName, StrParamName);
				ClearFields = new string[]{ClearField1};
			}
		}

		public class Params
		{
			public string ParamStr;
			public int ParamInt;

			public Params(int IntParameter, string StrParameter)
			{
				ParamInt = IntParameter;
				ParamStr = StrParameter;
			}
			public Params(){}
		}

		public class PrimaryKeys
		{
			public string ParamStr, ParamInt;

			public PrimaryKeys (string IntParamName, string StrParamName = "")
			{
				ParamStr = StrParamName;
				ParamInt = IntParamName;
			}
		}
	}
}

