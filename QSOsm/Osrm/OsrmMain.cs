using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RestSharp;

namespace QSOsm.Osrm
{
	public static class OsrmMain
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		#region Настройки
		public static string ServerUrl;

		#endregion

		public static RouteResponse GetRoute(List<PointOnEarth> routePOIs, bool alt = false, bool geometry = false)
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

			if(!geometry) // По умолчанию включено simplified
				request.AddQueryParameter("overview", "false");

			var response = client.Execute<RouteResponse> (request);
			if (response.Data.Code != "Ok")
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
