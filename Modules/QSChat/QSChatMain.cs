using System;
using System.Collections.Generic;
using Gtk;

namespace QSChat
{
	internal static class QSChatMain
	{
		private static Dictionary<string, string> usersColors = new Dictionary<string, string>();

		public static TextTagTable BuildTagTable()
		{
			TextTagTable textTags = new TextTagTable();
			var tag = new TextTag("date");
			tag.Justification = Justification.Center;
			tag.Weight = Pango.Weight.Bold;
			textTags.Add(tag);
			tag = new TextTag("user1");
			tag.Foreground = "#FF00FF";
			textTags.Add(tag);
			tag = new TextTag("user2");
			tag.Foreground = "#9400D3";
			textTags.Add(tag);
			tag = new TextTag("user3");
			tag.Foreground = "#191970";
			textTags.Add(tag);
			tag = new TextTag("user4");
			tag.Foreground = "#7F0000";
			textTags.Add(tag);
			tag = new TextTag("user5");
			tag.Foreground = "#FF8C00";
			textTags.Add(tag);
			tag = new TextTag("user6");
			tag.Foreground = "#FFA500";
			textTags.Add(tag);
			tag = new TextTag("user7");
			tag.Foreground = "#32CD32";
			textTags.Add(tag);
			tag = new TextTag("user8");
			tag.Foreground = "#3CB371";
			textTags.Add(tag);
			tag = new TextTag("user9");
			tag.Foreground = "#007F00";
			textTags.Add(tag);
			tag = new TextTag("user10");
			tag.Foreground = "#FFFF00";
			textTags.Add(tag);
			return textTags;
		}

		public static string GetUserTag(string userName)
		{
			if (usersColors.ContainsKey(userName))
				return usersColors[userName];
			else
			{
				string tagName = String.Format("user{0}", usersColors.Count % 10 + 1);
				usersColors.Add(userName, tagName);
				return tagName;
			}
		}
	}
}

