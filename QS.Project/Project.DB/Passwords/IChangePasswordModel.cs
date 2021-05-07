using System.Security;

namespace QS.Project.DB.Passwords
{
    public interface IChangePasswordModel
    {
        bool PasswordWasChanged { get; }
        SecureString NewPassword { get; }
        
        void ChangePassword(SecureString newPassword);
        bool IsCurrentUserPassword(SecureString password);
    }
}
