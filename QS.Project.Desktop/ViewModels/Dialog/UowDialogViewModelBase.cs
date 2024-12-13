using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Validation;

namespace QS.ViewModels.Dialog
{
	public abstract class UowDialogViewModelBase : DialogViewModelBase, IDisposable
	{
		protected UowDialogViewModelBase(
			INavigationManager navigation,
			IUnitOfWork unitOfWork,
			IValidator validator = null) : base(navigation)
		{
			UoW = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			this.validator = validator;
		}

		public virtual IUnitOfWork UoW { get; private set; }

		public override string Title {
			get => base.Title;
			set {
				base.Title = value;
				if(UoW.ActionTitle != null && String.IsNullOrWhiteSpace(UoW.ActionTitle.UserActionTitle) && !String.IsNullOrWhiteSpace(value))
					UoW.ActionTitle.UserActionTitle = $"Диалог «{value}»";
			}
		}

		private bool manualChange = false;

		public virtual bool HasChanges {
			get { return manualChange || UoW.HasChanges; }
			set { manualChange = value; }
		}

		
		/// <summary>
		///	Основное действие сохранения, состоит из 3-х этапов:
		/// 1 - Валидация объектов, если есть замечания возвращаем false.
		/// 2 - Сохранение сущностей, вызывается метод SaveEntities. Он должен быть переопределен в диалогах, иначе нечего будет сохранять. Если есть ошибки возвращаем false.
		/// 3 - Коммит транзакции.
		/// </summary>
		/// <returns>False если операция сохранения не удалась.</returns>
		public virtual bool Save()
		{
			if(!Validate())
				return false;

			if(!SaveEntities())
				return false;
			
			UoW.Commit();
			return true;
		}

		/// <summary>
		/// Сохранить и закрыть диалог. Как правило, действие вызывается кнопкой "Сохранить" в любом диалоге.
		/// </summary>
		/// <returns>False если при сохранении были ошибки.</returns>
		public bool SaveAndClose()
		{
			if (Save()) {
				Close(false, CloseSource.Save);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Должен быть переопределен в дочерних классах для сохранения сущностей. Надо вызвать UoW.Save(Entity) для всего что сохраняем.
		/// </summary>
		/// <returns>Без переопределения всегда возвращает false.</returns>
		protected virtual bool SaveEntities() => false; //Возвращаем false, так как ничего не сохраняли. Реализация пол умолчанию добавлена, чтобы не заставлять реализовывать в диалогах которые ничего без сохранения.

		public virtual void Dispose()
		{
			UoW?.Dispose();
		}

		#region Валидация

		protected readonly IValidator validator;
		public readonly List<ValidationRequest> Validations = new List<ValidationRequest>();

		protected virtual bool Validate()
		{
			if(validator == null || !Validations.Any())
				return true;

			return validator.Validate(Validations);
		}

		#endregion
	}
}
