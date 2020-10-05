using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Linq;
using YACCS.Commands.Models;

namespace YACCS.Tests.Commands
{
	[TestClass]
	public class CommandTrie_Tests
	{
		public const string DUPE_ID = "dupe_id";

		[TestMethod]
		public void AddAndRemove_Tests()
		{
			var trie = new CommandTrie(StringComparer.OrdinalIgnoreCase);

			var c1 = FakeDelegateCommand.New()
				.AddName(new Name(new[] { "1" }))
				.ToCommand();
			Assert.AreEqual(1, trie.Add(c1));
			Assert.AreEqual(1, trie.Count);
			Assert.AreEqual(1, trie.Root["1"].Values.Count);

			var c2 = FakeDelegateCommand.New()
				.AddName(new Name(new[] { "2" }))
				.AddName(new Name(new[] { "3" }))
				.ToCommand();
			Assert.AreEqual(2, trie.Add(c2));
			Assert.AreEqual(2, trie.Count);
			Assert.AreEqual(1, trie.Root["2"].Values.Count);
			Assert.AreEqual(1, trie.Root["3"].Values.Count);

			var c3 = FakeDelegateCommand.New()
				.AddName(new Name(new[] { "4", "1" }))
				.AddName(new Name(new[] { "4", "2" }))
				.AddName(new Name(new[] { "4", "3" }))
				.ToCommand();
			Assert.AreEqual(3, trie.Add(c3));
			Assert.AreEqual(3, trie.Count);
			Assert.AreEqual(0, trie.Root["4"].Values.Count);
			Assert.AreEqual(1, trie.Root["4"]["1"].Values.Count);
			Assert.AreEqual(1, trie.Root["4"]["2"].Values.Count);
			Assert.AreEqual(1, trie.Root["4"]["3"].Values.Count);
			Assert.AreEqual(1, trie.Root["4"].GetCommands().Count);

			var c4 = FakeDelegateCommand.New()
				.AddName(new Name(new[] { "4", "1" }))
				.ToCommand();
			Assert.AreEqual(1, trie.Add(c4));
			Assert.AreEqual(4, trie.Count);
			Assert.AreEqual(0, trie.Root["4"].Values.Count);
			Assert.AreEqual(2, trie.Root["4"]["1"].Values.Count);
			Assert.AreEqual(1, trie.Root["4"]["2"].Values.Count);
			Assert.AreEqual(1, trie.Root["4"]["3"].Values.Count);
			Assert.AreEqual(2, trie.Root["4"].GetCommands().Count);

			var c5 = FakeDelegateCommand.New()
				.AddAttribute(new IdAttribute(DUPE_ID))
				.AddName(new Name(new[] { "5" }))
				.ToCommand();
			Assert.AreEqual(1, trie.Add(c5));
			Assert.AreEqual(5, trie.Count);
			Assert.AreEqual(1, trie.Root["5"].Values.Count);

			var c6 = FakeDelegateCommand.New()
				.AddAttribute(new IdAttribute(DUPE_ID))
				.AddName(new Name(new[] { "6" }))
				.ToCommand();
			Assert.ThrowsException<ArgumentException>(() =>
			{
				trie.Add(c6);
			});
			Assert.ThrowsException<ArgumentException>(() =>
			{
				trie.Remove(c6);
			});

			var c7 = FakeDelegateCommand.New()
				.AddName(new Name(new[] { "4" }))
				.ToCommand();
			Assert.AreEqual(1, trie.Remove(c4));
			Assert.AreEqual(1, trie.Add(c7));
			Assert.AreEqual(1, trie.Root["4"].Values.Count);
			Assert.AreEqual(1, trie.Root["4"]["1"].Values.Count);
			Assert.AreEqual(1, trie.Root["4"]["2"].Values.Count);
			Assert.AreEqual(1, trie.Root["4"]["3"].Values.Count);
			Assert.AreEqual(2, trie.Root["4"].GetCommands().Count);

			Assert.AreEqual(1, trie.Remove(c7));
			Assert.AreEqual(0, trie.Root["4"].Values.Count);
			Assert.AreEqual(1, trie.Root["4"]["1"].Values.Count);
			Assert.AreEqual(1, trie.Root["4"]["2"].Values.Count);
			Assert.AreEqual(1, trie.Root["4"]["3"].Values.Count);
			Assert.AreEqual(1, trie.Root["4"].GetCommands().Count);

			var c8 = FakeDelegateCommand.New()
				.ToCommand();
			Assert.AreEqual(0, trie.Remove(c8));

			foreach (var command in trie.ToList())
			{
				trie.Remove(command);
			}
			Assert.AreEqual(0, trie.Count);
		}
	}
}