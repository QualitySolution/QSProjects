using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Gamma.GtkWidgets;
using Gtk;
using NLog;
using QS.Dialog.GtkUI;
using QS.Project.DB;
using QS.Project.Dialogs.GtkUI.ServiceDlg;
using QS.Project.Domain;
using QS.Project.Repositories;
using QS.Project.Services.GtkUI;
using QS.Services;
using QS.Widgets.GtkUI;

namespace QS.Project.Dialogs.GtkUI
{
	public partial class UserDialog : Gtk.Dialog
	{
		#region Глобальные настройки

		public static int? RequestWidth { get; set; } = null;
		public static int? RequestHeight { get; set; } = null;

		public static Func<List<IPermissionsView>> PermissionViewsCreator;

		public static Func<List<IUserPermissionTab>> UserPermissionViewsCreator;
		#endregion

		private static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly IInteractiveService interactiveService;
        private const string passFill = "n0tChanG3d";

		private bool IsNewUser => User.Id == 0;

		Dictionary<string, CheckButton> RightCheckButtons;
		List<IPermissionsView> permissionViews;
		List<IUserPermissionTab> userPermissionViews;
		MySQLUserRepository mySQLUserRepository;

		public UserBase User { get; private set; }

		public UserDialog(int userId, IInteractiveService interactiveService)
		{
            this.interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
            this.Build();

			mySQLUserRepository = new MySQLUserRepository(new MySQLProvider(new GtkRunOperationService(), new GtkQuestionDialogsInteractive()), new GtkInteractiveService());

			User = mySQLUserRepository.GetUser(userId);

			ConfigureDlg();
		}

		public UserDialog(IInteractiveService interactiveService)
		{
            this.interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
            this.Build();

			mySQLUserRepository = new MySQLUserRepository(new MySQLProvider(new GtkRunOperationService(), new GtkQuestionDialogsInteractive()), new GtkInteractiveService());

			User = new UserBase();

			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			Title = User.Name;

			if(RequestWidth.HasValue) {
				WidthRequest = RequestWidth.Value;
			}
			if(RequestHeight.HasValue) {
				HeightRequest = RequestHeight.Value;
			}

			RightCheckButtons = new Dictionary<string, CheckButton>();
			if(PermissionViewsCreator != null) {
				permissionViews = PermissionViewsCreator();
			}

			labelId.Binding.AddFuncBinding(User, x => x.Id.ToString(), w => w.LabelProp).InitializeFromSource();
			entryName.Binding.AddBinding(User, x => x.Name, w => w.Text).InitializeFromSource();
			entryLogin.Binding.AddBinding(User, x => x.Login, w => w.Text).InitializeFromSource();
			entryPassword.Text = mySQLUserRepository.GetPasswordProxy();
			entryEmail.Binding.AddBinding(User, x => x.Email, w => w.Text).InitializeFromSource();
			checkAdmin.Binding.AddBinding(User, x => x.IsAdmin, w => w.Active).InitializeFromSource();
			textviewComments.Binding.AddBinding(User, x => x.Email, w => w.Buffer.Text).InitializeFromSource();
			checkDeactivated.Binding.AddBinding(User, x => x.Deactivated, w => w.Active).InitializeFromSource();
			InitializePermissionViews();
		}

		private void InitializePermissionViews()
		{
			if(IsNewUser) {
				return;
			}

			userpermissionwidget.InitilizeTabs();
			userPermissionViews = UserPermissionViewsCreator();
			foreach(var tab in userPermissionViews) {
				userpermissionwidget.AddTab(tab);
			}

			if(permissionViews != null) {
				var permissionFieldNames = permissionViews.Select(x => x.DBFieldName);
				var permissionFiledValues = mySQLUserRepository.GetExtraFieldValues(User.Id, permissionFieldNames);

				foreach(var view in permissionViews) {
					userpermissionwidget.AddTab((Widget)view, view.ViewName);
					if(permissionFiledValues.ContainsKey(view.DBFieldName)) {
						view.DBFieldValue = permissionFiledValues[view.DBFieldName];
					}
					(view as Widget).Show();
				}
			}
			userpermissionwidget.ConfigureDlg(User.Id);
		}

		protected void OnButtonOkClicked(object sender, EventArgs e)
		{
			if(entryLogin.Text == "root") {
				string Message = "Операции с пользователем root запрещены.";
				MessageDialog md = new MessageDialog(this, DialogFlags.DestroyWithParent,
									   MessageType.Warning,
									   ButtonsType.Ok,
									   Message);
				md.Run();
				md.Destroy();
				return;
			}
			var regex = new Regex(@"^[a-zA-Z\d.,-_]");
			if (!regex.IsMatch(entryLogin.Text))
			{
				interactiveService.ShowMessage(Dialog.ImportanceLevel.Error, "Логин может состоять только из букв английского алфавита, нижнего подчеркивания, дефиса, точки и запятой");
				entryLogin.Text = string.Empty;
				return;
			}
			if(IsNewUser) {
				mySQLUserRepository.CreateUser(User, entryPassword.Text, GetExtraFieldsForSelect(), GetExtraFieldsForInsert(), GetPermissionValues());
			} else {
				mySQLUserRepository.UpdateUser(User, entryPassword.Text, GetExtraFieldsForUpdate(), GetPermissionValues());
				userpermissionwidget.Save();
			}
		}

		#region Дополнительные вкладки

		private Dictionary<string, string> GetPermissionValues()
		{
			Dictionary<string, string> permissionValues = new Dictionary<string, string>();
			if(permissionViews != null) {
				foreach(var view in permissionViews) {
					permissionValues.Add(view.DBFieldName, view.DBFieldValue);
				}
			}
			return permissionValues;
		}

		private string GetExtraFieldsForUpdate()
		{
			if(permissionViews == null || permissionViews.Count == 0)
				return string.Empty;

			string FieldsString = string.Empty;
			foreach(var view in permissionViews) {
				FieldsString += ", " + view.DBFieldName + " = @" + view.DBFieldName;
			}
			return FieldsString;
		}

		private string GetExtraFieldsForSelect()
		{
			if(permissionViews == null || permissionViews.Count == 0)
				return string.Empty;

			return string.Join(string.Empty, permissionViews.Select(x => $", {x.DBFieldName}"));
		}

		private string GetExtraFieldsForInsert()
		{
			if(permissionViews == null || permissionViews.Count == 0)
				return string.Empty;

			return string.Join(string.Empty, permissionViews.Select(x => $", @{x.DBFieldName}"));
		}

		#endregion
	}
}
