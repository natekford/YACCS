using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.Parsing;

namespace YACCS.Tests.Parsing
{
	[TestClass]
	public class ParseArgs_Tests
	{
		//A "B "C \"D E\"" F G"
		private const string INPUT_1 = "A \"B \"C \\\"D E\\\"\" F G\"";
		//H "A "B "C \"D E\"" F G"" I    J
		private const string INPUT_2 = "H \"" + INPUT_1 + "\" I    J";
		//"K" "L M" N "H "A "B "C \"D E\"" F G"" I J" O "H "A "B "C \"D E\"" F G"" I J" P Q
		private const string INPUT_3 = "\"K\" \"L M\" N \"" + INPUT_2 + "\" O \"" + INPUT_2 + "\" P Q";

		private readonly IArgumentSplitter _Splitter = new ArgumentSplitter(CommandServiceConfig.Instance);

		[TestMethod]
		public void Empty_Test()
		{
			const string INPUT = "";
			Assert.IsTrue(_Splitter.TrySplit(INPUT, out var parsed));
			Assert.AreEqual(0, parsed.Length);
		}

		[TestMethod]
		public void Mismatch_Test()
		{
			const string INPUT = "\"an end quote is missing";
			Assert.IsFalse(_Splitter.TrySplit(INPUT, out _));
		}

		[TestMethod]
		public void NestedQuotes1_Test()
		{
			const string INPUT = INPUT_1;
			Assert.IsTrue(_Splitter.TrySplit(INPUT, out var parsed));
			Assert.AreEqual(2, parsed.Length);
			Assert.AreEqual("A", parsed.Span[0]);
			Assert.AreEqual("B \"C \\\"D E\\\"\" F G", parsed.Span[1]);
		}

		[TestMethod]
		public void NestedQuotes2_Test()
		{
			const string INPUT = INPUT_2;
			Assert.IsTrue(_Splitter.TrySplit(INPUT, out var parsed));
			Assert.AreEqual(4, parsed.Length);
			Assert.AreEqual("H", parsed.Span[0]);
			Assert.AreEqual("A \"B \"C \\\"D E\\\"\" F G\"", parsed.Span[1]);
			Assert.AreEqual("I", parsed.Span[2]);
			Assert.AreEqual("J", parsed.Span[3]);
		}

		[TestMethod]
		public void NestedQuotes3_Test()
		{
			const string INPUT = INPUT_3;
			Assert.IsTrue(_Splitter.TrySplit(INPUT, out var parsed));
			Assert.AreEqual(8, parsed.Length);
			Assert.AreEqual("K", parsed.Span[0]);
			Assert.AreEqual("L M", parsed.Span[1]);
			Assert.AreEqual("N", parsed.Span[2]);
			Assert.AreEqual("H \"A \"B \"C \\\"D E\\\"\" F G\"\" I    J", parsed.Span[3]);
			Assert.AreEqual("O", parsed.Span[4]);
			Assert.AreEqual("H \"A \"B \"C \\\"D E\\\"\" F G\"\" I    J", parsed.Span[5]);
			Assert.AreEqual("P", parsed.Span[6]);
			Assert.AreEqual("Q", parsed.Span[7]);
		}

		[TestMethod]
		public void NoQuotes_Test()
		{
			const string INPUT = "these are some arguments";
			Assert.IsTrue(_Splitter.TrySplit(INPUT, out var parsed));
			Assert.AreEqual(INPUT.Split(' ').Length, parsed.Length);
		}

		[TestMethod]
		public void SimpleQuotes_Test()
		{
			const string INPUT = "\"test value \"aaaaaa\" dog\"";
			Assert.IsTrue(_Splitter.TrySplit(INPUT, out var parsed));
			Assert.AreEqual(1, parsed.Length);
			Assert.AreEqual(INPUT[1..^1], parsed.Span[0]);
		}
	}
}