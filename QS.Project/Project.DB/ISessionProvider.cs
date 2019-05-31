using NHibernate;

namespace QS.Project.DB
{
	public interface ISessionProvider
	{
		ISession OpenSession();
	}
}
