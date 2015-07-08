using System;
using System.Collections.Generic;
using Gtk.DataBindings;
using System.Collections;

namespace QSOrmProject.RepresentationModel
{
	public interface IRepresentationModel
	{
		Type NodeType { get;}

		Type ObjectType { get;}

		IUnitOfWork UoW { get;}

		IRepresentationFilter RepresentationFilter{ get;}

		IMappingConfig TreeViewConfig { get;}

		IList ItemsList { get;}

		IEnumerable<string> SearchFields { get;}

		void UpdateNodes();
	}
}

