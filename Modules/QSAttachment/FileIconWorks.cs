using System;
using System.Collections.Generic;

namespace QSAttachment
{
	public class FileIconWorks
	{
		internal static Dictionary<string, string> extensionIcons = new Dictionary<string, string>(){
			{"doc", "QSAttachment.icons.x-office-document.png"},
			{"odt", "QSAttachment.icons.x-office-document.png"},
			{"xls", "QSAttachment.icons.x-office-spreadsheet.png"},
			{"ods", "QSAttachment.icons.x-office-spreadsheet.png"},
			{"jpg", "QSAttachment.icons.image-x-generic.png"},
			{"jpeg", "QSAttachment.icons.image-x-generic.png"},
			{"png", "QSAttachment.icons.image-x-generic.png"},
			{"bmp", "QSAttachment.icons.image-x-generic.png"},
		};

		public static string GetIconResourceName(string extension)
		{
			if (extensionIcons.ContainsKey(extension))
				return extensionIcons[extension];
			else
				return "QSAttachment.icons.text-x-generic.png";
		}


	}
}

