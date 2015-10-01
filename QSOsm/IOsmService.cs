using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using QSOsm.DTO;

namespace QSOsm
{
	[ServiceContract]
	public interface IOsmService
	{
		[OperationContract]
		[WebGet (ResponseFormat = WebMessageFormat.Json)]
		/// <summary>
		/// Returns all the cities from OSM database.
		/// </summary>
		/// <returns>The cities.</returns>
		List<OsmCity> GetCities ();

		[OperationContract]
		[WebGet (ResponseFormat = WebMessageFormat.Json)]
		/// <summary>
		/// Gets the streets for city, identified by id.
		/// </summary>
		/// <returns>The streets.</returns>
		/// <param name="OsmId">City OSM identifier.</param>
		List<OsmStreet> GetStreets (long OsmId);

		[OperationContract]
		[WebGet (ResponseFormat = WebMessageFormat.Json)]
		/// <summary>
		/// Gets the city identifier.
		/// </summary>
		/// <returns>The city identifier.</returns>
		/// <param name="City">City name.</param>
		/// <param name="District">District.</param>
		long GetCityId (string City, string District);

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
		List<string> GetHouseNumbers (long CityOSMId, string Street, string District);
	}
}

