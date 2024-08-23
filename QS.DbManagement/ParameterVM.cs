namespace QS.DbManagement
{
	public class ParameterVM
	{
		public ParameterVM(string title, object value = null)
		{
			Title = title;
			Value = value;
		}

		public string Title { get; set; }

		public object Value { get; set; }
	}
}
