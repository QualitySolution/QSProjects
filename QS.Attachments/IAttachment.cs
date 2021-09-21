using QS.DomainModel.Entity;

namespace QS.Attachments
{
	public interface IAttachment : IDomainObject
	{
		string FileName { get; set; }
		byte[] ByteFile { get; set; }
		string PartOfPath { get; }
	}
}