using QS.DomainModel.Entity;

namespace QS.Test.TestApp.Domain.Entity
{
	public class PropertyChangedClass : PropertyChangedBase
	{
		private string property1;
		[PropertyChangedAlso("NotExistProperty")]
		[PropertyChangedAlso(nameof(Property2))]
		public virtual string Property1 {
			get => property1;
			set => SetField(ref property1, value);
		}

		private string property2;
		public virtual string Property2 {
			get => property2;
			set => SetField(ref property2, value);
		}

		private bool property10;
		[PropertyChangedAlso(nameof(Property2))]
		public virtual bool Property10 {
			get => property10;
			set => SetField(ref property10, value);
		}

		#region Ручной вызов
		public void ManualInternalFire(string propertyName)
		{
			OnPropertyChanged(propertyName);
		}
		#endregion
	}
}