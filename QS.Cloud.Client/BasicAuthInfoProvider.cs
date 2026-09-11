namespace QS.Cloud.Client
{
	public class BasicAuthInfoProvider : IBasicAuthInfoProvider
	{
		public BasicAuthInfoProvider(string userName, string password)
		{
			UserName = userName; Password = password;
		}

		public string UserName { get; }

		public string Password { get; private set; }

		public void UpdatePassword(string newPassword) => Password = newPassword;
	}

	public interface IBasicAuthInfoProvider
	{
		string UserName { get; }

		string Password { get; }

		void UpdatePassword(string newPassword);
	}
}
