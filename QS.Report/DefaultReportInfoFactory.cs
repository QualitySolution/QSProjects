using MySqlConnector;
using System;
using System.Collections.Generic;

namespace QS.Report {
	public class DefaultReportInfoFactory : IReportInfoFactory {
		private readonly MySqlConnectionStringBuilder connectionStringBuilder;

		public DefaultReportInfoFactory(MySqlConnectionStringBuilder connectionStringBuilder) {
			this.connectionStringBuilder = connectionStringBuilder ?? throw new ArgumentNullException(nameof(connectionStringBuilder));
		}

		public ReportInfo Create() {
			return new ReportInfo(connectionStringBuilder.ConnectionString);
		}

		public ReportInfo Create(string identifier, string title, Dictionary<string, object> parameters) {
			return new ReportInfo(connectionStringBuilder.ConnectionString) {
				Identifier = identifier,
				Title = title,
				Parameters = parameters
			};
		}
	}
}
