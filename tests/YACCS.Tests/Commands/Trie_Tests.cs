using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Trie;

namespace YACCS.Tests.Commands
{
	[TestClass]
	public sealed class Trie_Tests
	{
		private const int SIZE = 100;
		private static readonly TimeSpan Delay = TimeSpan.FromMilliseconds(1);

		[TestMethod]
		public async Task Threading_Test()
		{
			await Threading_Test(new HashSet<int>()).ConfigureAwait(false);
			await Threading_Test(new TestTrie()).ConfigureAwait(false);
		}

		private static async Task Threading_Test(ICollection<int> collection)
		{
			var wrapper = new Wrapper(collection);
			var tcs = new TaskCompletionSource();
			var task = Task.Run(async () =>
			{
				for (var i = 0; i < SIZE; ++i)
				{
					if (!tcs.Task.IsCompleted && i >= (Delay * 5).TotalMilliseconds)
					{
						tcs.SetResult();
					}

					collection.Add(i);
					await Task.Delay(Delay).ConfigureAwait(false);
				}
			});

			await tcs.Task.ConfigureAwait(false);

			await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
			{
				foreach (var _ in collection)
				{
					await Task.Delay(Delay * 2).ConfigureAwait(false);
				}
			}).ConfigureAwait(false);

			wrapper.Remove(2);
			await task.ConfigureAwait(false);

			var set = collection.ToHashSet();
			Assert.IsTrue(set.Remove(SIZE / 2));
			wrapper.Remove(SIZE / 2);
			Assert.AreEqual(set.Count, collection.Count);

			wrapper.Remove((int)(SIZE * 0.10));
			wrapper.Remove((int)(SIZE * 0.15));
			wrapper.Remove((int)(SIZE * 0.20));
			wrapper.Remove((int)(SIZE * 0.35));
			wrapper.Remove((int)(SIZE * 0.55));
			Assert.AreEqual(wrapper.ExpectedCount, collection.Count);
		}

		private sealed class TestTrie : Trie<int, int>
		{
			public TestTrie() : base(EqualityComparer<int>.Default)
			{
			}

			protected override IEnumerable<IReadOnlyList<int>> GetPaths(int item)
				=> new[] { new[] { item } };
		}

		private sealed class Wrapper
		{
			private readonly ICollection<int> _Collection;
			public int ExpectedCount { get; set; } = SIZE;

			public Wrapper(ICollection<int> collection)
			{
				_Collection = collection;
			}

			public void Remove(int value)
			{
				Assert.IsTrue(_Collection.Remove(value));
				--ExpectedCount;
			}
		}
	}
}