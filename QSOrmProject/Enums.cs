using System.Data.Bindings;

namespace QSOrmProject
{

	public enum SpecialComboState {
		[ItemTitle("Ничего")]
		None,
		[ItemTitle("Все")]
		All,
		[ItemTitle("Нет")]
		Not
	}
}
