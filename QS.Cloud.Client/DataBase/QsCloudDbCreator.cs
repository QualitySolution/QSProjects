using QS.Cloud.Core;
using QS.DBScripts;
using QS.DBScripts.Controllers;
using QS.DBScripts.Models;
using QS.Dialog;
using System;
using System.Threading;

namespace QS.Cloud.Client.DataBase
{
	public class QsCloudDbCreator : IDbCreatorModel
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private readonly int baseId;
		private readonly IProgressBarDisplayable progress;
		private readonly IDbCreatorInteraction interaction;
		private readonly IDbScriptsConfiguration configuration;
		private readonly CancellationToken cancellationToken;

		private LoginManagementCloudClient loginClient;

		public QsCloudDbCreator(
			int baseId,
			BasicAuthInfoProvider authInfo,
			IDbScriptsConfiguration configuration,
			IProgressBarDisplayable progress,
			IDbCreatorInteraction interaction,
			CancellationToken cancellationToken)
		{
			this.baseId = baseId;
			loginClient = new LoginManagementCloudClient(authInfo);
			this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			this.progress = progress ?? throw new ArgumentNullException(nameof(progress));
			this.interaction = interaction ?? throw new ArgumentNullException(nameof(interaction));
			this.cancellationToken = cancellationToken;
		}

		public bool RunCreation(string dbName, string dbTitle) {
			try {
				cancellationToken.ThrowIfCancellationRequested();

				StartSessionResponse session = loginClient.StartSession(baseId);

				if(!session.Success) {
					interaction.ReportError("Ошибка в создании сессии", "Запрос в облако");
					return false;
				}
				if(!session.IsAdmin) {
					interaction.ReportError("Вы не имеете прав Администратора", "Запрос в облако");
					return false;
				}

				var infoProvider = new SessionInfoProvider(sessionId: session.SessionId);
				var sessionLife = new AliveCloudClient(infoProvider);
				sessionLife.NewMessage += (mes) => {
					progress.Update("Сессия: " + mes + " в статусе " + sessionLife.LastStatus.ToString());
				};
				sessionLife.KeepAlive();

				var creator = new MySqlDbCreateModel(
					session.Db.Server, session.Db.Port, session.Db.Login, session.Db.Password,
					configuration.MakeCreationScript(), progress, interaction, cancellationToken);
				creator.FillBaseGuid = false;
				bool success = creator.RunCreation(session.Db.BaseName, dbTitle);

				sessionLife.Dispose();
				return success;
			}
			catch(OperationCanceledException) {
				logger.Info("Создание базы в облаке отменено пользователем.");
				return false;
			}
			catch(Exception ex) {
				logger.Error(ex, "Ошибка при создании базы в облаке.");
				interaction.ReportError(ex.Message, null);
				throw;
			}
			finally {
				if(progress.IsStarted)
					progress.Close();
			}
		}
	}
}
