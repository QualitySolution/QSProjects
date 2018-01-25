using System.ComponentModel.DataAnnotations;
using DiffPlex;
using DiffPlex.DiffBuilder;
using Gamma.Utilities;
using QSHistoryLog.Domain;

namespace QSHistoryLog
{
	public class FieldChange
	{
		public virtual int Id { get; set; }

		public virtual HistoryChangeSet ChangeSet { get; set; }

		public virtual string Path { get; set; }
		public virtual FieldChangeType Type { get; set; }
		public virtual string OldValue { get; set; }
		public virtual string NewValue { get; set; }
		public virtual int? OldId { get; set; }
		public virtual int? NewId { get; set; }

		string oldPangoText;
		public virtual string OldPangoText {
			get { if (!isPangoMade)
					MakeDiffPangoMarkup ();
				return oldPangoText;
			}
			protected set {
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
			protected set {
				newPangoText = value;
			}
		}

		private bool isPangoMade = false;

		public virtual string FieldName
		{
			get { return HistoryMain.ResolveFieldNameFromPath (Path);}
		}

		public virtual string TypeText
		{
			get { 
					return Type.GetEnumTitle ();
				}
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

	public enum FieldChangeType
	{
		[Display(Name = "Добавлено")]
		Added,
		[Display(Name = "Изменено")]
		Changed,
		[Display(Name = "Удалено")]
		Removed,
		[Display(Name = "Без изменений")]
		Unchanged
	}

	public class FieldChangeTypeStringType : NHibernate.Type.EnumStringType
	{
		public FieldChangeTypeStringType() : base(typeof(FieldChangeType))
		{
		}
	}
}

