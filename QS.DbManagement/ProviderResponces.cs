using System.Collections.Generic;

namespace QS.DbManagement.Responces
{
	public class Response {
		public bool Success { get; set; }
		public string ErrorMessage { get; set; }
	}

    public class LoginToServerResponse : Response
    {
		public bool IsAdmin { get; set; }
		public bool NeedToUpdateLauncher { get; set; }
    }

	public class ChangePasswordResponse : Response { }

	public class CreateDatabaseResponse : Response { }

	public class DropDatabaseResponse : Response { }

	public class AddUserResponse : Response { }

	public class DeleteUserResponse : Response { }

	public class GetUserDatabasesResponse : Response {
		public List<DbInfo> Bases { get; set; }
	}

	public class LoginToDatabaseResponse : Response {
		public string ConnectionString { get; set; }
		public string Login { get; set; }

		public Dictionary<string,string> Parameters { get; set; }
	}
}
