using Microsoft.Extensions.DependencyInjection;
using QS.DBScripts;
using QS.DBScripts.Models;
using QS.Utilities.Extensions;
using System;
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


			CreatorFactory = args => {
				var p = (MariaDBProvider)args.Provider;
				var scripts = args.ServiceProvider.GetRequiredService<IDbScriptsConfiguration>();
				return new MySqlDbCreateModel(
					p.ConnectionStringBuilder.ConnectionString,
					scripts.MakeCreationScript(), args.Progress, args.Interaction, args.CancellationToken) { FillBaseGuid = false };
			};

			ImportFactory = args => {
				var p = (MariaDBProvider)args.Provider;
				return new MariaDbImportModel(
					p.ConnectionStringBuilder, args.ImportDumpFilePath, args.Progress, args.CancellationToken);
			};

		}

		public override bool CanConnect(IEnumerable<ConnectionParameterValue> parameters) {
			return parameters.Any(p => p.Name == "Server" && !string.IsNullOrEmpty(p.Value)) &&
				parameters.Any(p => p.Name == "Login" && !string.IsNullOrEmpty(p.Value));
		}

		public override IDbProvider CreateProvider(IList<ConnectionParameterValue> parameters, string password = null)
			=> new MariaDBProvider(parameters, password);
	}
}
