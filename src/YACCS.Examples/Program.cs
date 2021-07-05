using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Help;
using YACCS.Interactivity.Input;
using YACCS.Localization;
using YACCS.Parsing;
using YACCS.TypeReaders;

namespace YACCS.Examples
{
	public sealed class Program
	{
		private readonly ConsoleCommandService _CommandService;
		private readonly ICommandServiceConfig _Config;
		private readonly ConsoleHandler _Console;
		private readonly HelpFormatter _HelpFormatter;
		private readonly ConsoleInput _Input;
		private readonly ILocalizer _Localizer;
		private readonly TypeNameRegistry _Names;
		private readonly IServiceProvider _Services;
		private readonly IArgumentSplitter _Splitter;
		private readonly TagConverter _Tags;
		private readonly TypeReaderRegistry _TypeReaders;

		private Program()
		{
			_Config = CommandServiceConfig.Instance;
			_Localizer = new ResourceManagerLocalizer();
			_Names = new TypeNameRegistry();
			_Splitter = ArgumentSplitter.Instance;
			_Tags = new TagConverter();
			_TypeReaders = new TypeReaderRegistry(new[] { typeof(Program).Assembly });

			_Console = new ConsoleHandler(_Names);
			_CommandService = new ConsoleCommandService(_Config, _Splitter, _TypeReaders, _Console);
			_CommandService.CommandExecuted += (e) =>
			{
				_Console.WriteResult(e.Result);
				var exceptions = e.GetAllExceptions();
				if (exceptions.Any())
				{
					_Console.WriteLine(string.Join(Environment.NewLine, exceptions), ConsoleColor.Red);
				}
				return Task.CompletedTask;
			};
			_HelpFormatter = new HelpFormatter(_Names, _Tags);
			_Input = new ConsoleInput(_TypeReaders, _Console);

			_Services = new ServiceCollection()
				.AddSingleton<ICommandService>(_CommandService)
				.AddSingleton<ICommandServiceConfig>(_Config)
				.AddSingleton<ConsoleHandler>(_Console)
				.AddSingleton<IHelpFormatter>(_HelpFormatter)
				.AddSingleton<IInput<IContext, string>>(_Input)
				.AddSingleton<ILocalizer>(_Localizer)
				.AddSingleton<IReadOnlyDictionary<Type, string>>(_Names)
				.AddSingleton<IArgumentSplitter>(_Splitter)
				.AddSingleton<ITagConverter>(_Tags)
				.AddSingleton<IReadOnlyDictionary<Type, ITypeReader>>(_TypeReaders)
				.BuildServiceProvider();
		}

		public static Task Main()
			=> new Program().RunAsync();

		private async Task ExecuteAsync()
		{
			await _Console.WaitForBothIOLocksAsync().ConfigureAwait(false);
			_Console.ReleaseIOLocks();

			_Console.WriteLine();
			_Console.WriteLine("Enter a command and its arguments: ");

			var input = await _Console.ReadLineAsync().ConfigureAwait(false);
			if (input is null)
			{
				return;
			}

			var context = new ConsoleContext(_Services.CreateScope(), input);
			var result = await _CommandService.ExecuteAsync(context, input).ConfigureAwait(false);
			if (!result.InnerResult.IsSuccess)
			{
				_Console.ReleaseIOLocks();
			}
			_Console.WriteResult(result);
		}

		private async Task RegisterCommandsAsync()
		{
			var commands = Assembly.GetExecutingAssembly().GetAllCommandsAsync();
			await _CommandService.AddRangeAsync(commands).ConfigureAwait(false);

			// Example delegate command registration
			static int Add(int a, int b)
				=> a + b;

			var @delegate = (Func<int, int, int>)Add;
			var names = new[] { new ImmutableName(new[] { nameof(Add) }) };
			var add = new DelegateCommand(@delegate, names).MakeMultipleImmutable();
			_CommandService.AddRange(add);

			_Console.WriteLine($"Successfully registered {_CommandService.Commands.Count} commands.");
		}

		private async Task RunAsync()
		{
			await RegisterCommandsAsync().ConfigureAwait(false);
			while (true)
			{
				await ExecuteAsync().ConfigureAwait(false);
			}
		}
	}
}