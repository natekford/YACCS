using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Tests.Commands.Models
{
	[TestClass]
	public class ConvertValue_Tests
	{
		[TestMethod]
		public async Task Result_Test()
		{
			var value = 0;
			var @delegate = (Func<IResult>)(() => { ++value; return new ValueResult(value); });
			var command = new DelegateCommand(@delegate, Array.Empty<ImmutableName>()).ToImmutable();
			var results = new[]
			{
				await command.ExecuteAsync(null!, null!).ConfigureAwait(false),
				await command.ExecuteAsync(null!, Array.Empty<object>()).ConfigureAwait(false),
			};

			Assert.AreEqual(2, value);
			for (var i = 0; i < results.Length; ++i)
			{
				var result = results[i];

				Assert.IsTrue(result.IsSuccess);
				Assert.IsInstanceOfType(result, typeof(ValueResult));
				Assert.AreEqual(i + 1, ((ValueResult)result).Value);
			}
		}

		[TestMethod]
		public async Task Task_Test()
		{
			var value = 0;
			var @delegate = (Func<Task>)(() => { ++value; return Task.CompletedTask; });
			var command = new DelegateCommand(@delegate, Array.Empty<ImmutableName>()).ToImmutable();
			var results = new[]
			{
				await command.ExecuteAsync(null!, null!).ConfigureAwait(false),
				await command.ExecuteAsync(null!, Array.Empty<object>()).ConfigureAwait(false),
			};

			Assert.AreEqual(2, value);
			foreach (var result in results)
			{
				Assert.IsTrue(result.IsSuccess);
				Assert.IsInstanceOfType(result, typeof(SuccessResult));
			}
		}

		[TestMethod]
		public async Task TaskWithValue_Test()
		{
			var value = 0;
			var @delegate = (Func<Task<int>>)(() => { ++value; return Task.FromResult(value); });
			var command = new DelegateCommand(@delegate, Array.Empty<ImmutableName>()).ToImmutable();
			var results = new[]
			{
				await command.ExecuteAsync(null!, null!).ConfigureAwait(false),
				await command.ExecuteAsync(null!, Array.Empty<object>()).ConfigureAwait(false),
			};

			Assert.AreEqual(2, value);
			for (var i = 0; i < results.Length; ++i)
			{
				var result = results[i];

				Assert.IsTrue(result.IsSuccess);
				Assert.IsInstanceOfType(result, typeof(ValueResult));
				Assert.AreEqual(i + 1, ((ValueResult)result).Value);
			}
		}

		[TestMethod]
		public async Task Void_Test()
		{
			var value = 0;
			void @delegate() => ++value;
			var command = new DelegateCommand((Action)@delegate, Array.Empty<ImmutableName>()).ToImmutable();
			var results = new[]
			{
				await command.ExecuteAsync(null!, null!).ConfigureAwait(false),
				await command.ExecuteAsync(null!, Array.Empty<object>()).ConfigureAwait(false),
			};

			Assert.AreEqual(2, value);
			foreach (var result in results)
			{
				Assert.IsTrue(result.IsSuccess);
				Assert.IsInstanceOfType(result, typeof(SuccessResult));
			}
		}
	}
}