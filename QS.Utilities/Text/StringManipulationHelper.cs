using System;
namespace QS.Utilities.Text
{
	public static class StringManipulationHelper
	{
		/// <summary>
		/// Заменяем первое вохждение найденной строки.
		/// </summary>
		public static string ReplaceFirstOccurrence(this string Source, string Find, string Replace)
		{
			int Place = Source.IndexOf(Find);
			if (Place == -1)
				return Source;

			return Source.Remove(Place, Find.Length).Insert(Place, Replace);
		}

		/// <summary>
		/// Заменяем последнее вохждение найденной строки.
		/// </summary>
		public static string ReplaceLastOccurrence(this string Source, string Find, string Replace)
		{
			int Place = Source.LastIndexOf(Find);
			if (Place == -1)
				return Source;

			return Source.Remove(Place, Find.Length).Insert(Place, Replace);
		}
	}
}
