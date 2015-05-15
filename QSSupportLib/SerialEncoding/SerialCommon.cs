using System;
using System.Text;

namespace QSSupportLib.Serial
{
	public static class SerialCommon
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public const int SplitBy = 4;

		public static string AddHyphens(string dirtyNumber)
		{
			logger.Debug("Serial digits = {0}", dirtyNumber.Length);

			StringBuilder result = new StringBuilder();
			int untilHyphen = SplitBy;
			foreach (var character in dirtyNumber)
			{
				if(untilHyphen == 0)
				{
					untilHyphen = SplitBy;
					result.Append('-');
				}
				result.Append(character);
				untilHyphen--;
			}
			return result.ToString();
		}

	}
}

