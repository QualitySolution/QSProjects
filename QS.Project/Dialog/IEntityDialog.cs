using QS.DomainModel.UoW;

namespace QS.Dialog
{
	/// <summary>
	/// Диалог редактирования сущьности
	/// </summary>
	public interface IEntityDialog : ISingleUoWDialog
	{
		object EntityObject { get; }
	}

	/// <summary>
	/// Диалог с 
	/// </summary>
	public interface ISingleUoWDialog
	{
		IUnitOfWork UoW { get; }
	}

	public interface IEditableDialog
	{
		bool IsEditable { get; set; }
	}
}

