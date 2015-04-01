using System;
using QSProjectsLib;
using System.Collections.Generic;

namespace QSSupportLib
{
	public class AppVersion
	{
		public string Product;
		public Version Version;
		public string Edition;
		public List<string> AllowEdition;

		[Obsolete("Используйте автоматический конструктор и атрибут AssemblyEdition для указания редакции")]
		public AppVersion(string product, string edition, Version version )
		{
			Product = product;
			Edition = edition;
			AllowEdition = new List<string> (){ Edition };
			Version = version;
			Console.WriteLine(Product);
		}

		public AppVersion()
		{
			var assembly = System.Reflection.Assembly.GetEntryAssembly ();
			var name = assembly.GetName ();
			Product = name.Name;
			Version = name.Version;

			var att = assembly.GetCustomAttributes (typeof(AssemblyEditionAttribute), false);

			if (att.Length > 0) { 
				AllowEdition = new List<string>( ((AssemblyEditionAttribute)att [0]).AllowEdition);
				Edition = ((AssemblyEditionAttribute)att [0]).Edition;
				if (!AllowEdition.Contains (Edition))
					AllowEdition.Add (Edition);
			}
		}
	}
}

