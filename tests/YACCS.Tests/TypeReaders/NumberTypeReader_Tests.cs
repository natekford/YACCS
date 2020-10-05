using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.TypeReaders;

namespace YACCS.Tests.TypeReaders
{
	[TestClass]
	public class NumberTypeReader_Tests : TypeReader_Tests<int>
	{
		public override TypeReader<int> Reader { get; }
			= new NumberTypeReader<int>(int.TryParse);

		[TestMethod]
		public async Task Valid_Test()
		{
			var result = await Reader.ReadAsync(Context, "1").ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.Value, typeof(int));
		}
	}
}