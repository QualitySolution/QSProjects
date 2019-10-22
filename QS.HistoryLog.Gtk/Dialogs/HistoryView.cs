using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Widgets;
using NHibernate;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
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
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		List<ChangedEntity> changedEntities;
		bool canUpdate = false;
		private int pageSize = 250;
		private int takenRows = 0;
		private bool takenAll = false;
		private IDiffFormatter diffFormatter = new PangoDiffFormater();

		IUnitOfWork UoW;

		public HistoryView()
		{
			this.Build();

			UoW = UnitOfWorkFactory.CreateWithoutRoot();

			datacomboObject.SetRenderTextFunc<HistoryObjectDesc>(x => x.DisplayName);
			datacomboObject.ItemsList = HistoryMain.TraceClasses.OrderBy(x => x.DisplayName)?.ToList();
			comboProperty.SetRenderTextFunc<HistoryFieldDesc>(x => x.DisplayName);
			comboAction.ItemsEnum = typeof(EntityChangeOperation);
			ComboWorks.ComboFillReference(comboUsers, "users", ComboWorks.ListMode.WithAll, true, "name");
			selectperiod.ActiveRadio = SelectPeriod.Period.Today;

			datatreeChangesets.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<ChangedEntity>()
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
			UpdateJournal();
		}

		void Vadjustment_ValueChanged(object sender, EventArgs e)
		{
			if(takenAll || datatreeChangesets.Vadjustment.Value + datatreeChangesets.Vadjustment.PageSize < datatreeChangesets.Vadjustment.Upper)
				return;

			var lastPos = datatreeChangesets.Vadjustment.Value;
			UpdateJournal(true);
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

		void UpdateJournal(bool nextPage = false)
		{
			DateTime startTime = DateTime.Now;
			if(!nextPage) {
				takenRows = 0;
				takenAll = false;
			}

			if(!canUpdate)
				return;

			logger.Info("Получаем журнал изменений{0}...", takenRows > 0 ? $"({takenRows}+)" : "");
			ChangeSet changeSetAlias = null;

			var query = UoW.Session.QueryOver<ChangedEntity>()
				.JoinAlias(ce => ce.ChangeSet, () => changeSetAlias)
				.Fetch(SelectMode.Fetch, x => x.ChangeSet)
				.Fetch(SelectMode.Fetch, x => x.ChangeSet.User);

			if(!selectperiod.IsAllTime)
				query.Where(ce => ce.ChangeTime >= selectperiod.DateBegin && ce.ChangeTime < selectperiod.DateEnd);

			if(datacomboObject.SelectedItem is HistoryObjectDesc selectedClassType)
				query.Where(ce => ce.EntityClassName == selectedClassType.ObjectName);

			if(ComboWorks.GetActiveId(comboUsers) > 0)
				query.Where(() => changeSetAlias.User.Id == ComboWorks.GetActiveId(comboUsers));

			if(comboAction.SelectedItem is EntityChangeOperation)
				query.Where(ce => ce.Operation == (EntityChangeOperation)comboAction.SelectedItem);

			if(!string.IsNullOrWhiteSpace(entrySearchEntity.Text)) {
				var pattern = $"%{entrySearchEntity.Text}%";
				query.Where(ce => ce.EntityTitle.IsLike(pattern));
			}

			if(!string.IsNullOrWhiteSpace(entSearchId.Text)) {
				if(int.TryParse(entSearchId.Text, out int id))
					query.Where(ce => ce.EntityId == id);
			}

			if(!string.IsNullOrWhiteSpace(entrySearchValue.Text) || comboProperty.SelectedItem is HistoryFieldDesc) {
				FieldChange fieldChangeAlias = null;
				query.JoinAlias(ce => ce.Changes, () => fieldChangeAlias);

				if(comboProperty.SelectedItem is HistoryFieldDesc selectedProperty)
					query.Where(() => fieldChangeAlias.Path == selectedProperty.FieldName);

				if(!string.IsNullOrWhiteSpace(entrySearchValue.Text)) {
					var pattern = $"%{entrySearchValue.Text}%";
					query.Where(
						() => fieldChangeAlias.OldValue.IsLike(pattern) || fieldChangeAlias.NewValue.IsLike(pattern)
					);
				}
			}

			var taked = query.OrderBy(x => x.ChangeTime).Desc
							 .Skip(takenRows)
							 .Take(pageSize)
							 .List();

			if(takenRows > 0) {
				changedEntities.AddRange(taked);
				datatreeChangesets.YTreeModel.EmitModelChanged();
			} else {
				changedEntities = taked.ToList();
				datatreeChangesets.ItemsDataSource = changedEntities;
			}

			if(taked.Count < pageSize)
				takenAll = true;

			takenRows = changedEntities.Count;

			logger.Debug("Время запроса {0}", DateTime.Now - startTime);
			logger.Info(NumberToTextRus.FormatCase(changedEntities.Count, "Загружено изменение {0}{1} объекта.", "Загружено изменение {0}{1} объектов.", "Загружено изменение {0}{1} объектов.", takenAll ? "" : "+"));
		}

		protected void OnComboUsersChanged(object sender, EventArgs e)
		{
			UpdateJournal();
		}

		protected void OnButtonSearchClicked(object sender, EventArgs e)
		{
			UpdateJournal();
		}

		protected void OnSelectperiodDatesChanged(object sender, EventArgs e)
		{
			UpdateJournal();
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
			UpdateJournal();
		}

		protected void OnComboPropertyItemSelected(object sender, ItemSelectedEventArgs e)
		{
			UpdateJournal();
		}

		protected void OnEntrySearchValueActivated(object sender, EventArgs e)
		{
			buttonSearch.Click();
		}

		protected void OnComboActionChanged(object sender, EventArgs e)
		{
			UpdateJournal();
		}

		public override void Destroy()
		{
			UoW.Dispose();
			base.Destroy();
		}

		protected void OnEntrySearchEntityActivated(object sender, EventArgs e)
		{
			UpdateJournal();
		}

		protected void OnBtnFilterClicked(object sender, EventArgs e)
		{
			tblSettings.Visible = !tblSettings.Visible;
			btnFilter.Label = tblSettings.Visible ? "Скрыть фильтр" : "Показать фильтр";
		}
	}
}