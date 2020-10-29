using QS.Project.Versioning;

namespace QS.Project.DB
{
	public class NhDataBaseInfo : IDataBaseInfo
	{
		public NhDataBaseInfo()
		{
			//Наверно фиговый способ получить имя базы. Но ничего лучше не придумал.
			var session = OrmConfig.OpenSession();
			Name = session.Connection.Database;
			session.Close();
		}

		public string Name { get; private set; }
	}
}
