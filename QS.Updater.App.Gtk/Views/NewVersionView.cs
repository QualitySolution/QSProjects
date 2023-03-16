using System;
using System.Reflection;
using Gdk;
using Gtk;
using NHibernate.Dialect;
using QS.Updater.App.ViewModels;
using QS.Updates;
using QS.Utilities.Debug;
using QS.Views.Dialog;

namespace QS.Updater.App.Views {
	public partial class NewVersionView : DialogViewBase<NewVersionViewModel> {
		public NewVersionView(NewVersionViewModel viewModel) : base(viewModel) {
			this.Build();

			labelMainInfo.Binding.AddBinding(ViewModel, v => v.MainInfoText, w => w.LabelProp).InitializeFromSource();
			labelDBUpdateInfo.Binding.AddSource(ViewModel)
				.AddBinding(v => v.DbUpdateInfo, w => w.LabelProp)
				.AddBinding(v => v.VisibleDbInfo, w => w.Visible)
				.InitializeFromSource();

			hboxSelectRelease.Binding.AddBinding(ViewModel, v => v.VisibleSelectRelease, w => w.Visible).InitializeFromSource();
			comboSelectInstaller.SetRenderTextFunc<ReleaseInfo>(x => x.Version);
			comboSelectInstaller.ItemsList = ViewModel.CanSelectedReleases;
			comboSelectInstaller.Binding.AddBinding(ViewModel, v => v.SelectedRelease, w => w.SelectedItem).InitializeFromSource();

			for(uint i = 0; i < ViewModel.Releases.Length; i++) {
				uint baseRow = i * 4;
				var release = ViewModel.Releases[i];

				var emptySpace = new Label();
				emptySpace.HeightRequest = 15;
				tableReleases.Attach(emptySpace, 2, 3, baseRow, baseRow + 1, AttachOptions.Fill, AttachOptions.Shrink, 0, 0);
				
				var versionLabel = new Label{ Markup = $"<b>{release.Version}</b>"};
				versionLabel.UseMarkup = true;
				tableReleases.Attach(versionLabel, 0, 1, baseRow + 1, baseRow + 2, AttachOptions.Fill, AttachOptions.Shrink, 0, 0);
				
				if(release.DatabaseUpdate != DatabaseUpdate.None) {
					var image = new Gtk.Image(Assembly.GetExecutingAssembly(),
						"QS.Updater.App.Icons." + (release.DatabaseUpdate == DatabaseUpdate.BreakingChange
							? "db-red.png"
							: "db-violet.png"));
					image.TooltipText = release.DatabaseUpdate == DatabaseUpdate.BreakingChange
						? "Требуется обновление базы данных нарушающее обратную совместимость с предыдущими версиями."
						: "Требуется обновление базы данных.";
						tableReleases.Attach(image, 1, 2, baseRow + 1, baseRow + 2, AttachOptions.Fill, AttachOptions.Shrink, 0, 0);
				}

				var dateLabel = new Label($"{release.Date.ToDateTime().ToLongDateString()}");
				tableReleases.Attach(dateLabel, 2, 3, baseRow + 1, baseRow + 2, AttachOptions.Fill, AttachOptions.Shrink, 0, 0);

				if(!String.IsNullOrEmpty(release.NewsLink)) {
					var newsLabel = new Label{Markup = $"<b><a href=\"{release.NewsLink}\" title=\"Перейти на сайт компании\">Прочитать новость</a></b>"};
					newsLabel.AddEvents ((int)EventMask.ButtonPressMask);
					newsLabel.ButtonPressEvent += (o, args) => System.Diagnostics.Process.Start(release.NewsLink);
					tableReleases.Attach(newsLabel, 3, 4, baseRow + 1, baseRow + 2, AttachOptions.Fill, AttachOptions.Shrink, 0, 0);
				}

				var separator = new HSeparator();
				tableReleases.Attach(separator, 0, 4, baseRow + 2, baseRow + 3, AttachOptions.Fill, AttachOptions.Fill, 10, 0);
				
				var changesText = new TextView();
				changesText.Editable = false;
				changesText.WrapMode = WrapMode.WordChar;
				changesText.Buffer.Text = release.Changes.Replace('*','✦');
				Toplevel.Realized += (sender, args) => {
					//Здесь таким хитрым способом устанавливаем цвет фона у textView. Подбирал способ очень долго.
					//Фишка в том что реальная информация о стиле появляется только после Realized, до этого времени если взять цвет фона,
					//он будет отличатся от установленного в системе стиля.
					changesText.ModifyBase(StateType.Normal, Toplevel.Style.Background(StateType.Normal));
				};
				tableReleases.Attach(changesText, 0, 4, baseRow + 3, baseRow + 4, AttachOptions.Fill | AttachOptions.Expand, AttachOptions.Shrink, 0, 0);
			}
			tableReleases.ShowAll();
		}

		protected void OnButtonSkipClicked(object sender, EventArgs e) {
			ViewModel.SkipVersion();
		}

		protected void OnButtonCancelClicked(object sender, EventArgs e) {
			ViewModel.Later();
		}

		protected void OnButtonOkClicked(object sender, EventArgs e) {
			ViewModel.Install();
		}
	}
}
