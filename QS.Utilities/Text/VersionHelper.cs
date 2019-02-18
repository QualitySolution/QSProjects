using System;
namespace QS.Utilities.Text
{
	public static class VersionHelper
	{
		public static string VersionToShortString(this Version version)
		{
			return version.ToString(version.Revision <= 0 ? (version.Build <= 0 ? 2 : 3) : 4);
		}

		public static string VersionToShortString(this string version)
		{
			var ver = Version.Parse(version);
			return ver.ToString(ver.Revision == 0 ? (ver.Build == 0 ? 2 : 3) : 4);
		}
	}
}
