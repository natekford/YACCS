using System;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.Results;
using YACCS.TypeReaders;

namespace YACCS.Tests.Commands
{
	[TestClass]
	public class TypeReaderRegistry_Tests
	{
		[TestMethod]
		public void InvalidReaderRegistered_Test()
		{
			var registry = new TypeReaderRegistry();
			Assert.ThrowsException<ArgumentException>(() =>
			{
				registry.Register(new UriTypeReader(), typeof(FakeStruct));
			});
		}

		[TestMethod]
		public void NonGenericReaderRegistered_Test()
		{
			var registry = new TypeReaderRegistry();
			registry.Register(new BadStringReader(), typeof(string));

			var r1 = registry.GetReader(typeof(string));
			Assert.IsNotNull(r1);

			Assert.ThrowsException<ArgumentException>(() =>
			{
				registry.GetReader<string>();
			});
		}

		[TestMethod]
		public void NoReaderRegistered_Test()
		{
			var registry = new TypeReaderRegistry();
			Assert.ThrowsException<ArgumentException>(() =>
			{
				var reader = registry.GetReader<FakeStruct>();
			});
		}

		[TestMethod]
		public void RegisterChildTypeReaderToParent_Test()
		{
			var registry = new TypeReaderRegistry();
			var reader = new TryParseTypeReader<Child>((string input, out Child output) =>
			{
				output = new Child();
				return true;
			});
			registry.Register(reader, typeof(Parent));

			// This is the entire reason I added the ITask package.
			// Why couldn't there be an ITask in the base library
			var r1 = registry.GetReader<Parent>();
			Assert.IsNotNull(r1);

			Assert.ThrowsException<ArgumentException>(() =>
			{
				registry.GetReader<Child>();
			});
		}

		[TestMethod]
		public void RegisterValueType_Test()
		{
			var registry = new TypeReaderRegistry();
			var reader = new TryParseTypeReader<FakeStruct>((string input, out FakeStruct output) =>
			{
				output = new FakeStruct();
				return true;
			});
			registry.Register(reader);

			var r1 = registry.GetReader<FakeStruct>();
			Assert.IsNotNull(r1);

			var r2 = registry.GetReader<FakeStruct?>();
			Assert.IsNotNull(r2);
		}

		[TestMethod]
		public void TryGetEnumReader_Test()
		{
			var registry = new TypeReaderRegistry();
			var reader = registry.GetReader<BindingFlags>();
			Assert.IsNotNull(reader);
		}

		private struct FakeStruct
		{
		}

		public class BadStringReader : ITypeReader
		{
			public Type OutputType => typeof(string);

			public Task<ITypeReaderResult> ReadAsync(IContext context, string input)
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