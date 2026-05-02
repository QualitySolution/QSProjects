using Microsoft.Extensions.DependencyInjection;
using QS.DBScripts;
using QS.DBScripts.Controllers;
using QS.DBScripts.Models;
using QS.Utilities.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace QS.DbManagement
{
	public class MariaDbConnectionTypeBase : ConnectionTypeBase {

		public MariaDbConnectionTypeBase() {
			ConnectionTypeName = "MariaDB";
			Title = "MariaDB или MySQL";
			Parameters.Add(new ConnectionParameter("Server", "Адрес сервера"));
			Parameters.Add(new ConnectionParameter("Login", "Пользователь"));
			IconBytes = Assembly.GetExecutingAssembly().GetResourceByteArray("QS.DbManagement.Assets.mariadb.ico");
		}

		public override bool CanConnect(IEnumerable<ConnectionParameterValue> parameters) {
			return parameters.Any(p => p.Name == "Server" && !string.IsNullOrEmpty(p.Value)) &&
				parameters.Any(p => p.Name == "Login" && !string.IsNullOrEmpty(p.Value));
		}

		public override IDBCreator CreatorFactory(CreatorFactoryArgs args){
			var provider = (MariaDBProvider)args.Provider;
			var scripts = args.ServiceProvider.GetRequiredService<IDbScriptsConfiguration>();
			return new MySqlDbCreateModel(
				provider.ConnectionStringBuilder.ConnectionString,
				scripts,
				args.Progress,
				args.Interaction,
				args.CancellationToken);
		}

		public override IDbProvider CreateProvider(IList<ConnectionParameterValue> parameters, string password = null) 
			=> new MariaDBProvider(parameters, password);
	}
}
