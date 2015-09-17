using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace QSOsm
{
	[ServiceContract]
	public interface IOsmService
	{
		[OperationContract]
		[WebGet (ResponseFormat = WebMessageFormat.Json)]
		List<OsmCity> GetCities ();

		[OperationContract]
		[WebGet (ResponseFormat = WebMessageFormat.Json)]
		List<OsmStreet> GetStreets (long OsmId);

	}
}

