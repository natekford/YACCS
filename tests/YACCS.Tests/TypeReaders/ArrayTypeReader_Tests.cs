using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.TypeReaders;

namespace YACCS.Tests.TypeReaders
{
	[TestClass]
	public class ArrayTypeReader_Tests : TypeReader_Tests<int[]>
	{
		public override IContext Context { get; }
		public override ITypeReader<int[]> Reader { get; }

		public ArrayTypeReader_Tests()
		{
			Context = new FakeContext();
			var dict = Context.Get<IReadOnlyDictionary<Type, ITypeReader>>();
			Reader = (ITypeReader<int[]>)dict[typeof(int[])];
		}

		[TestMethod]
		public async Task Int_Test()
		{
			var result = await Reader.ReadAsync(Context, "1 2 3").ConfigureAwait(false);
			Assert.IsTrue(result.InnerResult.IsSuccess);
			Assert.IsInstanceOfType(result.Value, typeof(int[]));
			Assert.AreEqual(1, result.Value![0]);
			Assert.AreEqual(2, result.Value![1]);
			Assert.AreEqual(3, result.Value![2]);
		}
	}
}