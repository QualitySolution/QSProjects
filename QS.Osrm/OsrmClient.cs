using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

namespace QS.Osrm
{
	public class OsrmClient
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public string ServerUrl { get; set; }

        public OsrmClient()
        {
        }

        public OsrmClient(string serverUrl)
        {
            ServerUrl = serverUrl;
        }

		public RouteResponse GetRoute(List<PointOnEarth> routePOIs, bool alt = false, GeometryOverview geometry = GeometryOverview.False)
		{
			var requestTask = GetRouteAsync(routePOIs, alt, geometry);
			requestTask.Wait();
			return requestTask.Result;
		}

		public async Task<RouteResponse> GetRouteAsync(List<PointOnEarth> routePOIs, bool alt = false, GeometryOverview geometry = GeometryOverview.False)
		{
			if (routePOIs.Count < 2)
				throw new ArgumentException("Список точке для прокладки маршрута, должен содержать хотя бы 2 точки.", nameof(routePOIs));

			DateTime startTime = DateTime.Now;

			using (var client = new RestClient(ServerUrl))
            {
				client.UseNewtonsoftJson();
				var request = new RestRequest("/route/v1/car/{points}", Method.Get);

				var points = String.Join(";", routePOIs.Select(point => String.Format(CultureInfo.InvariantCulture, "{0},{1}", point.Longitude, point.Latitude)));

				request.AddUrlSegment("points", points);

				if (alt) // По умолчанию выключено
					request.AddQueryParameter("alternatives", "true");

				if (geometry != GeometryOverview.Simplified) // По умолчанию включено simplified
					request.AddQueryParameter("overview", geometry.ToString().ToLower());

				var response = await client.ExecuteAsync<RouteResponse>(request);
				if (response.Data == null)
				{
					logger.Error("Ошибка в обработке запроса к osrm status={0} message={1}", response.ResponseStatus, response.ErrorMessage);
				}
				else if (response.Data.Code != "Ok")
				{
					logger.Error("Ошибка при получении маршрута со osrm {0}: {1}", response.Data.Code, response.Data.Message);
					logger.Debug("Запрошен машрут: {0}", String.Join(" -> ", routePOIs.Select(point => String.Format(CultureInfo.InvariantCulture, "{0},{1}", point.Latitude, point.Longitude))));
					logger.Debug($"Полный ответ: {response.Content}");
				}
				else
				{
					logger.Debug($"Полный ответ за {(DateTime.Now - startTime).TotalSeconds} сек.: {response.Content}");
				}
				return response.Data;
			}
		}
	}
}
