using System;
using System.Collections.Generic;
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
			var trie = new CommandTrie(StringComparer.OrdinalIgnoreCase, new TypeReaderRegistry());

			var c1 = FakeDelegateCommand.New()
				.AddName(new Name(new[] { "1" }))
				.ToImmutable()
				.Single();
			Assert.AreEqual(1, trie.Add(c1));
			Assert.AreEqual(1, trie.Count);
			Assert.AreEqual(1, trie.Root["1"].AllValues.Count);
			Assert.AreEqual(1, trie.Root["1"].DirectValues.Count);

			var c2 = FakeDelegateCommand.New()
				.AddName(new Name(new[] { "2" }))
				.AddName(new Name(new[] { "3" }))
				.ToImmutable()
				.Single();
			Assert.AreEqual(2, trie.Add(c2));
			Assert.AreEqual(2, trie.Count);
			Assert.AreEqual(1, trie.Root["2"].AllValues.Count);
			Assert.AreEqual(1, trie.Root["2"].DirectValues.Count);
			Assert.AreEqual(1, trie.Root["3"].AllValues.Count);
			Assert.AreEqual(1, trie.Root["3"].DirectValues.Count);

			var c3 = FakeDelegateCommand.New()
				.AddName(new Name(new[] { "4", "1" }))
				.AddName(new Name(new[] { "4", "2" }))
				.AddName(new Name(new[] { "4", "3" }))
				.ToImmutable()
				.Single();
			Assert.AreEqual(3, trie.Add(c3));
			Assert.AreEqual(3, trie.Count);
			Assert.AreEqual(1, trie.Root["4"].AllValues.Count);
			Assert.AreEqual(0, trie.Root["4"].DirectValues.Count);
			Assert.AreEqual(1, trie.Root["4"]["1"].AllValues.Count);
			Assert.AreEqual(1, trie.Root["4"]["2"].AllValues.Count);
			Assert.AreEqual(1, trie.Root["4"]["3"].AllValues.Count);

			var c4 = FakeDelegateCommand.New()
				.AddName(new Name(new[] { "4", "1" }))
				.ToImmutable()
				.Single();
			Assert.AreEqual(1, trie.Add(c4));
			Assert.AreEqual(4, trie.Count);
			Assert.AreEqual(2, trie.Root["4"].AllValues.Count);
			Assert.AreEqual(0, trie.Root["4"].DirectValues.Count);
			Assert.AreEqual(2, trie.Root["4"]["1"].AllValues.Count);
			Assert.AreEqual(1, trie.Root["4"]["2"].AllValues.Count);
			Assert.AreEqual(1, trie.Root["4"]["3"].AllValues.Count);

			var c5 = FakeDelegateCommand.New()
				.AddAttribute(new IdAttribute(DUPE_ID))
				.AddName(new Name(new[] { "5" }))
				.ToImmutable()
				.Single();
			((ICollection<IImmutableCommand>)trie).Add(c5);
			Assert.AreEqual(5, trie.Count);
			Assert.AreEqual(1, trie.Root["5"].AllValues.Count);
			Assert.AreEqual(1, trie.Root["5"].DirectValues.Count);

			var c6 = FakeDelegateCommand.New()
				.AddName(new Name(new[] { "4" }))
				.ToImmutable()
				.Single();
			Assert.AreEqual(1, trie.Remove(c4));
			Assert.AreEqual(1, trie.Add(c6));
			Assert.AreEqual(2, trie.Root["4"].AllValues.Count);
			Assert.AreEqual(1, trie.Root["4"].DirectValues.Count);
			Assert.AreEqual(1, trie.Root["4"]["1"].AllValues.Count);
			Assert.AreEqual(1, trie.Root["4"]["2"].AllValues.Count);
			Assert.AreEqual(1, trie.Root["4"]["3"].AllValues.Count);

			Assert.IsTrue(((ICollection<IImmutableCommand>)trie).Remove(c6));
			Assert.AreEqual(1, trie.Root["4"].AllValues.Count);
			Assert.AreEqual(0, trie.Root["4"].DirectValues.Count);
			Assert.AreEqual(1, trie.Root["4"]["1"].AllValues.Count);
			Assert.AreEqual(1, trie.Root["4"]["2"].AllValues.Count);
			Assert.AreEqual(1, trie.Root["4"]["3"].AllValues.Count);
			Assert.AreEqual(1, trie.Root["4"].AllValues.Count);

			var c7 = FakeDelegateCommand.New()
				.ToImmutable()
				.Single();
			Assert.AreEqual(0, trie.Remove(c7));

			trie.Clear();
			Assert.AreEqual(0, trie.Count);
		}

		[TestMethod]
		public void Duplicate_Tests()
		{
			var trie = new CommandTrie(StringComparer.OrdinalIgnoreCase, new TypeReaderRegistry());

			var c1 = FakeDelegateCommand.New()
				.AddName(new Name(new[] { "a" }))
				.AddAttribute(new IdAttribute(DUPE_ID))
				.ToImmutable()
				.Single();
			Assert.AreEqual(1, trie.Add(c1));
			Assert.AreEqual(1, trie.Count);
			Assert.AreEqual(1, trie.Root["a"].AllValues.Count);
			Assert.IsTrue(trie.Contains(c1));

			var c2 = FakeDelegateCommand.New()
				.AddName(new Name(new[] { "a" }))
				.AddAttribute(new IdAttribute(DUPE_ID))
				.ToImmutable()
				.Single();
			Assert.IsFalse(trie.Contains(c2));
			Assert.AreEqual(1, trie.Add(c2));
			Assert.IsTrue(trie.Contains(c2));

			var c3 = FakeDelegateCommand.New()
				.AddName(new Name(new[] { "b" }))
				.AddAttribute(new IdAttribute(DUPE_ID))
				.ToImmutable()
				.Single();
			Assert.AreEqual(1, trie.Add(c3));
			Assert.AreEqual(3, trie.Count);
			Assert.AreEqual(1, trie.Root["b"].AllValues.Count);
			Assert.IsTrue(trie.Contains(c3));

			var @delegate = (Action<string>)(x => { });
			var c4 = new DelegateCommand(@delegate, new[] { new Name(new[] { "a" }) })
				.AddAttribute(new IdAttribute(DUPE_ID))
				.ToImmutable()
				.Single();
			Assert.AreEqual(1, trie.Add(c4));
			Assert.AreEqual(4, trie.Count);
			Assert.AreEqual(3, trie.Root["a"].AllValues.Count);
			Assert.IsTrue(trie.Contains(c4));
		}
	}
}