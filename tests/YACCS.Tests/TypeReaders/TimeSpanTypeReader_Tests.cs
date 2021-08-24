
using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.TypeReaders;

namespace YACCS.Tests.TypeReaders
{
	[TestClass]
	public class TimeSpanTypeReader_Tests : TypeReader_Tests<TimeSpan>
	{
		public override ITypeReader<TimeSpan> Reader { get; }
			= new TimeSpanTypeReader<TimeSpan>(TimeSpan.TryParse);

		[TestMethod]
		public async Task Valid_Test()
		{
			var result = await Reader.ReadAsync(Context, new[] { "00:00:01" }).ConfigureAwait(false);
			Assert.IsTrue(result.InnerResult.IsSuccess);
			Assert.IsInstanceOfType(result.Value, typeof(TimeSpan));
			Assert.AreEqual(TimeSpan.FromSeconds(1), result.Value);
		}
	}
}