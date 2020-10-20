using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace YACCS.Examples
{
	public abstract class ConsoleRead
	{
		protected TrackingTextWriter Writer { get; }

		protected ConsoleRead(TrackingTextWriter writer)
		{
			Writer = writer;
		}

		public abstract string? ReadLine();

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

		[DebuggerDisplay("{DebuggerDisplay,nq}")]
		protected class TrackingTextWriter : TextWriter
		{
			public TextWriter Inner { get; }
			public int LineCount { get; protected set; }
			public CurrentPosition OldPos { get; protected set; } = CurrentPosition.Zero;
			public override Encoding Encoding => Inner.Encoding;

			private string DebuggerDisplay => $"Line Count = {LineCount}";

			public TrackingTextWriter(TextWriter writer)
			{
				Inner = writer;
			}

			public override void Write(char value)
			{
				if (value == '\n')
				{
					UpdatePositionOnNewLineReceived();
				}
				Inner.Write(value);
			}

			protected virtual void UpdatePositionOnNewLineReceived()
			{
				++LineCount;
				OldPos = new CurrentPosition(this);
			}
		}
	}
}