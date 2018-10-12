using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Widgets;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using QS.HistoryLog.Domain;
using QS.Utilities;
using QSHistoryLog;
using QSOrmProject;
using QSProjectsLib;
using QSTDI;
using QSWidgetLib;

namespace QS.HistoryLog.Dialogs
{
	[System.ComponentModel.DisplayName("Просмотр журнала изменений")]
	[WidgetWindow(DefaultWidth = 852, DefaultHeight = 600)]
	public partial class HistoryView : TdiTabBase
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		IList<ChangedEntity> changedEntities;
		bool canUpdate = false;

		IUnitOfWork UoW;

		public HistoryView ()
		{ 
			this.Build ();

			UoW = UnitOfWorkFactory.CreateWithoutRoot();

			datacomboObject.SetRenderTextFunc<HistoryObjectDesc> (x => x.DisplayName);
			datacomboObject.ItemsList = HistoryMain.TraceClasses.ToList();
			comboProperty.SetRenderTextFunc<HistoryFieldDesc> (x => x.DisplayName);
			comboAction.ItemsEnum = typeof(EntityChangeOperation);
			ComboWorks.ComboFillReference(comboUsers, "users", ComboWorks.ListMode.WithAll);
			selectperiod.ActiveRadio = SelectPeriod.Period.Today;

			datatreeChangesets.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<ChangedEntity> ()
				.AddColumn ("Время").AddTextRenderer (x => x.ChangeTimeText)
				.AddColumn ("Пользователь").AddTextRenderer (x => x.ChangeSet.UserName)
				.AddColumn ("Действие").AddTextRenderer (x => x.OperationText)
				.AddColumn ("Тип объекта").AddTextRenderer (x => x.ObjectTitle)
				.AddColumn ("Объект").AddTextRenderer (x => x.EntityTitle)
				.AddColumn ("Откуда изменялось").AddTextRenderer(x => x.ChangeSet.ActionName)
				.Finish();
			datatreeChangesets.Selection.Changed += OnChangeSetSelectionChanged;

			datatreeChanges.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<FieldChange> ()
				.AddColumn ("Поле").AddTextRenderer (x => x.FieldTitle)
				.AddColumn ("Операция").AddTextRenderer (x => x.TypeText)
				.AddColumn ("Новое значение").AddTextRenderer (x => x.NewPangoText, useMarkup: true)
				.AddColumn ("Старое значение").AddTextRenderer (x => x.OldPangoText, useMarkup: true)
				.Finish ();

			canUpdate = true;
			UpdateJournal ();
		}

		void OnChangeSetSelectionChanged (object sender, EventArgs e)
		{

			var selected = (ChangedEntity)datatreeChangesets.GetSelectedObject ();
			datatreeChanges.ItemsDataSource = selected == null ? null : selected.Changes;
		}

		void UpdateJournal()
		{
			if (!canUpdate)
				return;

			logger.Info("Получаем журнал изменений...");
			var query = UoW.Session.QueryOver<ChangedEntity>();

			if(!selectperiod.IsAllTime) 
				query.Where(ce => ce.ChangeTime >= selectperiod.DateBegin && ce.ChangeTime < selectperiod.DateEnd.AddDays(1));

			var selectedClassType = (datacomboObject.SelectedItem as HistoryObjectDesc);
			if(selectedClassType != null)
				query.Where(ce => ce.EntityClassName == selectedClassType.ObjectName);

			if(ComboWorks.GetActiveId(comboUsers) > 0) {
				ChangeSet changeSetAlias = null;
				query.JoinAlias(ce => ce.ChangeSet, () => changeSetAlias)
				     .Where(() => changeSetAlias.User.Id == ComboWorks.GetActiveId(comboUsers));
			}

			if(comboAction.SelectedItem is EntityChangeOperation) {
				query.Where(ce => ce.Operation == (EntityChangeOperation)comboAction.SelectedItem);
			}

			if(!String.IsNullOrWhiteSpace(entrySearchEntity.Text)) {
				var pattern = $"%{entrySearchEntity.Text}%";
				query.Where(ce => ce.EntityTitle.IsLike(pattern));
			}

				if(!String.IsNullOrWhiteSpace(entrySearchValue.Text) || comboProperty.SelectedItem is HistoryFieldDesc)
			{
				FieldChange fieldChangeAlias = null;
				query.JoinAlias(ce => ce.Changes, () => fieldChangeAlias);

				var selectedProperty = comboProperty.SelectedItem as HistoryFieldDesc;
				if(selectedProperty != null)
					query.Where(() => fieldChangeAlias.Path == selectedProperty.FieldName);


				if(!String.IsNullOrWhiteSpace(entrySearchValue.Text)) {
					var pattern = $"%{entrySearchValue.Text}%";
					query.Where(() => fieldChangeAlias.OldValue.IsLike(pattern)
					            || fieldChangeAlias.NewValue.IsLike(pattern));
				}
			}

			changedEntities = query.List();
			datatreeChangesets.ItemsDataSource = changedEntities;
			logger.Info(NumberToTextRus.FormatCase (changedEntities.Count, "Загружено изменение {0} объекта.", "Загружено изменение {0} объектов.", "Загружено изменение {0} объектов."));
		}

		protected void OnComboUsersChanged (object sender, EventArgs e)
		{
			UpdateJournal ();
		}

		protected void OnButtonSearchClicked (object sender, EventArgs e)
		{
			UpdateJournal ();
		}

		protected void OnSelectperiodDatesChanged (object sender, EventArgs e)
		{
			UpdateJournal ();
		}

		void PropertyComboFill()
		{
			bool lastStateUpdate = canUpdate;
			canUpdate = false;
			if (datacomboObject.SelectedItem is HistoryObjectDesc) {
				comboProperty.ItemsList = (datacomboObject.SelectedItem as HistoryObjectDesc).TracedProperties;
			} else
				comboProperty.ItemsList = null;
			canUpdate = lastStateUpdate;
		}

		protected void OnDatacomboObjectItemSelected (object sender, ItemSelectedEventArgs e)
		{
			PropertyComboFill ();
			UpdateJournal ();
		}

		protected void OnComboPropertyItemSelected (object sender, ItemSelectedEventArgs e)
		{
			UpdateJournal ();
		}

		protected void OnEntrySearchActivated (object sender, EventArgs e)
		{
			buttonSearch.Click ();
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
	}
}

