#if true

using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;

namespace YACCS.Tests.Commands
{
	[TestClass]
	public class Trie_Tests
	{
		private const int SIZE = 300000;

		[TestMethod]
		public void Threading_Test()
		{
			Threading_Test(new HashSet<int>());
			Threading_Test(new TestTrie());
		}

		private static void Threading_Test(ICollection<int> collection)
		{
			_ = Task.Run(() =>
			{
				for (var i = 0; i < SIZE; ++i)
				{
					collection.Add(i);
				}
			});

			while (collection.Count < SIZE / 100)
			{
			}

			// Originally this was testing ToArray()/ToList() but that only seemed to
			// throw about 75% of the time, so maybe calling CopyTo directly will
			// throw 100% of the time?
			Assert.ThrowsException<ArgumentException>(() =>
			{
				var array = new int[collection.Count];
				collection.CopyTo(array, 0);
			});

			Assert.ThrowsException<InvalidOperationException>(() =>
			{
				foreach (var item in collection)
				{
				}
			});

			while (collection.Count < SIZE)
			{
			}

			var set = collection.ToHashSet();
			Assert.IsTrue(set.Remove(50));
			Assert.IsTrue(collection.Remove(50));
			Assert.AreEqual(set.Count, collection.Count);

			Assert.IsTrue(collection.Remove((int)(SIZE * 0.10)));
			Assert.IsTrue(collection.Remove((int)(SIZE * 0.15)));
			Assert.IsTrue(collection.Remove((int)(SIZE * 0.20)));
			Assert.IsTrue(collection.Remove((int)(SIZE * 0.35)));
			Assert.IsTrue(collection.Remove((int)(SIZE * 0.55)));
			Assert.AreEqual(SIZE - 6, collection.Count);
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

#endif