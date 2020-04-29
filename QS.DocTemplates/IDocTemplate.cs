using System.ComponentModel;

namespace QS.DocTemplates
{
	public interface IDocTemplate: INotifyPropertyChanged
	{
		string Name { get;}
		byte[] File { get;}
		byte[] TempalteFile { get; set;}
		byte[] ChangedDocFile { get; set;}
		IDocParser DocParser { get;}
	}
}

