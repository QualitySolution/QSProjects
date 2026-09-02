using Microsoft.Extensions.DependencyInjection;
using QS.DbManagement;
using QS.DBScripts;
using QS.DBScripts.Controllers;
using QS.DBScripts.Models;
using QS.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace QS.Cloud.Client.DataBase {
	public class QsCloudConnectionTypeBase : ConnectionTypeBase {

		public QsCloudConnectionTypeBase() {
			Title = "QS: Облако";
			ConnectionTypeName = "QSCloud";

			Parameters.Add(new ConnectionParameter("Account","Организация"));
			Parameters.Add(new ConnectionParameter("Login","Пользователь"));

			IconBytes = Assembly.GetExecutingAssembly().GetResourceByteArray("QS.Cloud.Client.Icons.qscloud.ico");

			CreatorFactory = args => {
				var provider = (QSCloudProvider)args.Provider;
				var scripts = args.ServiceProvider.GetRequiredService<IDbScriptsConfiguration>();
				return new QsCloudDbCreator(
					provider.BaseId,
					provider.AuthInfo,
					scripts,
					args.Progress,
					args.Interaction,
					args.CancellationToken);
			};
		}

		public override bool CanConnect(IEnumerable<ConnectionParameterValue> parameters) {
			return parameters.Any(p => p.Name == "Account" && !string.IsNullOrEmpty(p.Value)) &&
				parameters.Any(p => p.Name == "Login" && !string.IsNullOrEmpty(p.Value));
		}

		public override IDbProvider CreateProvider(IList<ConnectionParameterValue> parameters, string password = null) {
			return new QSCloudProvider(parameters, password);
		}
	}
}
