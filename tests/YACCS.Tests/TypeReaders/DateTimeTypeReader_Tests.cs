using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.TypeReaders;

namespace YACCS.Tests.TypeReaders;

[TestClass]
public class DateTimeTypeReader_Tests : TypeReader_Tests<DateTime>
{
	public override ITypeReader<DateTime> Reader { get; }
		= new DateTimeTypeReader<DateTime>(DateTime.TryParse);

	[TestMethod]
	public async Task Valid_Test()
	{
		var value = await AssertSuccessAsync("01/01/2000 01:01:01.1").ConfigureAwait(false);
		Assert.AreEqual(new DateTime(2000, 1, 1, 1, 1, 1, 100), value);
	}
}