using System;

namespace QS.Tdi {
	public interface ICustomCancellationConfirmation {
		Func<int> CustomCancellationConfirmationDialogFunc { get; }
		bool HasCustomCancellationConfirmationDialog { get; }
	}
}