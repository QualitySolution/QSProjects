using System;
using System.IO;
using System.Xml;
using NLog;

namespace QSProjectsLib
{
	public class ViewReportExt
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public ViewReportExt()
		{
		}

		public static void Run(string ReportName, string Params)
		{
			Run(ReportName, Params, false);
		}

		public static void Run(string ReportName, string Params, bool UserVar)
		{
			string ReportPath = System.IO.Path.Combine( Directory.GetCurrentDirectory(), "Reports", ReportName + ".rdl");

			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(ReportPath);

			foreach (XmlNode node in xmlDoc.GetElementsByTagName("ConnectString"))
			{
				if(UserVar)
					node.InnerText = QSMain.ConnectionString + "Allow User Variables=True";
				else
					node.InnerText = QSMain.ConnectionString;
			}

			ReportPath = System.IO.Path.Combine (System.IO.Path.GetTempPath (), ReportName + ".rdl");
			xmlDoc.Save(ReportPath);

			logger.Debug("Report:{0} Parameters:{1}", ReportName, Params);
			string arg = "-r \"" + ReportPath +"\" -p \"" + Params + "\"";
			System.Diagnostics.Process.Start ("RdlReader.exe", arg);
		}
	}
}

