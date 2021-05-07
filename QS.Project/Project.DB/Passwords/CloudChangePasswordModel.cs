using System.Security;

namespace QS.Project.DB.Passwords
{
    public class CloudChangePasswordModel : IChangePasswordModel
    {
        public bool PasswordWasChanged { get; }
        public SecureString NewPassword { get; }
        public void ChangePassword(SecureString newPassword)
        {
            throw new System.NotImplementedException();
        }

        public bool IsCurrentUserPassword(SecureString password)
        {
            throw new System.NotImplementedException();
        }
    }
}
