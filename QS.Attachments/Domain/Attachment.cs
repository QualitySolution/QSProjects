using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace QS.Attachments.Domain
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "прикрепляемые файлы",
		Nominative = "прикрепляемый файл"
	)]
	public class Attachment : PropertyChangedBase, IDomainObject
	{
		private string _fileName;
		private byte[] _byteFile;

		public virtual int Id { get; set; }
		
		[Display(Name = "Имя файла")]
		public virtual string FileName
		{
			get => _fileName;
			set => SetField(ref _fileName, value);
		}
		
		[Display(Name = "Файл")]
		public virtual byte[] ByteFile
		{
			get => _byteFile;
			set => SetField(ref _byteFile, value);
		}

		public virtual string Title => FileName;
	}
}