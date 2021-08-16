using QS.DomainModel.Entity;

namespace QS.Test.TestApp.Domain.Entity
{
	public class PropertyChangedDescendantClass : PropertyChangedClass
	{
		[PropertyChangedAlso(nameof(Property3))]
		public override string Property1 { get; set; }
		
		public string Property3 { get; set; }
	}
}