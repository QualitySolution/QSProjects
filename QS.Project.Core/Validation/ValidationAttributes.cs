using System;
using System.ComponentModel.DataAnnotations;

namespace QS.Validation
{
	/// <summary>
	/// обязательное свойство
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class RequiredFieldAttribute : RequiredAttribute
	{
		public RequiredFieldAttribute() => ErrorMessage = "Заполните \"{0}\"";
	}

	/// <summary>
	/// ограничение длины строки
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class MaxTextAttribute : StringLengthAttribute
	{
		public MaxTextAttribute(int maximumLength) : base(maximumLength) =>
			ErrorMessage = "\"{0}\" не должно быть длиннее {1} символов";
	}
}
