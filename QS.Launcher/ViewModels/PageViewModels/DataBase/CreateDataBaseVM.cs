using Dapper;
using QS.DbManagement;
using ReactiveUI;
using QS.Launcher.AppRunner;
using QS.Project.Versioning;
using System;
using System.Windows.Input;
using System.Reactive;

namespace QS.Launcher.ViewModels.PageViewModels.DataBase {
	public class CreateDataBaseVM : ReactiveObject {
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public IDbProvider Provider { get; private set; }

		private string dbTitle;
		public string DbTitle { 
			get => dbTitle; 
			set => this.RaiseAndSetIfChanged(ref dbTitle, value);
		}
		private string dbName;
		public string DbName {
			get => dbName;
			set => this.RaiseAndSetIfChanged(ref dbName, value);
		}
		public ReactiveCommand<Unit, Unit> CreateDataBaseCommand { get; } 
		public event Action DatabaseCreated;

		public CreateDataBaseVM(IDbProvider dbProvider) {
			Provider = dbProvider;

			CreateDataBaseCommand = ReactiveCommand.Create(() => {
				if(Provider.CanCreateDatabase)
					Provider.CreateDatabase(DbName, DbTitle);
				else
					throw new InvalidOperationException("пользователь не должен получать доступ к созданию базы");

				DatabaseCreated?.Invoke();
			});
		}
	}
}
