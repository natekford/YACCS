using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.SwapArguments;

namespace YACCS.Tests.SwapArguments
{
	[TestClass]
	public class Swapper_Tests
	{
		[TestMethod]
		public void Basic_Test()
		{
			var swapper = new Swapper(new int[] { 4, 2, 3 });
			var original = Enumerable.Range(0, 10).ToArray();

			var copy = original.ToArray();
			swapper.Swap(copy);

			var expected = original.ToArray();
			expected[2] = 4;
			expected[3] = 2;
			expected[4] = 3;
			Assert.IsTrue(expected.SequenceEqual(copy));

			swapper.SwapBack(copy);
			Assert.IsTrue(original.SequenceEqual(copy));
		}

		[TestMethod]
		public void IndexOutOfRange_Test()
		{
			var swapper = new Swapper(new[] { 99, 1 });

			Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
			{
				swapper.Swap(new[] { 1 });
			});
		}

		[TestMethod]
		public void Permutations_Test()
		{
			var indices = new int[] { 2, 3, 4, 7, 9 };
			var original = Enumerable.Range(0, 10).ToArray();
			var swappers = Swapper.CreateSwappers(indices).ToList();

			var orderings = new List<int[]>
			{
				original
			};
			foreach (var swapper in swappers)
			{
				var copy = original.ToArray();
				swapper.Swap(copy);
				Assert.IsFalse(orderings.Any(x => x.SequenceEqual(copy)));
				orderings.Add(copy);

				var copy2 = copy.ToArray();
				swapper.SwapBack(copy2);
				Assert.IsTrue(original.SequenceEqual(copy2));
			}
			Assert.AreEqual(120, orderings.Count);
		}
	}
}