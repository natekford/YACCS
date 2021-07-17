﻿using System;
using System.Collections.Generic;
using System.Reflection;

using YACCS.Commands;
using YACCS.Localization;
using YACCS.Parsing;
using YACCS.TypeReaders;

namespace YACCS.Examples
{
	public sealed class ConsoleCommandServiceFactory
	{
		private readonly IEnumerable<Assembly> _CommandAssemblies;
		private readonly ICommandServiceConfig _Config;
		private readonly ConsoleHandler _Console;
		private readonly IArgumentHandler _Handler;
		private readonly Localized<ConsoleCommandService> _Localized;
		private readonly IReadOnlyDictionary<Type, ITypeReader> _Readers;
		private readonly IServiceProvider _Services;

		public ConsoleCommandServiceFactory(
			IServiceProvider services,
			ICommandServiceConfig config,
			IArgumentHandler handler,
			IReadOnlyDictionary<Type, ITypeReader> readers,
			ConsoleHandler console,
			IEnumerable<Assembly> commandAssemblies)
		{
			_CommandAssemblies = commandAssemblies;
			_Config = config;
			_Console = console;
			_Readers = readers;
			_Services = services;
			_Handler = handler;

			_Localized = new(_ => new(_Services, _Config, _Handler, _Readers, _Console, _CommandAssemblies));
		}

		public ConsoleCommandService GetCommandService()
			=> _Localized.GetCurrent();
	}
}