using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.TypeReaders;

namespace YACCS.Tests.Commands
{
	[TestClass]
	public class TypeReaderRegistry_Tests
	{
		[TestMethod]
		public void InvalidReaderRegistered_Test()
		{
			var registry = Utils.CreateServices().Get<TypeReaderRegistry>();
			Assert.ThrowsException<ArgumentException>(() =>
			{
				registry.Register(typeof(FakeStruct), new UriTypeReader());
			});
		}

		[TestMethod]
		public void NonGenericReaderRegistered_Test()
		{
			var registry = Utils.CreateServices().Get<TypeReaderRegistry>();
			registry.Register(typeof(string), new BadStringReader());
			Assert.IsNotNull(registry[typeof(string)]);
		}

		[TestMethod]
		public void NoReaderRegistered_Test()
		{
			var registry = Utils.CreateServices().Get<TypeReaderRegistry>();
			Assert.ThrowsException<KeyNotFoundException>(() =>
			{
				_ = registry[typeof(FakeStruct)];
			});
		}

		[TestMethod]
		public async Task RegisterChildTypeReaderToParent_Test()
		{
			var registry = Utils.CreateServices().Get<TypeReaderRegistry>();
			var reader = new TryParseTypeReader<Child>((string input, out Child output) =>
			{
				output = new Child();
				return true;
			});
			registry.Register(typeof(Parent), reader);

			var retrieved = registry[typeof(Parent)];
			var item = await retrieved.ReadAsync(null!, "joe").ConfigureAwait(false);
			Assert.IsNotNull(retrieved);
			Assert.IsInstanceOfType(item.Value, typeof(Parent));
			Assert.IsInstanceOfType(item.Value, typeof(Child));

			Assert.ThrowsException<KeyNotFoundException>(() =>
			{
				_ = registry[typeof(Child)];
			});
		}

		[TestMethod]
		public void RegisterValueType_Test()
		{
			var registry = Utils.CreateServices().Get<TypeReaderRegistry>();
			var reader = new TryParseTypeReader<FakeStruct>((string input, out FakeStruct output) =>
			{
				output = new FakeStruct();
				return true;
			});
			registry.Register(typeof(FakeStruct), reader);

			Assert.IsNotNull(registry[typeof(FakeStruct)]);
			Assert.IsNotNull(registry[typeof(FakeStruct?)]);
		}

		[TestMethod]
		public void TryGetEnumReader_Test()
		{
			var registry = Utils.CreateServices().Get<TypeReaderRegistry>();
			Assert.IsNotNull(registry[typeof(BindingFlags)]);
		}

		private struct FakeStruct
		{
		}

		public class BadStringReader : ITypeReader
		{
			public Type OutputType => typeof(string);

			public ValueTask<ITypeReaderResult> ReadAsync(
				IContext context,
				ReadOnlyMemory<string> input)
				=> throw new NotImplementedException();
		}

		public class Child : Parent
		{
		}

		public class Parent
		{
		}
	}
}