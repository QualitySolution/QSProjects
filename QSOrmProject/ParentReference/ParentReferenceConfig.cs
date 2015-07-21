using System;
using System.Collections.Generic;
using System.Linq;

namespace QSOrmProject
{
	public static class ParentReferenceConfig
	{
		private static List<IParentReferenceActions> actionsList = new List<IParentReferenceActions>();

		public static void AddActions(IParentReferenceActions actions)
		{
			actionsList.Add (actions);
		}

		public static ParentReferenceActions<TParentEntity, TChildEntity> FindDefaultActions<TParentEntity, TChildEntity>()
		{
			return actionsList.OfType<ParentReferenceActions<TParentEntity, TChildEntity>> ().FirstOrDefault ();
		}
	}

	public class ParentReferenceActions<TParentEntity, TChildEntity> : IParentReferenceActions
	{
		#region IParentReferenceActions implementation

		public Type Parent {
			get { return typeof(TParentEntity);
			}
		}

		public Type Child {
			get { return typeof(TChildEntity);
			}
		}

		#endregion

		public Action<TParentEntity, TChildEntity> AddNewChild { get; set;}
	}

	public interface IParentReferenceActions
	{
		Type Parent { get;}
		Type Child { get;}
	}
}

