using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.TypeReaders;

namespace YACCS.Tests.TypeReaders;

[TestClass]
public class NumberTypeReader_Tests : TypeReader_Tests<int>
{
	public override ITypeReader<int> Reader { get; }
		= new NumberTypeReader<int>(int.TryParse);

	[TestMethod]
	public async Task Valid_Test()
	{
		const int VALUE = 1;
		var value = await AssertSuccessAsync(VALUE.ToString()).ConfigureAwait(false);
		Assert.AreEqual(VALUE, value);
	}
}