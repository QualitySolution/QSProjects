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

		public abstract IDbCreatorModel CreatorFactory(CreatorFactoryArgs args);

		public IDbCreatorModel CreateCreator(CreatorFactoryArgs args) {
			return CreatorFactory(args);
		}
	}

	/// <summary>
	///   interaction — канал диалогов с пользователем
	///   serviceProvider — для резолва дополнительных зависимостей
	/// </summary>
	public class CreatorFactoryArgs {
		public IDbProvider Provider { get; set; }
		public IProgressBarDisplayable Progress { get; set; }
		public IDbCreatorInteraction Interaction { get; set; }
		public System.Threading.CancellationToken CancellationToken { get; set; }
		public IServiceProvider ServiceProvider { get; set; }
	}
}
