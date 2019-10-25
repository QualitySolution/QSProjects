namespace QS.Project.Domain
{
	public interface IEntityPermission
	{
		int Id { get; set; }
		TypeOfEntity TypeOfEntity { get; set; }
		bool CanCreate { get; set; }
		bool CanRead { get; set; }
		bool CanUpdate { get; set; }
		bool CanDelete { get; set; }
	}
}