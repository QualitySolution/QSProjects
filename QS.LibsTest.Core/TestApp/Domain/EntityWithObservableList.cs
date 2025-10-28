using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;

namespace QS.Test.TestApp.Domain
{
	/// <summary>
	/// Тестовая сущность с коллекцией элементов для интеграционных тестов Observable коллекций
	/// </summary>
	public class EntityWithObservableList : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		private string name;
		public virtual string Name {
			get => name;
			set => SetField(ref name, value);
		}

		private IObservableList<TestItem> items = new ObservableList<TestItem>();
		public virtual IObservableList<TestItem> Items {
			get => items;
			set => SetField(ref items, value);
		}

		public EntityWithObservableList()
		{
		}

		public EntityWithObservableList(string name)
		{
			this.name = name;
		}
	}
}

