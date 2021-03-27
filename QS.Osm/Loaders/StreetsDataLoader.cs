using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QS.Osm.DTO;

namespace QS.Osm.Loaders
{
	public interface IStreetsDataLoader
	{
		event Action StreetLoaded;

		/// <summary>
		/// Initialize loading geodata from service
		/// </summary>
		void LoadStreets(long cityId, string searchString, int limit = 50);

		OsmStreet[] GetStreets();

		/// <summary>
		/// Delay before query was executed (in millisecond)
		/// </summary>
		int Delay { get; set; }
	}

	public class StreetsDataLoader: OsmDataLoader, IStreetsDataLoader
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		protected OsmStreet[] Streets;

		protected Task<List<OsmStreet>> CurrentLoadTask;

		public event Action StreetLoaded;

		public int Delay { get; set; } = 1000;

		public StreetsDataLoader(IOsmService osm) : base(osm) { }

		public void LoadStreets(long cityId, string searchString, int limit = 50)
		{
			CancelLoading();

			CurrentLoadTask = Task.Run(() => GetOsmStreets(cityId,searchString, limit, cancelTokenSource.Token));
			CurrentLoadTask.ContinueWith(SaveStreets, cancelTokenSource.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
			CurrentLoadTask.ContinueWith((arg) => logger.Error(arg.Exception, "Ошибка при загрузке улиц"), TaskContinuationOptions.OnlyOnFaulted);
		}

		protected List<OsmStreet> GetOsmStreets(long cityId, string searchString, int limit, CancellationToken token)
		{
            try
            {
				Task.Delay(Delay, token).Wait();
				logger.Info($"Запрос улиц... Строка поиска : {searchString} , Кол-во записей {limit}");
				return Osm.GetStreetsByCriteria(cityId, searchString, limit);
			} catch (AggregateException ae)
            {
				ae.Handle(ex =>
				{
					if (ex is TaskCanceledException)
                    {
						logger.Info("Запрос улиц отменен");
					}
					return ex is TaskCanceledException;
				});
				return new List<OsmStreet>();
			}
		}

		protected void SaveStreets(Task<List<OsmStreet>> newStreets)
		{
			Streets = newStreets.Result.ToArray();
			logger.Info($"Улиц загружено : {Streets.Length}");
			StreetLoaded?.Invoke();
		}

		public OsmStreet[] GetStreets()
		{
			return Streets?.Clone() as OsmStreet[];
		}

		protected override void CancelLoading()
		{
			if(CurrentLoadTask == null || !CurrentLoadTask.IsCompleted)
				logger.Debug($"Отмена предыдущей загрузки улиц");

			cancelTokenSource.Cancel();
			cancelTokenSource = new CancellationTokenSource();
		}
	}
}
