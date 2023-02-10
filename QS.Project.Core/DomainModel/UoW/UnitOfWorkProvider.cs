namespace QS.DomainModel.UoW {
	/// <summary>
	/// Класс предназначен для отложенной передачи всем заинтересованным в UnitOfWork классам,
	/// таким как репозитории и модели, текущего экземпляра UnitOfWork.
	/// Предполагается что данный класс создается по экземпляру на скоуп и хранит для каждого скойпа свой Uow.
	/// </summary>
	public class UnitOfWorkProvider {
		public UnitOfWorkProvider(IUnitOfWork uoW = null) {
			UoW = uoW;
		}

		public IUnitOfWork UoW { get; set; }
	}
}
