using System;
using System.ComponentModel;
using Gtk;
using QS.Dialog;
using QS.Utilities;

namespace QS.Widgets
{
	[ToolboxItem(true)]
	[Category("QS.Project")]
	public class ProgressWidget : ProgressBar, IProgressBarDisplayable
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public void Add(double addValue = 1, string text = null)
		{
			if(Adjustment == null)
				return;
	
			if(text != null)
				Text = text;

			Adjustment.Value += addValue;
			if(Adjustment.Value > Adjustment.Upper)
				logger.Warn("Значение ({0}) прогресс больше максимального ({1})",
							Adjustment.Value,
							Adjustment.Upper
						   );
			GtkHelper.WaitRedraw();
		}

		public void Close()
		{
			Text = null;
			Visible = false;
			GtkHelper.WaitRedraw();
		}

		public void Start(double maxValue, double minValue = 0, string text = null, double startValue = 0)
		{
			Adjustment = new Adjustment(startValue, minValue, maxValue, 1, 1, 1);
			Text = text;
			Visible = true;
			GtkHelper.WaitRedraw();
		}

		public void Update(string text)
		{
			Text = text;
			GtkHelper.WaitRedraw();
		}

		public new void Update(double value)
		{
			if(Adjustment == null)
				return;
			Adjustment.Value = value;
			GtkHelper.WaitRedraw();
		}
	}
}
