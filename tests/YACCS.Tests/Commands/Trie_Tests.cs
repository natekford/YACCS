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

			while (collection.Count < 10)
			{
			}

			Assert.ThrowsException<InvalidOperationException>(() =>
			{
				foreach (var item in collection)
				{
				}
			});

			// Originally this was testing ToArray()/ToList() but that only seemed to
			// throw about 75% of the time, so maybe calling CopyTo directly will
			// throw 100% of the time?
			//
			// I don't know at this point, the only way I can think of the exception
			// not being thrown is if the collection gets finished before this is called
			// so I guess let's do that?
			//
			// Ok, so even though count != SIZE somehow the exception is still not happening
			// Maybe adding a sleep before it will for sure get some new items added and
			// cause the exception.
			var count = collection.Count;
			if (count != SIZE)
			{
				Thread.Sleep(25);
				Assert.ThrowsException<ArgumentException>(() =>
				{
					var array = new int[count];
					collection.CopyTo(array, 0);
				});
			}

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