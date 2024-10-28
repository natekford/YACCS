using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Results;
using YACCS.TypeReaders;

namespace YACCS.Tests.TypeReaders;

[TestClass]
public class NumberIntTypeReader_Tests : TypeReader_Tests<int>
{
	public override ITypeReader<int> Reader { get; }
		= new NumberTypeReader<int>(int.TryParse);

	[TestMethod]
	public async Task Comma_Test()
		=> await AssertFailureAsync<ParseFailed>("1,234,567").ConfigureAwait(false);

	[TestMethod]
	public async Task NonInteger_Test()
		=> await AssertFailureAsync<ParseFailed>("1.234").ConfigureAwait(false);

	[TestMethod]
	public async Task Valid_Test()
	{
		const int VALUE = 1;
		var value = await AssertSuccessAsync(VALUE.ToString()).ConfigureAwait(false);
		Assert.AreEqual(VALUE, value);
	}
}