using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using QS.Utilities.Extensions;

namespace QS.DbManagement
{
	public class MariaDbConnectionTypeBase : ConnectionTypeBase {

		public MariaDbConnectionTypeBase() {
			ConnectionTypeName = "MariaDB";
			Title = "MariaDB или MySQL";
			Parameters.Add(new ConnectionParameter("Server", "Адрес сервера"));
			Parameters.Add(new ConnectionParameter("Login", "Пользователь"));
			IconBytes = Assembly.GetExecutingAssembly().GetResourceByteArray("QS.DbManagement.Assets.mariadb.png");
		}

		public override bool CanConnect(IEnumerable<ConnectionParameterValue> parameters) {
			return parameters.Any(p => p.Name == "Server" && !string.IsNullOrEmpty(p.Value)) &&
				parameters.Any(p => p.Name == "Login" && !string.IsNullOrEmpty(p.Value));
		}

		public override IDbProvider CreateProvider(IList<ConnectionParameterValue> parameters, string password = null) 
			=> new MariaDBProvider(parameters, password);
	}
}
