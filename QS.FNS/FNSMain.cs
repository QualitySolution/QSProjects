using System.Collections.Generic;
using suggestionscsharp;

namespace QS.FNS
{
	public static class FNSMain
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public static SuggestClient Api { get; set; }

		static Dictionary<string, SuggestPartyResponse> partyCache = new Dictionary<string, SuggestPartyResponse>();

		public static void SetUp() {
			if (Api != null)
				return;
			var token = "0be9fe369318e9bc963fd885e6c3456804cafda4";
			var url = "https://suggestions.dadata.ru/suggestions/api/4_1/rs";
			Api = new SuggestClient(token, url);
		}

		public static SuggestPartyResponse CachedQueryParty(string query)
		{
			logger.Info ("Запрос контрагента по [{0}]...", query);
			if (partyCache.ContainsKey(query))
			{
				logger.Info("Найден в кеше");
				return partyCache[query];
			}
			else
			{
				var response = Api.QueryParty(query);
				partyCache.Add(query, response);
				logger.Debug ("Получено {0} подсказок...", response.suggestions.Count);
				return response;
			}
		}
	}
}

