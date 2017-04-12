using System;
using System.Collections.Generic;
using System.Globalization;
using RestSharp;

namespace QSOsm.Spuntik
{
	public static class SputnikMain
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public static SputnikRouteResponse GetRoute(List<PointOnEarth> routePOIs)
		{
			if (routePOIs.Count < 2)
				throw new ArgumentException ("Список точке для прокладки маршрута, должен содержать хотя бы 2 точки.", nameof (routePOIs));

			var client = new RestClient ();
			client.BaseUrl = new Uri ("http://routes.maps.sputnik.ru");

			var request = new RestRequest ("osrm/router/viaroute", Method.GET);

			foreach(var point in routePOIs)
			{
				request.AddQueryParameter ("loc", String.Format (CultureInfo.InvariantCulture, "{0},{1}", point.Latitude, point.Longitude));
			}
			request.AddQueryParameter ("alt", "false");

			var response = client.Execute<SputnikRouteResponse> (request);
			if (response.Data.Status != 0)
			{
				logger.Error ("Ошибка при получении маршрута со спутника {0}: {1}", response.Data.Status, response.Data.StatusMessage);
				logger.Debug ("Полный ответ: {0}", response.Content);
			}
			else
			{
				logger.Debug ("Полный ответ: {0}", response.Content);
			}
			return response.Data;
		}
	}
}
