using QS.DbManagement.Entities;
using ReactiveUI;

namespace QS.Launcher.ViewModels.PageViewModels {
	public class BaseAccessRowVM : ReactiveObject {
		public int BaseId { get; }
		public string BaseName { get; }
		public string Title { get; }

		public bool ShowReadOnly { get; }

		public bool CanEdit { get; }

		public BaseAccessRowVM(DbUserBaseAccess access, bool showReadOnly) {
			BaseId = access.BaseId;
			BaseName = access.BaseName;
			Title = access.Title;
			ShowReadOnly = showReadOnly;
			CanEdit = access.CanEdit;
			hasAccess = originalHasAccess = access.HasAccess;
			isAdmin = originalIsAdmin = access.IsAdmin;
			readOnly = originalReadOnly = access.ReadOnly;
		}

		private bool originalHasAccess;
		private bool originalIsAdmin;
		private bool originalReadOnly;

		public bool IsDirty =>
			HasAccess != originalHasAccess || IsAdmin != originalIsAdmin || ReadOnly != originalReadOnly;

		public void AcceptChanges() {
			originalHasAccess = HasAccess;
			originalIsAdmin = IsAdmin;
			originalReadOnly = ReadOnly;
		}

		private bool hasAccess;
		public bool HasAccess {
			get => hasAccess;
			set {
				this.RaiseAndSetIfChanged(ref hasAccess, value);
				if(!value) {
					IsAdmin = false;
					ReadOnly = false;
				}
			}
		}

		private bool isAdmin;
		public bool IsAdmin {
			get => isAdmin;
			set {
				this.RaiseAndSetIfChanged(ref isAdmin, value);
				if(value) {
					HasAccess = true;
					ReadOnly = false;
				}
			}
		}

		private bool readOnly;
		public bool ReadOnly {
			get => readOnly;
			set {
				this.RaiseAndSetIfChanged(ref readOnly, value);
				if(value) {
					HasAccess = true;
					IsAdmin = false;
				}
			}
		}

		public DbUserBaseAccess ToAccess(string name, string email) => new DbUserBaseAccess {
			BaseId = BaseId,
			BaseName = BaseName,
			Title = Title,
			HasAccess = HasAccess,
			IsAdmin = IsAdmin,
			ReadOnly = ReadOnly,
			Name = name,
			Email = email
		};
	}
}
