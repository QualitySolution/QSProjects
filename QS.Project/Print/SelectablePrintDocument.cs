using QS.DomainModel.Entity;

namespace QS.Print
{
	public class SelectablePrintDocument : PropertyChangedBase
	{
		private bool selected;
		public virtual bool Selected {
			get => selected;
			set => SetField(ref selected, value, () => Selected);
		}

		public IPrintableDocument Document { get; set; }

		private int copies;
		public int Copies {
			get => copies;
			set => SetField(ref copies, value < 1 ? 1 : value, () => Copies);
		}

		public SelectablePrintDocument(IPrintableDocument document)
		{
			Document = document;
			Copies = document.CopiesToPrint;
		}
	}
}