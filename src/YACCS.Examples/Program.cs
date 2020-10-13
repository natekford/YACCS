using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Linq;
using YACCS.Commands.Models;

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

			var result = await _CommandService.ExecuteAsync(context, input).ConfigureAwait(false);
			if (!string.IsNullOrWhiteSpace(result.Response))
			{
				WriteLine(result.Response, result.IsSuccess ? ConsoleColor.Green : ConsoleColor.Red);
			}

			Console.WriteLine();
		}

		private async Task RegisterCommandsAsync()
		{
			var commands = Assembly.GetExecutingAssembly().GetAllCommandsAsync();
			await _CommandService.AddRangeAsync(commands).ConfigureAwait(false);
			Console.WriteLine($"Successfully registered {_CommandService.Commands.Count} commands.");
			Console.WriteLine();

#if true
			static void DelegateCommand(int i, double d, string s)
				=> Console.WriteLine($"i am the delegate command: {i} {d} {s}");

			var @delegate = (Action<int, double, string>)DelegateCommand;
			var names = new[] { new Name(new[] { "delegate" }) };
			for (var i = 0; i < 1000; ++i)
			{
				var command = new DelegateCommand(@delegate, names)
					.AddAttribute(new PriorityAttribute(i))
					.ToImmutable()
					.Single();
				_CommandService.Commands.Add(command);
			}
#endif
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