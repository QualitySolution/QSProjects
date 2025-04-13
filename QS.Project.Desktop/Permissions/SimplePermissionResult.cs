namespace QS.Permissions {
	public class SimplePermissionResult : IPermissionResult {
		public SimplePermissionResult(bool canCreate, bool canRead, bool canUpdate, bool canDelete)
		{
			CanCreate = canCreate;
			CanRead = canRead;
			CanUpdate = canUpdate;
			CanDelete = canDelete;
		}

		public SimplePermissionResult()
		{
		}

		public bool CanCreate { get; set; }
		public bool CanRead { get; set; }
		public bool CanUpdate { get; set; }
		public bool CanDelete { get; set; }
	}
}
