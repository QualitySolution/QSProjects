namespace QS.DomainModel.Entity.EntityPermissions
{
	public struct EntityPermission
	{
		private bool initialized;
		public bool IsEmpty => !initialized;

		public bool Create { get; }
		public bool Read { get; }
		public bool Update { get; }
		public bool Delete { get; }

		public static EntityPermission Empty => new EntityPermission();

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
