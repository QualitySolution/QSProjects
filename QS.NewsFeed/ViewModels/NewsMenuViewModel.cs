using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel.Syndication;
using QS.ViewModels;

namespace QS.NewsFeed.ViewModels
{
	public class NewsMenuViewModel : ViewModelBase
	{
		private readonly FeedReader reader;

		#region Свойства для View
		public Stream MenuIcon => Assembly.GetExecutingAssembly().GetManifestResourceStream("QS.NewsFeed.Icons.internet-news-reader.png");

		private SyndicationFeed feed;
		public virtual SyndicationFeed Feed {
			get => feed;
			private set => SetField(ref feed, value);
		}

		public int UnreadNewsCount => reader.UnreadNewsCount;

		public IEnumerable<NewsArticle> News => Feed.Items.Select(x => new NewsArticle(x,reader.ItemIsRead(x)));
		#endregion

		public NewsMenuViewModel(FeedReader reader)
		{
			this.reader = reader ?? throw new ArgumentNullException(nameof(reader));
		}

		public void LoadFeed()
		{
			reader.LoadFeedAsync()
				.ContinueWith(tsk => {
					Feed = tsk.Result;
					OnPropertyChanged(nameof(UnreadNewsCount));
					reader.SaveFirstRead();
				});

		}

		public event EventHandler<ReadStatusChangedEventArgs> ReadStatusChanged;

		#region Действия View

		public void OpenNews(SyndicationItem item)
		{
			foreach (var link in item.Links) {
				//Здесь пробуем исправить ошибку 34641 на нашем багтрекере.
				//Предположил что проблема в этом https://github.com/dotnet/runtime/issues/28005
				//Но проверить действительно ли это так негде.
				ProcessStartInfo psi = new ProcessStartInfo
				{
					FileName = link.Uri.AbsoluteUri,
					UseShellExecute = true
				};
				Process.Start (psi);
			}
			reader.AddReadItem(item);
			ReadStatusChanged?.Invoke(this, new ReadStatusChangedEventArgs(item));
			OnPropertyChanged(nameof(UnreadNewsCount));
		}

		#endregion
	}

	public class ReadStatusChangedEventArgs : EventArgs
	{
		public readonly SyndicationItem Item;

		public ReadStatusChangedEventArgs(SyndicationItem item)
		{
			Item = item ?? throw new ArgumentNullException(nameof(item));
		}
	}

	public class NewsArticle
	{
		public readonly SyndicationItem Item;
		public readonly bool Read;

		public NewsArticle(SyndicationItem item, bool read)
		{
			Item = item ?? throw new ArgumentNullException(nameof(item));
			Read = read;
		}
	}
}
