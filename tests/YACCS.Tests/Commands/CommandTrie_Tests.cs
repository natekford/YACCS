using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Linq;
using YACCS.Commands.Models;
using YACCS.TypeReaders;

namespace YACCS.Tests.Commands
{
	[TestClass]
	public class CommandTrie_Tests
	{
		private const string DUPE_ID = "dupe_id";
		private readonly CommandTrie _Trie
			= new(new TypeReaderRegistry(), CommandServiceConfig.Default);

		[TestMethod]
		public void AddAndRemove_Tests()
		{
			var c1 = FakeDelegateCommand.New()
				.AddName(new[] { "1" })
				.ToImmutable()
				.Single();
			Assert.AreEqual(1, _Trie.Add(c1));
			Assert.AreEqual(1, _Trie.Count);
			Assert.AreEqual(1, _Trie.Root["1"].GetAllItems().Count);
			Assert.AreEqual(1, _Trie.Root["1"].Items.Count);

			var c2 = FakeDelegateCommand.New()
				.AddName(new[] { "2" })
				.AddName(new[] { "3" })
				.ToImmutable()
				.Single();
			Assert.AreEqual(2, _Trie.Add(c2));
			Assert.AreEqual(2, _Trie.Count);
			Assert.AreEqual(1, _Trie.Root["2"].GetAllItems().Count);
			Assert.AreEqual(1, _Trie.Root["2"].Items.Count);
			Assert.AreEqual(1, _Trie.Root["3"].GetAllItems().Count);
			Assert.AreEqual(1, _Trie.Root["3"].Items.Count);

			var c3 = FakeDelegateCommand.New()
				.AddName(new[] { "4", "1" })
				.AddName(new[] { "4", "2" })
				.AddName(new[] { "4", "3" })
				.ToImmutable()
				.Single();
			Assert.AreEqual(3, _Trie.Add(c3));
			Assert.AreEqual(3, _Trie.Count);
			Assert.AreEqual(1, _Trie.Root["4"].GetAllItems().Count);
			Assert.AreEqual(0, _Trie.Root["4"].Items.Count);
			Assert.AreEqual(1, _Trie.Root["4"]["1"].GetAllItems().Count);
			Assert.AreEqual(1, _Trie.Root["4"]["2"].GetAllItems().Count);
			Assert.AreEqual(1, _Trie.Root["4"]["3"].GetAllItems().Count);

			var c4 = FakeDelegateCommand.New()
				.AddName(new[] { "4", "1" })
				.ToImmutable()
				.Single();
			Assert.AreEqual(1, _Trie.Add(c4));
			Assert.AreEqual(4, _Trie.Count);
			Assert.AreEqual(2, _Trie.Root["4"].GetAllItems().Count);
			Assert.AreEqual(0, _Trie.Root["4"].Items.Count);
			Assert.AreEqual(2, _Trie.Root["4"]["1"].GetAllItems().Count);
			Assert.AreEqual(1, _Trie.Root["4"]["2"].GetAllItems().Count);
			Assert.AreEqual(1, _Trie.Root["4"]["3"].GetAllItems().Count);

			var c5 = FakeDelegateCommand.New()
				.AddAttribute(new IdAttribute(DUPE_ID))
				.AddName(new[] { "5" })
				.ToImmutable()
				.Single();
			((ICollection<IImmutableCommand>)_Trie).Add(c5);
			Assert.AreEqual(5, _Trie.Count);
			Assert.AreEqual(1, _Trie.Root["5"].GetAllItems().Count);
			Assert.AreEqual(1, _Trie.Root["5"].Items.Count);

			var c6 = FakeDelegateCommand.New()
				.AddName(new[] { "4" })
				.ToImmutable()
				.Single();
			Assert.AreEqual(1, _Trie.Remove(c4));
			Assert.AreEqual(1, _Trie.Add(c6));
			Assert.AreEqual(2, _Trie.Root["4"].GetAllItems().Count);
			Assert.AreEqual(1, _Trie.Root["4"].Items.Count);
			Assert.AreEqual(1, _Trie.Root["4"]["1"].GetAllItems().Count);
			Assert.AreEqual(1, _Trie.Root["4"]["2"].GetAllItems().Count);
			Assert.AreEqual(1, _Trie.Root["4"]["3"].GetAllItems().Count);

			Assert.IsTrue(((ICollection<IImmutableCommand>)_Trie).Remove(c6));
			Assert.AreEqual(1, _Trie.Root["4"].GetAllItems().Count);
			Assert.AreEqual(0, _Trie.Root["4"].Items.Count);
			Assert.AreEqual(1, _Trie.Root["4"]["1"].GetAllItems().Count);
			Assert.AreEqual(1, _Trie.Root["4"]["2"].GetAllItems().Count);
			Assert.AreEqual(1, _Trie.Root["4"]["3"].GetAllItems().Count);
			Assert.AreEqual(1, _Trie.Root["4"].GetAllItems().Count);

			var c7 = FakeDelegateCommand.New()
				.ToImmutable()
				.Single();
			Assert.AreEqual(0, _Trie.Remove(c7));

			_Trie.Clear();
			Assert.AreEqual(0, _Trie.Count);
		}

