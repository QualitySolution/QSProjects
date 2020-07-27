using System;
using System.Text;
using System.Numerics;

namespace QS.Serial.Encoding
{
	public static class SerialCommon
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public const int SplitBy = 4;
		public const string ProductCharDictionary = "0abcdefghijklmnopqrstuvwxyz";


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

		public static string GetProductFromBinary(byte[] array, int start)
		{
			byte[] productPart = ArrayHelpers.SubArray<byte>(array, start);
			BigInteger intData = new BigInteger(productPart);

			string result = "";
			while (intData > 0)
			{
				int remainder = (int)(intData % ProductCharDictionary.Length);
				intData /= ProductCharDictionary.Length;
				result = ProductCharDictionary[remainder] + result;
			}
			return result;
		}

	}
}

