using System;
using System.Linq;
using System.Reflection;
using Gtk;
using QS.Tdi.Gtk;
using QS.Utilities.Processes;
using QS.Utilities.Text;
using QS.ViewModels.Extension;

namespace QS.Tdi
{
	public class TabVBox : VBox
	{
		internal readonly IDialogDocumentation documentation;
		public Widget TabWidget { get; }
		Label titleLabel;
		internal Button docButton;

		public ITdiTab Tab { get; }
		public event EventHandler OnDocumentationOpened;

		public TabVBox(ITdiTab tabWidget, ITDIWidgetResolver widgetResolver, IDialogDocumentation documentation = null)
		{
			this.documentation = documentation;
			Tab = tabWidget;
			titleLabel = new Label ();
			
			docButton = new Button(new Image(Assembly.GetExecutingAssembly(), "QS.Icons.documentation.png"));
			docButton.Relief = ReliefStyle.None;
			docButton.ModifierStyle.Xthickness = 0;
			docButton.ModifierStyle.Ythickness = 0;
			docButton.Clicked += (sender, e) => {
				OpenHelper.OpenUrl(documentation.DocumentationUrl);
				OnDocumentationOpened?.Invoke(this, EventArgs.Empty);
			};
			docButton.ShowAll();
			
			if(Tab is ITdiTabWithPath tdiTabWithPath)
			{
				tdiTabWithPath.PathChanged += OnPathUpdated;
				OnPathUpdated(null, EventArgs.Empty);
			}
			else
			{
				Tab.TabNameChanged += Tab_TabNameChanged;
				Tab_TabNameChanged (null, null);
			}

			var titleHbox = new HBox();
			this.PackStart (titleHbox, false, false, 2);
			titleHbox.PackStart (titleLabel, true, true, 2);
			titleHbox.PackStart(docButton, false, true, 0);

			TabWidget = widgetResolver.Resolve(tabWidget);
			if(TabWidget == null)
				throw new InvalidCastException($"Ошибка приведения типа {nameof(ITdiTab)} к типу {nameof(Widget)}.");

			this.Add (TabWidget);
			titleHbox.Show();
			titleLabel.Show();
			TabWidget.Show ();
		}

		void OnPathUpdated (object sender, EventArgs e)
		{
			titleLabel.Markup = String.Format("<b>{0}</b>",
				String.Join(" <span color='blue'>➤</span> ",
					(Tab as ITdiTabWithPath).PathNames.Select(x => StringManipulationHelper.EllipsizeMiddle(x, TdiNotebook.MaxTabNameLenght))));
			DocButtonRefresh();
		}

		void Tab_TabNameChanged (object sender, TdiTabNameChangedEventArgs e)
		{
			titleLabel.Markup = $"<b>{Tab.TabName}</b>";
			DocButtonRefresh();
		}

		void DocButtonRefresh() {
			docButton.Visible = !String.IsNullOrEmpty(documentation?.DocumentationUrl); 
			docButton.TooltipText = documentation?.ButtonTooltip;
		}

		public override void Destroy()
		{
			TabWidget.Destroy();
			titleLabel.Destroy();
			
			if(Tab is ITdiTabWithPath tdiTabWithPath)
			{
				tdiTabWithPath.PathChanged -= OnPathUpdated;
			}
			else
			{
				Tab.TabNameChanged -= Tab_TabNameChanged;
			}
			base.Destroy();
		}
	}
}

