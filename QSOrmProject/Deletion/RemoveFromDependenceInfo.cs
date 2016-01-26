using System;
using System.Linq;
using System.Linq.Expressions;
using Gamma.Utilities;
using NHibernate.Mapping;

namespace QSOrmProject.Deletion
{

	public class RemoveFromDependenceInfo
	{
		public Type ObjectClass;

        public string CollectionName;

		public RemoveFromDependenceInfo(Type objectClass, string collectionName)
		{
			ObjectClass = objectClass;
			CollectionName = collectionName;
		}

		public IDeleteInfo GetClassInfo()
		{
			return DeleteConfig.ClassInfos.Find (i => i.ObjectClass == ObjectClass);
		}

		/// <summary>
		/// Создает класс зависимости удаления на основе свойства bag в родителе. 
		/// </summary>
		/// <param name="propertyRefExpr">Лямда функция указывающая на свойство, пример (e => e.Name)</param>
		/// <typeparam name="TObject">Тип объекта доменной модели</typeparam>
		public static RemoveFromDependenceInfo CreateFromBag<TObject> (Expression<Func<TObject, object>> propertyRefExpr){
			string collectionName = PropertyUtil.GetName (propertyRefExpr);
			//var collectionMap = OrmMain.OrmConfig.GetClassMapping (typeof(TObject)).GetProperty (collectionName).Value as Bag;
			//Type itemType = (collectionMap.Element as ManyToOne).Type.ReturnedClass;
			return new RemoveFromDependenceInfo(
				typeof(TObject),
				collectionName
			);
		}

	}

}
