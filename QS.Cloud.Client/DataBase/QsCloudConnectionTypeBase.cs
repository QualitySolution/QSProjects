using QS.DbManagement;
using QS.DbManagement.Entities;
using QS.Utilities.Extensions;
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
