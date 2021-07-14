using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.Help;
using YACCS.Help.Attributes;
using YACCS.Interactivity.Input;
using YACCS.Results;
using YACCS.Interactivity;

namespace YACCS.Examples
{

	public abstract class ConsoleCommands<T> : CommandGroup<T> where T : IContext
	{
		public ConsoleHandler Console { get; set; } = null!;

		[Command(nameof(Abstract), AllowInheritance = true)]
		public abstract string Abstract();
	}
}