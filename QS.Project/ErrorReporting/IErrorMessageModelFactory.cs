using System;
namespace QS.ErrorReporting
{
	public interface IErrorMessageModelFactory
	{
		ErrorMessageModelBase GetModel();
	}
}
