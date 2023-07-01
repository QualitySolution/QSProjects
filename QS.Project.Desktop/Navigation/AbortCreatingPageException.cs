using System;
namespace QS.Navigation
{
	public class AbortCreatingPageException : Exception
	{
		public string Title;

		public AbortCreatingPageException(string userMessage, string title) : base(userMessage)
		{
			Title = title;
		}
	}
}
