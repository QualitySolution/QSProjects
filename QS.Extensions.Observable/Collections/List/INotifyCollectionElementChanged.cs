using System.ComponentModel;

namespace QS.Extensions.Observable.Collections.List {

	/// <summary>
	/// Notify then property of collection element changed
	/// </summary>
	public interface INotifyCollectionElementChanged 
	{
		event PropertyChangedEventHandler PropertyOfElementChanged;
	}
}
