using QS.Dialog;
namespace QS.Services
{
	public interface IInteractiveService
	{
		IInteractiveMessage InteractiveMessage { get; }
		IInteractiveQuestion InteractiveQuestion { get; }
	}
}