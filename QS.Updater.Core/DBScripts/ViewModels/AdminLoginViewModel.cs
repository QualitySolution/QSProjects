using QS.Navigation;
using QS.ViewModels.Dialog;

namespace QS.DBScripts.ViewModels
{
	public class AdminLoginViewModel : WindowDialogViewModelBase
	{
		public AdminLoginViewModel(INavigationManager navigation, string loginByDefault = null) : base(navigation)
		{
			IsModal = true;
			WindowPosition = Dialog.WindowGravity.Center;
			login = loginByDefault;
		}

		#region Свойства

		private string login;
		public virtual string Login {
			get => login;
			set => SetField(ref login, value);
		}

		private string password;
		public virtual string Password {
			get => password;
			set => SetField(ref password, value);
		}

		#endregion

		#region Действия

		public void Accept()
		{
			Close(false, CloseSource.Save);
		}

		public void Cancel()
		{
			Close(false, CloseSource.Cancel);
		}

		#endregion
	}
}