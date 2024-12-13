﻿using QS.DomainModel.Entity;
using QS.DomainModel.UoW;

namespace QS.Dialog
{
	/// <summary>
	/// Диалог редактирования сущности
	/// </summary>
	public interface IEntityDialog : ISingleUoWDialog
	{
		object EntityObject { get; }
	}

	public interface IEntityDialog<TEntity> : IEntityDialog
		where TEntity : IDomainObject, new()
	{
		TEntity Entity { get; }
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

