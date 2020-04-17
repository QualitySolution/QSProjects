using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QSOsm.DTO;

namespace QSOsm.Loaders
{
	public interface ICitiesDataLoader
	{
		event Action CitiesLoaded;

		long GetCityId(string City, string CityDistrict, LocalityType localityType);

		/// <summary>
		/// Initialize loading geodata from service
		/// </summary>
		void LoadCities(string searchString, int limit = 50);

		OsmCity[] GetCities();

		/// <summary>
		/// Delay before query was executed (in millisecond)
		/// </summary>
		int Delay { get; set; }
	}

	public class CitiesDataLoader: OsmDataLoader, ICitiesDataLoader
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private OsmCity[] Cities;

		protected Task<List<OsmCity>> CurrentLoadTask;

		public event Action CitiesLoaded;

		public int Delay { get; set; } = 600;

		public CitiesDataLoader(IOsmService osm): base(osm) { }

		public void LoadCities(string searchString, int limit = 50)
		{
			CancelLoading();

			CurrentLoadTask = Task.Run(() => GetOsmCities(searchString, limit, cancelTokenSource.Token));
			CurrentLoadTask.ContinueWith(SaveCities, cancelTokenSource.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
			CurrentLoadTask.ContinueWith((arg) => logger.Error(arg.Exception, "Ошибка при загрузке городов"), TaskContinuationOptions.OnlyOnFaulted);
		}

		private List<OsmCity> GetOsmCities(string searchString, int limit, CancellationToken token)
		{
			Task.Delay(Delay).Wait();
			if(token.IsCancellationRequested)
				throw new OperationCanceledException();

			logger.Info($"Запрос городов... Строка поиска : {searchString} , Кол-во записей {limit}");
			return OsmWorker.GetOsmService().GetCitiesByCriteria(searchString, limit);
		}

		private void SaveCities(Task<List<OsmCity>> newCities)
		{
			Cities = newCities.Result.ToArray();
			logger.Info($"Городов загружено : {Cities.Length}");
			CitiesLoaded?.Invoke();
		}

		public OsmCity[] GetCities()
		{
			return Cities?.Clone() as OsmCity[];
		}

		public long GetCityId(string City, string CityDistrict, LocalityType localityType)
		{
			logger.Debug($"Запрос id для города {City}({CityDistrict})...");
			return Osm.GetCityId(City, CityDistrict, localityType.ToString()); ;
		}

		protected override void CancelLoading()
		{
			if(CurrentLoadTask == null)
				return;
			if(!CurrentLoadTask.IsCompleted)
				logger.Debug($"Отмена предыдущей загрузки городов");

			cancelTokenSource.Cancel();
			cancelTokenSource = new CancellationTokenSource();
		}
	}
}
