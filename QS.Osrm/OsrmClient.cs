using Microsoft.Extensions.Logging;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QS.Osrm
{
	public class OsrmClient : IOsrmClient 
	{
		private readonly string _responseOk = "Ok";
		private readonly string _urlSegmentPoints = "points";
		private readonly string _queryParameteExclude = "exclude";
		private readonly string _queryParameteOverview = "overview";
		private readonly string _queryParameteAlternatives = "alternatives";


		private readonly string _serverUrl;
		private readonly ILogger<OsrmClient> logger;

		public OsrmClient(ILogger<OsrmClient> logger, string serverUrl) {
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_serverUrl = serverUrl;
		}

		/// <summary>
		/// Построить маршрут через точки
		/// </summary>
		/// <param name="routePOIs">Список точек по которым строим маршрут</param>
		/// <param name="alt">По умолчанию выключено (ЧТО ЭТО?)</param>
		/// <param name="geometry">По умолчанию выключено (ЧТО ЭТО?)</param>
		/// <param name="excludeToll">Исключить из маршрута платные дороги</param>
		/// <returns></returns>
		public RouteResponse GetRoute(List<PointOnEarth> routePOIs, bool alt = false, GeometryOverview geometry = GeometryOverview.False, bool excludeToll = false) {
			if(routePOIs.Count < 2) {
				throw new ArgumentException("Список точке для прокладки маршрута, должен содержать хотя бы 2 точки.", nameof(routePOIs));
			}
			var points = ConvertPointsToString(routePOIs);
			var request = new RestRequest("/route/v1/car/{points}", Method.GET);
			request.AddUrlSegment(_urlSegmentPoints, points);

			if(alt) {
				request.AddQueryParameter(_queryParameteAlternatives, "true");
			}
			if(geometry != GeometryOverview.Simplified) {
				request.AddQueryParameter(_queryParameteOverview, geometry.ToString().ToLower());
			}
			if(excludeToll) {
				request.AddQueryParameter(_queryParameteExclude, "toll");
			}

			var client = new RestClient(_serverUrl);
			client.UseNewtonsoftJson();

			var startTime = DateTime.Now;
			var response = client.Execute<RouteResponse>(request);

			if(response.Data == null) {
				logger.LogError("Ошибка в обработке запроса к osrm status={ResponseStatus} message={Message}", response.ResponseStatus, response.ErrorMessage);
			}
			else if(response.Data.Code != _responseOk) {
				logger.LogError("Ошибка при получении маршрута со osrm {HttpCode}: {Message}", response.Data.Code, response.Data.Message);
				logger.LogDebug("Запрошен машрут: {Route}", ConvertPointsToString(routePOIs, " -> "));
				logger.LogDebug("Полный ответ: {ResponseContent}", response.Content);
			}
			else {

				logger.LogDebug("OSRM: {ServerUrl}. Полный ответ за {RequestDuration} сек.: {ResponseContent}", _serverUrl, (DateTime.Now - startTime).TotalSeconds, response.Content);
			}

			return response.Data;
		}

		/// <summary>
		/// Построить маршрут через точки
		/// </summary>
		/// <param name="routePOIs">Список точек по которым строим маршрут</param>
		/// <param name="alt">Показывать альтернативныe маршруты. По умолчанию выключено</param>
		/// <param name="geometry">Возвращать маршруть по точкам. По умолчанию выключено</param>
		/// <param name="excludeToll">Исключить из маршрута платные дороги</param>
		/// <returns></returns>
		public async Task<RouteResponse> GetRouteAsync(
			List<PointOnEarth> routePOIs,
			CancellationToken cancellationToken,
			bool alt = false,
			GeometryOverview geometry = GeometryOverview.False,
			bool excludeToll = false
		) 
		{
			if(routePOIs.Count < 2) {
				throw new ArgumentException("Список точке для прокладки маршрута, должен содержать хотя бы 2 точки.", nameof(routePOIs));
			}
			var points = ConvertPointsToString(routePOIs);
			var request = new RestRequest("/route/v1/car/{points}", Method.GET);
			request.AddUrlSegment(_urlSegmentPoints, points);

			if(alt) {
				request.AddQueryParameter(_queryParameteAlternatives, "true");
			}
			if(geometry != GeometryOverview.Simplified) {
				request.AddQueryParameter(_queryParameteOverview, geometry.ToString().ToLower());
			}
			if(excludeToll) {
				request.AddQueryParameter(_queryParameteExclude, "toll");
			}

			var client = new RestClient(_serverUrl);
			client.UseNewtonsoftJson();

			var startTime = DateTime.Now;
			var response = await client.ExecuteAsync<RouteResponse>(request, cancellationToken);

			if(response.Data == null) {
				logger.LogError("Ошибка в обработке запроса к osrm status={ResponseStatus} message={Message}", response.ResponseStatus, response.ErrorMessage);
			}
			else if(response.Data.Code != _responseOk) {
				logger.LogError("Ошибка при получении маршрута со osrm {HttpCode}: {Message}", response.Data.Code, response.Data.Message);
				logger.LogDebug("Запрошен машрут: {Route}", ConvertPointsToString(routePOIs, " -> "));
				logger.LogDebug("Полный ответ: {ResponseContent}", response.Content);
			}
			else {

				logger.LogDebug("OSRM: {ServerUrl}. Полный ответ за {RequestDuration} сек.: {ResponseContent}", _serverUrl, (DateTime.Now - startTime).TotalSeconds, response.Content);
			}

			return response.Data;
		}

		private string ConvertPointsToString(List<PointOnEarth> routePOIs, string separator = ";") {
			return String.Join(separator, routePOIs.Select(point => String.Format(CultureInfo.InvariantCulture, "{0},{1}", point.Longitude, point.Latitude)));
		}
	}
}
