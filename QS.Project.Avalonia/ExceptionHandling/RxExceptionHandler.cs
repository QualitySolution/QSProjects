using ReactiveUI;
using System;
using System.Diagnostics;
using System.Reactive.Concurrency;

namespace QS.Project.Avalonia.ExceptionHandling;
public class RxExceptionHandler : IObserver<Exception>
{
	public Action<Exception> OnExceptionAction { get; set; }

	public void OnCompleted()
	{
		if(Debugger.IsAttached) Debugger.Break();
	}

	public void OnError(Exception error)
	{
		if(Debugger.IsAttached) Debugger.Break();

		OnExceptionAction?.Invoke(error);

		RxApp.MainThreadScheduler.Schedule(() => { throw error; });
	}

	public void OnNext(Exception value)
	{
		if(Debugger.IsAttached) Debugger.Break();

		OnExceptionAction?.Invoke(value);

		RxApp.MainThreadScheduler.Schedule(() => { throw value; });
	}
}
