using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using QS.Utilities.Text;
using QSOsm.DTO;

namespace QSOsm.Loaders
{
	public interface IHousesDataLoader
	{
		event Action HousesLoaded;

		/// <summary>
		/// Initialize loading geodata from service
		/// </summary>
		void LoadHouses(OsmStreet street);

		OsmHouse[] GetHouses();
	}

	public class HousesDataLoader : OsmDataLoader, IHousesDataLoader
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		protected Task<List<OsmHouse>> CurrentLoadTask;

		protected OsmHouse[] Houses;

		public event Action HousesLoaded;

		public HousesDataLoader(IOsmService osm) : base(osm) { }

		public void LoadHouses(OsmStreet street)
		{
			CancelLoading();

			logger.Info($"Запрос домов...");
			if(street.Districts != null)
				CurrentLoadTask = Task.Run(() => Osm.GetHouseNumbers(street.CityId, street.Name, street.Districts), cancelTokenSource.Token);
			else
				CurrentLoadTask = Task.Run(() => Osm.GetHouseNumbersWithoutDistrict(street.CityId, street.Name), cancelTokenSource.Token);
			CurrentLoadTask.ContinueWith(SaveHouses, cancelTokenSource.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
			CurrentLoadTask.ContinueWith((arg) => logger.Error("Ошибка при загрузке домов", arg.Exception), TaskContinuationOptions.OnlyOnFaulted);
		}

		void SaveHouses(Task<List<OsmHouse>> newHouses)
		{
			var houses = newHouses.Result;
			//Удаляем литеры А у домов где других литер нет.
			foreach(var house in houses.Where(x => x.Letter == "А" || x.Letter == "а" || x.Letter == "A" || x.Letter == "a")) {
				if(!houses.Any(x => x.HouseNumber == house.HouseNumber && x.Letter != house.Letter))
					house.Letter = String.Empty;
			}
			houses = houses.OrderBy(x => x.ComplexNumber, new NaturalStringComparer()).ToList();
			Houses = houses.ToArray();
			logger.Info($"Домов загружено : {Houses.Length}");
			HousesLoaded?.Invoke();
		}

		public OsmHouse[] GetHouses()
		{
			return Houses?.Clone() as OsmHouse[];
		}

		protected override void CancelLoading()
		{
			if(CurrentLoadTask == null || !CurrentLoadTask.IsCompleted)
				logger.Debug($"Отмена предыдущей загрузки домов");

			cancelTokenSource.Cancel();
			cancelTokenSource = new CancellationTokenSource();
		}
	}
}
