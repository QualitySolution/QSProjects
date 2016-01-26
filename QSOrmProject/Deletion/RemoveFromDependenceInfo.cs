using System;
using System.Linq.Expressions;
using Gamma.Utilities;
using System.Reflection;

namespace QSOrmProject.Deletion
{

	public class RemoveFromDependenceInfo
	{
		public Type ObjectClass;

        public string CollectionName;
		public string RemoveMethodName;

		public RemoveFromDependenceInfo(Type objectClass, string collectionName, string removeMethodName) : this(objectClass, collectionName)
		{
			RemoveMethodName = removeMethodName;
		}

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
			return new RemoveFromDependenceInfo(
				typeof(TObject),
				collectionName
			);
		}

		public static RemoveFromDependenceInfo CreateFromBag<TObject, TRemove> (Expression<Func<TObject, object>> propertyRefExpr, Expression<Func<TObject, Action<TRemove>>> removeFunction ){
			string collectionName = PropertyUtil.GetName (propertyRefExpr);

			MethodInfo method = null;

			UnaryExpression convertExpr = removeFunction.Body as UnaryExpression;
			if (convertExpr != null && convertExpr.NodeType == ExpressionType.Convert)
			{
				var createDelegatExpr = convertExpr.Operand as MethodCallExpression;
				if(createDelegatExpr != null && createDelegatExpr.NodeType == ExpressionType.Call && createDelegatExpr.Type == typeof(Delegate))
				{
					var parameterExpr = createDelegatExpr.Arguments [2] as ConstantExpression;
					if (parameterExpr != null)
						method = parameterExpr.Value as MethodInfo;
				}
			}

			if (method == null)
				throw new InvalidCastException ();

			return new RemoveFromDependenceInfo(
				typeof(TObject),
				collectionName,
				method.Name
			);
		}

	}

}
