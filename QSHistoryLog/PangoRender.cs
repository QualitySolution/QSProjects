using System;
using System.Text;
using DiffPlex.DiffBuilder.Model;

namespace QSHistoryLog
{
	public static class PangoRender
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		public const string PangoInsertFormat = "<span background=\"#A6F3A6\">";
		public const string PangoDeleteFormat = "<span background=\"#F8CBCB\">";
		public const string PangoChangeFormat = "<span background=\"#F0DB88\">";
		public const string PangoEnd = "</span>";

		public static string RenderDiffLines(DiffPaneModel diffModel)
		{
			StringBuilder result = new StringBuilder ();
			foreach (var line in diffModel.Lines)
			{
				if (line.Type == ChangeType.Deleted)
					result.AppendLine (PangoDeleteFormat + line.Text + PangoEnd);
				else if (line.Type == ChangeType.Inserted)
					result.AppendLine (PangoInsertFormat + line.Text + PangoEnd);
				else if (line.Type == ChangeType.Unchanged)
					result.AppendLine (line.Text);
				else if (line.Type == ChangeType.Modified)
					result.AppendLine (RenderDiffWords(line));
				else if (line.Type == ChangeType.Imaginary)
				{
					result.AppendLine ();
					logger.Debug ("Imaginary line {0}", line.Text);
				}
			}
			return result.ToString ().TrimEnd (Environment.NewLine.ToCharArray ());
		}

		private static string RenderDiffWords(DiffPiece line)
		{
			StringBuilder result = new StringBuilder ();
			ChangeType lastAction = ChangeType.Unchanged;
			foreach (var word in line.SubPieces) {
				if (word.Type == ChangeType.Imaginary)
					continue;

				if (word.Type == ChangeType.Modified)
					result.Append (RenderDiffCharacter (word, ref lastAction));
				else
				{
					if (lastAction != ChangeType.Unchanged && lastAction != word.Type)
						result.Append (PangoEnd);
					result.Append (StartSpan (word.Type, lastAction)).Append (word.Text);
					lastAction = word.Type;
				}
			}
			if(lastAction != ChangeType.Unchanged)
				result.Append (PangoEnd);
			return result.ToString ();
		}

		private static string RenderDiffCharacter(DiffPiece word, ref ChangeType lastAction)
		{
			StringBuilder result = new StringBuilder ();
			foreach (var characters in word.SubPieces)
			{
				if (characters.Type == ChangeType.Imaginary)
					continue;

				if (lastAction != ChangeType.Unchanged && lastAction != characters.Type)
					result.Append (PangoEnd);

				result.Append (StartSpan (characters.Type, lastAction)).Append (characters.Text);
				lastAction = characters.Type;
			}
			return result.ToString ();
		}

		private static string StartSpan(ChangeType current, ChangeType last)
		{
			if (current != last) {
				if (current == ChangeType.Deleted)
					return PangoDeleteFormat;
				else if (current == ChangeType.Inserted)
					return PangoInsertFormat;
			}
			return String.Empty;
		}
	}
}

