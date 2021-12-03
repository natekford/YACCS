using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using MorseCode.ITask;

using YACCS.Commands;
using YACCS.Commands.Linq;
using YACCS.Commands.Models;
using YACCS.TypeReaders;

namespace YACCS.Tests.TypeReaders;

[TestClass]
public class TypeReaderRegistry_Tests
{
	private readonly TypeReaderRegistry _Readers = new();

	[TestMethod]
	public async Task AggregateReader_Test()
	{
		var reader = _Readers.GetTypeReader<WillCreateAggregate>();

		var intResult = await reader.ReadAsync(new FakeContext(), new[] { "1" }).ConfigureAwait(false);
		Assert.IsTrue(intResult.InnerResult.IsSuccess);
		Assert.AreEqual(1, intResult.Value!.IntValue);

		var doubleResult = await reader.ReadAsync(new FakeContext(), new[] { "2.22" }).ConfigureAwait(false);
		Assert.IsTrue(doubleResult.InnerResult.IsSuccess);
		Assert.AreEqual(2.22d, doubleResult.Value!.DoubleValue);
	}

	[TestMethod]
	public void InvalidReaderRegistered_Test()
	{
		Assert.ThrowsException<ArgumentException>(() =>
		{
			_Readers.Register(typeof(FakeStruct), new UriTypeReader());
		});
	}

	[TestMethod]
	public void NonGenericReaderRegistered_Test()
	{
		_Readers.Register(typeof(string), new BadStringReader());
		Assert.IsNotNull(_Readers[typeof(string)]);

		Assert.ThrowsException<ArgumentException>(() =>
		{
			_Readers.GetTypeReader<string>();
		});
	}

	[TestMethod]
	public void NoReaderRegistered_Test()
	{
		Assert.ThrowsException<ArgumentException>(() =>
		{
			_Readers.GetTypeReader<FakeStruct>();
		});
	}

	[TestMethod]
	public async Task RegisterChildTypeReaderToParent_Test()
	{
		var reader = new TryParseTypeReader<Child>((string input, out Child output) =>
		{
			output = new Child();
			return true;
		});
		_Readers.Register(typeof(Parent), reader);

		var retrieved = _Readers.GetTypeReader<Parent>();
		var item = await retrieved.ReadAsync(new FakeContext(), new[] { "joe" }).ConfigureAwait(false);
		Assert.IsNotNull(retrieved);
		Assert.IsInstanceOfType(item.Value, typeof(Parent));
		Assert.IsInstanceOfType(item.Value, typeof(Child));

		var parameter = new Parameter(typeof(Parent), "joe", null);
		var typed = parameter.AsType<Parent>();
		typed.SetTypeReader(retrieved);
		Assert.AreEqual(retrieved, typed.TypeReader);

		Assert.ThrowsException<ArgumentException>(() =>
		{
			_Readers.GetTypeReader<Child>();
		});
	}

	[TestMethod]
	public void RegisterValueType_Test()
	{
		var reader = new TryParseTypeReader<FakeStruct>((string input, out FakeStruct output) =>
		{
			output = new FakeStruct();
			return true;
		});
		_Readers.Register(typeof(FakeStruct), reader);

		Assert.IsNotNull(_Readers.GetTypeReader<FakeStruct>());
		Assert.IsNotNull(_Readers.GetTypeReader<FakeStruct?>());
	}

	[TestMethod]
	public void TryGetEnumReader_Test()
		=> Assert.IsNotNull(_Readers.GetTypeReader<BindingFlags>());

	private struct FakeStruct
	{
	}

	private class BadStringReader : ITypeReader
	{
		public Type ContextType => typeof(IContext);
		public Type OutputType => typeof(string);

		public ITask<ITypeReaderResult> ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input)
			=> throw new NotImplementedException();
	}

	private class Child : Parent
	{
	}

	private class Parent
	{
	}

	[IntReader]
	[DoubleReader]
	private class WillCreateAggregate
	{
		public double? DoubleValue { get; init; }
		public int? IntValue { get; init; }

		[AttributeUsage(AttributeTargets.Class)]
		public sealed class DoubleReaderAttribute : Attribute, ITypeReaderGeneratorAttribute
		{
			public ITypeReader GenerateTypeReader(Type type)
			{
				return new TryParseTypeReader<WillCreateAggregate>(
					(string input, out WillCreateAggregate value) =>
				{
					var result = double.TryParse(input, out var @double);
					value = new WillCreateAggregate { DoubleValue = result ? @double : null };
					return result;
				});
			}
		}

		[AttributeUsage(AttributeTargets.Class)]
		public sealed class IntReaderAttribute : Attribute, ITypeReaderGeneratorAttribute
		{
			public ITypeReader GenerateTypeReader(Type type)
			{
				return new TryParseTypeReader<WillCreateAggregate>(
					(string input, out WillCreateAggregate value) =>
				{
					var result = int.TryParse(input, out var @int);
					value = new WillCreateAggregate { IntValue = result ? @int : null };
					return result;
				});
			}
		}
	}
}
