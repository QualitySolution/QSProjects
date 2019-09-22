using System.ComponentModel.DataAnnotations;
using QS.Validation;

namespace QS.Services
{
	public interface IValidationService
	{
		IValidator GetValidator();
	}
}