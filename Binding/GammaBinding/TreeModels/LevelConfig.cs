using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Collections;

namespace Gamma.Binding
{
	public class LevelConfig<TNode, TParentNode, TChildNode>: ILevelLinkUp<TNode, TParentNode>, ILevelLinkDown<TNode, TChildNode>, ILevelConfig
		where TNode: class
		where TParentNode: class
		where TChildNode: class
	{
		public ILevelLinkDown<TParentNode, TNode> ParentLevel { get; private set;}
		public ILevelLinkUp<TChildNode, TNode> ChildLevel { get; private set;}

		public Func<TNode, TParentNode> ParentFunc;
		public Func<TNode, IList<TChildNode>> ChildsFunc;

		public bool IsFirstLevel { get; private set;}
		public bool IsLastLevel { get; private set;}

		public ILevelConfig LevelUp{
			get{
				return (ILevelConfig)ParentLevel;
			}
		}

		public LevelConfig(ILevelLinkDown<TParentNode, TNode> parentLevel, Expression<Func<TNode, TParentNode>> parentPropertyExpr, Expression<Func<TNode, IList<TChildNode>>> childsCollectionPropertyExpr)
		{
			ParentLevel = parentLevel;

			if (parentPropertyExpr == null)
				IsFirstLevel = true;
			else
			{
				ParentFunc = parentPropertyExpr.Compile();
			}

			if (childsCollectionPropertyExpr == null)
				IsLastLevel = true;
			else
			{
				ChildsFunc = childsCollectionPropertyExpr.Compile();
			}
		}

		public Type NodeType
		{
			get
			{
				return typeof(TNode);
			}
		}

		public object GetParent(object node)
		{
			var typedNode = node as TNode;
			if (IsFirstLevel || typedNode == null)
				return null;

			return ParentFunc(typedNode);
		}

		public int IndexOnParent(object node)
		{
			var typedNode = node as TNode;
			if (IsFirstLevel || typedNode == null)
				return -1;

			var list = MyList(typedNode);

			return list.IndexOf(typedNode);
		}

		public IList<TNode> MyList(TNode node)
		{
			var parent = GetParent(node);
			return ParentLevel.GetChilds(parent);
		}

		public IList MyList(object node)
		{
			return (IList)MyList((TNode)node);
		}

		public TParentNode GetParent(TNode node)
		{
			if (IsFirstLevel || ParentFunc == null)
				return null;
			return ParentFunc(node);
		}

		public IList<TChildNode> GetChilds(TNode node)
		{
			if (IsLastLevel || ChildsFunc == null)
				return null;
			return ChildsFunc(node);
		}

		public IList GetChilds(object node)
		{
			return (IList)GetChilds((TNode)node);
		}

		#region Fluent

		public LevelConfig<TChildNode, TNode, TNextChildNode> NextLevel<TNextChildNode>(Expression<Func<TChildNode, TNode>> parentPropertyExpr, Expression<Func<TChildNode, IList<TNextChildNode>>> childsCollectionPropertyExpr)
			where TNextChildNode: class
		{
			var nextLevel = new LevelConfig<TChildNode, TNode, TNextChildNode>(this, parentPropertyExpr, childsCollectionPropertyExpr);
			ChildLevel = nextLevel;
			return nextLevel;
		}

		public LevelConfig<TChildNode, TNode, TypeNotNeed> LastLevel(Expression<Func<TChildNode, TNode>> parentPropertyExpr)
		{
			var nextLevel = new LevelConfig<TChildNode, TNode, TypeNotNeed>(this, parentPropertyExpr, null);
			ChildLevel = nextLevel;
			return nextLevel;
		}

		public ILevelConfig[] EndConfig()
		{
			var list = new List<ILevelConfig>();
			ILevelConfig curLevel = this;
			while(curLevel != null)
			{
				list.Add(curLevel);
				curLevel = curLevel.LevelUp;
			}
			list.Reverse();
			return list.ToArray();
		}

		#endregion

	}

	public static class LevelConfigFactory{
		public static LevelConfig<TNextNode, TypeNotNeed, TNextChildNode> FirstLevel<TNextNode, TNextChildNode>(Expression<Func<TNextNode, IList<TNextChildNode>>> childsCollectionPropertyExpr)
			where TNextChildNode: class
			where TNextNode: class
		{
			return new LevelConfig<TNextNode, TypeNotNeed, TNextChildNode>(null, null, childsCollectionPropertyExpr);
		}
	}

	public interface ILevelLinkUp<TNode, TUp>
	{
		
	}

	public interface ILevelLinkDown<TNode, TDown>
	{
		IList<TDown> GetChilds(TNode node);
	}

	public interface ILevelConfig
	{
		ILevelConfig LevelUp { get;}

		Type NodeType { get;}

		bool IsFirstLevel { get;}
		bool IsLastLevel { get; }

		object GetParent(object node);
		IList GetChilds(object node);
		int IndexOnParent(object node);
		IList MyList(object node);
	}

	public class TypeNotNeed
	{
		
	}
}

