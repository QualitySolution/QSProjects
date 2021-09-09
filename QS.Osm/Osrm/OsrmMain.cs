using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RestSharp;

namespace QS.Osm.Osrm
{
	public static class OsrmMain
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public static string ServerUrl;

		public static RouteResponse GetRoute(List<PointOnEarth> routePOIs, bool alt = false, GeometryOverview geometry = GeometryOverview.False)
		{
			if (routePOIs.Count < 2)
				throw new ArgumentException ("Список точке для прокладки маршрута, должен содержать хотя бы 2 точки.", nameof (routePOIs));

			DateTime startTime = DateTime.Now;

			var client = new RestClient ();
			client.BaseUrl = new Uri (ServerUrl);

			var request = new RestRequest ("/route/v1/car/{points}", Method.GET);

			var points = String.Join(";", routePOIs.Select(point => String.Format(CultureInfo.InvariantCulture, "{0},{1}", point.Longitude, point.Latitude ))) ;

			request.AddUrlSegment("points", points);

			if(alt) // По умолчанию выключено
				request.AddQueryParameter ("alternatives", "true");

			if(geometry != GeometryOverview.Simplified) // По умолчанию включено simplified
				request.AddQueryParameter("overview", geometry.ToString().ToLower());

			var response = client.Execute<RouteResponse> (request);
			if(response.Data == null)
			{
				logger.Error("Ошибка в обработке запроса к osrm status={0} message={1}", response.ResponseStatus, response.ErrorMessage);
			}
			else if (response.Data.Code != "Ok")
			{
				logger.Error ("Ошибка при получении маршрута со osrm {0}: {1}", response.Data.Code, response.Data.Message);
				logger.Debug("Запрошен машрут: {0}", String.Join(" -> ", routePOIs.Select(point => String.Format(CultureInfo.InvariantCulture, "{0},{1}", point.Latitude, point.Longitude))));
				logger.Debug ("Полный ответ: {0}", response.Content);
			}
			else
			{
				logger.Debug ("Полный ответ за {1} сек.: {0}", response.Content, (DateTime.Now - startTime).TotalSeconds);
			}
			return response.Data;
		}
	}
}
