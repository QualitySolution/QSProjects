using System.Collections.Generic;

namespace QS.Project.Services.FileDialog
{
    public class DialogFileFilter
    {
		public string Name { get; }
		public IEnumerable<string> FilterExtensions { get; }

		/// <summary>
		/// Создание фильтра для диалога работы с файловой системой
		/// </summary>
		/// <param name="name">Имя фильтра, например: Текстовые файлы</param>
		/// <param name="filterExtensions">Расширения файлов, например: *.txt, *.ini </param>
        public DialogFileFilter(string name, params string[] filterExtensions)
        {
            Name = name;
            FilterExtensions = filterExtensions;
        }
    }
}
