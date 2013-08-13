using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace QSSupportLib
{
	public class BaseParam
	{
		public string Product 
		{
			get{return All["product_name"];}
		}

		public string Version 
		{
			get{return All["version"];}
		}

		public string Edition 
		{
			get{return All["edition"];}
		}

		public string SerialNumber 
		{
			get{return All["serial_number"];}
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