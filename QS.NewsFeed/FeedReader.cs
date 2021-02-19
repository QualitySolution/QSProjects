﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;
using Dapper;
using QS.Project.DB;
using QS.Services;

namespace QS.NewsFeed
{
	public class FeedReader
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public List<NewsFeed> NewsFeeds;
		private readonly IConnectionFactory connectionFactory;
		private readonly IUserService userService;

		public FeedReader(List<NewsFeed> feeds, IConnectionFactory connectionFactory, IUserService userService)
		{
			NewsFeeds = feeds ?? throw new ArgumentNullException(nameof(feeds));
			this.connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
			this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
		}

		#region Работа с прочитанным новостями

		public int UnreadNewsCount => NewsFeeds.Sum(x => x.UnreadNewsCount);

		public void LoadReadFeed()
		{
			logger.Info ("Получаем таблицу прочитанных пользователем новостей...");

			if (NewsFeeds == null)
				NewsFeeds = new List<NewsFeed> ();

			using(var connection = connectionFactory.OpenConnection()) {
				string sql = "SELECT * FROM `read_news` WHERE user_id = @user_id";
				var param = new { user_id = userService.CurrentUserId };
				var readed = connection.Query(sql, param);

				foreach(var row in readed) {
					NewsFeed feed = NewsFeeds.Find(f => f.Id == row.feed_id);
					if(feed != null) {
						feed.FirstRead = false;
						feed.DataBaseId = row.id;
						string[] items = (row.items ?? "").Split(',');
						foreach(var item in items)
							feed.ReadItems.Add(item);
					}
					else
						logger.Warn($"В базе найден feed_id={row.feed_id}, но в программе он не настроен.");
				}
			}
			logger.Info ("Ok");
		}

		public bool ItemIsRead(SyndicationItem synitem)
		{
			NewsFeed feed = NewsFeeds.Find (f => f.Id == synitem.SourceFeed.Id);
			if (feed.FirstRead) {
				feed.ReadItems.Add (synitem.Id);
				return true;
			}
			return feed.ReadItems.Contains(synitem.Id);
		}

		public void AddReadItem(SyndicationItem item)
		{
			var feed = NewsFeeds.Find(f => f.Id == item.SourceFeed.Id);
			if (feed.AddReadItem(item.Id)) {
				UpdateFeedReads(feed);
			}
		}

		public void SaveFirstRead()
		{
			string sql = "INSERT INTO read_news (user_id, feed_id, items) " +
					"VALUES (@user_id, @feed_id, @items)";
			var inserts = NewsFeeds.Where(f => f.FirstRead).Select(feed => new {
				user_id = userService.CurrentUserId,
				feed_id = feed.Id,
				items = String.Join(",", feed.ReadItems)
			}).ToArray();

			if(inserts.Any()) {
				logger.Info("Сохраняем новыe feeds");
				using(var connection = connectionFactory.OpenConnection()) {
					connection.Execute(sql, inserts);
					NewsFeeds.Where(f => f.FirstRead).AsList().ForEach(x => x.FirstRead = false);
				}
				logger.Info("Ok");
			}
		}

		public void UpdateFeedReads(NewsFeed feed)
		{
			if (feed.FirstRead)
				throw new InvalidOperationException ("Нельзя обновить новый фид с FirstRead = true");
			logger.Info ("Обновляем прочитанные новости...");
			string sql = "UPDATE read_news SET items = @items WHERE id = @id";
			var parameters = new { id = feed.DataBaseId, items = String.Join(",", feed.ReadItems) };
			using(var connection = connectionFactory.OpenConnection()) {
				connection.Execute(sql, parameters);
			}
			logger.Info ("Ok");
		}

		#endregion
		#region Загрузка фида

		public async Task<SyndicationFeed> LoadFeedAsync()
		{
			if (NewsFeeds == null || NewsFeeds.Count == 0) {
				logger.Warn("Нет настроенных лент новостей, выходим...");
				return null;
			}

			return await Task.Run(() => ThreadWorks());
		}

		private SyndicationFeed ThreadWorks()
		{
			logger.Info("Поток: Получаем ленты новостей.");
			SyndicationFeed mainFeed = new SyndicationFeed();
			foreach (var feed in NewsFeeds) {
				if (!feed.Active)
					continue;
				SyndicationFeed syndicationFeed;
				try {
					using (XmlReader reader = XmlReader.Create(feed.FeedUri.AbsoluteUri)) {
						syndicationFeed = SyndicationFeed.Load(reader);
					}
					syndicationFeed.Id = feed.Id;
					syndicationFeed.Items.ToList().ForEach(i => i.SourceFeed = syndicationFeed);
					feed.Feed = syndicationFeed;
				}
				catch (Exception ex) {
					logger.Warn(ex, "Не удалось прочитать feed");
					continue;
				}
				SyndicationFeed tempFeed = new SyndicationFeed(
												   mainFeed.Items.Union(syndicationFeed.Items).OrderByDescending(u => u.PublishDate));

				mainFeed = tempFeed;
			}
			return mainFeed;
		}

		#endregion
	}
}

