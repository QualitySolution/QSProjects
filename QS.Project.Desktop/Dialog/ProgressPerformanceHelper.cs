using System;
using NLog;
using QS.Utilities.Debug;

namespace QS.Dialog {
	
	/// <summary>
	/// Помощник замера производительности с отображением в прогресса для пользователя.
	/// Так как часто при выполнении пошаговых действий требующих отслеживания времени выполнения шагов
	/// требуется по тем же контрольным точкам выводить прогресс выполнения пользователю, то этот класс
	/// позволяет сделать это одновременно.
	/// </summary>
	public class ProgressPerformanceHelper : PerformanceHelper {
		private readonly IProgressBarDisplayable progress;
		
		/// <summary>
		/// Если true, названия шагов буду выводится так же на прогресс бар.
		/// </summary>
		public bool ShowProgressText { get; set; }
		
		/// <param name="progress">прогресс бар на который нужно выводить шаги</param>
		/// <param name="stepsCount">количество шагов</param>
		/// <param name="nameFirstInterval">Название первого шага(интервала времени)</param>
		/// <param name="logger">логер, если указан в каждой контрольной точки времени будет выводится сообщение в лог</param>
		/// <param name="showProgressText">Если true, названия шагов буду выводится так же на прогресс бар.</param>
		public ProgressPerformanceHelper(
			IProgressBarDisplayable progress,
			uint stepsCount,
			string nameFirstInterval = "Старт",
			Logger logger = null,
			bool showProgressText = false 
			) : base(nameFirstInterval, logger) {
			this.progress = progress ?? throw new ArgumentNullException(nameof(progress));
			ShowProgressText = showProgressText;
			progress.Start(maxValue: stepsCount, text: showProgressText ? nameFirstInterval : null);
		}
		
		/// <summary>
		/// Начинаем новый интервал.
		/// Делаем шаг прогресса.
		/// </summary>
		/// <param name="name">Название интервала времени</param>
		public override void CheckPoint(string name = null) {
			base.CheckPoint(name);
			
			if(ShowProgressText && !String.IsNullOrEmpty(name))
				progress?.Add(text: name);
			else 
				progress?.Add();
		}
		
		/// <summary>
		/// Начинаем новую группу.
		/// Так же добавляет шаг к прогрессу.
		/// </summary>
		/// <param name="name">Название группы</param>
		public override void StartGroup(string name) {
			base.StartGroup(name);
			if(ShowProgressText && !String.IsNullOrEmpty(name))
				progress.Add(text: name);
			else 
				progress.Add();
		}
		
		/// <summary>
		/// Закрывает группу.
		/// Добавляет шаг к прогрессу.
		/// </summary>
		public override void EndGroup() {
			//Внутри при добавлении контрольной точки добавляет шаг к прогрессу.
			base.EndGroup();
		}

		/// <summary>
		/// Завершаем процесс.
		/// Закрываем прогресс бар.
		/// Если добавлен логер выводим все контрольные точки в лог.
		/// </summary>
		public void End() {
			CheckPoint("Конец");
			if(logger != null) {
				base.PrintAllPoints(logger);
			}
			progress.Close();
		}
	}
}
