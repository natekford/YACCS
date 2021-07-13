using System;
using System.ComponentModel;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace YACCS.Tests.Commands
{
	[TestClass]
	public class AsyncEvent_Tests
	{
		private readonly FakeEventArgs _Args = new();
		private readonly AsyncEvent<FakeEventArgs> _Event = new();

		[TestMethod]
		public async Task EmptyInvoke_Test()
			=> await _Event.InvokeAsync(_Args).ConfigureAwait(false);

		[TestMethod]
		public async Task Exception_Test()
		{
			var value1 = 0;
			var value2 = 0;
			Task Handler(FakeEventArgs e)
			{
				value1 = 1;
				throw new InvalidOperationException();
			}

			Task ExceptionHandler(ExceptionEventArgs<FakeEventArgs> e)
			{
				Assert.AreSame(_Args, e.EventArgs);
				Assert.AreEqual(1, e.Exceptions.Count);
				Assert.IsInstanceOfType(e.Exceptions[0], typeof(InvalidOperationException));
				value2 = 1;
				return Task.CompletedTask;
			}

			_Event.Add(Handler);
			_Event.Exception.Add(ExceptionHandler);

			await _Event.InvokeAsync(_Args).ConfigureAwait(false);
			Assert.AreEqual(1, value1);
			Assert.AreEqual(1, value2);
		}

		[TestMethod]
		public async Task Handled_Test()
		{
			var value = 0;
			static Task Handler1(FakeEventArgs e)
			{
				e.Handled = true;
				return Task.CompletedTask;
			}

			Task Handler2(FakeEventArgs e)
			{
				value = 1;
				return Task.CompletedTask;
			}

			_Event.Add(Handler1);
			_Event.Remove(Handler2);

			await _Event.InvokeAsync(_Args).ConfigureAwait(false);
			Assert.AreEqual(0, value);
		}

		[TestMethod]
		public void NullEvent_Test()
		{
			Assert.ThrowsException<ArgumentNullException>(() =>
			{
				_Event.Add(null!);
			});
			Assert.ThrowsException<ArgumentNullException>(() =>
			{
				_Event.Remove(null!);
			});
		}

		[TestMethod]
		public async Task Remove_Test()
		{
			var value = 0;
			Task Handler(FakeEventArgs e)
			{
				value = 1;
				return Task.CompletedTask;
			}

			_Event.Add(Handler);
			_Event.Remove(Handler);

			await _Event.InvokeAsync(_Args).ConfigureAwait(false);
			Assert.AreEqual(0, value);
		}

		private class FakeEventArgs : HandledEventArgs
		{
		}
	}
}