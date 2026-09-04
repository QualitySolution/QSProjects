using System;
using System.IO;
using System.Reflection;

namespace QS.DBScripts.Models
{
	public class CreationScript
	{
		public string Name;
		public Version Version;

		public Assembly ResourceAssembly;
		public String ResourceName;
		public string FileName;

		public CreationScript(Assembly resourceAssembly, string resourceName, Version version, string name = null)
		{
			Name = name;
			Version = version;
			ResourceAssembly = resourceAssembly;
			ResourceName = resourceName;
		}
		
		public CreationScript(string fileName, Version version, string name = null)
		{
			Name = name;
			Version = version;
			FileName = fileName;
		}

		public string GetSqlScript()
		{
			if (ResourceAssembly != null && !String.IsNullOrEmpty(ResourceName))
			{
				using (Stream stream = ResourceAssembly.GetManifestResourceStream(ResourceName)) {
					if (stream == null)
						throw new InvalidOperationException(String.Format("Ресурс {0} со скриптом не найден.", ResourceName));
					StreamReader reader = new StreamReader(stream);
					return reader.ReadToEnd();
				}
			}

			if (!String.IsNullOrEmpty(FileName))
			{
				using (StreamReader sr = new StreamReader(FileName))
				{
					return sr.ReadToEnd();
				}
			}

			throw new NotSupportedException(
				"Для получения скрипта sql должен быть указано либо имя файла либо название ресурса");
		}
	}
}