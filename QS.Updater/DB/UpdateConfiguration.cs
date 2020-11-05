using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace QS.Updater.DB
{
	public class UpdateConfiguration
	{
		public IEnumerable<UpdateHop> MicroUpdates => Hops.Where(x => x.UpdateType == UpdateType.MicroUpdate);

		public IEnumerable<UpdateHop> Updates => Hops.Where(x => x.UpdateType == UpdateType.Update);

		private List<UpdateHop> Hops = new List<UpdateHop>();

		public UpdateConfiguration()
		{
		}

		#region Конфигурирование
		/// <summary>
		/// Метод добавит скрипт микрообновление.
		/// Микрообновление - обновляет базу, но при этом оставляет совместимосость с предыдущими версиями приложения.
		/// </summary>
		/// <param name="source">Изначальная версия</param>
		/// <param name="destination">Версия до которой обновится база</param>
		/// <param name="scriptResource">Имя ресурса скрипта, асамблея ресурса будет подставлена та которая вызовет эту функцию.</param>
		public void AddMicroUpdate(Version source, Version destination, string scriptResource)
		{
			if (source == destination)
				throw new ArgumentException($"{nameof(source)} и {nameof(destination)} не должны быть равны");

			Hops.Add(new UpdateHop {
				UpdateType = UpdateType.MicroUpdate,
				Source = source,
				Destanation = destination,
				Resource = scriptResource,
				Assembly = Assembly.GetCallingAssembly()
			});
		}

		/// <summary>
		/// Метод добавит скрипт добавит обновление.
		/// Полное обновление базы данных на следующую версию, с нарушением совместимости. Обычно это следующая ветка релиза.
		/// </summary>
		/// <param name="source">Изначальная версия</param>
		/// <param name="destination">Версия до которой обновится база</param>
		/// <param name="scriptResource">Имя ресурса скрипта, асамблея ресурса будет подставлена та которая вызовет эту функцию.</param>
		public void AddUpdate(Version source, Version destination, string scriptResource)
		{
			if (source.Major == destination.Major && source.Minor == destination.Minor)
				throw new ArgumentException($"У {nameof(source)} и {nameof(destination)} не должны быть равны первые две цифры версии X.Y");


			Hops.Add(new UpdateHop {
				UpdateType = UpdateType.Update,
				Source = source,
				Destanation = destination,
				Resource = scriptResource,
				Assembly = Assembly.GetCallingAssembly()
			});
		}
		#endregion

		#region Использование

		public IEnumerable<UpdateHop> GetHopsToLast(Version fromVersion)
		{
			var last = fromVersion;
			while(true) {
				while(true) {
					var nextMicro = GetNextMicroUpdate(last);
					if (nextMicro != null) {
						last = nextMicro.Destanation;
						yield return nextMicro;
					}
					else break;
				}
				var next = GetNextUpdate(last);
				if (next != null) {
					last = next.Destanation;
					yield return next;
				}
				else break;
			}
		}

		private UpdateHop GetNextMicroUpdate(Version version)
		{
			return MicroUpdates.FirstOrDefault(x => x.Source == version);
		}

		private UpdateHop GetNextUpdate(Version version)
		{
			return Updates.FirstOrDefault(x => x.Source.Major == version.Major && x.Source.Minor == version.Minor);
		}

		#endregion
	}

	public class UpdateHop
	{
		public UpdateType UpdateType;
		public Version Source;
		public Version Destanation;

		public string Resource;
		public Assembly Assembly;
	}

	public enum UpdateType
	{
		Update,
		MicroUpdate
	}
}
