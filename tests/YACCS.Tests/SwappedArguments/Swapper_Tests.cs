using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.SwappedArguments;

namespace YACCS.Tests.SwappedArguments;

[TestClass]
public class Swapper_Tests
{
	[TestMethod]
	public void Basic_Test()
	{
		var indices = new int[] { 4, 2, 3 };
		var swapper = new Swapper(indices);

		var original = Enumerable.Range(0, 10).ToArray();
		var copy = swapper.SwapForwards(original);

		var expected = original.ToArray();
		expected[2] = original[4];
		expected[3] = original[2];
		expected[4] = original[3];
		Assert.IsTrue(expected.SequenceEqual(copy));

		var copy2 = swapper.SwapBackwards(copy);
		Assert.IsTrue(original.SequenceEqual(copy2));
	}

	[TestMethod]
	public void IndexOutOfRange_Test()
	{
		var swapper = new Swapper(new[] { 99, 1 });

		Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
		{
			_ = swapper.SwapForwards(new[] { 0, 1 }).ToList();
		});
	}

	[TestMethod]
	public void Permutations_Test()
	{
		var indices = new int[] { 2, 3, 4, 7, 9 };
		var original = Enumerable.Range(0, 10).ToArray();

		var orderings = new List<int[]>
		{
			original
		};
		foreach (var swapper in Swapper.CreateSwappers(indices))
		{
			var copy = swapper.SwapForwards(original);
			Assert.IsFalse(orderings.Any(x => x.SequenceEqual(copy)));
			orderings.Add([.. copy]);

			var copy2 = swapper.SwapBackwards(copy);
			Assert.IsTrue(original.SequenceEqual(copy2));
		}
		Assert.AreEqual(120, orderings.Count);
	}

	[TestMethod]
	public void SwapTwoItems_Test()
	{
		var swapper = new Swapper(new[] { 1, 0 });
		var original = new[] { 'a', 'b' };

		var copy = swapper.SwapForwards(original);
		Assert.IsTrue(new[] { 'b', 'a' }.SequenceEqual(copy));

		var copy2 = swapper.SwapBackwards(copy);
		Assert.IsTrue(original.SequenceEqual(copy2));
	}
}