using System;

namespace QS.Tdi
{
	public delegate void HandleSwitchIn (ITdiTab tabFrom);
	public delegate void HandleSwitchOut (ITdiTab tabTo);

	public interface ITdiTab
	{
		HandleSwitchIn HandleSwitchIn { get; }
		HandleSwitchOut HandleSwitchOut { get; }
		string TabName { get; }
		ITdiTabParent TabParent { set; get; }
		event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;

		/// <summary>
		/// Событие вызывается после закрытия вкладки. Необходимо для того чтобы код открывший вкладку или имеющий на нее ссылку, смог ее освободить или предпринять какие до действия.
		/// </summary>
		event EventHandler TabClosed;

		/// <summary>
		/// Внимание! этот метод нужен в первую очередь для генерации события TabClosed и должен вызываться только родителем вкладки. Не вызывайте его из клиентского кода.
		/// Внутри вкладки его можно использоваться для закрытия ресурсов.
		/// </summary>
		void OnTabClosed();

		bool FailInitialize { get;}

		bool CompareHashName(string hashName);
	}

	public class TdiTabNameChangedEventArgs : EventArgs
	{
		public string NewName { get; private set; }

		public TdiTabNameChangedEventArgs(string newName)
		{
			NewName = newName;
		}
	}
}