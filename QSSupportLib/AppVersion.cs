using System;
using QSProjectsLib;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace QSSupportLib
{
	public class AppVersion
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public string Product;
		public string ProductTitle { get; private set;}
		public Version Version;
		public string Edition;
		public bool IsBeta { get; private set;}
		public DateTime? BuildDate { get; private set;}
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
				AllowEdition = new List<string>();
				if(((AssemblyEditionAttribute)att [0]).AllowEdition != null)
					AllowEdition.AddRange( ((AssemblyEditionAttribute)att [0]).AllowEdition);
				Edition = ((AssemblyEditionAttribute)att [0]).Edition;
				if (!AllowEdition.Contains (Edition))
					AllowEdition.Add (Edition);
			}

			att = assembly.GetCustomAttributes (typeof(AssemblyTitleAttribute), false);
			ProductTitle = ((AssemblyTitleAttribute)att [0]).Title;

			logger.Info ("Продукт: {0} v. {1}", ProductTitle, Version);
			logger.Info ("Редакция: {0}", Edition);

			att = assembly.GetCustomAttributes (typeof(AssemblyBetaBuildAttribute), false);
			if(att.Length > 0)
			{
				BuildDate = System.IO.File.GetLastWriteTime(assembly.Location);
				logger.Debug ("Бета сборка от {0:g}", BuildDate);
			}
		}
	}
}

