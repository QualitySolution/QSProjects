using System;
using System.Threading;
using Gtk;

namespace QSProjectsLib
{
	public partial class LongOperationDlg : Gtk.Window, IWorker
	{	
		public event EventHandler Cancel;
		volatile bool finished;

		private LongOperationDlg(int steps)
			: base(Gtk.WindowType.Toplevel)
		{
			this.Build();
			progressbar1.Adjustment = new Gtk.Adjustment(0, 0, steps-1, 1, 0, 0);	
		}

		public void OnButtonCancelClicked(object sender, EventArgs args){
			if (Cancel != null)
			{
				Cancel(this,null);
				finished = true;
			}
		}

		string operationName;
		public string OperationName
		{
			get{
				return operationName;
			}
			set{
				Gtk.Application.Invoke((sender, args) =>
					{
						labelMsg.Text = value;
					});	
			}
		}

		int stepsCount;
		public int StepsCount
		{
			get{
				return (int)progressbar1.Adjustment.Upper;
			}
			set{
				Gtk.Application.Invoke((sender, args) =>
					{					
						progressbar1.Adjustment.Upper = value-1;
					});
			}
		}
			
		public void ReportProgress(int currentStep,string suboperationName)
		{
			Gtk.Application.Invoke((sender, args) =>
				{
					progressbar1.Adjustment.Value = currentStep;
					progressbar1.Text = String.Format("{0} ({1}/{2})",
						suboperationName, progressbar1.Adjustment.Value + 1, progressbar1.Adjustment.Upper+1);			
				});
		}

		public static void StartOperation(Action<IWorker> doWork, string name, int steps, bool modal=true){			
			var statusWindow = new LongOperationDlg(steps);
			statusWindow.OperationName = name;
			Thread thread = new Thread(()=>{
				doWork(statusWindow);
				statusWindow.finished=true;
			});
			thread.Name = name;
			statusWindow.buttonCancel.Clicked += (sender,args) =>
			{
					thread.Abort();
					statusWindow.Destroy();
			};
			statusWindow.Modal = modal;

			statusWindow.Show();		
			thread.Start ();
			while (!statusWindow.finished)
			{
				Gtk.Main.Iteration ();
			}
			if (thread.IsAlive)
				thread.Interrupt();
			statusWindow.Destroy();
		}
	}

	public interface IWorker{
		void ReportProgress(int currentStep, string suboperationName);
		int StepsCount{get;set;}
		string OperationName{get;set;}
	}
}