using System;

using YACCS.Results;

namespace YACCS.Examples
{
	public class ConsoleWriter
	{
		private readonly ITypeRegistry<string> _Names;

		public ConsoleWriter(ITypeRegistry<string> names)
		{
			_Names = names;
		}

		public void WriteLine(string input, ConsoleColor color)
		{
			var oldColor = Console.ForegroundColor;
			Console.ForegroundColor = color;
			Console.WriteLine(input);
			Console.ForegroundColor = oldColor;
		}

		public void WriteResponse(IResult result)
		{
			var response = result switch
			{
				ParseFailedResult pfr => $"Failed to parse {_Names.Get(pfr.Type)}.",
				_ => result.Response,
			};
			if (!string.IsNullOrWhiteSpace(response))
			{
				WriteLine(response, result.IsSuccess ? ConsoleColor.Green : ConsoleColor.Red);
			}
		}
	}
}