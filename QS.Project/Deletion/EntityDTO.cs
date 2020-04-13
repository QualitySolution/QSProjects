using System;
using System.Collections.Generic;

namespace QS.Deletion
{
	public class EntityDTO : IEquatable<EntityDTO>
	{
		public uint Id;
		public Type ClassType;
		public string Title;
		public object Entity;
		public List<EntityDTO> PullsUp = new List<EntityDTO>();

		public bool Equals(EntityDTO other)
		{
			return Id == other.Id && ClassType == other.ClassType;
		}
	}
}

