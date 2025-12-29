using QS.DomainModel.Entity;

namespace QS.Test.TestApp.Domain
{
	/// <summary>
	/// Тестовый класс на основе PropertyChangedBase для проверки уведомлений об изменении свойств
	/// </summary>
	public class TestItem : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		private string name;
		public virtual string Name {
			get => name;
			set => SetField(ref name, value);
		}

		private int itemValue;
		public virtual int Value {
			get => itemValue;
			set => SetField(ref itemValue, value);
		}

		public TestItem() { }

		public TestItem(string name, int value = 0)
		{
			this.name = name;
			this.itemValue = value;
		}
	}
}

