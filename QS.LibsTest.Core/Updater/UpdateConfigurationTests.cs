using System;
using System.Linq;
using NUnit.Framework;
using QS.Updater.DB;

namespace QS.Test.Updater
{
	[TestFixture(TestOf = typeof(UpdateConfiguration))]
	public class UpdateConfigurationTests
	{
		[Test(Description = "Реальный пример конфига, чтобы убедится что не поломался старый вариант настроек.")]
		public void OldConfigTest()
		{
			var config = new UpdateConfiguration();
			config.AddMicroUpdate(
				new Version(1, 0),
				new Version(1, 0, 4),
				"workwear.Updates.1.0.4.sql");
			config.AddMicroUpdate(
				new Version(1, 0, 4),
				new Version(1, 0, 5),
				"workwear.Updates.1.0.5.sql");
			config.AddUpdate(
				new Version(1, 0),
				new Version(1, 1),
				"workwear.Updates.Update to 1.1.sql");
			config.AddUpdate(
				new Version(1, 1),
				new Version(1, 2),
				"workwear.Updates.Update to 1.2.sql");
			config.AddMicroUpdate(
				new Version(1, 2),
				new Version(1, 2, 1),
				"workwear.Updates.1.2.1.sql");
			config.AddMicroUpdate(
				new Version(1, 2, 1),
				new Version(1, 2, 2),
				"workwear.Updates.1.2.2.sql");
			config.AddMicroUpdate(
				new Version(1, 2, 2),
				new Version(1, 2, 4),
				"workwear.Updates.1.2.4.sql");
			config.AddUpdate(
				new Version(1, 2),
				new Version(2, 0),
				"workwear.Updates.2.0.sql");
			config.AddMicroUpdate(
				new Version(2, 0),
				new Version(2, 0, 2),
				"workwear.Updates.2.0.2.sql");
			config.AddUpdate(
				new Version(2, 0),
				new Version(2, 1),
				"workwear.Updates.2.1.sql");
			config.AddMicroUpdate(
				new Version(2, 1),
				new Version(2, 1, 1),
				"workwear.Updates.2.1.1.sql");
			config.AddUpdate(
				new Version(2, 1),
				new Version(2, 2),
				"workwear.Updates.2.2.sql");
			config.AddUpdate(
				new Version(2, 2),
				new Version(2, 3),
				"workwear.Updates.2.3.sql");
			config.AddMicroUpdate(
				new Version(2, 3),
				new Version(2, 3, 3),
				"workwear.Updates.2.3.3.sql");

			var hops = config.GetHopsToLast(new Version(1, 0, 4)).ToArray();
			Assert.That(hops[0].Resource, Is.EqualTo("workwear.Updates.1.0.5.sql"));
			Assert.That(hops[1].Resource, Is.EqualTo("workwear.Updates.Update to 1.1.sql"));
			Assert.That(hops[2].Resource, Is.EqualTo("workwear.Updates.Update to 1.2.sql"));
			Assert.That(hops[3].Resource, Is.EqualTo("workwear.Updates.1.2.1.sql"));
			Assert.That(hops[4].Resource, Is.EqualTo("workwear.Updates.1.2.2.sql"));
			Assert.That(hops[5].Resource, Is.EqualTo("workwear.Updates.1.2.4.sql"));
			Assert.That(hops[6].Resource, Is.EqualTo("workwear.Updates.2.0.sql"));
			Assert.That(hops[7].Resource, Is.EqualTo("workwear.Updates.2.0.2.sql"));
			Assert.That(hops[8].Resource, Is.EqualTo("workwear.Updates.2.1.sql"));
			Assert.That(hops[9].Resource, Is.EqualTo("workwear.Updates.2.1.1.sql"));
			Assert.That(hops[10].Resource, Is.EqualTo("workwear.Updates.2.2.sql"));
			Assert.That(hops[11].Resource, Is.EqualTo("workwear.Updates.2.3.sql"));
			Assert.That(hops[12].Resource, Is.EqualTo("workwear.Updates.2.3.3.sql"));
		}
	}
}
