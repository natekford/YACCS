using System;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using MorseCode.ITask;

using YACCS.Commands;
using YACCS.Commands.Linq;
using YACCS.Commands.Models;
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

			Assert.ThrowsException<ArgumentException>(() =>
			{
				registry.GetTypeReader<string>();
			});
		}

		[TestMethod]
		public void NoReaderRegistered_Test()
		{
			var registry = Utils.CreateServices().Get<TypeReaderRegistry>();
			Assert.ThrowsException<ArgumentException>(() =>
			{
				registry.GetTypeReader<FakeStruct>();
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

			var retrieved = registry.GetTypeReader<Parent>();
			var item = await retrieved.ReadAsync(new FakeContext(), "joe").ConfigureAwait(false);
			Assert.IsNotNull(retrieved);
			Assert.IsInstanceOfType(item.Value, typeof(Parent));
			Assert.IsInstanceOfType(item.Value, typeof(Child));

			var parameter = new Parameter(typeof(Parent), "joe", null);
			var typed = parameter.AsType<Parent>();
			typed.SetTypeReader(retrieved);
			Assert.AreEqual(retrieved, typed.TypeReader);

			Assert.ThrowsException<ArgumentException>(() =>
			{
				registry.GetTypeReader<Child>();
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

			Assert.IsNotNull(registry.GetTypeReader<FakeStruct>());
			Assert.IsNotNull(registry.GetTypeReader<FakeStruct?>());
		}

		[TestMethod]
		public void TryGetEnumReader_Test()
		{
			var registry = Utils.CreateServices().Get<TypeReaderRegistry>();
			var reader = registry.GetTypeReader<BindingFlags>();
			Assert.IsNotNull(reader);
		}

		private struct FakeStruct
		{
		}

		public class BadStringReader : ITypeReader
		{
			public Type ContextType => typeof(IContext);
			public Type OutputType => typeof(string);

			public ITask<ITypeReaderResult> ReadAsync(
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