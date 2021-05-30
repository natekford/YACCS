using System;
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using MorseCode.ITask;

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
			var registry = new TypeReaderRegistry();
			Assert.ThrowsException<ArgumentException>(() =>
			{
				registry.Register(typeof(FakeStruct), new UriTypeReader());
			});
		}

		[TestMethod]
		public void NonGenericReaderRegistered_Test()
		{
			var registry = new TypeReaderRegistry();
			registry.Register(typeof(string), new BadStringReader());

			var r1 = registry[typeof(string)];
			Assert.IsNotNull(r1);

			Assert.ThrowsException<ArgumentException>(() =>
			{
				registry.GetTypeReader<string>();
			});
		}

		[TestMethod]
		public void NoReaderRegistered_Test()
		{
			var registry = new TypeReaderRegistry();
			Assert.ThrowsException<ArgumentException>(() =>
			{
				var reader = registry.GetTypeReader<FakeStruct>();
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
			registry.Register(typeof(Parent), reader);

			// This is the entire reason I added the ITask package.
			// Why couldn't there be an ITask in the base library
			var r1 = registry.GetTypeReader<Parent>();
			Assert.IsNotNull(r1);

			Assert.ThrowsException<ArgumentException>(() =>
			{
				registry.GetTypeReader<Child>();
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
			registry.Register(typeof(FakeStruct), reader);

			var r1 = registry.GetTypeReader<FakeStruct>();
			Assert.IsNotNull(r1);

			var r2 = registry.GetTypeReader<FakeStruct?>();
			Assert.IsNotNull(r2);
		}

		[TestMethod]
		public void TryGetEnumReader_Test()
		{
			var registry = new TypeReaderRegistry();
			var reader = registry.GetTypeReader<BindingFlags>();
			Assert.IsNotNull(reader);
		}

		private struct FakeStruct
		{
		}

		public class BadStringReader : ITypeReader
		{
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