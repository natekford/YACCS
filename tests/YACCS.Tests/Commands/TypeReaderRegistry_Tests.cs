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
			var readers = Utils.CreateServices().Get<TypeReaderRegistry>();
			Assert.ThrowsException<ArgumentException>(() =>
			{
				readers.Register(typeof(FakeStruct), new UriTypeReader());
			});
		}

		[TestMethod]
		public void NonGenericReaderRegistered_Test()
		{
			var readers = Utils.CreateServices().Get<TypeReaderRegistry>();
			readers.Register(typeof(string), new BadStringReader());
			Assert.IsNotNull(readers[typeof(string)]);

			Assert.ThrowsException<ArgumentException>(() =>
			{
				readers.GetTypeReader<string>();
			});
		}

		[TestMethod]
		public void NoReaderRegistered_Test()
		{
			var readers = Utils.CreateServices().Get<TypeReaderRegistry>();
			Assert.ThrowsException<ArgumentException>(() =>
			{
				readers.GetTypeReader<FakeStruct>();
			});
		}

		[TestMethod]
		public async Task RegisterChildTypeReaderToParent_Test()
		{
			var readers = Utils.CreateServices().Get<TypeReaderRegistry>();
			var reader = new TryParseTypeReader<Child>((string input, out Child output) =>
			{
				output = new Child();
				return true;
			});
			readers.Register(typeof(Parent), reader);

			var retrieved = readers.GetTypeReader<Parent>();
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
				readers.GetTypeReader<Child>();
			});
		}

		[TestMethod]
		public void RegisterValueType_Test()
		{
			var readers = Utils.CreateServices().Get<TypeReaderRegistry>();
			var reader = new TryParseTypeReader<FakeStruct>((string input, out FakeStruct output) =>
			{
				output = new FakeStruct();
				return true;
			});
			readers.Register(typeof(FakeStruct), reader);

			Assert.IsNotNull(readers.GetTypeReader<FakeStruct>());
			Assert.IsNotNull(readers.GetTypeReader<FakeStruct?>());
		}

		[TestMethod]
		public void TryGetEnumReader_Test()
		{
			var readers = Utils.CreateServices().Get<TypeReaderRegistry>();
			var reader = readers.GetTypeReader<BindingFlags>();
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