		[TestMethod]
		public void ContainsLotsOfName_Test()
		{
			var command = FakeDelegateCommand.New();
			const int COUNT = 100000;
			for (var i = 0; i < COUNT; ++i)
			{
				command.AddName(new[] { i.ToString() });
			}
			var immutable = command.ToImmutable().Single();

			Assert.IsFalse(_Trie.Contains(immutable));
			Assert.AreEqual(COUNT, _Trie.Add(immutable));
			Assert.IsTrue(_Trie.Contains(immutable));
		}

		[TestMethod]
		public void Duplicate_Tests()
		{
			var c1 = FakeDelegateCommand.New()
				.AddName(new[] { "a" })
				.AddAttribute(new IdAttribute(DUPE_ID))
				.ToImmutable()
				.Single();
			Assert.AreEqual(1, _Trie.Add(c1));
			Assert.AreEqual(1, _Trie.Count);
			Assert.AreEqual(1, _Trie.Root["a"].GetAllItems().Count);
			Assert.IsTrue(_Trie.Contains(c1));

			var c2 = FakeDelegateCommand.New()
				.AddName(new[] { "a" })
				.AddAttribute(new IdAttribute(DUPE_ID))
				.ToImmutable()
				.Single();
			Assert.IsFalse(_Trie.Contains(c2));
			Assert.AreEqual(1, _Trie.Add(c2));
			Assert.IsTrue(_Trie.Contains(c2));

			var c3 = FakeDelegateCommand.New()
				.AddName(new[] { "b" })
				.AddAttribute(new IdAttribute(DUPE_ID))
				.ToImmutable()
				.Single();
			Assert.AreEqual(1, _Trie.Add(c3));
			Assert.AreEqual(3, _Trie.Count);
			Assert.AreEqual(1, _Trie.Root["b"].GetAllItems().Count);
			Assert.IsTrue(_Trie.Contains(c3));

			var @delegate = (Action<string>)(x => { });
			var c4 = new DelegateCommand(@delegate, new[] { new[] { "a" } })
				.AddAttribute(new IdAttribute(DUPE_ID))
				.ToImmutable()
				.Single();
			Assert.AreEqual(1, _Trie.Add(c4));
			Assert.AreEqual(4, _Trie.Count);
			Assert.AreEqual(3, _Trie.Root["a"].GetAllItems().Count);
			Assert.IsTrue(_Trie.Contains(c4));
		}

		[TestMethod]
		public void NameWithSeparator_Test()
		{
			var command = FakeDelegateCommand.New()
				.AddName(new[] { "asdf asdf", "bob" })
				.ToImmutable()
				.Single();
			Assert.ThrowsException<ArgumentException>(() =>
			{
				_Trie.Add(command);
			});
		}

		[TestMethod]
		public void NoName_Test()
		{
			var command = FakeDelegateCommand.New()
				.ToImmutable()
				.Single();
			Assert.IsFalse(_Trie.Contains(command));
			Assert.ThrowsException<ArgumentException>(() =>
			{
				_Trie.Add(command);
			});
		}
	}
}