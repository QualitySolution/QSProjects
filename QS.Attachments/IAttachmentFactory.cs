namespace QS.Attachments
{
	public interface IAttachmentFactory
	{
		IAttachment CreateNewAttachment(string attachedFileName, byte[] file);
	}
}