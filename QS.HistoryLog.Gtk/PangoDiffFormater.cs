using System;
using DiffPlex;
using DiffPlex.DiffBuilder;

namespace QS.HistoryLog
{
	public class PangoDiffFormater : IDiffFormatter
	{
		public PangoDiffFormater()
		{
		}

		public void SideBySideDiff(string oldValue, string newValue, out string oldDiff, out string newDiff)
		{
			var d = new Differ();
			var differ = new SideBySideFullDiffBuilder(d);
			var diffRes = differ.BuildDiffModel(oldValue, newValue);
			oldDiff = PangoRender.RenderDiffLines(diffRes.OldText);
			newDiff = PangoRender.RenderDiffLines(diffRes.NewText);
		}
	}
}
