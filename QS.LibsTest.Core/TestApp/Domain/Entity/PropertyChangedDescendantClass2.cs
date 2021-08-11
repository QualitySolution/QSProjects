using System.Collections.Generic;
using QS.DomainModel.Entity;

namespace QS.Test.TestApp.Domain.Entity
{
	public class PropertyChangedDescendantClass2 : PropertyChangedClass
	{
		[PropertyChangedAlso(nameof(Property3))]
		public new List<string> Property1 { get; set; }
		
		public string Property3 { get; set; }
	}
}