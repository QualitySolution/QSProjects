using System;
using System.Linq;

namespace QSProjectsLib
{
	public static class BooleanWorks
	{
		public static int CountTrue(params bool[] args)
		{
			return args.Count(t => t);
		}
	}
}

