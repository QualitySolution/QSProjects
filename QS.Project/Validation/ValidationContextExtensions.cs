using System.ComponentModel.DataAnnotations;

namespace QS.Validation
{
	public static class ValidationContextExtensions
	{
		public static T GetService<T>(this ValidationContext context) where T : class
		{
			return (T)context.GetService(typeof(T));
		}
	}
}
