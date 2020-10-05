using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Tests.Commands;
using YACCS.TypeReaders;

namespace YACCS.Tests.TypeReaders
{
	[TestClass]
	public class ContextTypeReader_Tests : TypeReader_Tests<FakeContext2>
	{
		public override TypeReader<FakeContext2> Reader { get; }
			= new ContextTypeReader<FakeContext2>();

		[TestMethod]
		public async Task Valid_Test()
		{
			var result = await Reader.ReadAsync(new FakeContext2(), "").ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.Value, typeof(FakeContext2));
		}
	}
}