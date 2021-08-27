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
			var value = await AssertSuccessAsync("00:00:01").ConfigureAwait(false);
			Assert.AreEqual(TimeSpan.FromSeconds(1), value);
		}
	}
}