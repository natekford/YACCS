using System;
using System.IO;

namespace YACCS.Examples
{
	public class ConsoleReadLine : ConsoleRead
	{
		protected new ReadLineTextWriter Writer { get; }

		public ConsoleReadLine() : this(new ReadLineTextWriter(Console.Out))
		{
		}

		protected ConsoleReadLine(ReadLineTextWriter writer) : base(writer)
		{
			Writer = writer;
			Console.SetOut(writer);
		}

		public override string? ReadLine()
		{
			var line = Console.ReadLine();
			Writer.UpdateLastInput(line);
			return line;
		}

		protected class ReadLineTextWriter : TrackingTextWriter
		{
			private string? _LastInput;

			public ReadLineTextWriter(TextWriter writer) : base(writer)
			{
			}

			public void UpdateLastInput(string? input)
				=> _LastInput = input;

			public override void Write(string? value)
			{
				var (left, top) = Console.GetCursorPosition();
				var (oldLeft, oldTop) = OldPos.GetCurrentCoords();
				if (top < oldTop || (top == oldTop && left < oldLeft))
				{
					Console.SetCursorPosition(oldLeft, oldTop + 1);
					Console.WriteLine(_LastInput);
					Console.WriteLine();
				}

				base.Write(value);
			}
		}
	}
}