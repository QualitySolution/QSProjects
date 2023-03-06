using System;
using NUnit.Framework;
using QS.Utilities.Text;

namespace QS.Test.Utilities.Text {
	[TestFixture(TestOf = typeof(VersionHelper))]
	public class VersionHelperTest {
		[TestCase("1.2", ExpectedResult = "1.2")]
		[TestCase("2.0", ExpectedResult = "2.0")]
		[TestCase("2.0.1", ExpectedResult = "2.0.1")]
		[TestCase("2.0.0.6", ExpectedResult = "2.0.0.6")]
		[TestCase("1.2.3.6", ExpectedResult = "1.2.3.6")]
		public string VersionToShortString_InputStringCases(string version) {
			return version.VersionToShortString();
		}
		
		[TestCase(1, 2, ExpectedResult = "1.2")]
		[TestCase(2, 0, ExpectedResult = "2.0")]
		[TestCase(2,0,1, ExpectedResult = "2.0.1")]
		[TestCase(2, 0, 0, 6, ExpectedResult = "2.0.0.6")]
		[TestCase(1,2,3,6, ExpectedResult = "1.2.3.6")]
		public string VersionToShortString_InputVersionCases(int major, int minor, int? build = null, int? revision = null) {
			Version version;
			if(revision.HasValue)
				version = new Version(major, minor, build.Value, revision.Value);
			else if(build.HasValue) 
				version = new Version(major, minor, build.Value);
			else 
				version = new Version(major, minor);
				
			return version.VersionToShortString();
		}
	}
}
