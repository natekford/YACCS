using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Tests.Commands;
using YACCS.TypeReaders;

namespace YACCS.Tests.TypeReaders
{
	[TestClass]
	public class ContextTypeReader_Tests : TypeReader_Tests<FakeContext2>
	{
		public override ITypeReader<FakeContext2> Reader { get; }
			= new ContextTypeReader<FakeContext2>();

		[TestMethod]
		public async Task Valid_Test()
		{
			var context = new FakeContext2();
			var result = await Reader.ReadAsync(context, "").ConfigureAwait(false);
			Assert.IsTrue(result.InnerResult.IsSuccess);
			Assert.IsInstanceOfType(result.Value, typeof(FakeContext2));
			Assert.AreEqual(context, result.Value);
		}
	}
}