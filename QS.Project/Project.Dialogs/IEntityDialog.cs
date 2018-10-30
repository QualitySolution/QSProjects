using QS.DomainModel.UoW;

namespace QS.Project.Dialogs
{
	public interface IEntityDialog
	{
		IUnitOfWork UoW { get; }

		object EntityObject { get; }
	}

	public interface IEditableDialog
	{
		bool IsEditable { get; set; }
	}
}

