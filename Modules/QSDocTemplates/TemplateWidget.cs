using System;
using QSProjectsLib;
using Gamma.Binding.Core;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace QSDocTemplates
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TemplateWidget : Gtk.Bin
	{
		public BindingControler<TemplateWidget> Binding { get; private set; }

		private FileWorker worker = new FileWorker();

		public bool FileChanged { get; private set; }

		public event EventHandler BeforeOpen;

		public bool CanOpenDocument = true;

		private bool canRevertCommon = true;

		public bool CanRevertCommon { 
			get => canRevertCommon; 
			set => canRevertCommon = value; 
		}


		IDocTemplate template;
		public IDocTemplate Template {
			get {
				return template;
			}
			set {
				if(template != null)
					template.PropertyChanged -= Template_PropertyChanged;

				template = value;
				if(template != null) {
					template.PropertyChanged += Template_PropertyChanged;
				}
				if(comboTemplates.SelectedItem != template)
					UpdateTemplatesCombo();
				UpdateState();
				UpdateSize();
				Binding.FireChange(x => x.Template);
			}
		}

		IList<IDocTemplate> availableTemplates;

		public IList<IDocTemplate> AvailableTemplates {
			get {
				return availableTemplates;
			}
			set {
				availableTemplates = value;
				UpdateTemplatesCombo();
			}
		}

		void Template_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
		}

		public byte[] ChangedDoc {
			get {
				return Template?.ChangedDocFile;
			}
			set {
				if(Template == null)
					return;
				Template.ChangedDocFile = value;
				UpdateSize();
				UpdateState();
			}
		}

		public TemplateWidget()
		{
			this.Build();

			Binding = new BindingControler<TemplateWidget>(this, new Expression<Func<TemplateWidget, object>>[] {
				(w => w.Template),
				(w => w.ChangedDoc),
			});

			comboTemplates.SetRenderTextFunc<IDocTemplate>(x => x.Name);

			worker.FileUpdated += Worker_FileUpdated;
			UpdateState();
			UpdateSize();

			CanOpenDocument = true;
		}

		void Worker_FileUpdated(object sender, FileUpdatedEventArgs e)
		{
			Binding.FireChange(x => x.ChangedDoc);
			FileChanged = true;
			UpdateState();
			UpdateSize();
		}

		void UpdateTemplatesCombo()
		{
			var list = AvailableTemplates != null ? new List<IDocTemplate>(AvailableTemplates) : new List<IDocTemplate>();
			comboTemplates.ItemsList = list;
			comboTemplates.Visible = Template != null || list.Count > 0;
			if(Template != null)
				comboTemplates.SelectedItem = Template;
		}

		void UpdateState()
		{
			if(Template == null) {
				labelStatus.Markup = "<span foreground=\"red\">Шаблон не определен!</span>";
				buttonEdit.Sensitive = buttonPrint.Sensitive =
					buttonRevertCommon.Sensitive = buttonOpen.Sensitive = false;
			} else if(Template.DocParser == null) {
				labelStatus.Markup = "<span foreground=\"red\">Парсер не задан!</span>";
				buttonEdit.Sensitive = buttonPrint.Sensitive =
					buttonRevertCommon.Sensitive = buttonOpen.Sensitive = false;
			} else if(ChangedDoc != null) {
				if(FileChanged)
					labelStatus.Markup = "<span foreground=\"blue\">Собственный документ</span> <span foreground=\"green\">(изменён)</span>";
				else
					labelStatus.Markup = "<span foreground=\"blue\">Собственный документ</span>";
				buttonEdit.Sensitive = buttonPrint.Sensitive = buttonOpen.Sensitive = true;
				buttonRevertCommon.Sensitive = CanRevertCommon;
			} else {
				labelStatus.Markup = "<span foreground=\"green\">Общий шаблон</span>";
				buttonEdit.Sensitive = buttonPrint.Sensitive = buttonOpen.Sensitive = true;
				buttonRevertCommon.Sensitive = false;
			}
		}

		void UpdateSize()
		{
			ylabelSize.LabelProp = String.Empty;
			if(ChangedDoc != null) {
				ylabelSize.LabelProp = StringWorks.BytesToIECUnitsString((uint)ChangedDoc.LongLength);
			} else if(Template != null) {
				ylabelSize.LabelProp = StringWorks.BytesToIECUnitsString((uint)Template.File.LongLength);
			}
		}

		protected void OnButtonPrintClicked(object sender, EventArgs e)
		{
			BeforeOpen?.Invoke(this, EventArgs.Empty);
			if(CanOpenDocument) {
				worker.OpenInOffice(Template, true, FileEditMode.Document, true);
			}
			CanOpenDocument = true;
		}

		protected void OnButtonEditClicked(object sender, EventArgs e)
		{
			BeforeOpen?.Invoke(this, EventArgs.Empty);
			if(CanOpenDocument) {
				worker.OpenInOffice(Template, false, FileEditMode.Document);
			}
			CanOpenDocument = true;
		}

		protected void OnButtonRevertCommonClicked(object sender, EventArgs e)
		{
			ChangedDoc = null;
		}

		protected void OnButtonOpenClicked(object sender, EventArgs e)
		{
			BeforeOpen?.Invoke(this, EventArgs.Empty);
			if(CanOpenDocument) {
				worker.OpenInOffice(Template, true, FileEditMode.Document);
			}
			CanOpenDocument = true;
		}

		protected void OnComboTemplatesChanged(object sender, EventArgs e)
		{
			if(Template != comboTemplates.SelectedItem)
				Template = (IDocTemplate)comboTemplates.SelectedItem;
		}
	}
}