using System;
using System.Reflection;
using Gtk;

namespace QSSupportLib
{
	public class MainSupport
	{
		public static BaseParam BaseParameters;
		public static AppVersion ProjectVerion;

		public MainSupport ()
		{
		}
		
		public static void TestVersion(Window Parrent)
		{
			string TextMes = TestVersionText ();
			
			if(TextMes != null)
			{
				MessageDialog VersionError = new MessageDialog (Parrent , DialogFlags.DestroyWithParent,
					                                      MessageType.Warning, 
					                                      ButtonsType.Close, 
				                                                TextMes);
				VersionError.Run();
				VersionError.Destroy();
				Environment.Exit(0);
			}
		}

		public static bool TestVersionSilent () 
		{
			return !(MainSupport.BaseParameters.Product == null && MainSupport.BaseParameters.Edition == null && MainSupport.BaseParameters.Version == null);
		}

		private static string TestVersionText()
		{
			string ErrorText;

			if (MainSupport.BaseParameters.Product == null) {
				ErrorText = "Название продуката в базе данных не указано.";
				return ErrorText;
			}

			if (MainSupport.BaseParameters.Product != ProjectVerion.Product) {
				ErrorText = "База данных не для того продукта.";
				return ErrorText;
			}

			if (MainSupport.BaseParameters.Version == null) {
				ErrorText = "Версия базы данных не определена.";
				return ErrorText;
			}

			string[] ver = MainSupport.BaseParameters.Version.Split('.');
			if(ProjectVerion.Version.Major.ToString() != ver[0] || ProjectVerion.Version.Minor.ToString() != ver[1])
			{
				ErrorText = "Версия продукта не совпадает с версией базы данных.\n";
				ErrorText += String.Format("Версия продукта: {0}.{1}.{2}", ProjectVerion.Version.Major, ProjectVerion.Version.Minor, ProjectVerion.Version.Build); 
				ErrorText += "\nВерсия базы данных: " + MainSupport.BaseParameters.Version;
				return ErrorText;
			}

			if(MainSupport.BaseParameters.Edition == null)
			{
				ErrorText = "Редакция базы не указана!\n";
				ErrorText += "Редакция продукта: " + ProjectVerion.Edition + "\nРедакция базы данных: " + MainSupport.BaseParameters.Edition;
				return ErrorText;
			}

			if(MainSupport.BaseParameters.Edition != ProjectVerion.Edition)
			{
				ErrorText = "Редакция продукта не совпадает с редакцией базы данных.\n";
				ErrorText += "Редакция продукта: " + ProjectVerion.Edition + "\nРедакция базы данных: " + MainSupport.BaseParameters.Edition;
				return ErrorText;
			}

			return null;
		}

		public static string GetTitle()
		{
			System.Reflection.Assembly assembly = Assembly.GetCallingAssembly();
			object[] att = assembly.GetCustomAttributes (typeof(AssemblyTitleAttribute), false);

			return ((AssemblyTitleAttribute)att [0]).Title;
		}

	}
}