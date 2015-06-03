using System;

namespace QSProjectsLib
{
	public class SubjectName
	{
		/// <summary>
		/// Именительный падеж (Кто? Что?)
		/// </summary>
		public string Nominative;

		/// <summary>
		/// Именительный падеж (Кто? Что?) можественное число.
		/// </summary>
		public string NominativePlural;

		/// <summary>
		/// Род
		/// </summary>
		public GrammaticalGender Gender;

		/// <summary>
		/// Родительный падеж (Кого? Чего?)
		/// </summary>
		public string Genitive;

		/// <summary>
		/// Родительный падеж (Кого? Чего?) можественное число.
		/// </summary>
		public string GenitivePlural;

		/// <summary>
		/// Винительный падеж (Кого? Что?)
		/// </summary>
		public string Accusative;

		/// <summary>
		/// Винительный падеж (Кого? Что?) можественное число.
		/// </summary>
		public string AccusativePlural;

	}

	public enum GrammaticalGender
	{
		/// <summary>
		/// Не указан
		/// </summary>
		Unknown,
		/// <summary>
		/// Мужской род
		/// </summary>
		Masculine,
		/// <summary>
		/// Женский род
		/// </summary>
		Feminine,
		/// <summary>
		/// Средний род
		/// </summary>
		Neuter
	}
}

