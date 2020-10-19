#define READLINE
#define READKEY

using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using YACCS.Results;

namespace YACCS.Examples
{
	public class ConsoleHandler
	{
		private readonly Channel<string?> _Channel;
		private readonly SemaphoreSlim _Input;
		private readonly ITypeRegistry<string> _Names;
		private readonly SemaphoreSlim _Output;

#if READLINE
		private string? _LastInput;
		private (int Left, int Top) _LastPos = (0, 0);
#elif READKEY
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
				var history = new List<string>();
				var input = new StringBuilder(256);
				var (left, top) = Console.GetCursorPosition();

				void Move(int add)
				{
					var (l, t) = Console.GetCursorPosition();
					var newL = Math.Min(input!.Length, Math.Max(l + add, 0));
					Console.SetCursorPosition(newL, t);
				}

				void Remove(int add)
				{
					var (l, _) = Console.GetCursorPosition();
					var index = l - left + add;
					if (index >= input!.Length)
					{
						return;
					}

					input.Remove(index, 1);
					Redraw();
				}

				void Redraw()
				{
					var (l, t) = Console.GetCursorPosition();

					Console.SetCursorPosition(left, top);
					Console.WriteLine(input.ToString());
					Console.SetCursorPosition(l, t);
				}

				while (true)
				{
					var key = Console.ReadKey(true);

					switch (key.Key)
					{
						case ConsoleKey.LeftArrow:
							Move(-1);
							break;

						case ConsoleKey.RightArrow:
							Move(1);
							break;

						case ConsoleKey.Delete:
							Remove(1);
							break;

						case ConsoleKey.Backspace:
							Remove(0);
							break;

						case ConsoleKey.Enter:
							history.Add(input.ToString());
							input.Clear();
							await _Channel.Writer.WriteAsync(history[^1]).ConfigureAwait(false);
							break;

						default:
							if (input.Length == 0)
							{
								(left, top) = Console.GetCursorPosition();
							}
							input.Append(key.KeyChar);
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
			if (top < _LastPos.Top || (top == _LastPos.Top && left < _LastPos.Left))
			{
				Console.SetCursorPosition(_LastPos.Left, _LastPos.Top);
				Console.WriteLine(_LastInput);
				Console.WriteLine();
			}
#endif

			var oldColor = Console.ForegroundColor;
			Console.ForegroundColor = color ?? Console.ForegroundColor;
			Console.WriteLine(input);
			Console.ForegroundColor = oldColor;

#if READLINE
			_LastPos = Console.GetCursorPosition();
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
	}
}