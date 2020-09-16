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
			await HandleUrisAsync(client, (uri, html) => Console.WriteLine($"Retrieved {html.Length} characters from {uri} on thread with id = {Thread.CurrentThread.ManagedThreadId}."));
		}

		private static async Task HandleUrisAsync(HttpClient client, Action<string, string> handler)
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
	}
}
