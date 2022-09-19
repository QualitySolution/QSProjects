using System;
namespace QS.Utilities.Text
{
	public static class StringManipulationHelper
	{
		/// <summary>
		/// Заменяем первое вхождение найденной строки.
		/// </summary>
		public static string ReplaceFirstOccurrence(this string Source, string Find, string Replace)
		{
			int Place = Source.IndexOf(Find);
			if (Place == -1)
				return Source;

			return Source.Remove(Place, Find.Length).Insert(Place, Replace);
		}

		/// <summary>
		/// Заменяем последнее вхождение найденной строки.
		/// </summary>
		public static string ReplaceLastOccurrence(this string Source, string Find, string Replace)
		{
			int Place = Source.LastIndexOf(Find);
			if (Place == -1)
				return Source;

			return Source.Remove(Place, Find.Length).Insert(Place, Replace);
		}

		public static string EllipsizeMiddle(string input, int maxLength) {
			if(input?.Length > maxLength) {
				return input.Substring(0, maxLength/2) + "…" + input.Substring(input.Length - (maxLength/2 - 1));
			}

			return input;
		}
	}
}
