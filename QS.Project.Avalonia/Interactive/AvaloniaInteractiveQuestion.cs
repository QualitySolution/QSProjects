using System;
using QS.Dialog;

namespace QS.Project.Interactive;
public class AvaloniaInteractiveQuestion : IInteractiveQuestion {
	public bool Question(string message, string title = null) {
		// TODO: Implement Question dialog behaviour to DialogWindow.axaml.cs
		throw new NotImplementedException();
	}

	public string Question(string[] buttons, string message, string title = null) {
		// How to solve answer from several buttons?
		throw new NotImplementedException();
	}
}
