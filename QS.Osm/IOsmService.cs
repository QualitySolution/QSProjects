using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using QS.Osm.DTO;

namespace QS.Osm
{
	[ServiceContract]
	public interface IOsmService
	{
		[OperationContract]
		[WebGet (ResponseFormat = WebMessageFormat.Json)]
		/// <summary>
		/// Returns cities by criteria from OSM database.
		/// </summary>
		/// <returns>The cities.</returns>
		///	<param name="searchString">city search criteria</param>
		List<OsmCity> GetCitiesByCriteria (string searchString, int limit);


		[OperationContract]
		[WebGet(ResponseFormat = WebMessageFormat.Json)]
		/// <summary>
		/// Returns all the cities from OSM database.
		/// </summary>
		/// <returns>The cities.</returns>
		List<OsmCity> GetCities();

		[OperationContract]
		[WebGet(ResponseFormat = WebMessageFormat.Json)]
		/// <summary>
		/// Gets the streets for city, identified by id.
		/// </summary>
		/// <returns>The streets.</returns>
		/// <param name="OsmId">City OSM identifier.</param>
		List<OsmStreet> GetStreets(long cityId);

		[OperationContract]
		[WebGet (ResponseFormat = WebMessageFormat.Json)]
		/// <summary>
		/// Gets the streets for city by criteria
		/// </summary>
		/// <returns>The streets.</returns>
		/// <param name="OsmId">City OSM identifier.</param>
		///	<param name="searchString">city search criteria</param>
		/// <param name="limit">limit on the number of returned rows</param>
		List<OsmStreet> GetStreetsByCriteria(long cityId, string searchString, int limit);

		[OperationContract]
		[WebGet (ResponseFormat = WebMessageFormat.Json)]
		/// <summary>
		/// Gets the city identifier.
		/// </summary>
		/// <returns>The city identifier.</returns>
		/// <param name="City">City name.</param>
		/// <param name="District">District.</param>
		long GetCityId (string City, string District, string Locality);

		[OperationContract]
		[WebGet (ResponseFormat = WebMessageFormat.Json)]
		/// <summary>
		/// Gets the point region.
		/// </summary>
		/// <returns>The point region (Saint-Petersburg or Leningrad region).</returns>
		/// <param name="OsmId">Point OSM identifier.</param>
		string GetPointRegion (long OsmId);

		[OperationContract]
		[WebGet (ResponseFormat = WebMessageFormat.Json)]
		List<OsmHouse> GetHouseNumbersWithoutDistrict (long cityId, string street);

		[OperationContract]
		[WebGet (ResponseFormat = WebMessageFormat.Json)]
		List<OsmHouse> GetHouseNumbers (long cityId, string street, string districts);

		[OperationContract]
		[WebGet(ResponseFormat = WebMessageFormat.Json)]
		long GetBuildingCountInCity(string city);
		
		[OperationContract]
		[WebGet(ResponseFormat = WebMessageFormat.Json)]
		long GetBuildingCountInRegion(string region);

		[OperationContract]
		[WebGet(ResponseFormat = WebMessageFormat.Json)]
		bool ServiceStatus();
	}
}

