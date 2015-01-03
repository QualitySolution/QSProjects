using System;
using System.Collections.Generic;
using Gtk;
using NLog;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Linq;
using QSWidgetLib;
using QSProjectsLib;
using System.Threading;

namespace QSSupportLib
{
	public class NewsMenuItem : MenuItem
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		Label menuLabel;
		Image newsicon;
		int UnreadNewsCount;

		public NewsMenuItem ()
		{
			HBox box = new HBox (false, 0);

			menuLabel = new Label ();
			box.Add (menuLabel);

			newsicon = new Image (Gdk.Pixbuf.LoadFromResource ("QSSupportLib.icons.internet-news-reader.png"));
			newsicon.TooltipText = "Нет непрочитанных новостей.";
			newsicon.Show ();
			box.Add (newsicon);

			this.Add (box);
			this.RightJustified = true;
			this.ShowAll ();
			menuLabel.Visible = false;
		}

		public void LoadFeed()
		{
			if (MainNewsFeed.NewsFeeds == null || MainNewsFeed.NewsFeeds.Count == 0) {
				logger.Warn ("Нет настроенных лент новостей, выходим...");
				return;
			}

			Thread loadThread = new Thread (new ThreadStart (ThreadWorks));
			loadThread.Start ();
		}

		private void ThreadWorks()
		{
			logger.Info ("Поток: Получаем ленты новостей.");
			SyndicationFeed mainFeed = new SyndicationFeed(); 
			foreach (var feed in MainNewsFeed.NewsFeeds) 
			{ 
				if (!feed.Active)
					continue;
				SyndicationFeed syndicationFeed;
				try
				{
					using (XmlReader reader = XmlReader.Create(feed.FeedUri.AbsoluteUri)) 
					{
						syndicationFeed = SyndicationFeed.Load(reader); 
					}
					syndicationFeed.Id = feed.Id;
					syndicationFeed.Items.ToList().ForEach(i => i.SourceFeed = syndicationFeed);
				}
				catch(Exception ex)
				{
					logger.WarnException ("Не удалось прочитать feed", ex);
					continue;
				}
				SyndicationFeed tempFeed = new SyndicationFeed( 
				                                   mainFeed.Items.Union(syndicationFeed.Items).OrderByDescending(u => u.PublishDate)); 

				mainFeed = tempFeed; 
			}
			logger.Info ("Создаем меню новостей..");
			Application.Invoke (delegate {
				logger.Info ("Запуск операций в основном потоке..");
				UnreadNewsCount = 0;
				var newsMenu = new Menu ();
				MenuItemId<SyndicationItem> newsItem;
				foreach (var news in mainFeed.Items) {
					Label itemLabel = new Label ();
					if (MainNewsFeed.ItemIsRead (news))
						itemLabel.Text = news.Title.Text;
					else {
						itemLabel.Markup = String.Format ("<b>{0}</b>", news.Title.Text);
						UnreadNewsCount++;
					}
					newsItem = new MenuItemId<SyndicationItem> ();
					newsItem.Add (itemLabel);
					newsItem.ID = news;
					newsItem.TooltipMarkup = String.Format ("<b>{0:D}</b> {1}", news.PublishDate, news.Summary.Text);
					newsItem.Activated += OnNewsActivated;
					newsMenu.Append (newsItem);
				}
				this.Submenu = newsMenu;
				UpdateIcon ();
				newsMenu.ShowAll ();
				MainNewsFeed.SaveFirstRead ();
				logger.Info("Ок");
			});
		}

		void OnNewsActivated (object sender, EventArgs e)
		{
			if (sender is MenuItemId<SyndicationItem>) {
				var news = (sender as MenuItemId<SyndicationItem>).ID;
				foreach(var link in news.Links)
				{
					System.Diagnostics.Process.Start (link.Uri.AbsoluteUri);
				}
				var feed = MainNewsFeed.NewsFeeds.Find (f => f.Id == news.SourceFeed.Id);
				if (feed.AddReadItem (news.Id)) {
					UnreadNewsCount--;
					((sender as MenuItemId<SyndicationItem>).Child as Label).Text = news.Title.Text;
					UpdateIcon ();
				}
			} else
				logger.Warn ("Некорректная привязка события.");

		}

		void UpdateIcon()
		{
			if (UnreadNewsCount > 0)
			{
				menuLabel.Markup = String.Format ("<span foreground=\"red\" weight=\"bold\">+{0}</span>", UnreadNewsCount);
				menuLabel.TooltipText = RusNumber.FormatCase (UnreadNewsCount, "{0} непрочитанная новость", "{0} непрочитанных новости", "{0} непрочитанных новостей");
			}

			menuLabel.Visible = (UnreadNewsCount > 0);
			newsicon.Visible = (UnreadNewsCount == 0);
		}
	}
}

