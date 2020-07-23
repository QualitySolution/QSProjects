using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;

namespace QS.NewsFeed
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
		public uint DataBaseId;
		public Uri FeedUri;
		public HashSet<string> ReadItems = new HashSet<string>();
		public SyndicationFeed Feed;

		public NewsFeed (string feedId , string title, string url)
		{
			Title = title;
			FeedUri = new Uri (url);
			Id = feedId;
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
			return true;
		}

		public int UnreadNewsCount => Feed.Items.Count(x => !ReadItems.Contains(x.Id));
	}
}

