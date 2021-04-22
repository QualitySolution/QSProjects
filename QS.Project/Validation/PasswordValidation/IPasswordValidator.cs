using System.Collections.Generic;

namespace QS.Validation
{
    public interface IPasswordValidator
    {
        bool Validate(string password, out IList<string> errorMessages);

        IPasswordValidationSettings Settings { get; }
    }
}
