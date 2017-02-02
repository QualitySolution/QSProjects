using System;
using suggestionscsharp;

namespace QSFNS
{
	public static class FNSMain
	{
		public static SuggestClient Api { get; set; }

		public static void SetUp() {
			if (Api != null)
				return;
			var token = "0be9fe369318e9bc963fd885e6c3456804cafda4";
			var url = "https://suggestions.dadata.ru/suggestions/api/4_1/rs";
			Api = new SuggestClient(token, url);
		}
	}
}

