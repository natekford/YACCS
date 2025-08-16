using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Concurrent;
using System.Diagnostics;

using YACCS.Commands;

namespace YACCS.Tests.Commands;

[TestClass]
public sealed class BackgroundCommandQueue_Tests
{
	private readonly BackgroundCommandQueue _Queue = new();

	[TestMethod]
	public async Task Blocking_Test()
	{
		_Queue.Start(1);

		await _Queue.EnqueueAsync(() => Task.Delay(-1)).ConfigureAwait(false);
		var i = 0;
		await _Queue.EnqueueAsync(() => Task.FromResult(++i)).ConfigureAwait(false);
		await Task.Delay(100).ConfigureAwait(false);

		Assert.AreEqual(0, i);
	}

	[TestMethod]
	public async Task Delay_Test()
	{
		_Queue.Start(2);

		const int DELAY = 25;

		var sw = Stopwatch.StartNew();
		var items = new ConcurrentDictionary<int, long>();
		var tcs1 = new TaskCompletionSource();
		await _Queue.EnqueueAsync(async () =>
		{
			await Task.Delay(DELAY).ConfigureAwait(false);
			items[1] = sw.ElapsedMilliseconds;
			if (items.Count == 2)
			{
				tcs1.TrySetResult();
			}
		}).ConfigureAwait(false);
		await _Queue.EnqueueAsync(async () =>
		{
			await Task.Delay(DELAY).ConfigureAwait(false);
			items[2] = sw.ElapsedMilliseconds;
			if (items.Count == 2)
			{
				tcs1.TrySetResult();
			}
		}).ConfigureAwait(false);

		await tcs1.Task.ConfigureAwait(false);
		Assert.HasCount(2, items);

		var tcs2 = new TaskCompletionSource();
		var tcs3 = new TaskCompletionSource();
		await _Queue.EnqueueAsync(async () =>
		{
			await Task.Delay(DELAY).ConfigureAwait(false);
			items[3] = sw.ElapsedMilliseconds;
			if (items.Count == 4)
			{
				tcs2.TrySetResult();
			}
		}).ConfigureAwait(false);
		await _Queue.EnqueueAsync(async () =>
		{
			await Task.Delay(DELAY).ConfigureAwait(false);
			items[4] = sw.ElapsedMilliseconds;
			if (items.Count == 4)
			{
				tcs2.TrySetResult();
			}
		}).ConfigureAwait(false);
		await _Queue.EnqueueAsync(async () =>
		{
			await Task.Delay(DELAY).ConfigureAwait(false);
			items[5] = sw.ElapsedMilliseconds;
			if (items.Count == 5)
			{
				tcs3.TrySetResult();
			}
			else
			{
				tcs3.SetException(new Exception("tcs3 not set"));
			}
		}).ConfigureAwait(false);

		await tcs2.Task.ConfigureAwait(false);
		Assert.HasCount(4, items);

		await tcs3.Task.ConfigureAwait(false);
		Assert.HasCount(5, items);

		Assert.AreEqual(items[1], items[2], DELAY / 2);
		Assert.AreNotEqual(items[2], items[3], DELAY);
		Assert.AreEqual(items[3], items[4], DELAY / 2);
		Assert.AreNotEqual(items[4], items[5], DELAY);
	}

	[TestMethod]
	public async Task Exception_Test()
	{
		_Queue.Start(1);

		await _Queue.EnqueueAsync(() => throw new Exception()).ConfigureAwait(false);
		await Task.Delay(50).ConfigureAwait(false);

		Assert.ThrowsExactly<AggregateException>(
			() => _Queue.EnqueueAsync(() => Task.CompletedTask));
	}
}