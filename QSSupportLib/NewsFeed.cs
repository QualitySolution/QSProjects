using System;
using System.Collections.Generic;

namespace QSSupportLib
{
	public class NewsFeed
	{
		public bool Active = true;
		public bool FirstRead = true;
		public string Title;
		/// <summary>
		/// В базе данных хранится максимум 64 символа.
		/// </summary>
		public string Id;
		public int DataBaseId;
		public Uri FeedUri;
		public List<string> ReadItems;

		public NewsFeed (string feedId , string title, string url)
		{
			Title = title;
			FeedUri = new Uri (url);
			Id = feedId;
			ReadItems = new List<string> ();
		}

		/// <summary>
		/// Добавляем id новости в прочитанные.
		/// </summary>
		/// <returns><c>false</c> если новость уже помечена как прочитанная, иначе <c>true</c>.</returns>
		/// <param name="itemid">Id новости</param>
		public bool AddReadItem(string itemid)
		{
			if (ReadItems.Contains (itemid))
				return false;
			ReadItems.Add (itemid);
			MainNewsFeed.UpdateFeedReads (this);
			return true;
		}
	}
}

