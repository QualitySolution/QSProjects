using System;

namespace QS.Project.DB {
	public class DataBaseInfo : IDataBaseInfo {
		public DataBaseInfo(string name, bool isDemo = false) {
			Name = name;
			IsDemo = isDemo;
		}

		public string Name { get; private set; }

		public bool IsDemo { get; private set; }

		public Guid? BaseGuid { get; }

		public Version Version { get; }
	}
}
