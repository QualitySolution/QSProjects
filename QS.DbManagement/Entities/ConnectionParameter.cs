namespace QS.DbManagement.Entities
{
	public class ConnectionParameter
	{
		public ConnectionParameter(string name, string title) {
			Name = name;
			Title = title;
		}

		public string Name { get; set; }
		public string Title { get; set; }
	}
}
