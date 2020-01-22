using System;
namespace QS.Dialog
{
	public interface IProgressBarDisplayable
	{
		void Start(double maxValue, double minValue = 0, string text = null, double startValue = 0);

		void Update(double curValue);

		void Update(string curText);

		void Add(double addValue = 1, string text = null);

		void Close();
	}
}
