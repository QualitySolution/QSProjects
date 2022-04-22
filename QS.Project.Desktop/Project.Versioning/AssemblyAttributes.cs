using System;

namespace QS.Project.Versioning
{
	/// <summary>
	/// Атрибут для указания ссылки на ресурс логотипа программы.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly)]
	public class AssemblyLogoIconAttribute : Attribute 
	{
		public string ResourceName;
		public AssemblyLogoIconAttribute() : this(string.Empty) {}
		public AssemblyLogoIconAttribute(string resourceName) { ResourceName = resourceName; }
	}

	/// <summary>
	/// Атрибут для указания контактной информации о поддежке программы
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly)]
	public class AssemblySupportAttribute : Attribute 
	{
		public string SupportInfo;
		public AssemblySupportAttribute(string supportInfo)
		{ 
			SupportInfo = supportInfo;
		}
	}

	/// <summary>
	/// Атрибут для указания авторов программы. Можно добавлять несколько.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public class AssemblyAuthorAttribute : Attribute 
	{
		/// <summary>
		/// Имя автора
		/// </summary>
		public string Name;
		public string Email;
		/// <summary>
		/// Годы деятельности автора на проекте.
		/// </summary>
		public string YearsOfActivity;
		public AssemblyAuthorAttribute(string name, string email = null, string years = null) { 
			Name = name;
			Email = email;
			YearsOfActivity = years; 
		}
	}

	/// <summary>
	/// Атрибут для указания модификации сборки, например для отдельных сборок под конкретных заказчиков.
	/// Не является обязательным для общей сборки проекта.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly)]
	public class AssemblyModificationAttribute : Attribute 
	{
		public string Name;
		public string Title;
		public bool HideFromUser;
		public AssemblyModificationAttribute(string name) { Name = name; }
		public AssemblyModificationAttribute() { }
	}

	/// <summary>
	/// Атрибут позволяющий добавлять совместимые на уровне базы данных модификации. Можно указывать несколько.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public class AssemblyCompatibleModificationAttribute : Attribute
	{
		public string Name;
		public AssemblyCompatibleModificationAttribute(string name) { Name = name; }
		public AssemblyCompatibleModificationAttribute() { }
	}

	/// <summary>
	/// Атрибут позволяющий указать сайт программы.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly)]
	public class AssemblyAppWebsiteAttribute : Attribute 
	{
		public string Link;
		public AssemblyAppWebsiteAttribute(string link) { Link = link; }
	}

	/// <summary>
	/// Если атрибут добавлен, то это бета версия программы.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly)]
	public class AssemblyBetaBuildAttribute : Attribute 
	{
		
	}

	/// <summary>
	/// Атрибут позволяющий указать код продукта, это уникальный номер программы для Quality Solution
	/// Зашит внутри серийного номера начиная с версии 2.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly)]
	public class AssemblyProductCodeAttribute : Attribute
	{
		public byte Number;
		public AssemblyProductCodeAttribute(byte number) { Number = number; }
	}
}
