using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;

namespace QS.Extensions.Observable.Collections.List {
	public class ObservableListConvention : ICollectionConvention {
		public void Apply(ICollectionInstance instance) {
			var inspector = instance as CollectionInspector;
			if(inspector == null) {
				return;
			}

			if(inspector.CollectionType is null) {
				return;
			}

			var collectionPropertyTypeDefinition = inspector.CollectionType.GetGenericTypeDefinition();
			var requiredInterfaceTypeDefinition = typeof(IObservableList<>).GetGenericTypeDefinition();

			if(collectionPropertyTypeDefinition == requiredInterfaceTypeDefinition) {
				var baseGenericListType = typeof(ObservableList<>);
				var genericListType = baseGenericListType.MakeGenericType(inspector.CollectionType.GetGenericArguments());

				instance.CollectionType(genericListType);
			}
		}
	}
}
