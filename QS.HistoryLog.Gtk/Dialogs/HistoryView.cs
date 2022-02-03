using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gamma.Widgets;
using QS.HistoryLog.Domain;
using QS.Utilities;
using QSOrmProject;
using QSProjectsLib;
using QSWidgetLib;

namespace QS.HistoryLog.Dialogs
{
	[System.ComponentModel.DisplayName("Просмотр журнала изменений")]
	[WidgetWindow(DefaultWidth = 852, DefaultHeight = 600)]
	public partial class HistoryView : QS.Dialog.Gtk.TdiTabBase
	{
		List<ChangedEntity> changedEntities;
		bool canUpdate = false;
		private int pageSize = 250;
		private int takenRows = 0;
		private bool takenAll = false;
		private IDiffFormatter diffFormatter = new PangoDiffFormater();


		public HistoryView()
		{
			this.Build();
			changedEntities = viewModel.ChangedEntities;

			datacomboObject.SetRenderTextFunc<HistoryObjectDesc>(x => x.DisplayName);
			datacomboObject.ItemsList = HistoryMain.TraceClasses.OrderBy(x => x.DisplayName)?.ToList();
			comboProperty.SetRenderTextFunc<HistoryFieldDesc>(x => x.DisplayName);
			comboAction.ItemsEnum = typeof(EntityChangeOperation);
			ComboWorks.ComboFillReference(comboUsers, "users", ComboWorks.ListMode.WithAll, true, "name");
			selectperiod.ActiveRadio = SelectPeriod.Period.Today;

			datatreeChangesets.ColumnsConfig = ColumnsConfigFactory.Create<ChangedEntity>()
				.AddColumn("Время").AddTextRenderer(x => x.ChangeTimeText)
				.AddColumn("Пользователь").AddTextRenderer(x => x.ChangeSet.UserName)
				.AddColumn("Действие").AddTextRenderer(x => x.OperationText)
				.AddColumn("Тип объекта").AddTextRenderer(x => x.ObjectTitle)
				.AddColumn("Код объекта").AddTextRenderer(x => x.EntityId.ToString())
				.AddColumn("Имя объекта").AddTextRenderer(x => x.EntityTitle)
				.AddColumn("Откуда изменялось").AddTextRenderer(x => x.ChangeSet.ActionName)
				.Finish();
			datatreeChangesets.Selection.Changed += OnChangeSetSelectionChanged;
			GtkScrolledWindowChangesets.Vadjustment.ValueChanged += Vadjustment_ValueChanged;

			datatreeChanges.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<FieldChange>()
				.AddColumn("Поле").AddTextRenderer(x => x.FieldTitle)
				.AddColumn("Операция").AddTextRenderer(x => x.TypeText)
				.AddColumn("Новое значение").AddTextRenderer(x => x.NewFormatedDiffText, useMarkup: true)
				.AddColumn("Старое значение").AddTextRenderer(x => x.OldFormatedDiffText, useMarkup: true)
				.Finish();

			canUpdate = true;
			ViewModel.UpdateJournal();
		}

		void Vadjustment_ValueChanged(object sender, EventArgs e)
		{
			if(takenAll || datatreeChangesets.Vadjustment.Value + datatreeChangesets.Vadjustment.PageSize < datatreeChangesets.Vadjustment.Upper)
				return;

			var lastPos = datatreeChangesets.Vadjustment.Value;
			ViewModel.UpdateJournal(true);
			QSMain.WaitRedraw();
			datatreeChangesets.Vadjustment.Value = lastPos;
		}

		void OnChangeSetSelectionChanged(object sender, EventArgs e)
		{
			var selected = (ChangedEntity)datatreeChangesets.GetSelectedObject();
			if(selected != null) {
				selected.Changes.ToList().ForEach(x => x.DiffFormatter = diffFormatter);
				datatreeChanges.ItemsDataSource = selected.Changes;
			} else
				datatreeChanges.ItemsDataSource = null;
		}

		protected void OnComboUsersChanged(object sender, EventArgs e)
		{
			ViewModel.UpdateJournal();
		}

		protected void OnButtonSearchClicked(object sender, EventArgs e)
		{
			ViewModel.UpdateJournal();
		}

		protected void OnSelectperiodDatesChanged(object sender, EventArgs e)
		{
			ViewModel.UpdateJournal();
		}

		void PropertyComboFill()
		{
			bool lastStateUpdate = canUpdate;
			canUpdate = false;
			if(datacomboObject.SelectedItem is HistoryObjectDesc) {
				comboProperty.ItemsList = (datacomboObject.SelectedItem as HistoryObjectDesc).TracedProperties?.OrderBy(x => x.DisplayName);
			} else
				comboProperty.ItemsList = null;
			canUpdate = lastStateUpdate;
		}

		protected void OnDatacomboObjectItemSelected(object sender, ItemSelectedEventArgs e)
		{
			PropertyComboFill();
			ViewModel.UpdateJournal();
		}

		protected void OnComboPropertyItemSelected(object sender, ItemSelectedEventArgs e)
		{
			ViewModel.UpdateJournal();
		}

		protected void OnEntrySearchValueActivated(object sender, EventArgs e)
		{
			buttonSearch.Click();
		}

		protected void OnComboActionChanged(object sender, EventArgs e)
		{
			ViewModel.UpdateJournal();
		}

		protected void OnEntrySearchEntityActivated(object sender, EventArgs e)
		{
			ViewModel.UpdateJournal();
		}

		protected void OnBtnFilterClicked(object sender, EventArgs e)
		{
			tblSettings.Visible = !tblSettings.Visible;
			btnFilter.Label = tblSettings.Visible ? "Скрыть фильтр" : "Показать фильтр";
		}
	}
}