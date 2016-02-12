using System;
using System.Collections.Generic;

namespace QSOrmProject.DomainMapping
{
	public class GenericSearchProvider<TEntity> : ISearchProvider
	{
		private readonly List<Func<TEntity, string>> searchBy = new List<Func<TEntity, string>>();

		#region ISearchProvider implementation

		public bool Match(object entity, string searchText)
		{
			if (String.IsNullOrWhiteSpace(searchText))
				return true;
			
			TEntity item = (TEntity)entity;
			foreach(var field in searchBy)
			{
				var str = field(item);
				if (String.IsNullOrEmpty(str))
					continue;
				if (str.IndexOf (searchText, StringComparison.CurrentCultureIgnoreCase) > -1) 
					return true;
			}
			return false;
		}

		#endregion

		public GenericSearchProvider()
		{
		}

		public void AddSearchByFunc(Func<TEntity, string> func)
		{
			searchBy.Add(func);
		}
	}
}

