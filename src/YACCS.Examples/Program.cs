using System;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using YACCS.Commands;

namespace YACCS.Examples
{
	public sealed class Program
	{
		private readonly CommandService _CommandService;
		private readonly IServiceProvider _Services;
		private readonly TypeReaderRegistry _TypeReaders;

		private Program()
		{
			_TypeReaders = new TypeReaderRegistry();
			_CommandService = new CommandService(CommandServiceConfig.Default, _TypeReaders);
			_Services = new ServiceCollection()
				.AddSingleton<ITypeReaderRegistry>(_TypeReaders)
				.AddSingleton<ICommandService>(_CommandService)
				.BuildServiceProvider();
		}

		public static Task Main()
			=> new Program().RunAsync();

		private static void WriteLine(string input, ConsoleColor color)
		{
			var oldColor = Console.ForegroundColor;
			Console.ForegroundColor = color;
			Console.WriteLine(input);
			Console.ForegroundColor = oldColor;
		}

		private void AddExecutionHandlers()
		{
			_CommandService.CommandExecuted += (e) =>
			{
				var result = e.Result;
				if (!string.IsNullOrWhiteSpace(result.Response))
				{
					var color = result.IsSuccess ? ConsoleColor.Green : ConsoleColor.Red;
					WriteLine(result.Response, color);
				}
				return Task.CompletedTask;
			};
			_CommandService.CommandExecutedException += (e) =>
			{
				var output = string.Join(Environment.NewLine, e.Exceptions);
				WriteLine(output, ConsoleColor.Red);
				return Task.CompletedTask;
			};
		}

		private async Task ExecuteAsync()
		{
			Console.WriteLine("Enter a command and its arguments: ");

			var input = Console.ReadLine();
			var context = new ConsoleContext(_Services, input);
			await _CommandService.ExecuteAsync(context, input).ConfigureAwait(false);

			Console.WriteLine();
		}

		private async Task RegisterCommandsAsync()
		{
			await foreach (var command in Assembly.GetExecutingAssembly().GetAllCommandsAsync())
			{
				_CommandService.Add(command);
			}
			Console.WriteLine($"Successfully registered {_CommandService.Commands.Count} commands.");
			Console.WriteLine();
		}

		private async Task RunAsync()
		{
			await RegisterCommandsAsync().ConfigureAwait(false);
			AddExecutionHandlers();

			while (true)
			{
				await ExecuteAsync().ConfigureAwait(false);
			}
		}
	}
}