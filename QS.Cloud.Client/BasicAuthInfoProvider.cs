namespace QS.Cloud.Client
{
	public class BasicAuthInfoProvider : IBasicAuthInfoProvider
	{
		public BasicAuthInfoProvider(string userName, string password)
		{
			UserName = userName; Password = password;
		}

		public string UserName { get; }

		public string Password { get; }
	}

	public interface IBasicAuthInfoProvider
	{
		string UserName { get; }

		string Password { get; }
	}
}
