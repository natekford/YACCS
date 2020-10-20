using System;

using System.Collections.Generic;
using System.IO;
using System.Text;

namespace YACCS.Examples
{
	// This does not work in a lot of cases and only potentially prevents a small issue with
	// the line based one so don't use it
	public class TrackingReadKeyTextWriter : TrackingTextWriter
	{
		protected StringBuilder Builder { get; set; } = new StringBuilder(256);
		protected CurrentPosition CurPos { get; set; } = CurrentPosition.Zero;
		protected string Empty { get; set; } = new string(' ', 256);
		protected int? HistoryIndex { get; set; }
		protected List<string> HistoryList { get; set; } = new List<string>();

		public TrackingReadKeyTextWriter(TextWriter writer) : base(writer)
		{
		}

		public override string? ReadLine()
		{
			while (true)
			{
				var key = Console.ReadKey(true);
				UpdatePositionOnKeyReceived();
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
						if (Builder.Length > Empty.Length)
						{
							Empty = new string(' ', Builder.Length);
						}
						HistoryList.Add(Builder.ToString());
						HistoryIndex = null;
						Builder.Clear();

						Console.WriteLine();
						return HistoryList[^1];

					default:
						Builder.Append(key.KeyChar);
						Redraw(1);
						break;
				}
			}
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

		private void History(int diff)
		{
			var (curLeft, _) = CurPos.GetCurrentCoords();

			HistoryIndex ??= HistoryList.Count;
			HistoryIndex += diff;
			HistoryIndex = Math.Max(Math.Min(HistoryIndex.Value, HistoryList.Count - 1), 0);

			Builder.Clear();
			if (HistoryList.Count != 0)
			{
				Builder.Append(HistoryList[HistoryIndex.Value]);
			}
			Redraw(Builder.Length - curLeft);
		}

		private void Move(int leftDiff)
		{
			var (left, top) = CurPos.GetCurrentCoords();

			var newLeft = Math.Min(Builder.Length, Math.Max(left + leftDiff, 0));
			Console.SetCursorPosition(newLeft, top);
		}

		private void Redraw(int leftDiff)
		{
			var (curLeft, _) = CurPos.GetCurrentCoords();
			var (oldLeft, oldTop) = OldPos.GetCurrentCoords();

			Console.SetCursorPosition(oldLeft, oldTop);
			Writer.Write(Builder.ToString());
			Writer.Write(Empty);

			var left = curLeft + (leftDiff % Console.BufferWidth);
			if (left < 0)
			{
				// Wrap around
				left += Console.BufferWidth;
			}
			Console.SetCursorPosition(left, oldTop);
		}

		private void Remove(int leftDiff)
		{
			var (curLeft, curTop) = CurPos.GetCurrentCoords();
			var (oldLeft, oldTop) = OldPos.GetCurrentCoords();

			var index = ((curTop - oldTop) * Console.BufferWidth) + curLeft - oldLeft + leftDiff;
			if (index < 0 || index >= Builder.Length)
			{
				return;
			}

			Builder.Remove(index, 1);
			Redraw(leftDiff);
		}
	}
}