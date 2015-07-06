using System;
using System.Collections.Generic;
using Gtk;

namespace QSOrmProject.RepresentationModel
{
	public interface IRepresentationModel
	{
		Type NodeType { get;}

		Type ObjectType { get;}

		IUnitOfWork UoW { get;}

		IRepresentationFilter RepresentationFilter{ get;}

		NodeStore NodeStore { get; }

		List<IColumnInfo> Columns { get;}

		void UpdateNodes();
	}
}

