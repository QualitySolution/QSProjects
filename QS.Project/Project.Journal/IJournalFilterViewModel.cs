﻿using System;
using System.ComponentModel;

namespace QS.Project.Journal
{
	public interface IJournalFilterViewModel : INotifyPropertyChanged
	{
		bool IsShow { get; set; }
	}
}
