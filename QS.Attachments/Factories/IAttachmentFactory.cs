using QS.Attachments.Domain;

namespace QS.Attachments.Factories
{
	public interface IAttachmentFactory
	{
		Attachment CreateNewAttachment(string attachedFileName, byte[] file);
	}
}