using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.Navigation;
using QS.Project.DB;
using QS.Project.Versioning;
using QS.Updates;
using QS.Utilities.Text;
using QS.ViewModels.Dialog;

namespace QS.Updater.App.ViewModels {
	public class NewVersionViewModel : WindowDialogViewModelBase {
		static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		
		private readonly IApplicationInfo applicationInfo;
		private readonly ISkipVersionState skipVersionState;
		private readonly ModalProgressCreator progressCreator;
		private readonly IGuiDispatcher guiDispatcher;
		private readonly IInteractiveMessage interactive;
		private readonly IDataBaseInfo dataBaseInfo;

		public NewVersionViewModel(
			ReleaseInfo[] releases,
			IApplicationInfo applicationInfo,
			INavigationManager navigation,
			ISkipVersionState skipVersionState,
			ModalProgressCreator progressCreator,
			IGuiDispatcher guiDispatcher,
			IInteractiveMessage interactive,
			IDataBaseInfo dataBaseInfo = null) : base(navigation) {
			Title = "Доступна новая версия программы!";
			this.Releases = releases ?? throw new ArgumentNullException(nameof(releases));
			this.applicationInfo = applicationInfo ?? throw new ArgumentNullException(nameof(applicationInfo));
			this.skipVersionState = skipVersionState ?? throw new ArgumentNullException(nameof(skipVersionState));
			this.progressCreator = progressCreator ?? throw new ArgumentNullException(nameof(progressCreator));
			this.guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.dataBaseInfo = dataBaseInfo;
			if(!releases.Any())
				throw new ArgumentException("Коллекция должна быть не пустая.", nameof(this.Releases));
			
			//Заполняем выпуски которые могут быть установлены.
			//Здесь пропускаем все дубликаты ссылок на установку.
			HashSet<string> added = new HashSet<string>();
			foreach(var release in Releases) {
				if(String.IsNullOrWhiteSpace(release.InstallerLink))
					continue;
				if(added.Contains(release.InstallerLink)) 
					continue;
				added.Add(release.InstallerLink);
				CanSelectedReleases.Add(release);
			}
			
			SelectedRelease = CanSelectedReleases.FirstOrDefault();
		}

		#region Свойства View
		public readonly ReleaseInfo[] Releases;
		public string MainInfoText => $"Доступная версия: {Releases.First().Version} \t Установленная версия: {applicationInfo.Version.VersionToShortString()}";
		public string DbUpdateInfo {
			get {
				if(WillDbChange.Any(x => x.DatabaseUpdate == DatabaseUpdate.BreakingChange))
					return $"Внимание! Потребуется провести обновление базы данных. Убедитесь что вы знаете пароль администратора. Клиенты с версиями ниже {SelectedRelease.Version:2} не смогут работать с базой после ее обновления.";
				if(WillDbChange.Any(x => x.DatabaseUpdate == DatabaseUpdate.Required))
					return $"Потребуется провести изменение базы данных. Убедитесь что вы знаете пароль администратора. Совместимость с клиентами {SelectedRelease.Version:2}.х будет сохранена.";
				return null;
			}
		}

		public readonly List<ReleaseInfo> CanSelectedReleases = new List<ReleaseInfo>();

		private ReleaseInfo selectedRelease;
		[PropertyChangedAlso(nameof(VisibleDbInfo))]
		[PropertyChangedAlso(nameof(DbUpdateInfo))]
		public virtual ReleaseInfo SelectedRelease {
			get => selectedRelease;
			set => SetField(ref selectedRelease, value);
		}

		public bool VisibleDbInfo => WillDbChange.Any();
		public bool VisibleSelectRelease => CanSelectedReleases.Count() > 1;
		#endregion

		#region Helpers
		private IEnumerable<ReleaseInfo> WillInstall => Releases.SkipWhile(x => SelectedRelease != x);
		private IEnumerable<ReleaseInfo> WillDbChange => WillInstall
			.Where(x => dataBaseInfo != null)//Не убирать пока поддерживаем работу без dataBaseInfo
			.Where(x => x.DatabaseUpdate != DatabaseUpdate.None)
			.Where(x => Version.Parse(x.Version) > dataBaseInfo.Version);
		#endregion

		#region Действия
		public void SkipVersion() {
			skipVersionState.SaveSkipVersion(Version.Parse(Releases.First().Version));
			Close(false, CloseSource.Cancel);
		}

		public void Later() {
			Close(false, CloseSource.Cancel);
		}

		public void Install() {
			string tempPath = Path.Combine(Path.GetTempPath(),
				  String.Format(@"QSInstaller-{0}.exe", Guid.NewGuid().ToString().Substring(0, 8)));

			bool loadingComplete = false;
			progressCreator.UserCanCancel = true;
			progressCreator.Start(100, text :"Новая версия скачивается, подождите...");

			WebClient webClient = new WebClient();
			progressCreator.Canceled += (sender, args) => webClient.CancelAsync();
			
			webClient.DownloadProgressChanged += (sender, e) => guiDispatcher.RunInGuiTread(delegate {
				progressCreator.Update(e.ProgressPercentage);
			});
			
			webClient.DownloadFileCompleted += (sender, e) => guiDispatcher.RunInGuiTread(delegate {
				loadingComplete = true;
				progressCreator.Close();
				if (e.Error != null)
				{
					logger.Error(e.Error, "Не удалось скачать файл обновления.");
					interactive.ShowMessage(ImportanceLevel.Error, "Не удалось скачать файл.");
				}
				else if (!e.Cancelled)
				{
					logger.Info("Скачивание обновления завершено. Запускаем установку...");
					Process File = new Process();
					File.StartInfo.FileName = tempPath;
					try
					{
						File.Start();
						Environment.Exit(0);
					}
					catch (Exception ex)
					{
						logger.Error(ex, "Не удалось запустить скачанный установщик.");
						interactive.ShowMessage(ImportanceLevel.Error, "Не удалось запустить скачанный файл.");
					}
				}
			});
			
			logger.Debug("Скачиваем из {0} в {1}", SelectedRelease.InstallerLink, tempPath);

			webClient.DownloadFileAsync(new Uri(SelectedRelease.InstallerLink), tempPath);
			// Ждем окончания загрузки файла не возвращая управление, иначе в процессе скачивания продолжается работа, а это не надо во всех случаях
			guiDispatcher.WaitInMainLoop(() => loadingComplete, 50);
		}
		#endregion
	}
}
