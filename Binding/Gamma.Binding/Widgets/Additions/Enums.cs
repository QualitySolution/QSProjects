using System.ComponentModel.DataAnnotations;

namespace Gamma.Widgets
{
	public enum SpecialComboState {
		[Display(Name = "Ничего")]
		None,
		[Display(Name = "Все")]
		All,
		[Display(Name = "Нет")]
		Not
	}
}
