using System;

namespace QSSupportLib
{
	public class AppVersion
	{
		public string Product;
		public Version Version;
		//public string VersionString;
		//public string[] VersionStrings;
		public string Edition;
		
		public AppVersion(string product, string edition, Version version )
		{
			Product = product;
			Edition = edition;
			Version = version;
			Console.WriteLine(Product);			
		}
	}
}

