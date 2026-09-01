using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace QS.ErrorReporting
{
	public class InMemoryLogService : IAsyncLogService, ILoggerProvider
	{
		private const int DefaultRowCount = 300;
		private readonly object syncRoot = new object();
		private readonly Queue<string> logLines;
		private readonly int capacity;

		public InMemoryLogService(int capacity = DefaultRowCount)
		{
			this.capacity = capacity > 0 ? capacity : DefaultRowCount;
			logLines = new Queue<string>(this.capacity);
		}

		public Task<string> GetLogAsync(uint? rowCount = null)
		{
			var count = (int)(rowCount ?? DefaultRowCount);
			if(count <= 0) {
				count = DefaultRowCount;
			}

			string[] lines;
			lock(syncRoot) {
				lines = logLines.ToArray();
			}

			var startIndex = Math.Max(0, lines.Length - count);
			return Task.FromResult(string.Join(Environment.NewLine, lines, startIndex, lines.Length - startIndex));
		}

		public ILogger CreateLogger(string categoryName)
		{
			return new InMemoryLogger(this, categoryName);
		}

		public void Dispose()
		{
		}

		private void AddText(string text)
		{
			var lines = text
				.Replace("\r\n", "\n")
				.Replace('\r', '\n')
				.Split(new[] { '\n' }, StringSplitOptions.None);

			lock(syncRoot) {
				foreach(var line in lines) {
					if(logLines.Count >= capacity) {
						logLines.Dequeue();
					}

					logLines.Enqueue(line);
				}
			}
		}

		private class InMemoryLogger : ILogger
		{
			private readonly InMemoryLogService logService;
			private readonly string categoryName;

			public InMemoryLogger(InMemoryLogService logService, string categoryName)
			{
				this.logService = logService;
				this.categoryName = categoryName;
			}

			public IDisposable BeginScope<TState>(TState state)
			{
				return NullScope.Instance;
			}

			public bool IsEnabled(LogLevel logLevel)
			{
				return logLevel != LogLevel.None;
			}

			public void Log<TState>(
				LogLevel logLevel,
				EventId eventId,
				TState state,
				Exception exception,
				Func<TState, Exception, string> formatter)
			{
				if(!IsEnabled(logLevel)) {
					return;
				}

				var message = formatter != null ? formatter(state, exception) : state?.ToString();
				if(string.IsNullOrWhiteSpace(message) && exception == null) {
					return;
				}

				var builder = new StringBuilder();
				builder.Append(DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss.fff zzz"));
				builder.Append(' ');
				builder.Append(logLevel);
				builder.Append(' ');
				builder.Append(categoryName);

				if(eventId.Id != 0 || !string.IsNullOrWhiteSpace(eventId.Name)) {
					builder.Append('[');
					builder.Append(eventId.Id);
					if(!string.IsNullOrWhiteSpace(eventId.Name)) {
						builder.Append(':');
						builder.Append(eventId.Name);
					}
					builder.Append(']');
				}

				builder.Append(": ");
				builder.Append(message);

				if(exception != null) {
					builder.AppendLine();
					builder.Append(exception);
				}

				logService.AddText(builder.ToString());
			}
		}

		private class NullScope : IDisposable
		{
			public static readonly NullScope Instance = new NullScope();

			public void Dispose()
			{
			}
		}
	}
}
