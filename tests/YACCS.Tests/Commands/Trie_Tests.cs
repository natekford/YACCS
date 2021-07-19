using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;

namespace YACCS.Tests.Commands
{
	[TestClass]
	public class Trie_Tests
	{
		private const int SIZE = 100000;
		private readonly TestTrie _Trie = new();

		[TestMethod]
		public void Stress_Test()
		{
			_ = Task.Run(() =>
			{
				for (var i = 0; i < SIZE; ++i)
				{
					_Trie.Add(i);
				}
			});

			var removedOnce = false;
			while (_Trie.Count < SIZE / 100)
			{
				var removedCount = _Trie.Remove(2);
				if (removedCount == 1)
				{
					Assert.IsFalse(removedOnce);
					removedOnce = true;
					continue;
				}
				Assert.AreEqual(0, removedCount);
			}

			var items = new List<int>();
			foreach (var item in _Trie)
			{
				items.Add(item);
			}
			// This ends up getting about 1500-2000 items
			// Start at 5 because we remove 2
			for (var i = 5; i < items.Count; ++i)
			{
				Assert.AreEqual(items[i - 1] + 1, items[i]);
			}

			while (_Trie.Count < SIZE - 1)
			{
			}

			var dict = new ConcurrentDictionary<int, byte>();
			foreach (var item in _Trie)
			{
				dict.TryAdd(item, 0);
			}

			var swRemoveFromDict = new Stopwatch();
			swRemoveFromDict.Start();
			dict.TryRemove(50, out _);
			swRemoveFromDict.Stop();

			var swRemoveFromTrie = new Stopwatch();
			swRemoveFromTrie.Start();
			Assert.AreEqual(1, _Trie.Remove(50));
			swRemoveFromTrie.Stop();

			Assert.AreEqual(1, _Trie.Remove((int)(SIZE * 0.10)));
			Assert.AreEqual(1, _Trie.Remove((int)(SIZE * 0.15)));
			Assert.AreEqual(1, _Trie.Remove((int)(SIZE * 0.20)));
			Assert.AreEqual(1, _Trie.Remove((int)(SIZE * 0.35)));
			Assert.AreEqual(1, _Trie.Remove((int)(SIZE * 0.55)));
			Assert.AreEqual(SIZE - 7, _Trie.Count);
		}

		private sealed class TestTrie : Trie<int, int>
		{
			public TestTrie() : base(EqualityComparer<int>.Default)
			{
			}

			protected override IEnumerable<IReadOnlyList<int>> GetPaths(int item)
				=> new[] { new[] { item } };
		}
	}
}