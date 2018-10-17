
namespace QS.Print
{
	public interface IPrintableDocument
	{
		PrinterType PrintType { get; }
		DocumentOrientation Orientation { get; }
		int CopiesToPrint { get; set; }
		string Name { get; }
	}

	public enum PrinterType
	{
		None, RDL, ODT
	}

	public enum DocumentOrientation
	{
		Portrait, Landscape
	}
}