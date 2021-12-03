using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Reflection;

using YACCS.TypeReaders;

namespace YACCS.Tests.TypeReaders;

[TestClass]
public class EnumTypeReader_Tests : TypeReader_Tests<BindingFlags>
{
	public override ITypeReader<BindingFlags> Reader { get; }
		= new EnumTypeReader<BindingFlags>();

	[TestMethod]
	public async Task Valid_Test()
	{
		const BindingFlags FLAGS = BindingFlags.CreateInstance;
		var value = await AssertSuccessAsync(FLAGS.ToString()).ConfigureAwait(false);
		Assert.AreEqual(FLAGS, value);
	}
}