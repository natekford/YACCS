using System;
using System.Collections.Generic;
using System.Reflection;

using YACCS.Commands;
using YACCS.Localization;
using YACCS.Parsing;
using YACCS.TypeReaders;

namespace YACCS.Examples
{
	public class ConsoleCommandServiceFactory
	{
		private readonly IEnumerable<Assembly> _CommandAssemblies;
		private readonly ICommandServiceConfig _Config;
		private readonly ConsoleHandler _Console;
		private readonly Localized<ConsoleCommandService> _Localized;
		private readonly IReadOnlyDictionary<Type, ITypeReader> _Readers;
		private readonly IArgumentSplitter _Splitter;

		public ConsoleCommandServiceFactory(
			ICommandServiceConfig config,
			IArgumentSplitter splitter,
			IReadOnlyDictionary<Type, ITypeReader> readers,
			ConsoleHandler console,
			IEnumerable<Assembly> commandAssemblies)
		{
			_CommandAssemblies = commandAssemblies;
			_Config = config;
			_Console = console;
			_Readers = readers;
			_Splitter = splitter;

			_Localized = new(_ => new(_Config, _Splitter, _Readers, _Console, _CommandAssemblies));
		}

		public ConsoleCommandService GetCommandService()
			=> _Localized.GetCurrent();
	}
}