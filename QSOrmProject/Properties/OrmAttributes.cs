using System;
using QSProjectsLib;

namespace QSOrmProject
{
	[AttributeUsage (AttributeTargets.Class)]
	public class OrmSubjectAttribute : Attribute
	{
		private string journalName;

		public string JournalName {
			get {
				if (String.IsNullOrWhiteSpace (journalName) && AllNames != null)
					return StringWorks.StringToTitleCase (AllNames.NominativePlural);
				else
					return journalName;
			}
			set {
				journalName = value;
			}
		}

		private string objectName;

		public string ObjectName {
			get {
				if (String.IsNullOrWhiteSpace (objectName) && AllNames != null)
					return AllNames.Nominative;
				else
					return objectName;
			}
			set {
				objectName = value;
			}
		}

		/// <summary>
		/// Именительный падеж (Кто? Что?)
		/// </summary>
		public string Nominative {
			get {
				return AllNames.Nominative;
			}
			set {
				AllNames.Nominative = value;
			}
		}

		/// <summary>
		/// Именительный падеж (Кто? Что?) можественное число.
		/// </summary>
		public string NominativePlural {
			get {
				return AllNames.NominativePlural;
			}
			set {
				AllNames.NominativePlural = value;
			}
		}

		/// <summary>
		/// Род
		/// </summary>
		public GrammaticalGender Gender {
			get {
				return AllNames.Gender;
			}
			set {
				AllNames.Gender = value;
			}
		}

		/// <summary>
		/// Родительный падеж (Кого? Чего?)
		/// </summary>
		public string Genitive {
			get {
				return AllNames.Genitive;
			}
			set {
				AllNames.Genitive = value;
			}
		}

		/// <summary>
		/// Родительный падеж (Кого? Чего?) можественное число.
		/// </summary>
		public string GenitivePlural {
			get {
				return AllNames.GenitivePlural;
			}
			set {
				AllNames.GenitivePlural = value;
			}
		}

		/// <summary>
		/// Винительный падеж (Кого? Что?)
		/// </summary>
		public string Accusative {
			get {
				return AllNames.Accusative;
			}
			set {
				AllNames.Accusative = value;
			}
		}

		/// <summary>
		/// Винительный падеж (Кого? Что?) можественное число.
		/// </summary>
		public string AccusativePlural {
			get {
				return AllNames.AccusativePlural;
			}
			set {
				AllNames.AccusativePlural = value;
			}
		}

		/// <summary>
		/// Предложный падеж (О ком? О чём?)
		/// </summary>
		public string Prepositional {
			get {
				return AllNames.Prepositional;
			}
			set {
				AllNames.Prepositional = value;
			}
		}

		/// <summary>
		/// Предложный падеж (О ком? О чём?) можественное число.
		/// </summary>
		public string PrepositionalPlural {
			get {
				return AllNames.PrepositionalPlural;
			}
			set {
				AllNames.PrepositionalPlural = value;
			}
		}

		public SubjectName AllNames = new SubjectName();
		public ReferenceButtonMode DefaultJournalMode = ReferenceButtonMode.CanAll;

		public OrmSubjectAttribute ()
		{
		}

		[Obsolete("Используйте заполнение атрибута через параметры.")]
		public OrmSubjectAttribute (string journalName)
		{
			this.journalName = journalName;
		}
	}

	[AttributeUsage (AttributeTargets.Class)]
	public class OrmDefaultIsFilteredAttribute : Attribute
	{
		public bool DefaultIsFiltered;

		public OrmDefaultIsFilteredAttribute (bool defaultIsFiltered)
		{
			DefaultIsFiltered = defaultIsFiltered;
		}
	}

	[AttributeUsage (AttributeTargets.Property)]
	public class PropertyChangedAlsoAttribute : Attribute
	{
		public string[] PropertiesNames;

		public PropertyChangedAlsoAttribute (params string[] propertiesNames)
		{
			PropertiesNames = propertiesNames;
		}
	}

}
