using System;
namespace QS.DomainModel.Entity
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field)]
	public class AppellativeAttribute : Attribute
	{
		/// <summary>
		/// Именительный падеж (Кто? Что?)
		/// </summary>
		public string Nominative { get; set; }

		/// <summary>
		/// Именительный падеж (Кто? Что?) можественное число.
		/// </summary>
		public string NominativePlural { get; set; }

		/// <summary>
		/// Род
		/// </summary>
		public GrammaticalGender Gender { get; set; }

		/// <summary>
		/// Родительный падеж (Кого? Чего?)
		/// </summary>
		public string Genitive { get; set; }

		/// <summary>
		/// Родительный падеж (Кого? Чего?) можественное число.
		/// </summary>
		public string GenitivePlural { get; set; }

		/// <summary>
		/// Винительный падеж (Кого? Что?)
		/// </summary>
		public string Accusative { get; set; }

		/// <summary>
		/// Винительный падеж (Кого? Что?) можественное число.
		/// </summary>
		public string AccusativePlural { get; set; }

		/// <summary>
		/// Предложный падеж (О ком? О чём?)
		/// </summary>
		public string Prepositional { get; set; }

		/// <summary>
		/// Предложный падеж (О ком? О чём?) можественное число.
		/// </summary>
		public string PrepositionalPlural { get; set; }

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
