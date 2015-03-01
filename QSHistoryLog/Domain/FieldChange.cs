using System;
using SIT.Components.ObjectComparer;
using DiffPlex;
using DiffPlex.DiffBuilder;
using System.Text;

namespace QSHistoryLog
{
	public class FieldChange
	{
		public virtual int Id { get; set; }

		public virtual string Path { get; set; }
		public virtual ChangeType Type { get; set; }
		public virtual string OldValue { get; set; }
		public virtual string NewValue { get; set; }

		string oldPangoText;
		public virtual string OldPangoText {
			get { if (!isPangoMade)
					MakeDiffPangoMarkup ();
				return oldPangoText;
			}
			private set {
				oldPangoText = value;
			}
		}

		string newPangoText;
		public virtual string NewPangoText {
			get {
				if (!isPangoMade)
					MakeDiffPangoMarkup ();
				return newPangoText;
			}
			private set {
				newPangoText = value;
			}
		}

		private bool isPangoMade = false;

		public string FieldName
		{
			get { return HistoryMain.ResolveFieldNameFromPath (Path);}
		}

		public string TypeText
		{
			get {
				switch (Type) {
				case ChangeType.Added:
					return "Добавлено";
				case ChangeType.Changed:
					return "Изменено";
				case ChangeType.Removed:
					return "Удалено";
				case ChangeType.Unchanged:
					return "Без изменений";
				default:
					return String.Empty;
				}}
		}

		public FieldChange ()
		{
		}

		private void MakeDiffPangoMarkup()
		{
			var d = new Differ ();
			var differ = new SideBySideFullDiffBuilder(d);
			var diffRes = differ.BuildDiffModel(OldValue, NewValue);
			OldPangoText = PangoRender.RenderDiffLines (diffRes.OldText);
			NewPangoText = PangoRender.RenderDiffLines (diffRes.NewText);
			isPangoMade = true;
		}

	}
}

