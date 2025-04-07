namespace QS.Permissions {
	public class SimplePermissionResult : IPermissionResult {
		public bool CanCreate { get; set; }
		public bool CanRead { get; set; }
		public bool CanUpdate { get; set; }
		public bool CanDelete { get; set; }
	}
}
