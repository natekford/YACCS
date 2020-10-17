using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Interactivity.Input;
using YACCS.Commands.Linq;
using YACCS.Commands.Models;
using YACCS.Help;
using YACCS.TypeReaders;

namespace YACCS.Examples
{
	public sealed class Program
	{
		private readonly CommandService _CommandService;
		private readonly HelpFormatter _HelpFormatter;
		private readonly ConsoleInput _Input;
		private readonly TypeNameRegistry _Names;
		private readonly SemaphoreSlim _Semaphore;
		private readonly IServiceProvider _Services;
		private readonly TagConverter _Tags;
		private readonly TypeReaderRegistry _TypeReaders;
		private readonly ConsoleWriter _Writer;

		private Program()
		{
			_TypeReaders = new TypeReaderRegistry();
			_Names = new TypeNameRegistry();
			_Tags = new TagConverter();
			_Semaphore = new SemaphoreSlim(1);

			_CommandService = new CommandService(CommandServiceConfig.Default, _TypeReaders);
			_HelpFormatter = new HelpFormatter(_Names, _Tags);
			_Input = new ConsoleInput(_TypeReaders);
			_Writer = new ConsoleWriter(_Names);

			_Services = new ServiceCollection()
				.AddSingleton<ICommandService>(_CommandService)
				.AddSingleton<IHelpFormatter>(_HelpFormatter)
				.AddSingleton<IInput<IContext, string>>(_Input)
				.AddSingleton<ITypeRegistry<string>>(_Names)
				.AddSingleton<ITagConverter>(_Tags)
				.AddSingleton<ITypeRegistry<ITypeReader>>(_TypeReaders)
				.AddSingleton(_Semaphore)
				.AddSingleton(_Writer)
				.BuildServiceProvider();
		}

		public static Task Main()
			=> new Program().RunAsync();

		private async Task ExecuteAsync()
		{
			await _Semaphore.WaitAsync().ConfigureAwait(false);
			_Semaphore.Release();
			await Task.Delay(25).ConfigureAwait(false);

			Console.WriteLine();
			Console.WriteLine("Enter a command and its arguments: ");

			var input = Console.ReadLine();
			var context = new ConsoleContext(_Services, input);
			var result = await _CommandService.ExecuteAsync(context, input).ConfigureAwait(false);

			_Writer.WriteResponse(result);
		}

		private async Task RegisterCommandsAsync()
		{
			var commands = Assembly.GetExecutingAssembly().GetAllCommandsAsync();
			await _CommandService.AddRangeAsync(commands).ConfigureAwait(false);
			Console.WriteLine($"Successfully registered {_CommandService.Commands.Count} commands.");

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
			_CommandService.CommandExecuted += (e) =>
			{
				_Writer.WriteResponse(e.Result);
				return Task.CompletedTask;
			};
			_CommandService.CommandExecutedException += (e) =>
			{
				_Writer.WriteLine(string.Join(Environment.NewLine, e.Exceptions), ConsoleColor.Red);
				return Task.CompletedTask;
			};

			while (true)
			{
				await ExecuteAsync().ConfigureAwait(false);
			}
		}
	}
}