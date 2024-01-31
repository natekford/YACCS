using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Results;

namespace YACCS.Tests.Results;

[TestClass]
public class LocalizedResults_Tests
{
	[TestMethod]
	public void FormattableLocalizedResult_Test()
	{
		var result = new MustBeLessThan(5);

		Assert.AreEqual("Must be less than or equal to 5.", result.Response);
		Assert.AreEqual("Must be less than or equal to 5.", result.ToString());
	}

	[TestMethod]
	public void SingletonLocalizedResult_Test()
	{
		var result = Canceled.Instance;

		Assert.AreEqual("An operation was canceled.", result.Response);
		Assert.AreEqual("An operation was canceled.", result.ToString());
	}
}