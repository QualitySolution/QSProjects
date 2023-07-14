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
		//Используется чтобы отслеживать настоящее количество шагов(вызовов Add).
		//Так как Adjustment.Value не дает значению выйти за указанные границы.
		private double checkValue;

		public double Value => Adjustment.Value;
		public bool IsStarted => Visible;

		public void Add(double addValue = 1, string text = null)
		{
			if(Adjustment == null)
				return;
	
			if(text != null)
				Text = text;

			Adjustment.Value += addValue;
			checkValue += addValue;
			if(checkValue > Adjustment.Upper)
				logger.Warn($"Значение прогресса {checkValue} больше максимального {Adjustment.Upper}");
			GtkHelper.WaitRedraw(50);
		}

		public void Close()
		{
			Text = null;
			Visible = false;
			if(Convert.ToInt64(checkValue) != Convert.ToInt64(Adjustment.Upper))
				logger.Warn($"Прогресс остановлен на шаге {checkValue} из {Adjustment.Upper}");
			GtkHelper.WaitRedraw();
		}

		public void Start(double maxValue = 1, double minValue = 0, string text = null, double startValue = 0)
		{
			Adjustment = new Adjustment(startValue, minValue, maxValue, 1, 1, 1);
			checkValue = startValue;
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
			checkValue = value;
			GtkHelper.WaitRedraw();
		}
	}
}
