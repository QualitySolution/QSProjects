namespace QS.Validation {
	public interface IQSValidatorFactory<T> where T : class {
		IQSValidator<T> CreateForInstance(T instance);
	}
}
