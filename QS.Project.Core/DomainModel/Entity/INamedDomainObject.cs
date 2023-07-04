namespace QS.DomainModel.Entity {
	public interface INamedDomainObject : IDomainObject {
		string Name { get; }
	}
}
