using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Globalization;

using YACCS.Results;
using YACCS.TypeReaders;

namespace YACCS.Tests.TypeReaders;

[TestClass]
public class NumberFloatTypeReader_Tests : TypeReader_Tests<double>
{
	public override ITypeReader<double> Reader { get; }
		= new NumberTypeReader<double>(double.TryParse);

	[TestMethod]
	public async Task Comma_Test()
	{
		CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

		await AssertFailureAsync<ParseFailed>("1,234,567").ConfigureAwait(false);
	}

	[TestMethod]
	public async Task ValidFloat_Test()
	{
		CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

		const double VALUE = 1.234;
		var value = await AssertSuccessAsync(VALUE.ToString()).ConfigureAwait(false);
		Assert.AreEqual(VALUE, value);
	}

	[TestMethod]
	public async Task ValidFloatOtherCulture_Test()
	{
		CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");

		const double VALUE = 1.234;
		var value = await AssertSuccessAsync(VALUE.ToString()).ConfigureAwait(false);
		Assert.AreEqual(VALUE, value);
	}

	[TestMethod]
	public async Task ValidInt_Test()
	{
		const int VALUE = 1;
		var value = await AssertSuccessAsync(VALUE.ToString()).ConfigureAwait(false);
		Assert.AreEqual(VALUE, value);
	}
}