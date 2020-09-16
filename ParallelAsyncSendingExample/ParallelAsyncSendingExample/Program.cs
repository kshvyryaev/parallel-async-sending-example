using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ParallelAsyncSendingExample
{
	internal class Program
	{
		private static readonly string[] _uris = new[]
		{
			"https://github.com/naudio/NAudio",
			"https://twitter.com/mark_heath",
			"https://github.com/markheath/azure-functions-links",
			"https://pluralsight.com/authors/mark-heath",
			"https://github.com/markheath/advent-of-code-js",
			"http://stackoverflow.com/users/7532/mark-heath",
			"https://mvp.microsoft.com/en-us/mvp/Mark%20%20Heath-5002551",
			"https://github.com/markheath/func-todo-backend",
			"https://github.com/markheath/typescript-tetris"
		};

		private static async Task Main()
		{
			using var client = new HttpClient();

			Console.WriteLine("SemaphoreSlim workflow:");
			await HandleUrisBySemaphoreSlimAsync(client, Handler);

			Console.WriteLine("Nito.AsyncEx.AsyncSemaphore workflow:");
			await HandleUrisByAsyncExAsyncSemaphoreAsync(client, Handler);

			Console.WriteLine("Microsoft.VisualStudio.Threading.AsyncSemaphore workflow:");
			await HandleUrisByVSThreadingAsyncSemaphoreAsync(client, Handler);

			static void Handler(string uri, string html) => Console.WriteLine($"Retrieved {html.Length} characters from {uri}.");
		}

		private static async Task HandleUrisBySemaphoreSlimAsync(HttpClient client, Action<string, string> handler)
		{
			var throttler = new SemaphoreSlim(Environment.ProcessorCount);

			var tasks = _uris.Select(async uri =>
			{
				await throttler.WaitAsync();

				try
				{
					var html = await client.GetStringAsync(uri);
					handler(uri, html);
				}
				finally
				{
					throttler.Release();
				}
			});

			await Task.WhenAll(tasks);
		}

		private static async Task HandleUrisByAsyncExAsyncSemaphoreAsync(HttpClient client, Action<string, string> handler)
		{
			var throttler = new Nito.AsyncEx.AsyncSemaphore(Environment.ProcessorCount);

			var tasks = _uris.Select(async uri =>
			{
				await throttler.WaitAsync();

				try
				{
					var html = await client.GetStringAsync(uri);
					handler(uri, html);
				}
				finally
				{
					throttler.Release();
				}
			});

			await Task.WhenAll(tasks);
		}

		private static async Task HandleUrisByVSThreadingAsyncSemaphoreAsync(HttpClient client, Action<string, string> handler)
		{
			using var throttler = new Microsoft.VisualStudio.Threading.AsyncSemaphore(Environment.ProcessorCount);

			var tasks = _uris.Select(async uri =>
			{
				using var releaser = await throttler.EnterAsync();

				var html = await client.GetStringAsync(uri);
				handler(uri, html);
			});

			await Task.WhenAll(tasks);
		}
	}
}
