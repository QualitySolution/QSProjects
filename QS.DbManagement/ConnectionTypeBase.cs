using System;
using System.Collections.Generic;
using QS.DBScripts.Controllers;
using QS.Dialog;

namespace QS.DbManagement {

	public abstract class ConnectionTypeBase {
		public string Title { get; protected set; }
		public string ConnectionTypeName { get; protected set; }

		public List<ConnectionParameter> Parameters { get; } = new List<ConnectionParameter>();

		public byte[] IconBytes { get; protected set; }

		public abstract bool CanConnect(IEnumerable<ConnectionParameterValue> parameters);

		public abstract IDbProvider CreateProvider(IList<ConnectionParameterValue> parameters, string password = null);

		/// <summary>
		/// Заполняется композиционным корнем приложения 
		/// который один знает обо всех конкретных реализациях creator-ов и
		/// о том, как из IDbProvider достать строку подключения
		///
		///   interaction        — канал диалогов с пользователем
		///   serviceProvider    — для резолва дополнительных зависимостей
		/// </summary>
		public Func<CreatorFactoryArgs, IDBCreator> CreatorFactory { get; set; }

		public IDBCreator CreateCreator(CreatorFactoryArgs args) {
			if(CreatorFactory == null)
				throw new InvalidOperationException(
					$"Для типа подключения '{ConnectionTypeName}' не задана CreatorFactory. "
					+ "Зарегистрируйте её в композиционном корне приложения.");
			return CreatorFactory(args);
		}
	}
	public class CreatorFactoryArgs {
		public IDbProvider Provider { get; set; }
		public IProgressBarDisplayable Progress { get; set; }
		public IDbCreatorInteraction Interaction { get; set; }
		public System.Threading.CancellationToken CancellationToken { get; set; }
		public IServiceProvider ServiceProvider { get; set; }
	}
}
