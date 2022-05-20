using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

namespace QS.Osrm
{
    public class OsrmClient
    {
        private readonly string _responseOk = "Ok";
        private readonly string _urlSegmentPoints = "points";
        private readonly string _queryParameteExclude = "exclude";
        private readonly string _queryParameteOverview = "overview";
        private readonly string _queryParameteAlternatives = "alternatives";

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public string ServerUrl { get; set; }

        public OsrmClient()
        {
        }

        public OsrmClient(string serverUrl)
        {
            ServerUrl = serverUrl;
        }

        /// <summary>
        /// Построить маршрут через точки
        /// </summary>
        /// <param name="routePOIs">Список точек по которым строим маршрут</param>
        /// <param name="alt">По умолчанию выключено (ЧТО ЭТО?)</param>
        /// <param name="geometry">По умолчанию выключено (ЧТО ЭТО?)</param>
        /// <param name="excludeToll">Исключить из маршрута платные дороги</param>
        /// <returns></returns>
        public RouteResponse GetRoute(List<PointOnEarth> routePOIs, bool alt = false, GeometryOverview geometry = GeometryOverview.False, bool excludeToll = false)
        {
            var requestTask = GetRouteAsync(routePOIs, alt, geometry, excludeToll);
            requestTask.Wait();
            return requestTask.Result;
        }

		/// <summary>
		/// Построить маршрут через точки
		/// </summary>
		/// <param name="routePOIs">Список точек по которым строим маршрут</param>
		/// <param name="alt">Показывать альтернативныe маршруты. По умолчанию выключено</param>
		/// <param name="geometry">Возвращать маршруть по точкам. По умолчанию выключено</param>
		/// <param name="excludeToll">Исключить из маршрута платные дороги</param>
		/// <returns></returns>
		public async Task<RouteResponse> GetRouteAsync(List<PointOnEarth> routePOIs, bool alt = false, GeometryOverview geometry = GeometryOverview.False, bool excludeToll = false)
        {
            if (routePOIs.Count < 2)
            {
                throw new ArgumentException("Список точке для прокладки маршрута, должен содержать хотя бы 2 точки.", nameof(routePOIs));
            }
            var points = ConvertPointsToString(routePOIs);
            var request = new RestRequest("/route/v1/car/{points}", Method.GET);
            request.AddUrlSegment(_urlSegmentPoints, points);

            if (alt)
            {
                request.AddQueryParameter(_queryParameteAlternatives, "true");
            }
            if (geometry != GeometryOverview.Simplified)
            {
                request.AddQueryParameter(_queryParameteOverview, geometry.ToString().ToLower());
            }
            if (excludeToll)
            {
                request.AddQueryParameter(_queryParameteExclude, "toll");
            }

            var client = new RestClient(ServerUrl);
            client.UseNewtonsoftJson();
            var response = await client.ExecuteAsync<RouteResponse>(request);

            Logging(response, routePOIs);

            return response.Data;
        }

        private void Logging(IRestResponse<RouteResponse> response, List<PointOnEarth> routePOIs)
        {
            if (response.Data == null)
            {
                logger.Error("Ошибка в обработке запроса к osrm status={0} message={1}", response.ResponseStatus, response.ErrorMessage);
            }
            else if (response.Data.Code != _responseOk)
            {
                logger.Error("Ошибка при получении маршрута со osrm {0}: {1}", response.Data.Code, response.Data.Message);
                logger.Debug("Запрошен машрут: {0}", ConvertPointsToString(routePOIs, " -> "));
                logger.Debug("Полный ответ: {0}", response.Content);
            }
            else
            {
                DateTime startTime = DateTime.Now;
                logger.Debug("Полный ответ за {0} сек.: {1}", (DateTime.Now - startTime).TotalSeconds, response.Content);
            }
        }

        private string ConvertPointsToString(List<PointOnEarth> routePOIs, string separator = ";")
        {
            return String.Join(separator, routePOIs.Select(point => $"{point.Longitude},{point.Latitude}"));
        }
    }
}
