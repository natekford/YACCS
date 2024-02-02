using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Globalization;

using YACCS.Localization;

namespace YACCS.Tests.Localization;

[TestClass]
public class LocalizedAttributes_Tests
{
	private const string KEY1 = "A";
	private const string KEY2 = "B";
	private const string KEY3 = "C";

	[TestMethod]
	public void LocalizedCategory_Test()
	{
		var attr = new LocalizedCategoryAttribute(KEY1);

		Assert.AreEqual(KEY1, attr.Key);
		Assert.AreEqual(KEY2, attr.Category);

		CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
		Assert.AreEqual(KEY3, attr.Category);
	}

	[TestMethod]
	public void LocalizedCommand_Test()
	{
		var attr = new LocalizedCommandAttribute(KEY1);

		Assert.AreEqual(KEY1, attr.Keys.Single());
		var names1 = attr.Names;
		Assert.AreEqual(KEY2, names1.Single());

		var prevCulture = CultureInfo.CurrentUICulture;
		CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
		Assert.AreEqual(KEY3, attr.Names.Single());

		CultureInfo.CurrentUICulture = prevCulture;
		var names2 = attr.Names;
		Assert.AreSame(names1, names2);
	}

	[TestMethod]
	public void LocalizedName_Test()
	{
		var attr = new LocalizedNameAttribute(KEY1);

		Assert.AreEqual(KEY1, attr.Key);
		Assert.AreEqual(KEY2, attr.Name);

		CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
		Assert.AreEqual(KEY3, attr.Name);
	}

	[TestCleanup]
	public void TestCleanup()
		=> CultureInfo.CurrentUICulture = CultureInfo.InstalledUICulture;

	[TestInitialize]
	public void TestInitialize()
	{
		CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");

		var localizer = new RuntimeLocalizer();
		localizer.Overrides[CultureInfo.CurrentUICulture].Add(KEY1, KEY2);
		localizer.Overrides[CultureInfo.InvariantCulture].Add(KEY1, KEY3);
		Localize.Instance.Append(localizer);
	}
}