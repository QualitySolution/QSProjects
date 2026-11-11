using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using NUnit.Framework;
using QS.BaseParameters;
using QS.Project.DB;
using QS.Project.Core;

namespace QS.Test.Project.DB {
	[TestFixture]
	public class DataBaseInfoTests {
		[Test]
		public void ReadsMetadataAndObservesParameterChanges() {
			var guid = Guid.NewGuid();
			var parameters = new ParametersService(new Dictionary<string, string> {
				{ "version", "1.7.8" }, { "BaseGuid", guid.ToString() }
			});
			var info = new DataBaseInfo("insider", parameters, true);
			Assert.That(info.Name, Is.EqualTo("insider"));
			Assert.That(info.IsDemo, Is.True);
			Assert.That(info.Version, Is.EqualTo(new Version(1, 7, 8)));
			Assert.That(info.BaseGuid, Is.EqualTo(guid));
			parameters.All["version"] = "1.8";
			Assert.That(info.Version, Is.EqualTo(new Version(1, 8)));
		}

		[TestCase(null)]
		[TestCase("")]
		[TestCase(" ")]
		[TestCase("not-a-version")]
		public void InvalidVersionIsAnExplicitError(string value) {
			var values = new Dictionary<string, string>();
			if(value != null) values["version"] = value;
			var info = new DataBaseInfo("broken_db", new ParametersService(values));
			var error = Assert.Throws<InvalidOperationException>(() => { var version = info.Version; });
			Assert.That(error.Message, Does.Contain("broken_db").And.Contain("version"));
		}

		[Test]
		public void GuidCanBeAbsentButNotMalformed() {
			var parameters = new ParametersService(new Dictionary<string, string> { { "version", "1.7" } });
			var info = new DataBaseInfo("insider", parameters);
			Assert.That(info.BaseGuid, Is.Null);
			parameters.All["BaseGuid"] = "bad-guid";
			Assert.That(Assert.Throws<InvalidOperationException>(() => { var guid = info.BaseGuid; }).Message,
				Does.Contain("insider").And.Contain("BaseGuid"));
		}

		[Test]
		public void RegistrationReusesProvidedParametersService() {
			var parameters = new ParametersService(new Dictionary<string, string> { { "version", "1.7.8" } });
			var services = new ServiceCollection();
			services.AddSingleton(new MySqlConnectionStringBuilder { Database = "insider" });
			services.AddSingleton(parameters);
			services.AddDatabaseInfo();
			using(var provider = services.BuildServiceProvider()) {
				Assert.That(provider.GetRequiredService<ParametersService>(), Is.SameAs(parameters));
				Assert.That(provider.GetRequiredService<IDataBaseInfo>().Version, Is.EqualTo(new Version(1, 7, 8)));
			}
		}

		[Test]
		public void RegistrationProvidesParametersServiceWithoutOpeningConnection() {
			var services = new ServiceCollection();
			services.AddSingleton(new MySqlConnectionStringBuilder { Database = "great_company" });
			services.AddDatabaseInfo();
			using(var provider = services.BuildServiceProvider()) {
				Assert.That(provider.GetRequiredService<IConnectionFactory>(), Is.Not.Null);
				Assert.That(provider.GetRequiredService<ParametersService>(), Is.Not.Null);
				Assert.That(provider.GetRequiredService<IDataBaseInfo>().Name, Is.EqualTo("great_company"));
			}
		}
	}
}
