using QS.Attachments.Domain;

namespace QS.Attachments.Factories
{
	public class AttachmentFactory : IAttachmentFactory
	{
		public Attachment CreateNewAttachment(string attachedFileName, byte[] file)
		{
			return new Attachment
			{
				FileName = attachedFileName,
				ByteFile = file
			};
		}
	}
}
