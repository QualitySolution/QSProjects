using System;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Gamma.Binding.Core;
using Gamma.GtkWidgets;
using Gtk;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using QS.Project.Dialogs.Gtk;

namespace QS.Widgets.Gtk
{
	/// <summary>
	/// Виджет автоматически дополняет ввод ранее введенными в поле данными.
	/// Данные запрашиваются из базы на сосновании биндинга на свойство.
	/// Для корректной работы свойство с данными должно быть забинджено первым.
	/// </summary>
	[ToolboxItem(true)]
	public class FieldCompletionEntry : yEntry
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private ListStore completionListStore;

		public FieldCompletionEntry()
		{

			this.Completion = new EntryCompletion();
			this.Completion.MinimumKeyLength = 0;
			this.Completion.MatchSelected += Completion_MatchSelected;
			this.Completion.MatchFunc = Completion_MatchFunc;
			var cell = new CellRendererText();
			this.Completion.PackStart(cell, true);
			this.Completion.SetCellDataFunc(cell, OnCellLayoutDataFunc);
		}

		//Костыль, для отображения выпадающего списка
		protected override bool OnKeyPressEvent(Gdk.EventKey evnt)
		{
			if(evnt.Key == Gdk.Key.Control_R)
				this.InsertText("");

			return base.OnKeyPressEvent(evnt);
		}

		void OnCellLayoutDataFunc(CellLayout cell_layout, CellRenderer cell, TreeModel tree_model, TreeIter iter)
		{
			var value = (string)tree_model.GetValue(iter, 0);
			string pattern = String.Format("\\b{0}", Regex.Escape(Text.ToLower()));
			value = Regex.Replace(value, pattern, (match) => String.Format("<b>{0}</b>", match.Value), RegexOptions.IgnoreCase);
			(cell as CellRendererText).Markup = value;
		}

		bool Completion_MatchFunc(EntryCompletion completion, string key, TreeIter iter)
		{
			var val = completion.Model.GetValue(iter, 0).ToString().ToLower();
			return Regex.IsMatch(val, String.Format("\\b{0}.*", Regex.Escape(this.Text.ToLower())));
		}

		[GLib.ConnectBefore]
		void Completion_MatchSelected(object o, MatchSelectedArgs args)
		{
			Text = (string)args.Model.GetValue(args.Iter, 0);
			args.RetVal = true;
		}

		private void fillAutocomplete()
		{
			logger.Info("Запрос данных для автодополнения...");
			completionListStore = new ListStore(typeof(string));

			IUnitOfWork localUoW;

			var dlg = DialogHelper.FindParentDialog(this);

			if(dlg != null)
				localUoW = dlg.UoW;
			else
				localUoW = UnitOfWorkFactory.CreateWithoutRoot();

			var bindSource = Binding.BindedSources.FirstOrDefault();
			if(bindSource == null)
				throw new InvalidOperationException($"У виджета {this.Name} не добавлен ни один источник для биндинга через {this.Name}.Binding");

			var bridge = bindSource.BindedBridges.OfType<IPropertyBindingBridge>().FirstOrDefault();
			if(bridge == null)
				throw new InvalidOperationException($"В виджете {this.Name} не добавлено ни одного биндинга на свойство.");

			var clazz = NHibernate.Proxy.NHibernateProxyHelper.GuessClass(bindSource.DataSourceObject);

			var r = localUoW.Session
				.CreateCriteria(clazz)
				.SetProjection(Projections.Distinct(Projections.Property(bridge.SourcePropertyName)))
				.List<string>();

			foreach(var item in r) {
				if(String.IsNullOrWhiteSpace(item))
					continue;
				completionListStore.AppendValues(item);
			}

			Completion.Model = completionListStore;
			logger.Debug("Получено {0} строк автодополения...", completionListStore.IterNChildren());
			if(this.HasFocus)
				this.Completion.Complete();
		}

		protected override bool OnFocusInEvent(Gdk.EventFocus evnt)
		{
			if(completionListStore == null)
				fillAutocomplete();
			return base.OnFocusInEvent(evnt);
		}
	}
}
