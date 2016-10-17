using System;
using QSProjectsLib;
using Gamma.Binding.Core;
using System.Linq.Expressions;

namespace QSDocTemplates
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TemplateWidget : Gtk.Bin
	{
		public BindingControler<TemplateWidget> Binding { get; private set;}

		private FileWorker worker = new FileWorker();

		public bool FileChanged { get; private set;}

		IDocTemplate template;
		public IDocTemplate Template
		{
			get
			{
				return template;
			}
			set
			{
				if (template != null)
					template.PropertyChanged -= Template_PropertyChanged;

				template = value;
				if(template != null)
					template.PropertyChanged += Template_PropertyChanged;
				UpdateState();
				UpdateSize();
			}
		}

		void Template_PropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
		}
			
		public byte[] ChangedDoc
		{
			get
			{
				return Template?.ChangedDocFile;
			}
			set
			{
				if (Template == null)
					return;
				Template.ChangedDocFile = value;
				UpdateSize();
				UpdateState();
			}
		}

		public TemplateWidget()
		{
			this.Build();

			Binding = new BindingControler<TemplateWidget> (this, new Expression<Func<TemplateWidget, object>>[] {
				(w => w.Template),
				(w => w.ChangedDoc),
			});

			worker.FileUpdated += Worker_FileUpdated;
		}

		void Worker_FileUpdated (object sender, FileUpdatedEventArgs e)
		{
			Binding.FireChange(x => x.ChangedDoc);
			FileChanged = true;
			UpdateState();
			UpdateSize();
		}

		void UpdateState()
		{
			if (Template == null)
			{
				labelStatus.Markup = "<span foreground=\"red\">Шаблон не определен!</span>";
				buttonEdit.Sensitive = buttonPrint.Sensitive = buttonRevertCommon.Sensitive = false;
			}
			else if (Template.DocParser == null)
			{
				labelStatus.Markup = "<span foreground=\"red\">Парсер не задан!</span>";
				buttonEdit.Sensitive = buttonPrint.Sensitive = buttonRevertCommon.Sensitive = false;
			}
			else if(ChangedDoc != null)
			{
				if(FileChanged)
					labelStatus.Markup = "<span foreground=\"blue\">Собственный документ</span> <span foreground=\"green\">(изменён)</span>";
				else
					labelStatus.Markup = "<span foreground=\"blue\">Собственный документ</span>";
				buttonEdit.Sensitive = buttonPrint.Sensitive = buttonRevertCommon.Sensitive = true;
			}
			else
			{
				labelStatus.Markup = "<span foreground=\"green\">Общий шаблон</span>";
				buttonEdit.Sensitive = buttonPrint.Sensitive = true;
				buttonRevertCommon.Sensitive = false;
			}
		}

		void UpdateSize()
		{
			ylabelSize.LabelProp = String.Empty;
			if(ChangedDoc != null)
			{
				ylabelSize.LabelProp = StringWorks.BytesToIECUnitsString((uint)ChangedDoc.LongLength);
			}
			else if(Template != null)
			{
				ylabelSize.LabelProp = StringWorks.BytesToIECUnitsString((uint)Template.File.LongLength);
			}
		}

		protected void OnButtonPrintClicked(object sender, EventArgs e)
		{
			worker.OpenInOffice(Template, true, FileEditMode.Document, true);
		}

		protected void OnButtonEditClicked(object sender, EventArgs e)
		{
			worker.OpenInOffice(Template, false, FileEditMode.Document);
		}

		protected void OnButtonRevertCommonClicked(object sender, EventArgs e)
		{
			ChangedDoc = null;
		}

		protected void OnButtonOpenClicked (object sender, EventArgs e)
		{
			worker.OpenInOffice(Template, true, FileEditMode.Document);
		}
	}
}

