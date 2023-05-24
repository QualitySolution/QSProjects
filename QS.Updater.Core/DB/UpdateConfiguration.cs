using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using QS.Utilities.Text;

namespace QS.Updater.DB
{
	public class UpdateConfiguration
	{
		public IEnumerable<UpdateHop> Updates => Hops;

		private List<UpdateHop> Hops = new List<UpdateHop>();

		public UpdateConfiguration()
		{
		}

		#region Конфигурирование
		
		/// <summary>
		/// Метод добавляет скрипт обновления с версии source на версию destination
		/// </summary>
		/// <param name="source">Изначальная версия</param>
		/// <param name="destination">Версия до которой обновится база</param>
		/// <param name="scriptResource">Имя ресурса скрипта, ассамблея ресурса будет подставлена та которая вызовет эту функцию.</param>
		/// <param name="executeBefore">Функция которая должна быть вызвана перед применением скрипта.</param>
		/// <param name="onTesting">Указывает что скрипт еще в разработке и не должен пока использоваться в приложении для обновления базы,
		/// а доступен только в SQL теста.</param>
		public void AddUpdate(Version source, Version destination, string scriptResource, Action<DbConnection> executeBefore = null, bool onTesting = false)
		{
			Hops.Add(new UpdateHop {
				Source = source,
				Destination = destination,
				OnTesting = onTesting,
				ExecuteBefore = executeBefore,
				Resource = scriptResource,
				Assembly = Assembly.GetCallingAssembly()
			});
		}
		#endregion

		#region Использование

		public IEnumerable<UpdateHop> GetHopsToLast(Version fromVersion, bool skipTesting = true) {
			var last = fromVersion;
			while(true) {
				var next = GetNextUpdate(last, skipTesting);
				if (next != null) {
					last = next.Destination;
					yield return next;
				}
				else break;
			}
		}

		private UpdateHop GetNextUpdate(Version version, bool skipTesting) {
			return Updates.FirstOrDefault(x => x.Source == version && (!skipTesting || !x.OnTesting));
		}
		#endregion
	}

	public class UpdateHop
	{
		public Version Source;
		public Version Destination;
		public bool OnTesting;

		/// <summary>
		/// Программный метод, выполняющийся перед обновлением. Тут можно выполнить действия трудно выполнимые внутри SQL скрипта.
		/// </summary>
		public Action<DbConnection> ExecuteBefore;

		public string Resource;
		public Assembly Assembly;

		public string Title => $"{Source.VersionToShortString()}→{Destination.VersionToShortString()}";
	}
}