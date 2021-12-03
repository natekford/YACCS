using Microsoft.VisualStudio.TestTools.UnitTesting;

using MorseCode.ITask;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Linq;
using YACCS.Commands.Models;
using YACCS.Trie;
using YACCS.TypeReaders;

namespace YACCS.Tests.Commands;

[TestClass]
public class CommandTrie_Tests
{
	private const string DUPE_ID = "dupe_id";
	private readonly CommandTrie _Trie = new(
		new TypeReaderRegistry(),
		CommandServiceConfig.Instance.Separator,
		CommandServiceConfig.Instance.CommandNameComparer
	);

	[TestMethod]
	public void AddAndRemove_Test()
	{
		var c1 = FakeDelegateCommand.New()
			.AddPath(new[] { "1" })
			.ToImmutable();
		_Trie.Add(c1);
		Assert.AreEqual(1, _Trie.Count);
		Assert.AreEqual(1, _Trie.Root.Edges.Count);
		Assert.AreEqual(1, _Trie.Root["1"].GetAllDistinctItems().Count);
		Assert.AreEqual(1, _Trie.Root["1"].Count);

		var c2 = FakeDelegateCommand.New()
			.AddPath(new[] { "2" })
			.AddPath(new[] { "3" })
			.ToImmutable();
		_Trie.Add(c2);
		Assert.AreEqual(2, _Trie.Count);
		Assert.AreEqual(3, _Trie.Root.Edges.Count);
		Assert.AreEqual(1, _Trie.Root["2"].GetAllDistinctItems().Count);
		Assert.AreEqual(1, _Trie.Root["2"].Count);
		Assert.AreEqual(1, _Trie.Root["3"].GetAllDistinctItems().Count);
		Assert.AreEqual(1, _Trie.Root["3"].Count);

		var c3 = FakeDelegateCommand.New()
			.AddPath(new[] { "4", "1" })
			.AddPath(new[] { "4", "2" })
			.AddPath(new[] { "4", "3" })
			.ToImmutable();
		_Trie.Add(c3);
		Assert.AreEqual(3, _Trie.Count);
		Assert.AreEqual(4, _Trie.Root.Edges.Count);
		Assert.AreEqual(1, _Trie.Root["4"].GetAllDistinctItems().Count);
		Assert.AreEqual(0, _Trie.Root["4"].Count);
		Assert.AreEqual(1, _Trie.Root["4"]["1"].GetAllDistinctItems().Count);
		Assert.AreEqual(1, _Trie.Root["4"]["2"].GetAllDistinctItems().Count);
		Assert.AreEqual(1, _Trie.Root["4"]["3"].GetAllDistinctItems().Count);

		var c4 = FakeDelegateCommand.New()
			.AddPath(new[] { "4", "1" })
			.ToImmutable();
		_Trie.Add(c4);
		Assert.AreEqual(4, _Trie.Count);
		Assert.AreEqual(4, _Trie.Root.Edges.Count);
		Assert.AreEqual(2, _Trie.Root["4"].GetAllDistinctItems().Count);
		Assert.AreEqual(0, _Trie.Root["4"].Count);
		Assert.AreEqual(2, _Trie.Root["4"]["1"].GetAllDistinctItems().Count);
		Assert.AreEqual(1, _Trie.Root["4"]["2"].GetAllDistinctItems().Count);
		Assert.AreEqual(1, _Trie.Root["4"]["3"].GetAllDistinctItems().Count);

		var c5 = FakeDelegateCommand.New()
			.AddAttribute(new IdAttribute(DUPE_ID))
			.AddPath(new[] { "5" })
			.ToImmutable();
		_Trie.Add(c5);
		Assert.AreEqual(5, _Trie.Count);
		Assert.AreEqual(5, _Trie.Root.Edges.Count);
		Assert.AreEqual(1, _Trie.Root["5"].GetAllDistinctItems().Count);
		Assert.AreEqual(1, _Trie.Root["5"].Count);

		var c6 = FakeDelegateCommand.New()
			.AddPath(new[] { "4" })
			.ToImmutable();
		Assert.IsTrue(_Trie.Remove(c4));
		_Trie.Add(c6);
		Assert.AreEqual(5, _Trie.Root.Edges.Count);
		Assert.AreEqual(2, _Trie.Root["4"].GetAllDistinctItems().Count);
		Assert.AreEqual(1, _Trie.Root["4"].Count);
		Assert.AreEqual(1, _Trie.Root["4"]["1"].GetAllDistinctItems().Count);
		Assert.AreEqual(1, _Trie.Root["4"]["2"].GetAllDistinctItems().Count);
		Assert.AreEqual(1, _Trie.Root["4"]["3"].GetAllDistinctItems().Count);

		Assert.IsTrue(_Trie.Remove(c6));
		Assert.AreEqual(1, _Trie.Root["4"].GetAllDistinctItems().Count);
		Assert.AreEqual(0, _Trie.Root["4"].Count);
		Assert.AreEqual(5, _Trie.Root.Edges.Count);
		Assert.AreEqual(1, _Trie.Root["4"]["1"].GetAllDistinctItems().Count);
		Assert.AreEqual(1, _Trie.Root["4"]["2"].GetAllDistinctItems().Count);
		Assert.AreEqual(1, _Trie.Root["4"]["3"].GetAllDistinctItems().Count);
		Assert.AreEqual(1, _Trie.Root["4"].GetAllDistinctItems().Count);

		var c7 = FakeDelegateCommand.New().ToImmutable();
		Assert.IsFalse(_Trie.Remove(c7));

		var items = new List<IImmutableCommand>();
		foreach (var item in _Trie)
		{
			items.Add(item);
			_Trie.Remove(item);
		}
		Assert.AreEqual(0, _Trie.Count);
		Assert.AreEqual(0, _Trie.Root.Edges.Count);

		foreach (var item in items)
		{
			_Trie.Add(item);
		}
		Assert.AreEqual(1, _Trie.Root["4"].GetAllDistinctItems().Count);
		Assert.AreEqual(0, _Trie.Root["4"].Count);
		Assert.AreEqual(5, _Trie.Root.Edges.Count);
		Assert.AreEqual(1, _Trie.Root["4"]["1"].GetAllDistinctItems().Count);
		Assert.AreEqual(1, _Trie.Root["4"]["2"].GetAllDistinctItems().Count);
		Assert.AreEqual(1, _Trie.Root["4"]["3"].GetAllDistinctItems().Count);
		Assert.AreEqual(1, _Trie.Root["4"].GetAllDistinctItems().Count);

		_Trie.Clear();
		Assert.AreEqual(0, _Trie.Count);
		Assert.AreEqual(0, _Trie.Root.Edges.Count);
	}

