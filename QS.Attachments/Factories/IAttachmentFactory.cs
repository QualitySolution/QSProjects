using QS.Attachments.Domain;
using QS.Attachments.ViewModels.Widgets;

namespace QS.Attachments.Factories
{
	public interface IAttachmentFactory
	{
		Attachment CreateNewAttachment(string attachedFileName, byte[] file);
	}
}