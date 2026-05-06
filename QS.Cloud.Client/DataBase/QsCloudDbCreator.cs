using QS.Cloud.Core;
using QS.DBScripts;
using QS.DBScripts.Controllers;
using QS.DBScripts.Models;
using QS.Dialog;
using System;
using System.Threading;
using System.Threading.Tasks;

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
			BasicAuthInfoProvider AuthInfo,
			IDbScriptsConfiguration configuration,
			IProgressBarDisplayable progress,
			IDbCreatorInteraction interaction,
			CancellationToken cancellationToken)
		{
			this.baseId = baseId;

			loginClient = new LoginManagementCloudClient(AuthInfo);

			this.configuration = configuration ?? throw new ArgumentNullException(nameof(progress));
			this.progress = progress ?? throw new ArgumentNullException(nameof(progress));
			this.interaction = interaction ?? throw new ArgumentNullException(nameof(interaction));
			this.cancellationToken = cancellationToken;
		}


		public async Task<bool> RunCreationAsync(string dbName, string dbTitle) {
			try {
				StartSessionResponse session = loginClient.StartSession(baseId);

				if(!session.Success) {
					await interaction.ReportErrorAsync("Ошибка в создании сесии", "Запрос в облако");
					throw new InvalidOperationException("Ошибка в создании сесии");
				}
				else if(!session.IsAdmin) {
					await interaction.ReportErrorAsync("Вы не имеете прав Администратора", "Запрос в облако");
				}

				var infoProvider = new SessionInfoProvider(sessionId: session.SessionId);
				var sessionLife = new AliveCloudClient(infoProvider);
				sessionLife.NewMessage += (mes) => {
					progress.Update("Сессия: " + mes + " в статусе " + sessionLife.LastStatus.ToString());
				};
				sessionLife.KeepAlive();

				var creator = new MySqlDbCreateModel(session.Db.Server, session.Db.Port, session.Db.Login, session.Db.Password, configuration, progress, interaction, cancellationToken);
				creator.FillBaseGuid = false;
				bool success = await creator.RunCreationAsync(dbName, dbTitle);

				sessionLife.Dispose();

				return success;
			}
			catch(OperationCanceledException) {
				logger.Info("Создание базы в облаке отменено пользователем.");
				return false;
			}
			catch(Exception ex) {
				logger.Error(ex, "Ошибка при создании базы в облаке.");
				await interaction.ReportErrorAsync(ex.Message, null);
				throw;
			}
			finally {
				if(progress.IsStarted)
					progress.Close();
			}
		}
	}
}