	[TestMethod]
	public void ContainsLotsOfName_Test()
	{
		var command = FakeDelegateCommand.New();
		const int COUNT = 100000;
		for (var i = 0; i < COUNT; ++i)
		{
			command.AddPath(new[] { i.ToString() });
		}
		var immutable = command.ToImmutable();

		Assert.IsFalse(_Trie.Contains(immutable));
		_Trie.Add(immutable);
		Assert.AreEqual(COUNT, _Trie.Root.Edges.Count);
		Assert.IsTrue(_Trie.Contains(immutable));
	}

	[TestMethod]
	public void Duplicate_Test()
	{
		var c1 = FakeDelegateCommand.New()
			.AddPath(new[] { "a" })
			.AddAttribute(new IdAttribute(DUPE_ID))
			.ToImmutable();
		_Trie.Add(c1);
		Assert.AreEqual(1, _Trie.Count);
		Assert.AreEqual(1, _Trie.Root["a"].GetAllDistinctItems().Count);
		Assert.IsTrue(_Trie.Contains(c1));

		var c2 = FakeDelegateCommand.New()
			.AddPath(new[] { "a" })
			.AddAttribute(new IdAttribute(DUPE_ID))
			.ToImmutable();
		Assert.IsFalse(_Trie.Contains(c2));
		_Trie.Add(c2);
		Assert.IsTrue(_Trie.Contains(c2));

		var c3 = FakeDelegateCommand.New()
			.AddPath(new[] { "b" })
			.AddAttribute(new IdAttribute(DUPE_ID))
			.ToImmutable();
		_Trie.Add(c3);
		Assert.AreEqual(3, _Trie.Count);
		Assert.AreEqual(1, _Trie.Root["b"].GetAllDistinctItems().Count);
		Assert.IsTrue(_Trie.Contains(c3));

		var @delegate = (Action<string>)(x => { });
		var c4 = new DelegateCommand(@delegate, new[] { new[] { "a" } })
			.AddAttribute(new IdAttribute(DUPE_ID))
			.ToImmutable();
		_Trie.Add(c4);
		Assert.AreEqual(4, _Trie.Count);
		Assert.AreEqual(3, _Trie.Root["a"].GetAllDistinctItems().Count);
		Assert.IsTrue(_Trie.Contains(c4));
	}

