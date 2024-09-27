using System;
using System.Collections.Generic;
using System.Text;

namespace QS.DbManagement.Responces
{
	public class Responce {
		public bool Success { get; set; }
		public string ErrorMessage { get; set; }
	}

    public class LoginToServerResponce : Responce
    {
		public bool IsAdmin { get; set; }
		public bool NeedToUpdateLauncher { get; set; }
    }

	public class ChangePasswordResponce : Responce { }

	public class CreateDatabaseResponce : Responce { }

	public class DropDatabaseResponce : Responce { }

	public class AddUserResponce : Responce { }

	public class DeleteUserResponce : Responce { }

	public class GetUserDatabasesResponce : Responce {
		public List<DbInfo> Bases { get; set; }
	}

	public class LoginToDatabaseResponce : Responce {
		public string ConnectionString { get; set; }

		public List<ConnectionParameter> Parameters { get; set; }
	}
}
