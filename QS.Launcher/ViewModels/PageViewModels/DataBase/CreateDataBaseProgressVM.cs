using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using QS.DbManagement;
using QS.DBScripts.Controllers;
using QS.Dialog;
using ReactiveUI;

namespace QS.Launcher.ViewModels.PageViewModels.DataBase {
	/// <summary>
	/// Универсальная страница прогресса: последовательно выполняет пайплайн фаз
	/// (создание базы, наполнение, резервное копирование и т.п.) в одном фоновом потоке.
	/// </summary>
	public class CreateDataBaseProgressVM : CarouselPageVM, IProgressBarDisplayable {
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public IDbProvider Provider { get; private set; }
		public Connection Connection { get; private set; }
		private IReadOnlyList<DbCreationPhase> phases = Array.Empty<DbCreationPhase>();

		private string operationTitle = "Создание базы данных";
		/// <summary>Заголовок страницы - задаётся настройками под конкретную операцию.</summary>
		public string OperationTitle {
			get => operationTitle;
			set => this.RaiseAndSetIfChanged(ref operationTitle, value);
		}

		private readonly IDbCreatorInteraction interaction;
		private readonly IServiceProvider services;
		private readonly IGuiDispatcher guiDispatcher;
		private readonly CancellationTokenSource cts;

		#region IProgressBarDisplayable поля

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

		public event Action OperationCompleted;
		public event Action OperationFailed;

		public ReactiveCommand<Unit, Unit> StartCommand { get; }
		public ReactiveCommand<Unit, Unit> CancelCommand { get; }

		public CreateDataBaseProgressVM(
			IDbCreatorInteraction interaction,
			IGuiDispatcher guiDispatcher,
			IServiceProvider services)
		{
			this.interaction = interaction ?? throw new ArgumentNullException(nameof(interaction));
			this.guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));
			this.services = services ?? throw new ArgumentNullException(nameof(services));
			cts = new CancellationTokenSource();

			StartCommand = ReactiveCommand.CreateFromTask(RunAsync);
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

		/// <summary>
		/// выносим всю синхронную цепочку фаз в пул, чтобы UI поток оставался свободным для перерисовки прогрессбара
		/// </summary>
		public async Task RunAsync() {
			try {
				var args = new CreatorFactoryArgs {
					Provider = Provider,
					Progress = this,
					Interaction = interaction,
					CancellationToken = cts.Token,
					ServiceProvider = services
				};

				bool success = await Task.Run(() => RunPipeline(args), cts.Token);

				if(success)
					OperationCompleted?.Invoke();
				else
					OperationFailed?.Invoke();
			}
			catch(OperationCanceledException) {
				logger.Info("Операция с базой отменена.");
				OperationFailed?.Invoke();
			}
			catch(Exception ex) {
				logger.Error(ex, "Сбой в процессе выполнения операции с базой.");
				interaction.ReportError(ex.Message, null);
				OperationFailed?.Invoke();
			}
		}
		private bool RunPipeline(CreatorFactoryArgs args) {
			foreach(var phase in phases) {
				args.CancellationToken.ThrowIfCancellationRequested();
				guiDispatcher.RunInGuiTread(() => CurrentText = phase.Title);
				if(!phase.Action(args))
					return false;
			}
			return true;
		}

		#region IProgressBarDisplayable методы
		public void Start(double maxValue = 1, double minValue = 0, string text = null, double startValue = 0) {
			guiDispatcher.RunInGuiTread(() => {
				MaxValue = maxValue;
				MinValue = minValue;
				Value = startValue;
				if(text != null) CurrentText = text;
				IsStarted = true;
			});
		}

		public void Update(double curValue) => guiDispatcher.RunInGuiTread(() => Value = curValue);

		public void UpdateMax(double maxValue) => guiDispatcher.RunInGuiTread(() => MaxValue = maxValue);

		public void Update(string curText) => guiDispatcher.RunInGuiTread(() => CurrentText = curText);

		public void Add(double addValue = 1, string text = null) {
			guiDispatcher.RunInGuiTread(() => {
				Value += addValue;
				if(text != null) CurrentText = text;
			});
		}

		public void Close() => guiDispatcher.RunInGuiTread(() => IsStarted = false);
		#endregion
	}
}
