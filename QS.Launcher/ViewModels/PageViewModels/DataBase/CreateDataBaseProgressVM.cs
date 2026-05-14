using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using QS.DbManagement;
using QS.DBScripts.Controllers;
using QS.Dialog;
using QS.Launcher.Services;
using ReactiveUI;

namespace QS.Launcher.ViewModels.PageViewModels.DataBase {
	/// <summary>
	/// показывает прогресс создания базы, прогресс приходит из не-UI потока, поэтому все мутации
	/// reactive-свойств проксируются через IUiThreadInvoker
	/// </summary>
	public class CreateDataBaseProgressVM : CarouselPageVM, IProgressBarDisplayable {
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public IDbProvider Provider { get; private set; }
		public Connection Connection { get; private set; }
		private IReadOnlyList<DbCreationPhase> phases = Array.Empty<DbCreationPhase>();

		private readonly IDbCreatorInteraction interaction;
		private readonly IServiceProvider services;
		private readonly IUiThreadInvoker uiThread;
		private readonly CancellationTokenSource cts;

		#region IProgressBarDisplayable backed properties

		private double minValue;
		private double maxValue = 1;
		private double currentValue;
		private string currentText;
		private bool isStarted;

		public double MinValue {
			get => minValue;
			private set => this.RaiseAndSetIfChanged(ref minValue, value);
		}
		public double MaxValue {
			get => maxValue;
			private set => this.RaiseAndSetIfChanged(ref maxValue, value);
		}
		public double Value {
			get => currentValue;
			private set => this.RaiseAndSetIfChanged(ref currentValue, value);
		}
		public string CurrentText {
			get => currentText;
			private set => this.RaiseAndSetIfChanged(ref currentText, value);
		}
		public bool IsStarted {
			get => isStarted;
			private set => this.RaiseAndSetIfChanged(ref isStarted, value);
		}

		#endregion

		/// <summary>Поднимается, когда база успешно создана, на него должен быть подписан DataBasesVM</summary>
		public event Action DatabaseCreated;

		/// <summary>Поднимается, когда создание завершилось отменой</summary>
		public event Action DatabaseCreationFailed;

		public ReactiveCommand<Unit, Unit> StartCreationCommand { get; }
		public ReactiveCommand<Unit, Unit> CancelCommand { get; }

		public CreateDataBaseProgressVM(
			IDbCreatorInteraction interaction,
			IUiThreadInvoker uiThread,
			IServiceProvider services)
		{
			this.interaction = interaction ?? throw new ArgumentNullException(nameof(interaction));
			this.uiThread = uiThread ?? throw new ArgumentNullException(nameof(uiThread));
			this.services = services ?? throw new ArgumentNullException(nameof(services));
			cts = new CancellationTokenSource();

			StartCreationCommand = ReactiveCommand.CreateFromTask(StartCreationAsync);
			CancelCommand = ReactiveCommand.Create(() => {
				cts.Cancel();
				PopToRootCommand?.Execute(null);
			});
		}

		public void SetPipeline(
			IDbProvider provider, Connection connection,
			IEnumerable<DbCreationPhase> phases) {
			Provider = provider ?? throw new ArgumentNullException(nameof(provider));
			Connection = connection ?? throw new ArgumentNullException(nameof(connection));
			if(phases == null) throw new ArgumentNullException(nameof(phases));
			this.phases = phases.ToList();
			if(this.phases.Count == 0)
				throw new ArgumentException("Пайплайн создания базы пуст.", nameof(phases));
		}

		public async Task StartCreationAsync() {
			try {
				var args = new CreatorFactoryArgs {
					Provider = Provider,
					Progress = this,
					Interaction = interaction,
					CancellationToken = cts.Token,
					ServiceProvider = services
				};

				for(int i = 0; i < phases.Count; i++) {
					cts.Token.ThrowIfCancellationRequested();
					var phase = phases[i];
					uiThread.Post(() => CurrentText = phase.Title);

					bool ok = await phase.Action(args);
					if(!ok) {
						DatabaseCreationFailed?.Invoke();
						return;
					}
				}
				DatabaseCreated?.Invoke();
			}
			catch(OperationCanceledException) {
				logger.Info("Создание базы отменено.");
				DatabaseCreationFailed?.Invoke();
			}
			catch(Exception ex) {
				logger.Error(ex, "Сбой в процессе создания базы.");
				await interaction.ReportErrorAsync(ex.Message, null);
				DatabaseCreationFailed?.Invoke();
			}
		}

		#region IProgressBarDisplayable
		public void Start(double maxValue = 1, double minValue = 0, string text = null, double startValue = 0) {
			uiThread.Post(() => {
				MaxValue = maxValue;
				MinValue = minValue;
				Value = startValue;
				if(text != null) CurrentText = text;
				IsStarted = true;
			});
		}

		public void Update(double curValue) => uiThread.Post(() => Value = curValue);

		public void UpdateMax(double maxValue) => uiThread.Post(() => MaxValue = maxValue);

		public void Update(string curText) => uiThread.Post(() => CurrentText = curText);

		public void Add(double addValue = 1, string text = null) {
			uiThread.Post(() => {
				Value += addValue;
				if(text != null) CurrentText = text;
			});
		}

		public void Close() => uiThread.Post(() => IsStarted = false);
		#endregion
	}
}
