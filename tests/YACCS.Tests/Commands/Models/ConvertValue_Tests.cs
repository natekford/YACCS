using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Tests.Commands.Models;

[TestClass]
public class ConvertValue_Tests
{
	[TestMethod]
	public async Task Result_Test()
	{
		var value = 0;
		var command = new DelegateCommand(() =>
		{
			++value;
			return new ValueResult(value);
		}, []).ToImmutable();
		var results = new[]
		{
			await command.ExecuteAsync(null!, null!).ConfigureAwait(false),
			await command.ExecuteAsync(null!, []).ConfigureAwait(false),
		};

		Assert.AreEqual(2, value);
		for (var i = 0; i < results.Length; ++i)
		{
			var result = results[i];

			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType<ValueResult>(result);
			Assert.AreEqual(i + 1, ((ValueResult)result).Value);
		}
	}

	[TestMethod]
	public async Task Task_Test()
	{
		var value = 0;
		var command = new DelegateCommand(() =>
		{
			++value;
			return Task.CompletedTask;
		}, []).ToImmutable();
		var results = new[]
		{
			await command.ExecuteAsync(null!, null!).ConfigureAwait(false),
			await command.ExecuteAsync(null!, []).ConfigureAwait(false),
		};

		Assert.AreEqual(2, value);
		foreach (var result in results)
		{
			Assert.IsTrue(result.IsSuccess);
			Assert.AreSame(Result.EmptySuccess, result);
		}
	}

	[TestMethod]
	public async Task TaskWithValue_Test()
	{
		var value = 0;
		var command = new DelegateCommand(() =>
		{
			++value;
			return Task.FromResult(value);
		}, []).ToImmutable();
		var results = new[]
		{
			await command.ExecuteAsync(null!, null!).ConfigureAwait(false),
			await command.ExecuteAsync(null!, []).ConfigureAwait(false),
		};

		Assert.AreEqual(2, value);
		for (var i = 0; i < results.Length; ++i)
		{
			var result = results[i];

			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType<ValueResult>(result);
			Assert.AreEqual(i + 1, ((ValueResult)result).Value);
		}
	}

	[TestMethod]
	public async Task Void_Test()
	{
		var value = 0;
		var command = new DelegateCommand(() =>
		{
			++value;
			// This can't be () => ++value otherwise it returns the value instead of
			// being a void delegate
		}, []).ToImmutable();
		var results = new[]
		{
			await command.ExecuteAsync(null!, null!).ConfigureAwait(false),
			await command.ExecuteAsync(null!, []).ConfigureAwait(false),
		};

		Assert.AreEqual(2, value);
		foreach (var result in results)
		{
			Assert.IsTrue(result.IsSuccess);
			Assert.AreSame(Result.EmptySuccess, result);
		}
	}
}