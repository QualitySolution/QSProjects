using System;
using System.Collections;
using System.Text.RegularExpressions;

namespace QSProjectsLib
{
	public class StringWorks
	{
		enum PowTwo : ulong{
			Pow10 = 1024,
			Pow20 = 1048576,
			Pow30 = 1073741824,
			Pow40 = 1099511627776,
			Pow50 = 1125899906842620,
			Pow60 = 1152921504606850000
		}


		public static class NaturalStringComparer
		{
			private static readonly Regex _re = new Regex(@"(?<=\D)(?=\d)|(?<=\d)(?=\D)", RegexOptions.Compiled);

			public static int Compare(string x, string y)
			{
				x = x.ToLower();
				y = y.ToLower();
				if(string.Compare(x, 0, y, 0, Math.Min(x.Length, y.Length)) == 0)
				{
					if(x.Length == y.Length) return 0;
					return x.Length < y.Length ? -1 : 1;
				}
				var a = _re.Split(x);
				var b = _re.Split(y);
				int i = 0;
				while(true)
				{
					int r = PartCompare(a[i], b[i]);
					if(r != 0) return r;
					++i;
				}
			}

			private static int PartCompare(string x, string y)
			{
				int a, b;
				if(int.TryParse(x, out a) && int.TryParse(y, out b))
					return a.CompareTo(b);
				return x.CompareTo(y);
			}
		}

		public static string BytesToIECUnitsString(ulong bytes)
		{
			if (bytes < (ulong)PowTwo.Pow10)
				return String.Format ("{0} Б", bytes);
			else if(bytes < (ulong)PowTwo.Pow20)
				return String.Format ("{0:N1} КиБ", (double)bytes/ (ulong)PowTwo.Pow10);
			else if(bytes < (ulong)PowTwo.Pow30)
				return String.Format ("{0:N1} МиБ", (double)bytes/ (ulong)PowTwo.Pow20);
			else if(bytes < (ulong)PowTwo.Pow40)
				return String.Format ("{0:N1} ГиБ", (double)bytes/ (ulong)PowTwo.Pow30);
			else if(bytes < (ulong)PowTwo.Pow50)
				return String.Format ("{0:N1} ТиБ", (double)bytes/ (ulong)PowTwo.Pow40);
			else if(bytes < (ulong)PowTwo.Pow60)
				return String.Format ("{0:N1} ПиБ", (double)bytes/ (ulong)PowTwo.Pow50);
			else
				return String.Format ("{0:N1} ЭиБ", (double)bytes/ (ulong)PowTwo.Pow60);
		}
	}
}

