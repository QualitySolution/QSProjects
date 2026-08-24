using System;
using QS.DbManagement.Entities;
using ReactiveUI;

namespace QS.Launcher.ViewModels.PageViewModels {
	public class UserCardVM : ReactiveObject {
		public UserCardVM() {
			AcceptChanges();
		}

		#region Поля

		private string login;
		public string Login {
			get => login;
			set => this.RaiseAndSetIfChanged(ref login, value);
		}

		private string name;
		public string Name {
			get => name;
			set => this.RaiseAndSetIfChanged(ref name, value);
		}

		private string email;
		public string Email {
			get => email;
			set => this.RaiseAndSetIfChanged(ref email, value);
		}

		private string phone;
		public string Phone {
			get => phone;
			set => this.RaiseAndSetIfChanged(ref phone, value);
		}

		private string post;
		public string Post {
			get => post;
			set => this.RaiseAndSetIfChanged(ref post, value);
		}

		private string comment;
		public string Comment {
			get => comment;
			set => this.RaiseAndSetIfChanged(ref comment, value);
		}

		private bool disabled;
		public bool Disabled {
			get => disabled;
			set => this.RaiseAndSetIfChanged(ref disabled, value);
		}

		private bool isAdmin;
		public bool IsAdmin {
			get => isAdmin;
			set => this.RaiseAndSetIfChanged(ref isAdmin, value);
		}

		private string newPassword;
		public string NewPassword {
			get => newPassword;
			set => this.RaiseAndSetIfChanged(ref newPassword, value);
		}

		private bool isNew;
		public bool IsNew {
			get => isNew;
			set {
				this.RaiseAndSetIfChanged(ref isNew, value);
				this.RaisePropertyChanged(nameof(PasswordWatermark));
			}
		}

		public string PasswordWatermark => IsNew
			? "задайте пароль нового пользователя"
			: "оставьте пустым, чтобы не менять";

		#endregion

		#region Видимость полей

		public bool ShowName { get; private set; }
		public bool ShowEmail { get; private set; }
		public bool ShowPhone { get; private set; }
		public bool ShowPost { get; private set; }
		public bool ShowComment { get; private set; }
		public bool ShowAdminFlag { get; private set; }
		public bool ShowDisabling { get; private set; }

		public void ApplySupportedFields(DbUserFields fields) {
			ShowName = fields.HasFlag(DbUserFields.Name);
			ShowEmail = fields.HasFlag(DbUserFields.Email);
			ShowPhone = fields.HasFlag(DbUserFields.Phone);
			ShowPost = fields.HasFlag(DbUserFields.Post);
			ShowComment = fields.HasFlag(DbUserFields.Comment);
			ShowAdminFlag = fields.HasFlag(DbUserFields.AdminFlag);
			ShowDisabling = fields.HasFlag(DbUserFields.Disabling);
		}

		#endregion

		#region Изменения

		private static readonly string signatureSeparator = ((char)1).ToString();

		private string loadedSignature;

		private string Signature() => string.Join(signatureSeparator,
			Login, Name, Email, Phone, Post, Comment,
			Disabled ? "1" : "0", IsAdmin ? "1" : "0");

		/// <summary>есть что писать</summary>
		public bool IsDirty => Signature() != loadedSignature;

		/// <summary>текущее состояние карточки исходным</summary>
		public void AcceptChanges() => loadedSignature = Signature();

		#endregion

		/// <summary>исходное состояние</summary>
		public void Load(DbUserInfo user) {
			if(user == null)
				throw new ArgumentNullException(nameof(user));

			Login = user.Login;
			Name = user.Name;
			Email = user.Email;
			Phone = user.Phone;
			Post = user.Post;
			Comment = user.Comment;
			Disabled = user.Disabled;
			IsAdmin = user.IsAdmin;
			NewPassword = null;

			AcceptChanges();
		}

		public DbUserInfo ToUser() => new DbUserInfo {
			Login = Login,
			Name = Name,
			Email = Email,
			Phone = Phone,
			Post = Post,
			Comment = Comment,
			Disabled = Disabled,
			IsAdmin = IsAdmin
		};
	}
}
