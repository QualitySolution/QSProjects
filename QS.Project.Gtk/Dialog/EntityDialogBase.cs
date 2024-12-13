using System;
using System.ComponentModel;
using System.Linq;
using Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.Permissions;
using QS.Project.Repositories;
using QS.Tdi;
using QS.Utilities;
using QS.Utilities.Text;
using QS.Project.Services;
using QS.Services;
using QS.Navigation;

namespace QS.Dialog.Gtk
{
	public abstract class EntityDialogBase<TEntity> : TdiTabBase, IEntityDialog<TEntity>, ITdiDialog, IEntityDialog
		where TEntity : IDomainObject, new()
	{
		protected bool IsDestroyed;

		public IUnitOfWork UoW { get; protected set; }

		public override bool CompareHashName (string hashName)
		{
			if (entity == null || UoW == null || Entity.Id == 0)
				return false;
			return GenerateHashName (entity.Id) == hashName;
		}

		public static string GenerateHashName (int id)
		{
			return DialogHelper.GenerateDialogHashName (typeof (TEntity), id);
		}

		public static string GenerateHashName(TEntity entity)
		{
			return DialogHelper.GenerateDialogHashName(typeof(TEntity), entity.Id);
		}

		public static string GenerateHashName()
		{
			return DialogHelper.GenerateDialogHashName(typeof(TEntity), 0);
		}

		private bool manualChange = false;

		public virtual bool HasChanges {
			get { return manualChange || UoW.HasChanges; }
			set { manualChange = value; }
		}

		public virtual bool HasCustomCancellationConfirmationDialog { get; private set; }

		public virtual Func<int> CustomCancellationConfirmationDialogFunc { get; private set; }

		public object EntityObject => Entity;

		private TEntity entity;
		public TEntity Entity {
			get => entity;
			set {
				entity = value;
				OnTabNameChanged ();
				CheckButtonSubscription ();
			}
		}

		private string tabName = String.Empty;

		public override string TabName {
			get {
				if (!String.IsNullOrWhiteSpace (tabName))
					return tabName;
				if (UoW != null && Entity != null) {
					var att = typeof (TEntity).GetCustomAttributes (typeof (AppellativeAttribute), true);
					AppellativeAttribute subAtt = (att.FirstOrDefault () as AppellativeAttribute);

					if (Entity.Id == 0) {
						if (subAtt != null && !String.IsNullOrWhiteSpace (subAtt.Nominative)) {
							switch (subAtt.Gender) {
							case GrammaticalGender.Masculine:
								return "Новый " + subAtt.Nominative;
							case GrammaticalGender.Feminine:
								return "Новая " + subAtt.Nominative;
							case GrammaticalGender.Neuter:
								return "Новое " + subAtt.Nominative;
							default:
								return "Новый(ая) " + subAtt.Nominative;
							}
						}
					} else {
						var notifySubject = Entity as INotifyPropertyChanged;

						var prop = Entity.GetType().GetProperty("Title");
						if (prop != null) {
							if (notifySubject != null) {
								notifySubject.PropertyChanged -= Subject_TitlePropertyChanged;
								notifySubject.PropertyChanged += Subject_TitlePropertyChanged;
							}
							return prop.GetValue (Entity, null).ToString ();
						}

						prop = Entity.GetType ().GetProperty ("Name");
						if (prop != null) {
							if (notifySubject != null) {
								notifySubject.PropertyChanged -= Subject_NamePropertyChanged;
								notifySubject.PropertyChanged += Subject_NamePropertyChanged;
							}
							return prop.GetValue (Entity, null).ToString ();
						}

						if (subAtt != null && !String.IsNullOrWhiteSpace (subAtt.Nominative))
							return subAtt.Nominative.StringToTitleCase();
					}
					return Entity.ToString ();
				}
				return String.Empty;
			}
			protected set {
				if (tabName == value)
					return;
				tabName = value;
				OnTabNameChanged ();
			}
		}

		void Subject_NamePropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Name")
				OnTabNameChanged ();
		}

		void Subject_TitlePropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Title")
				OnTabNameChanged ();
		}

		public abstract bool Save ();

		public event EventHandler<EntitySavedEventArgs> EntitySaved;

		protected IPermissionResult permissionResult { get; set; }

		protected EntityDialogBase() {
			InitializePermissionValidator();
		}

		protected void InitializePermissionValidator()
		{
			permissionResult = new PermissionResult(EntityPermission.AllAllowed);

			Type entityType = typeof(TEntity);
			int? currUserId;
			using(IUnitOfWork uow = ServicesConfig.UnitOfWorkFactory.Create()) {
				currUserId = UserRepository.GetCurrentUser(uow)?.Id;
			}
			if(!currUserId.HasValue)
				return;
			permissionResult = ServicesConfig.CommonServices.PermissionService.ValidateUserPermission(entityType, currUserId.Value);

			if(!permissionResult.CanRead) {
				var message = PermissionsSettings.GetEntityReadValidateResult(entityType);
				MessageDialogHelper.RunErrorDialog(message);
				FailInitialize = true;
			}
		}

		protected void OnEntitySaved (bool tabClosed = false)
		{
			if (EntitySaved != null)
				EntitySaved (this, new EntitySavedEventArgs (entity, tabClosed));
		}

		protected void OnButtonSaveClicked (object sender, EventArgs e)
		{
			saveButton.Sensitive = false;
			if (!this.HasChanges || Save ()) {
				OnEntitySaved (true);
				OnCloseTab (false, CloseSource.Save);
			}
			if(!IsDestroyed)
				saveButton.Sensitive = true;
		}

		protected void OnButtonCancelClicked (object sender, EventArgs e)
		{
			OnCloseTab (true, CloseSource.Cancel);
		}

		public override void Destroy ()
		{
			IsDestroyed = true;
			UoW?.Dispose();
			base.Destroy ();
		}

		protected override void OnTabNameChanged ()
		{
			base.OnTabNameChanged();

			if(UoW?.ActionTitle != null)
				UoW.ActionTitle.UserActionTitle = $"Диалог '{TabName}'";
		}

		protected void OpenTab (string hashName, Func<ITdiTab> newTabFunc)
		{
			ITdiTab tab = TabParent.FindTab (hashName);

			if (tab == null)
				TabParent.AddTab (newTabFunc (), this);
			else
				TabParent.SwitchOnTab (tab);
		}

		[Obsolete("Используете вместо этого метод OpenNewTab")]
		protected void OpenTab(ITdiTab tab)
		{
			TabParent.AddTab(tab, this);
		}

		public void SaveAndClose ()
		{
			OnButtonSaveClicked (this, EventArgs.Empty);
		}

		private Button saveButton, cancelButton;
		//FIXME Возможно временный метод, в нем не было необходимости пока в MonoDevelop не появился баг, с удалением подписок с кнопок Save и Cancel
		private void CheckButtonSubscription()
		{
			saveButton = GtkHelper.EnumerateAllChildren(this).OfType<Button> ().FirstOrDefault (x => x.Name == "buttonSave");
			if(saveButton != null)
			{
				saveButton.Clicked -= OnButtonSaveClicked;
				saveButton.Clicked += OnButtonSaveClicked;
			}
			cancelButton = GtkHelper.EnumerateAllChildren (this).OfType<Button> ().FirstOrDefault (x => x.Name == "buttonCancel");
			if (cancelButton != null) {
				cancelButton.Clicked -= OnButtonCancelClicked;
				cancelButton.Clicked += OnButtonCancelClicked;
			}
		}
	}
}

