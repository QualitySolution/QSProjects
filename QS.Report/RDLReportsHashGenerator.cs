using System;
using QS.Navigation;

namespace QS.Report
{
	public class RDLReportsHashGenerator : IExtraPageHashGenerator
	{

		public string GetHash(Type typeViewModel, object[] ctorValues)
		{
			string hash = null;
			foreach(var ctorArg in ctorValues) {
				if(ctorArg is ReportInfo reportInfo) {
					hash = $"{typeViewModel.FullName}#{reportInfo.Identifier}";
					var parameters = reportInfo.GetParametersString();
					if(!String.IsNullOrEmpty(parameters))
						hash += "?" + parameters;
				}
			}

			return hash;
		}
	}
}
