using QS.DbManagement.Entities;
using ReactiveUI;

namespace QS.Launcher.ViewModels.PageViewModels {
	public class BaseAccessRowVM : ReactiveObject {
		public int BaseId { get; }
		public string BaseName { get; }
		public string Title { get; }

		public bool ShowReadOnly { get; }

		public bool ShowAppPermissions { get; }

		public bool CanEdit { get; }

		public BaseAccessRowVM(DbUserBaseAccess access, bool showReadOnly, bool showAppPermissions) {
			BaseId = access.BaseId;
			BaseName = access.BaseName;
			Title = access.Title;
			ShowReadOnly = showReadOnly;
			ShowAppPermissions = showAppPermissions;
			CanEdit = access.CanEdit;
			hasAccess = originalHasAccess = access.HasAccess;
			isAdmin = originalIsAdmin = access.IsAdmin;
			readOnly = originalReadOnly = access.ReadOnly;
			canDelete = originalCanDelete = access.CanDelete;
			canAccountingSettings = originalCanAccountingSettings = access.CanAccountingSettings;
			canChangeDocumentDate = originalCanChangeDocumentDate = access.CanChangeDocumentDate;
		}

		private bool originalHasAccess;
		private bool originalIsAdmin;
		private bool originalReadOnly;
		private bool originalCanDelete;
		private bool originalCanAccountingSettings;
		private bool originalCanChangeDocumentDate;

		public bool IsDirty =>
			HasAccess != originalHasAccess || IsAdmin != originalIsAdmin || ReadOnly != originalReadOnly
			|| CanDelete != originalCanDelete
			|| CanAccountingSettings != originalCanAccountingSettings
			|| CanChangeDocumentDate != originalCanChangeDocumentDate;

		public void AcceptChanges() {
			originalHasAccess = HasAccess;
			originalIsAdmin = IsAdmin;
			originalReadOnly = ReadOnly;
			originalCanDelete = CanDelete;
			originalCanAccountingSettings = CanAccountingSettings;
			originalCanChangeDocumentDate = CanChangeDocumentDate;
		}

		public bool AppPermissionsVisible => ShowAppPermissions && HasAccess;

		private bool hasAccess;
		public bool HasAccess {
			get => hasAccess;
			set {
				this.RaiseAndSetIfChanged(ref hasAccess, value);
				this.RaisePropertyChanged(nameof(AppPermissionsVisible));
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

		private bool canDelete;
		public bool CanDelete {
			get => canDelete;
			set => this.RaiseAndSetIfChanged(ref canDelete, value);
		}

		private bool canAccountingSettings;
		public bool CanAccountingSettings {
			get => canAccountingSettings;
			set => this.RaiseAndSetIfChanged(ref canAccountingSettings, value);
		}

		private bool canChangeDocumentDate;
		public bool CanChangeDocumentDate {
			get => canChangeDocumentDate;
			set => this.RaiseAndSetIfChanged(ref canChangeDocumentDate, value);
		}

		public DbUserBaseAccess ToAccess() => new DbUserBaseAccess {
			BaseId = BaseId,
			BaseName = BaseName,
			Title = Title,
			HasAccess = HasAccess,
			IsAdmin = IsAdmin,
			ReadOnly = ReadOnly,
			CanDelete = CanDelete,
			CanAccountingSettings = CanAccountingSettings,
			CanChangeDocumentDate = CanChangeDocumentDate
		};
	}
}
