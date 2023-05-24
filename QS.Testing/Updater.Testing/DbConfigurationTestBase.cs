using System;
using System.Linq;
using NUnit.Framework;
using QS.Updater.DB;

namespace QS.Testing.Updater.Testing
{
	public abstract class DbConfigurationTestBase
	{
		protected void SequenceCheckTest(UpdateConfiguration configuration)
		{
			var startVersion = configuration.Updates.Select(x => x.Source).Min();
			var notUsedSteps = configuration.Updates.ToList();
			foreach (var hop in configuration.GetHopsToLast(startVersion, false)) {
				notUsedSteps.Remove(hop);
			}
			Assert.That(notUsedSteps, Is.Empty, $"В последовательности обновлений потеряны шаги: " + String.Join("; ", notUsedSteps.Select(x => x.Title)));
		}
	}
}