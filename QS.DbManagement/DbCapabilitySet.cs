namespace QS.DbManagement {
	/// <summary>собирается один раз на смену провайдера и дальше не меняется</summary>
	public class DbCapabilitySet {
		/// <summary>Ничего не доступно - состояние до входа на сервер</summary>
		public static readonly DbCapabilitySet None = new DbCapabilitySet();

		/// <summary>Создание базы из встроенного скрипта</summary>
		public bool CanCreate { get; internal set; }

		/// <summary>Наполнение новой базы дампом</summary>
		public bool CanImport { get; internal set; }

		public bool CanDrop { get; internal set; }

		public bool CanBackup { get; internal set; }

		/// <summary>Пересборка локальной метаинформации по реальному состоянию сервера</summary>
		public bool CanRefreshMetadata { get; internal set; }

		public bool CanChangeOwnPassword { get; internal set; }

		public bool CanManageUsers { get; internal set; }

		/// <summary>Есть ли что показать в меню строки списка баз</summary>
		public bool CanManageDatabases => CanDrop || CanBackup;

		/// <summary>Есть ли что показать в меню операций с базами</summary>
		public bool CanOpenDbOperations => CanCreate || CanImport || CanRefreshMetadata;
	}
}
