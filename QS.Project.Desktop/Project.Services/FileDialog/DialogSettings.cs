using System.Collections.Generic;

namespace QS.Project.Services.FileDialog
{
    public class DialogSettings
	{
		/// <summary>
		/// Определяет какой диалог будет запущен (Windows.Forms или Gtk)
		/// </summary>
		public virtual DialogPlatformType PlatformType{ get; set; }
		public virtual string Title { get; set; }
		public virtual string FileName { get; set; }
		public virtual string InitialDirectory { get; set; }
		public virtual string DefaultFileExtention { get; set; }
		public virtual bool SelectMultiple { get; set; }
		public virtual List<DialogFileFilter> FileFilters { get; } = new List<DialogFileFilter>();

		/// <summary>
		/// Определение дополнительных путей на панели быстрого доступа
		/// </summary>
		public virtual List<string> CustomDirectories { get; } = new List<string>();
    }
}
