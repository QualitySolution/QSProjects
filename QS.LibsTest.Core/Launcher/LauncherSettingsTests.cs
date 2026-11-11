using System;
using System.IO;
using NUnit.Framework;
using QS.Launcher;

namespace QS.Test.Launcher {
	[TestFixture]
	public class LauncherSettingsTests {
		[Test]
		public void SavesUpdatesWithoutLosingOtherSettings() {
			var directory = Path.Combine(Path.GetTempPath(), "qs-settings-test-" + Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(directory);
			try {
				var path = Path.Combine(directory, "settings.json");
				File.WriteAllText(path, "{\"other\":{\"value\":42}}");
				var settings = new LauncherSettings(path);
				settings.Reload();
				settings["updates:channel"] = "Stable";
				settings["updates:skipVersion"] = "2.8";
				var restored = new LauncherSettings(path);
				restored.Reload();
				Assert.That(restored["updates:channel"], Is.EqualTo("Stable"));
				Assert.That(restored["updates:skipVersion"], Is.EqualTo("2.8"));
				Assert.That(File.ReadAllText(path), Does.Contain("42"));
			}
			finally { Directory.Delete(directory, true); }
		}
	}
}
