using System;
using Gtk;

namespace QSSupportLib
{
	public class MainSupport
	{
		public static BaseParam Param;
		public static AppVersion ProjectVerion;

		public MainSupport ()
		{
		}
		
		public static void TestVersion(Window Parrent)
		{
			string errors = "";
			
			if(MainSupport.Param.Edition != ProjectVerion.Edition)
			{
				errors = "\nРедакция продукта не совпадает с редакцией базы данных.\n";
				errors += "Редакция продукта: " + ProjectVerion.Edition + "\nРедакция базы данных: " + MainSupport.Param.Edition + "\n";
			}
	
			string[] ver = MainSupport.Param.Version.Split('.');
			if(ProjectVerion.Version.Major.ToString() != ver[0] || ProjectVerion.Version.Minor.ToString() != ver[1])
			{
				errors = "\nВерсия продукта не совпадает с версией базы данных.\n";
				errors += String.Format("Версия продукта: {0}.{1}.{2}", ProjectVerion.Version.Major, ProjectVerion.Version.Minor, ProjectVerion.Version.Build); 
				errors += "\nВерсия базы данных: " + MainSupport.Param.Version + "\n";
			}
			
			if(MainSupport.Param.Product != ProjectVerion.Product)
				errors = "\nБаза данных не для того продукта.\n";

			if(errors != "")
			{
				MessageDialog VersionError = new MessageDialog (Parrent , DialogFlags.DestroyWithParent,
					                                      MessageType.Warning, 
					                                      ButtonsType.Close, 
					                                      errors);
				VersionError.Run();
				VersionError.Destroy();
				Environment.Exit(0);
			}
		}

	}
}