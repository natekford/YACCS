using System;

using System.Collections.Generic;
using System.IO;
using System.Text;

namespace YACCS.Examples
{
	// This does not work in a lot of cases and only potentially prevents a small issue with
	// ConsoleReadLine so don't use it
	public class ConsoleReadKey : ConsoleRead
	{
		private readonly List<string> _History = new List<string>();
		private string _Empty = new string(' ', 256);
		private int? _HistoryIndex;

		protected new KeyTrackingTextWriter Writer { get; }

		public ConsoleReadKey() : this(new KeyTrackingTextWriter(Console.Out))
		{
		}

		protected ConsoleReadKey(KeyTrackingTextWriter writer) : base(writer)
		{
			Writer = writer;
			Console.SetOut(writer);
		}

		public override string? ReadLine()
		{
			while (true)
			{
				var key = Console.ReadKey(true);
				Writer.UpdatePositionOnKeyReceived();
				switch (key.Key)
				{
					case ConsoleKey.UpArrow:
						History(-1);
						break;

					case ConsoleKey.DownArrow:
						History(1);
						break;

					case ConsoleKey.RightArrow:
						Move(1);
						break;

					case ConsoleKey.LeftArrow:
						Move(-1);
						break;

					case ConsoleKey.Delete:
						Remove(0);
						break;

					case ConsoleKey.Backspace:
						Remove(-1);
						break;

					case ConsoleKey.Enter:
						if (Writer.Builder.Length > _Empty.Length)
						{
							_Empty = new string(' ', Writer.Builder.Length);
						}
						_History.Add(Writer.Builder.ToString());
						_HistoryIndex = null;
						Writer.Builder.Clear();

						Console.WriteLine();
						return _History[^1];

					default:
						Writer.Builder.Append(key.KeyChar);
						Redraw(1);
						break;
				}
			}
		}

		private void History(int diff)
		{
			var (curLeft, _) = Writer.CurPos.GetCurrentCoords();

			_HistoryIndex ??= _History.Count;
			_HistoryIndex += diff;
			_HistoryIndex = Math.Max(Math.Min(_HistoryIndex.Value, _History.Count - 1), 0);

			Writer.Builder.Clear();
			if (_History.Count != 0)
			{
				Writer.Builder.Append(_History[_HistoryIndex.Value]);
			}
			Redraw(Writer.Builder.Length - curLeft);
		}

		private void Move(int leftDiff)
		{
			var (left, top) = Writer.CurPos.GetCurrentCoords();

			var newLeft = Math.Min(Writer.Builder!.Length, Math.Max(left + leftDiff, 0));
			Console.SetCursorPosition(newLeft, top);
		}

		private void Redraw(int leftDiff)
		{
			var (curLeft, curTop) = Writer.CurPos.GetCurrentCoords();
			var (oldLeft, _) = Writer.OldPos.GetCurrentCoords();

			Console.SetCursorPosition(oldLeft, curTop);
			Console.Write(Writer.Builder.ToString());
			Console.Write(_Empty);

			var left = curLeft + (leftDiff % Console.BufferWidth);
			if (left < 0)
			{
				// Wrap around
				left += Console.BufferWidth;
			}
			Console.SetCursorPosition(left, curTop);
		}

		private void Remove(int leftDiff)
		{
			var (curLeft, curTop) = Writer.CurPos.GetCurrentCoords();
			var (oldLeft, oldTop) = Writer.OldPos.GetCurrentCoords();

			var index = ((curTop - oldTop) * Console.BufferWidth) + curLeft - oldLeft + leftDiff;
			if (index < 0 || index >= Writer.Builder!.Length)
			{
				return;
			}

			Writer.Builder.Remove(index, 1);
			Redraw(leftDiff);
		}

		protected class KeyTrackingTextWriter : TrackingTextWriter
		{
			public StringBuilder Builder { get; protected set; } = new StringBuilder(256);
			public CurrentPosition CurPos { get; protected set; } = CurrentPosition.Zero;

			public KeyTrackingTextWriter(TextWriter writer) : base(writer)
			{
			}

			public void UpdatePositionOnKeyReceived()
			{
				CurPos = new CurrentPosition(this, Builder);
				if (Builder.Length == 0)
				{
					OldPos = new CurrentPosition(this);
				}
			}

			protected override void UpdatePositionOnNewLineReceived()
			{
				CurPos = new CurrentPosition(this, Builder);
				base.UpdatePositionOnNewLineReceived();
			}
		}
	}
}