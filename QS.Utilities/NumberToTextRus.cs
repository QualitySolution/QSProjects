// A set of C# classes for spelling Russian numerics 
// Copyright (c) 2002 RSDN Group

using System;
using System.Collections.Generic;
using System.Text;

namespace QS.Utilities
{
	public static class NumberToTextRus
    {
        private static string[] hunds =
        {
            "", "сто ", "двести ", "триста ", "четыреста ",
            "пятьсот ", "шестьсот ", "семьсот ", "восемьсот ", "девятьсот "
        };

        private static string[] tens =
        {
            "", "десять ", "двадцать ", "тридцать ", "сорок ", "пятьдесят ",
            "шестьдесят ", "семьдесят ", "восемьдесят ", "девяносто "
        };

        public static string Str(int val, bool male, string one, string two, string five)
        {
            string[] frac20 =
            {
                "", "один ", "два ", "три ", "четыре ", "пять ", "шесть ",
                "семь ", "восемь ", "девять ", "десять ", "одиннадцать ",
                "двенадцать ", "тринадцать ", "четырнадцать ", "пятнадцать ",
                "шестнадцать ", "семнадцать ", "восемнадцать ", "девятнадцать "
            };

            int num = val % 1000;
            if(0 == num) return "";
            if(num < 0) throw new ArgumentOutOfRangeException(nameof(val), "Параметр не может быть отрицательным");
            if(!male)
            {
                frac20[1] = "одна ";
                frac20[2] = "две ";
            }

            StringBuilder r = new StringBuilder(hunds[num / 100]);

            if(num % 100 < 20)
            {
                r.Append(frac20[num % 100]);
            }
            else
            {
                r.Append(tens[num % 100 / 10]);
                r.Append(frac20[num % 10]);
            }
            
            r.Append(Case(num, one, two, five));

            if(r.Length != 0) r.Append(" ");
            return r.ToString();
        }

        public static string Case(int val, string one, string two, string five)
        {
            int t=(val % 100 > 20) ? val % 10 : val % 20;

            switch (t)
            {
                case 1: return one;
                case 2: case 3: case 4: return two;
                default: return five;
            }
        }

		public static string FormatCase(int val, string one, string two, string five, params object[] addFormatValues)
		{
			var list = new List<object> {val};
			list.AddRange(addFormatValues);
			return String.Format (Case (val, one, two, five), list.ToArray());
		}
    }
}
