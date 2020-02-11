using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QSOsm.DTO;

namespace QSOsm.Loaders
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
			CurrentLoadTask.ContinueWith(SaveStreets,TaskContinuationOptions.OnlyOnRanToCompletion);
			CurrentLoadTask.ContinueWith((arg) => logger.Error(arg.Exception, "Ошибка при загрузке улиц"), TaskContinuationOptions.OnlyOnFaulted);
		}

		protected List<OsmStreet> GetOsmStreets(long cityId, string searchString, int limit, CancellationToken token)
		{
			Task.Delay(Delay).Wait();
			if(token.IsCancellationRequested)
				throw new OperationCanceledException();

			logger.Info($"Запрос улиц... Строка поиска : {searchString} , Кол-во записей {limit}");
			return Osm.GetStreetsByCriteria(cityId, searchString, limit);
		}


		protected void SaveStreets(Task<List<OsmStreet>> newSteets)
		{
			Streets = newSteets.Result.ToArray();
			logger.Info($"Улиц загружено : {Streets.Length}");
			StreetLoaded.Invoke();
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
