using System;
using QS.HistoryLog.Domain;
using QS.HistoryLog.ViewModels;
using QS.Project.Domain;
using QS.Views.Dialog;

namespace QS.HistoryLog.Views
{
	public partial class HistoryView : DialogViewBase<HistoryViewModel>
	{
		public HistoryView(HistoryViewModel viewModel): base(viewModel)
		{
			this.Build();

			ycomboUsers.SetRenderTextFunc<UserBase>(x => x.Name);
			ycomboUsers.Binding.AddSource(viewModel)
				.AddBinding(v => v.Users, w => w.ItemsList)
				.AddBinding(v => v.SelectedUser, w => w.SelectedItem)
				.InitializeFromSource();

			ycomboObjects.SetRenderTextFunc<HistoryObjectDesc>(x => x.DisplayName);
			ycomboObjects.Binding.AddSource(viewModel)
				.AddBinding(v => v.ChangeObjects, w => w.ItemsList)
				.AddBinding(v => v.SelectedChangeObject, w => w.SelectedItem)
				.InitializeFromSource();

			ycomboAction.ItemsEnum = typeof(EntityChangeOperation);
			ycomboAction.Binding.AddSource(viewModel)
				.AddBinding(v => v.ChangeOperation, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			ycomboFields.SetRenderTextFunc<HistoryFieldDesc>(x => x.DisplayName);
			ycomboFields.Binding.AddSource(viewModel)
				.AddBinding(v => v.HistoryField, w => w.ItemsList)
				.InitializeFromSource();

			ydateStartperiodpicker.Binding.AddSource(viewModel)
				.AddBinding(v => v.PeriodStartDate, w => w.Date)
				.InitializeFromSource();

			ydateEndperiodpicker.Binding.AddSource(viewModel)
				.AddBinding(v => v.PeriodEndDate, w => w.Date)
				.InitializeFromSource();

			yPeriodToday.Active = true;

			yentryName.Binding.AddSource(viewModel)
				.AddBinding(v => v.SsearchByName, w => w.Text)
				.InitializeFromSource();

			yentryId.Binding.AddSource(viewModel)
				.AddBinding(v => v.SearchById, w => w.Text)
				.InitializeFromSource();

			yentryChanged.Binding.AddSource(viewModel)
				.AddBinding(v => v.SearchByChanged, w => w.Text)
				.InitializeFromSource();
		}

		protected void OnPeriodToday(object o, EventArgs args)
		{
			if(yPeriodToday.Active) {
				ydateStartperiodpicker.Date = DateTime.Today;
				ydateEndperiodpicker.Date = ydateStartperiodpicker.Date.AddDays(1).AddTicks(-1);
			}
		}
		protected void OnPeriodWeek(object o, EventArgs args)
		{
			if(yPeriodWeek.Active) {
				ydateEndperiodpicker.Date = ydateStartperiodpicker.Date.AddDays(7).AddTicks(-1);
			}
		}
		protected void OnPeriodMonth(object o, EventArgs args)
		{
			if(yPeriodMonth.Active) {
				ydateEndperiodpicker.Date = ydateStartperiodpicker.Date.AddMonths(1).AddTicks(-1);
			}
		}
		protected void OnPeriodThreeMonth(object o, EventArgs args)
		{
			if(yPeriodThreeMonth.Active) {
				ydateEndperiodpicker.Date = ydateStartperiodpicker.Date.AddMonths(3).AddTicks(-1);
			}

		}
	}
}