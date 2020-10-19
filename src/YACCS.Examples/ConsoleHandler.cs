#define READLINE
#define READKEY

using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using YACCS.Results;

#if READLINE
#elif READKEY
using System.Collections.Generic;
using System.Text;
#endif

namespace YACCS.Examples
{
	public class ConsoleHandler
	{
		private readonly Channel<string?> _Channel;
		private readonly SemaphoreSlim _Input;
		private readonly ITypeRegistry<string> _Names;
		private readonly SemaphoreSlim _Output;
		private int _OldPos;

#if READLINE
		private string? _LastInput;
#elif READKEY
		private int _CurPos;
		private string _Empty = new string(' ', 256);
		private readonly List<string> _History = new List<string>();
		private int? _HistoryIndex;
		private readonly StringBuilder _Sb = new StringBuilder(256);
#endif

		public ConsoleHandler(ITypeRegistry<string> names)
		{
			_Names = names;
			_Input = new SemaphoreSlim(1, 1);
			_Output = new SemaphoreSlim(1, 1);
			_Channel = Channel.CreateUnbounded<string?>(new UnboundedChannelOptions
			{
				SingleReader = true,
				SingleWriter = true,
				AllowSynchronousContinuations = false,
			});
			_ = Task.Run(async () =>
			{
#if READLINE
				while (true)
				{
					_LastInput = Console.ReadLine();
					await _Channel.Writer.WriteAsync(_LastInput).ConfigureAwait(false);
				}
#elif READKEY
				while (true)
				{
					var key = Console.ReadKey(true);
					_CurPos = GetAbsolutePosition();
					if (_Sb.Length == 0)
					{
						_OldPos = _CurPos;
					}

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
							if (_Sb.Length > _Empty.Length)
							{
								_Empty = new string(' ', _Sb.Length);
							}
							_History.Add(_Sb.ToString());
							_HistoryIndex = null;
							_Sb.Clear();

							Console.WriteLine();
							await _Channel.Writer.WriteAsync(_History[^1]).ConfigureAwait(false);
							break;

						default:
							_Sb.Append(key.KeyChar);
							Console.Write(key.KeyChar);
							break;
					}
				}
#endif
			});
		}

		public async Task<string?> ReadLineAsync()
		{
			try
			{
				await _Channel.Reader.WaitToReadAsync().ConfigureAwait(false);
				return await _Channel.Reader.ReadAsync().ConfigureAwait(false);
			}
			catch (OperationCanceledException)
			{
				return null;
			}
		}

		public void ReleaseInputLock()
			=> _Input.ReleaseIfZero();

		public void ReleaseIOLocks()
		{
			ReleaseInputLock();
			ReleaseOutputLock();
		}

		public void ReleaseOutputLock()
			=> _Output.ReleaseIfZero();

		public async Task WaitForBothIOLocksAsync()
		{
			await _Input.WaitAsync().ConfigureAwait(false);
			await _Output.WaitAsync().ConfigureAwait(false);
		}

		public void WriteLine(string input = "", ConsoleColor? color = null)
		{
#if READLINE
			var (left, top) = Console.GetCursorPosition();
			var (oldLeft, oldTop) = GetPositionCoords(_OldPos);
			if (top < oldTop || (top == oldTop && left < oldLeft))
			{
				Console.SetCursorPosition(oldLeft, oldTop);
				Console.WriteLine(_LastInput);
				Console.WriteLine();
			}
#endif

			var oldColor = Console.ForegroundColor;
			Console.ForegroundColor = color ?? Console.ForegroundColor;
			Console.WriteLine(input);
			Console.ForegroundColor = oldColor;

			_OldPos = GetAbsolutePosition();
#if READLINE
#elif READKEY
			_CurPos = _OldPos;
#endif
		}

		public void WriteResult(IResult result)
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

		private int GetAbsolutePosition()
		{
			var (left, top) = Console.GetCursorPosition();
			return left + (top * Console.BufferWidth);
		}

		private (int Left, int Top) GetPositionCoords(int position)
		{
			var left = position % Console.BufferWidth;
			var top = position / Console.BufferWidth;
			return (left, top);
		}

#if READLINE
#elif READKEY

		private void Move(int leftDiff)
		{
			var (left, top) = GetPositionCoords(_CurPos);

			var newLeft = Math.Min(_Sb!.Length, Math.Max(left + leftDiff, 0));
			Console.SetCursorPosition(newLeft, top);
		}

		private void Remove(int add)
		{
			var (curLeft, curTop) = GetPositionCoords(_CurPos);
			var (oldLeft, oldTop) = GetPositionCoords(_OldPos);

			var index = ((curTop - oldTop) * Console.BufferWidth) + curLeft - oldLeft + add;
			if (index < 0 || index >= _Sb!.Length)
			{
				return;
			}

			_Sb.Remove(index, 1);
			Redraw(add);
		}

		private void Redraw(int leftDiff)
		{
			var (curLeft, _) = GetPositionCoords(_CurPos);
			var (oldLeft, oldTop) = GetPositionCoords(_OldPos);

			Console.SetCursorPosition(oldLeft, oldTop);
			Console.Write(_Sb.ToString());
			Console.Write(_Empty);
			Console.WriteLine();

			var left = curLeft + (leftDiff % Console.BufferWidth);
			if (left < 0)
			{
				// Wrap around
				left += Console.BufferWidth;
			}
			var top = oldTop + (_Sb.Length / Console.BufferWidth);
			Console.SetCursorPosition(left, top);
		}

		private void History(int change)
		{
			var (curLeft, _) = GetPositionCoords(_CurPos);

			_HistoryIndex ??= _History.Count;
			_HistoryIndex += change;
			_HistoryIndex = Math.Max(Math.Min(_HistoryIndex.Value, _History.Count - 1), 0);

			_Sb.Clear();
			if (_History.Count != 0)
			{
				_Sb.Append(_History[_HistoryIndex.Value]);
			}
			Redraw(_Sb.Length - curLeft);
		}

#endif
	}
}