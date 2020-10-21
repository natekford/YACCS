using System;
using System.IO;

namespace YACCS.Examples
{
	public class TrackingReadLineTextWriter : TrackingTextWriter
	{
		protected string? LastInput { get; set; }

		public TrackingReadLineTextWriter(TextWriter writer) : base(writer)
		{
		}

		public override string? ReadLine()
		{
			var line = Console.ReadLine();
			UpdateLastInput(line);
			return line;
		}

		public void UpdateLastInput(string? input)
			=> LastInput = input;

		public override void Write(string? value)
		{
			var (curLeft, curTop) = Console.GetCursorPosition();
			var (oldLeft, oldTop) = OldPos.GetCurrentCoords();
			if (curTop < oldTop || (curTop == oldTop && curLeft < oldLeft))
			{
				Console.SetCursorPosition(oldLeft, oldTop);
				Console.WriteLine(LastInput);
				Console.WriteLine();
			}

			base.Write(value);
		}
	}
}