	[TestMethod]
	public void Find_Test()
	{
		var c1 = FakeDelegateCommand.New()
			.AddPath(new[] { "1" })
			.ToImmutable();
		_Trie.Add(c1);
		var c2 = FakeDelegateCommand.New()
			.AddPath(new[] { "2" })
			.AddPath(new[] { "3" })
			.ToImmutable();
		_Trie.Add(c2);
		var c3 = FakeDelegateCommand.New()
			.AddPath(new[] { "4", "1" })
			.AddPath(new[] { "4", "2" })
			.AddPath(new[] { "4", "3" })
			.ToImmutable();
		_Trie.Add(c3);
		var c4 = FakeDelegateCommand.New()
			.AddPath(new[] { "4", "1" })
			.ToImmutable();
		_Trie.Add(c4);

		AssertFindTest(new[] { "" }, null);
		AssertFindTest(new[] { "not", "a", "command" }, null);
		AssertFindTest(new[] { "\"1" }, null);
		AssertFindTest(new[] { "1" }, 1, x =>
		{
			Assert.IsTrue(x.Contains(c1));
		});
		AssertFindTest(new[] { "2" }, 1, x =>
		{
			Assert.IsTrue(x.Contains(c2));
		});
		AssertFindTest(new[] { "3" }, 1, x =>
		{
			Assert.IsTrue(x.Contains(c2));
		});
		AssertFindTest(new[] { "4" }, 2, x =>
		{
			Assert.IsTrue(x.Contains(c3));
			Assert.IsTrue(x.Contains(c4));
		});
		AssertFindTest(new[] { "4", "1" }, 2, x =>
		{
			Assert.IsTrue(x.Contains(c3));
			Assert.IsTrue(x.Contains(c4));
		});
		AssertFindTest(new[] { "4", "2" }, 1, x =>
		{
			Assert.IsTrue(x.Contains(c3));
		});
		AssertFindTest(Array.Empty<string>(), 4, x =>
		{
			Assert.IsTrue(x.Contains(c1));
			Assert.IsTrue(x.Contains(c2));
			Assert.IsTrue(x.Contains(c3));
			Assert.IsTrue(x.Contains(c4));
		});
	}

	[TestMethod]
	public void InvalidTypeReader_Test()
	{
		static void Delegate(string input)
		{
		}

		var @delegate = (Action<string>)Delegate;
		var command = new DelegateCommand(@delegate, new[] { new[] { "joe" } }, typeof(FakeContext));
		command.Parameters[0].TypeReader = new TestTypeReader(typeof(OtherContext));
		Assert.ThrowsException<ArgumentException>(() =>
		{
			_Trie.Add(command.ToImmutable());
		});

		command.Parameters[0].TypeReader = new TestTypeReader(typeof(FakeContextChild));
		_Trie.Add(command.ToImmutable());
		Assert.AreEqual(1, _Trie.Count);
	}

	[TestMethod]
	public void NameWithSeparator_Test()
	{
		var command = FakeDelegateCommand.New()
			.AddPath(new[] { "asdf asdf", "bob" })
			.ToImmutable();
		Assert.ThrowsException<ArgumentException>(() =>
		{
			_Trie.Add(command);
		});
	}

	[TestMethod]
	public void NoName_Test()
	{
		var command = FakeDelegateCommand.New().ToImmutable();
		Assert.IsFalse(_Trie.Contains(command));
		Assert.ThrowsException<ArgumentException>(() =>
		{
			_Trie.Add(command);
		});
	}

	private void AssertFindTest(
		string[] path,
		int? expectedCount,
		Action<HashSet<IImmutableCommand>>? assert = null)
	{
		var node = _Trie.Root.FollowPath(path);
		if (expectedCount is null)
		{
			Assert.IsNull(node);
			return;
		}

		Assert.IsNotNull(node);
		var found = node!.GetAllDistinctItems();
		Assert.AreEqual(expectedCount.Value, found.Count);
		assert?.Invoke(found);
	}

	public sealed class TestTypeReader : ITypeReader
	{
		public Type ContextType { get; }
		public Type OutputType => typeof(string);

		public TestTypeReader(Type contextType)
		{
			ContextType = contextType;
		}

		public ITask<ITypeReaderResult> ReadAsync(IContext context, ReadOnlyMemory<string> input)
			=> throw new NotImplementedException();
	}
}
