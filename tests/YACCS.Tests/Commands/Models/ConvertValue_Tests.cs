using System;
using System.Threading.Tasks;

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
			var command = new DelegateCommand(@delegate, Array.Empty<IName>()).ToCommand();
			var results = new[]
			{
				await command.ExecuteAsync(null!, null!).ConfigureAwait(false),
				await command.ExecuteAsync(null!, new object[0]).ConfigureAwait(false),
			};

			Assert.AreEqual(2, value);
			for (var i = 0; i < results.Length; ++i)
			{
				var result = results[i];

				Assert.IsTrue(result.IsSuccess);
				Assert.IsInstanceOfType(result.InnerResult, typeof(ValueResult));
				Assert.AreEqual(i + 1, ((ValueResult)result.InnerResult).Value);
			}
		}

		[TestMethod]
		public async Task Task_Test()
		{
			var value = 0;
			var @delegate = (Func<Task>)(() => { ++value; return Task.CompletedTask; });
			var command = new DelegateCommand(@delegate, Array.Empty<IName>()).ToCommand();
			var results = new[]
			{
				await command.ExecuteAsync(null!, null!).ConfigureAwait(false),
				await command.ExecuteAsync(null!, new object[0]).ConfigureAwait(false),
			};

			Assert.AreEqual(2, value);
			foreach (var result in results)
			{
				Assert.IsTrue(result.IsSuccess);
				Assert.IsInstanceOfType(result.InnerResult, typeof(Results.SuccessResult));
			}
		}

		[TestMethod]
		public async Task TaskWithValue_Test()
		{
			var value = 0;
			var @delegate = (Func<Task<int>>)(() => { ++value; return Task.FromResult(value); });
			var command = new DelegateCommand(@delegate, Array.Empty<IName>()).ToCommand();
			var results = new[]
			{
				await command.ExecuteAsync(null!, null!).ConfigureAwait(false),
				await command.ExecuteAsync(null!, new object[0]).ConfigureAwait(false),
			};

			Assert.AreEqual(2, value);
			for (var i = 0; i < results.Length; ++i)
			{
				var result = results[i];

				Assert.IsTrue(result.IsSuccess);
				Assert.IsInstanceOfType(result.InnerResult, typeof(ValueResult));
				Assert.AreEqual(i + 1, ((ValueResult)result.InnerResult).Value);
			}
		}

		[TestMethod]
		public async Task Void_Test()
		{
			var value = 0;
			var @delegate = (Action)(() => ++value);
			var command = new DelegateCommand(@delegate, Array.Empty<IName>()).ToCommand();
			var results = new[]
			{
				await command.ExecuteAsync(null!, null!).ConfigureAwait(false),
				await command.ExecuteAsync(null!, new object[0]).ConfigureAwait(false),
			};

			Assert.AreEqual(2, value);
			foreach (var result in results)
			{
				Assert.IsTrue(result.IsSuccess);
				Assert.IsInstanceOfType(result.InnerResult, typeof(Results.SuccessResult));
			}
		}
	}
}