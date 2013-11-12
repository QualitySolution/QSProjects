using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace QSSupportLib
{
	public class BaseParam
	{
		public string Product 
		{
			get{return All.ContainsKey("product_name") ? All["product_name"] : null;}
		}

		public string Version 
		{
			get{return All.ContainsKey("version") ? All["version"] : null;}
		}

		public string Edition 
		{
			get{return All.ContainsKey("edition") ? All["edition"] : null;}
		}

		public string SerialNumber 
		{
			get{return All.ContainsKey("serial_number") ? All["serial_number"] : null;}
		}

		Dictionary<string, string> All;

		public BaseParam (MySqlConnection con)
		{
			All = new Dictionary<string, string>();
			string sql = "SELECT * FROM base_parameters";
			MySqlCommand cmd = new MySqlCommand(sql, con);
			MySqlDataReader rdr = cmd.ExecuteReader();
			string temp;
			while(rdr.Read())
			{
				if(rdr["str_value"] == DBNull.Value)
					temp = "";
				else
					temp = rdr.GetString(1);
				All.Add(rdr.GetString("name"), temp);
			}
			rdr.Close();
		}
	}
}