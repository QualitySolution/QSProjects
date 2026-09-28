using NUnit.Framework;
using ReactiveUI.Builder;

namespace QS.Test;

/// <summary>
/// Initializes ReactiveUI once for tests which construct reactive view models
/// without starting a desktop application.
/// </summary>
[SetUpFixture]
public sealed class ReactiveUITestSetup
{
	[OneTimeSetUp]
	public void InitializeReactiveUI()
	{
		RxAppBuilder.CreateReactiveUIBuilder()
			.WithCoreServices()
			.BuildApp();
	}
}
