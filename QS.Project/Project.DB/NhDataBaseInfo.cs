using System;
using QS.BaseParameters;

namespace QS.Project.DB
{
	public class NhDataBaseInfo : IDataBaseInfo
	{
		public NhDataBaseInfo(ParametersService baseParameters = null, bool isDemo = false)
		{
			if(baseParameters != null) {
				BaseGuid = baseParameters.Dynamic.BaseGuid(typeof(Guid));
				Version = baseParameters.Dynamic.version(typeof(Version));
			}

			//Наверно фиговый способ получить имя базы. Но ничего лучше не придумал.
			var session = OrmConfig.OpenSession();
			Name = session.Connection.Database;
			IsDemo = isDemo;
			session.Close();
		}

		public string Name { get; private set; }

		public bool IsDemo { get; private set; }

		public Guid? BaseGuid { get; }

		public Version Version { get; }
	}
}
