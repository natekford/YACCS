using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace YACCS.Examples
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public abstract class TrackingTextWriter : TextWriter
	{
		public override Encoding Encoding => Writer.Encoding;
		protected int LineCount { get; set; }
		protected CurrentPosition OldPos { get; set; } = CurrentPosition.Zero;
		protected TextWriter Writer { get; }
		private string DebuggerDisplay => $"Line Count = {LineCount}";

		protected TrackingTextWriter(TextWriter writer)
		{
			Writer = writer;
		}

		public abstract string? ReadLine();

		public override void Write(char value)
		{
			Writer.Write(value);
			if (value == '\n')
			{
				UpdatePositionOnNewLineReceived();
			}
		}

		protected virtual void UpdatePositionOnNewLineReceived()
		{
			++LineCount;
			OldPos = new CurrentPosition(this);
		}

		[DebuggerDisplay("{DebuggerDisplay,nq}")]
		protected class CurrentPosition
		{
			private readonly StringBuilder? _Sb;
			private readonly TrackingTextWriter? _Writer;

			public static CurrentPosition Zero { get; } = new CurrentPosition();

			public int AbsolutePosition { get; }
			public int BufferHeight { get; }
			public int BufferWidth { get; }
			public int Left { get; }
			public int Top { get; }
			private string DebuggerDisplay => $"Position = {AbsolutePosition}";

			public CurrentPosition(TrackingTextWriter writer) : this()
			{
				_Writer = writer;
			}

			public CurrentPosition(TrackingTextWriter writer, StringBuilder sb) : this()
			{
				_Sb = sb;
				_Writer = writer;
			}

			private CurrentPosition()
			{
				BufferWidth = Console.BufferWidth;
				BufferHeight = Console.BufferHeight;
				(Left, Top) = Console.GetCursorPosition();
				AbsolutePosition = Left + (Top * BufferWidth);
			}

			public virtual (int Left, int Top) GetCurrentCoords()
			{
				if (_Writer is null)
				{
					return (0, 0);
				}

				var curBufferWidth = Console.BufferWidth;
				if (BufferWidth == curBufferWidth)
				{
					return (Left, Top);
				}
				else if (_Sb is not null)
				{
					var left = _Sb.Length % curBufferWidth;
					var top = _Writer.LineCount + (_Sb.Length / curBufferWidth);
					return (left, top);
				}
				else
				{
					return (Left, _Writer.LineCount);
				}
			}
		}
	}
}