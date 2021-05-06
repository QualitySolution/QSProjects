using System.Data.Common;
using QS.DomainModel.UoW;

namespace QS.Project.DB.Passwords
{
    public interface IDatabasePasswordModel
    {
        void ChangePassword(DbConnection connection, string newPassword);
        void ChangePassword(IUnitOfWork uow, string newPassword);
        
        bool IsCurrentUserPassword(DbConnection connection, string password);
        bool IsCurrentUserPassword(IUnitOfWork uow, string password);
    }
}
