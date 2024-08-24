namespace QS.DbManagement
{
	public class ConnectionParameter
	{
		public ConnectionParameter(string title, object value = null)
		{
			Title = title;
			Value = value;
		}

		public string Title { get; set; }

		public object Value { get; set; }
	}
}
