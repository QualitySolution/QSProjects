namespace QS.DomainModel.Entity.EntityPermissions
{
	public struct EntityPermission
	{
		public static EntityPermission Empty => new EntityPermission();
		public static EntityPermission AllDenied => new EntityPermission(false, false, false, false);
		public static EntityPermission AllAllowed => new EntityPermission(true, true, true, true);

		private bool initialized;

		public bool IsEmpty => !initialized;

		public bool Create { get; }
		public bool Read { get; }
		public bool Update { get; }
		public bool Delete { get; }

		public EntityPermission(bool create, bool read, bool update, bool delete)
		{
			initialized = true;
			Create = create;
			Read = read;
			Update = update;
			Delete = delete;
		}
	}
}
