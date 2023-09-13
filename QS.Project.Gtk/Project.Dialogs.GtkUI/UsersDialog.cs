using System;
using System.Data.Bindings.Collections.Generic;
using Gamma.GtkWidgets;
using Gdk;
using Gtk;
using QS.Deletion;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.DB;
using QS.Project.Dialogs.GtkUI.ServiceDlg;
using QS.Project.Domain;
using QS.Project.Repositories;
using QS.Project.Services.GtkUI;

namespace QS.Project.Dialogs.GtkUI
{
	public partial class UsersDialog : Gtk.Dialog
	{
		UsersModel usersModel;
		MySQLUserRepository mySQLUserRepository;
        private readonly IInteractiveService interactiveService;
		private readonly Color _blueColor = new Color(0, 0, 255);
		private readonly Color _tealColor = new Color(0, 128, 128);

        public UsersDialog(IInteractiveService interactiveService)
        {
	        this.interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
            this.Build();
			usersModel = new UsersModel();
			usersModel.UsersUpdated += UsersModel_UsersUpdated;
			mySQLUserRepository = new MySQLUserRepository(new MySQLProvider(new GtkRunOperationService(), new GtkQuestionDialogsInteractive()), new GtkInteractiveService());
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			treeviewUsers.ColumnsConfig = ColumnsConfigFactory.Create<UserBase>()
				.AddColumn("Код")
					.AddNumericRenderer(x => x.Id)
				.AddColumn("Логин")
					.AddTextRenderer(x => x.Login)
				.AddColumn("Имя")
					.AddTextRenderer(x => x.Name)
				.AddColumn("Администратор")
					.AddToggleRenderer(x => x.IsAdmin)
					.Editing(false)
				.RowCells()
					.AddSetter<CellRendererText>(
						(c, n) => {
							if(n.IsAdmin && n.Deactivated) {
								c.ForegroundGdk = _tealColor;
								return;
							}
							if(n.IsAdmin && !n.Deactivated) {
								c.ForegroundGdk = _blueColor;
								return;
							}
							if(!n.IsAdmin && n.Deactivated) {
								c.ForegroundGdk = Rc.GetStyle(treeviewUsers).Text(StateType.Insensitive);
								return;
							}
							c.ForegroundGdk = Rc.GetStyle(treeviewUsers).Text(StateType.Normal);
						}
					)
				.Finish();
			usersModel.UpdateUsers(chkShowInactive.Active);
		}

		void UsersModel_UsersUpdated(object sender, EventArgs e)
		{
			treeviewUsers.ItemsDataSource = usersModel.ObservableUsers;
		}

		protected void OnChkShowInactiveToggled(object sender, EventArgs e)
		{
			usersModel.UpdateUsers(chkShowInactive.Active);
		}

		protected void OnButtonDeleteClicked(object sender, EventArgs e)
		{
			if (treeviewUsers.GetSelectedObject() is UserBase selectedUser && DeleteHelper.DeleteEntity(typeof(UserBase), selectedUser.Id)) {
				mySQLUserRepository.DropUser(selectedUser.Login);

				if (selectedUser.Id == UserRepository.GetCurrentUserId()) {
					MessageDialog md = new MessageDialog(this, DialogFlags.DestroyWithParent,
										   MessageType.Warning, ButtonsType.Close,
										   "Был удален пользователь, под которым Вы подключились к базе данных, чтобы не допустить некорректных операций программа закроется. Зайдите в программу от имени другого пользователя.");
					md.Run();
					md.Destroy();
					Environment.Exit(0);
				}

				usersModel.ObservableUsers.Remove(selectedUser);
			}
		}

		protected void OnButtonEditClicked(object sender, EventArgs e)
		{
			EditUser();
		}

		private void EditUser()
		{
			if (treeviewUsers.GetSelectedObject() is UserBase selectedUser) {
				UserDialog userDialog = new UserDialog(selectedUser.Id, interactiveService);
				userDialog.ShowAll();
				if (userDialog.Run() == (int)ResponseType.Ok)
					usersModel.UpdateUsers(chkShowInactive.Active);

				userDialog.Destroy();
			}
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			UserDialog userDialog = new UserDialog(interactiveService);
			userDialog.ShowAll();
			if (userDialog.Run() == (int)ResponseType.Ok)
				usersModel.UpdateUsers(chkShowInactive.Active);

			userDialog.Destroy();
		}

		protected void OnTreeviewUsersRowActivated(object o, RowActivatedArgs args)
		{
			EditUser();
		}
	}

	public class UsersModel : PropertyChangedBase
	{
		public event EventHandler UsersUpdated;

		GenericObservableList<UserBase> observableUsers;
		public virtual GenericObservableList<UserBase> ObservableUsers {
			get {
				if (observableUsers == null)
					ObservableUsers = new GenericObservableList<UserBase>();
				return observableUsers;
			}
			set => SetField(ref observableUsers, value, () => ObservableUsers);
		}

		public void UpdateUsers(bool showDeactivated)
		{
			using (var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var users = UserRepository.GetUsers(uow, showDeactivated);
				ObservableUsers = new GenericObservableList<UserBase>(users);
				UsersUpdated?.Invoke(this, EventArgs.Empty);
			}
		}
	}
}
