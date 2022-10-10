using System;
using QS.BaseParameters;

namespace QS.Project.DB
{
	public class NhDataBaseInfo : IDataBaseInfo
	{
		public NhDataBaseInfo(ParametersService baseParameters = null)
		{
			if(baseParameters != null)
				BaseGuid = baseParameters.Dynamic.BaseGuid(typeof(Guid));

			//Наверно фиговый способ получить имя базы. Но ничего лучше не придумал.
			var session = OrmConfig.OpenSession();
			Name = session.Connection.Database;
			IsDemo = session.Connection.DataSource == "demo.qsolution.ru";
			session.Close();
		}

		public string Name { get; private set; }

		public bool IsDemo { get; private set; }

		public Guid? BaseGuid { get; }
	}
}