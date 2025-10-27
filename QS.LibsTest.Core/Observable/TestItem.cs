using QS.DomainModel.Entity;

namespace QS.Test.Observable
{
	/// <summary>
	/// Тестовый класс на основе PropertyChangedBase для проверки уведомлений об изменении свойств
	/// </summary>
	public class TestItem : PropertyChangedBase
	{
		private string name;
		public string Name {
			get => name;
			set => SetField(ref name, value);
		}

		private int value;
		public int Value {
			get => this.value;
			set => SetField(ref this.value, value);
		}

		public TestItem(string name, int value = 0)
		{
			this.name = name;
			this.value = value;
		}
	}
}

