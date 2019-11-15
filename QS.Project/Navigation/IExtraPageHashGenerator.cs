using System;
namespace QS.Navigation
{
	/// <summary>
	/// Интерфейс для дополнительных генераторов хешей, логика которых находится во внешних библиотеках
	/// и является опциональной для проектов.
	/// </summary>
	public interface IExtraPageHashGenerator
	{
		string GetHash(Type typeViewModel, object[] ctorValues);
	}
}
