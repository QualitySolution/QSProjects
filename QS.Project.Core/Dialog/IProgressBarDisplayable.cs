using System;
namespace QS.Dialog
{
	/// <summary>
	/// Интерфейс позволяющий управлять прогресс баром не зависимо от графического тул кита.
	/// </summary>
	public interface IProgressBarDisplayable
	{
		void Start(double maxValue = 1, double minValue = 0, string text = null, double startValue = 0);

		void Update(double curValue);

		void Update(string curText);

		void Add(double addValue = 1, string text = null);

		double Value { get; }
		
		bool IsStarted { get; }

		void Close();
	}
}
