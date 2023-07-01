using QS.Project.Domain;
namespace QS.Services
{
	public interface IUserService
	{
		int CurrentUserId { get; }
		UserBase GetCurrentUser();
	}
}
