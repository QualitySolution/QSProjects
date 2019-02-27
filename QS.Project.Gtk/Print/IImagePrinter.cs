using Gtk;

namespace QS.Print
{
	public interface IImagePrinter
	{
		void Print(IPrintableDocument[] documents, PrintSettings printSettings = null);
	}
}
