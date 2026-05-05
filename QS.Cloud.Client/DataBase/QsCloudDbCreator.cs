using System;
using System.Threading;
using System.Threading.Tasks;
using QS.Cloud.Core;
using QS.DbManagement;
using QS.DBScripts.Controllers;
using QS.Dialog;

namespace QS.Cloud.Client.DataBase
{
	public class QsCloudDbCreator : IDbCreatorModel
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private readonly QSCloudProvider provider;
		private readonly IProgressBarDisplayable progress;
		private readonly IDbCreatorInteraction interaction;
		private readonly CancellationToken cancellationToken;

		public QsCloudDbCreator(
			IDbProvider provider,
			IProgressBarDisplayable progress,
			IDbCreatorInteraction interaction,
			CancellationToken cancellationToken)
		{
			this.provider = (provider ?? throw new ArgumentNullException(nameof(provider))) as QSCloudProvider
				?? throw new ArgumentException("Ожидается QSCloudProvider", nameof(provider));
			this.progress = progress ?? throw new ArgumentNullException(nameof(progress));
			this.interaction = interaction ?? throw new ArgumentNullException(nameof(interaction));
			this.cancellationToken = cancellationToken;
		}

		public async Task<bool> RunCreationAsync(string dbName, string dbTitle) {
			try {
				cancellationToken.ThrowIfCancellationRequested();

				// это унарный вызов, прогресс индикативный
				progress.Start(maxValue: 1, text: $"Создаём базу {dbTitle} в облаке");
				bool created = await Task.Run(
					() => provider.CreateDatabase(dbName, dbTitle),
					cancellationToken);

				if(!created) {
					await interaction.ReportErrorAsync(
						"Облако сообщило, что создание базы не удалось.", null);
					return false;
				}
				progress.Add(text: "База создана, начинаем наполнение");
				cancellationToken.ThrowIfCancellationRequested();

				// Сервер напоняет базу и шлёт прогресс
				bool finalSuccess = false;
				string finalError = null;
				using(var call = provider.FillDataBase(dbName, dbTitle, cancellationToken)) {
					while(await call.ResponseStream.MoveNext(cancellationToken)) {
						var msg = call.ResponseStream.Current;
						ApplyToProgress(msg);

						if(msg.Stage == FillDataBaseProgress.Types.Stage.Finished) {
							finalSuccess = msg.Success;
							finalError = msg.ErrorText;
							break;
						}
					}
				}

				if(!finalSuccess) {
					await interaction.ReportErrorAsync(
						string.IsNullOrEmpty(finalError) ? "Облако не смогло наполнить базу." : finalError,
						null);
					return false;
				}
				return true;
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

		private void ApplyToProgress(FillDataBaseProgress msg) {
			switch(msg.Stage) {
				//перезапускает шкалу с новым max
				case FillDataBaseProgress.Types.Stage.Started:
					progress.Start(
						maxValue: msg.Max <= 0 ? 1 : msg.Max,
						text: string.IsNullOrEmpty(msg.Text) ? null : msg.Text,
						startValue: msg.Current);
					break;
				case FillDataBaseProgress.Types.Stage.Progress:
					progress.Update(msg.Current);
					if(!string.IsNullOrEmpty(msg.Text))
						progress.Update(msg.Text);
					break;
				case FillDataBaseProgress.Types.Stage.Finished:
					if(!string.IsNullOrEmpty(msg.Text))
						progress.Update(msg.Text);
					break;
			}
		}
	}
}
