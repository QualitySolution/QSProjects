using System.ComponentModel.DataAnnotations;

namespace QS.Updater.App {
	public enum UpdateChannel {
		[Display(Name = "Текущий", ShortName = "Тек.")]
		Current,
		[Display(Name = "Стабильный", ShortName = "Стаб.")]
		Stable,
		[Display(Name = "Без обновлений", ShortName = "Без обн.")]
		OffAutoUpdate
	}
}
