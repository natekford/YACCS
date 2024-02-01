using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Trie;

namespace YACCS.Tests.Trie;

[TestClass]
public sealed class Trie_Tests
{
	private const int SIZE = 100;
	private static readonly TimeSpan Delay = TimeSpan.FromMilliseconds(1);

	[TestMethod]
	public void CopyTo_Test()
	{
		var values = new HashSet<int>()
		{
			8, 1, 2, 4, 5, 3, 7, 6, 9, 0
		};

		var trie = new IntTrie();
		foreach (var value in values)
		{
			trie.Add(value);
		}
		Assert.AreEqual(values.Count, trie.Count);

		var output = new int[values.Count];
		trie.CopyTo(output, 0);

		Assert.IsTrue(values.SetEquals(output));
	}

	[TestMethod]
	public void DuplicateIgnored_Test()
	{
		var trie = new IntTrie();
		Assert.AreEqual(0, trie.Count);
		trie.Add(1);
		Assert.AreEqual(1, trie.Count);
		trie.Add(1);
		Assert.AreEqual(1, trie.Count);
	}

	[TestMethod]
	public async Task Threading_Test()
	{
		await Threading_Test(new HashSet<int>()).ConfigureAwait(false);
		await Threading_Test(new IntTrie()).ConfigureAwait(false);
	}

	private static async Task Threading_Test(ICollection<int> collection)
	{
		var wrapper = new IntTrieWrapper(collection);
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

	private sealed class IntTrie : Trie<int, int>
	{
		public IntTrie() : base(EqualityComparer<int>.Default)
		{
		}

		protected override IEnumerable<IReadOnlyList<int>> GetPaths(int item)
			=> new[] { new[] { item } };
	}

	private sealed class IntTrieWrapper(ICollection<int> collection)
	{
		public int ExpectedCount { get; set; } = SIZE;

		public void Remove(int value)
		{
			Assert.IsTrue(collection.Remove(value));
			--ExpectedCount;
		}
	}
}