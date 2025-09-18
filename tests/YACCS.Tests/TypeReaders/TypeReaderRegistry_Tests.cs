using Microsoft.VisualStudio.TestTools.UnitTesting;

using MorseCode.ITask;

using System.Diagnostics.CodeAnalysis;
using System.Reflection;

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
	public async Task EnumArray_Test()
	{
		var result = _Readers.TryGetValue(typeof(BindingFlags[]), out var reader);
		Assert.IsTrue(result);
		Assert.IsNotNull(reader);

		var enumArrayResult = await reader.ReadAsync(new FakeContext(), new[] { "1", "2" }).ConfigureAwait(false);
		Assert.IsTrue(enumArrayResult.InnerResult.IsSuccess);
		var value = (BindingFlags[])enumArrayResult.Value!;
		Assert.HasCount(2, value);
		Assert.AreEqual(BindingFlags.IgnoreCase, value[0]);
		Assert.AreEqual(BindingFlags.DeclaredOnly, value[1]);
	}

	[TestMethod]
	public async Task EnumMatrix_Test()
	{
		var result = _Readers.TryGetValue(typeof(BindingFlags[][]), out var reader);
		Assert.IsTrue(result);
		Assert.IsNotNull(reader);

		var enumArrayResult = await reader.ReadAsync(new FakeContext(), new[] { "1 2", "4 8" }).ConfigureAwait(false);
		Assert.IsTrue(enumArrayResult.InnerResult.IsSuccess);
		var value = (BindingFlags[][])enumArrayResult.Value!;
		Assert.HasCount(2, value);
		Assert.AreEqual(BindingFlags.IgnoreCase, value[0][0]);
		Assert.AreEqual(BindingFlags.DeclaredOnly, value[0][1]);
		Assert.AreEqual(BindingFlags.Instance, value[1][0]);
		Assert.AreEqual(BindingFlags.Static, value[1][1]);
	}

	[TestMethod]
	public void InvalidReaderRegistered_Test()
	{
		Assert.ThrowsExactly<ArgumentException>(() =>
		{
			_Readers.Register(typeof(FakeStruct), new UriTypeReader());
		});
	}

	[TestMethod]
	public void NonGenericReaderRegistered_Test()
	{
		_Readers.Register(typeof(string), new BadStringReader());
		Assert.IsNotNull(_Readers[typeof(string)]);

		Assert.ThrowsExactly<ArgumentException>(() =>
		{
			_Readers.GetTypeReader<string>();
		});
	}

	[TestMethod]
	public void NoReaderRegistered_Test()
	{
		Assert.ThrowsExactly<ArgumentException>(() =>
		{
			_Readers.GetTypeReader<FakeStruct>();
		});
	}

	[TestMethod]
	public async Task RegisterChildTypeReaderToParent_Test()
	{
		var reader = new TryParseTypeReader<Child>((input, [MaybeNullWhen(false)] out output) =>
		{
			output = new Child();
			return true;
		});
		_Readers.Register(typeof(Parent), reader);

		var retrieved = _Readers.GetTypeReader<Parent>();
		var item = await retrieved.ReadAsync(new FakeContext(), new[] { "joe" }).ConfigureAwait(false);
		Assert.IsNotNull(retrieved);
		Assert.IsInstanceOfType<Parent>(item.Value);
		Assert.IsInstanceOfType<Child>(item.Value);

		var parameter = new Parameter(typeof(Parent), "joe", null);
		var typed = parameter.AsType<Parent>();
		typed.SetTypeReader(retrieved);
		Assert.AreEqual(retrieved, typed.TypeReader);

		Assert.ThrowsExactly<ArgumentException>(() =>
		{
			_Readers.GetTypeReader<Child>();
		});
	}

	[TestMethod]
	public void RegisterValueType_Test()
	{
		var reader = new TryParseTypeReader<FakeStruct>((input, [MaybeNullWhen(false)] out output) =>
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

	private struct FakeStruct;

	private class BadStringReader : ITypeReader
	{
		public Type ContextType => typeof(IContext);
		public Type OutputType => typeof(string);

		public ITask<ITypeReaderResult> ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input)
			=> throw new NotImplementedException();
	}

	private class Child : Parent;

	private class Parent;

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
				return new TryParseTypeReader<WillCreateAggregate>((input, [MaybeNullWhen(false)] out value) =>
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
				return new TryParseTypeReader<WillCreateAggregate>((input, [MaybeNullWhen(false)] out value) =>
				{
					var result = int.TryParse(input, out var @int);
					value = new WillCreateAggregate { IntValue = result ? @int : null };
					return result;
				});
			}
		}
	}
}