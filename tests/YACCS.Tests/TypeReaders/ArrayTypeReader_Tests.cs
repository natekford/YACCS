using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.TypeReaders;

namespace YACCS.Tests.TypeReaders;

[TestClass]
public class ArrayTypeReader_Tests : TypeReader_Tests<int[]>
{
	public override ITypeReader<int[]> Reader { get; }

	public ArrayTypeReader_Tests()
	{
		Reader = Context.Get<IReadOnlyDictionary<Type, ITypeReader>>().GetTypeReader<int[]>();
	}

	[TestMethod]
	public async Task Int_Test()
	{
		var value = await AssertSuccessAsync("1 2 3").ConfigureAwait(false);
		Assert.AreEqual(1, value[0]);
		Assert.AreEqual(2, value[1]);
		Assert.AreEqual(3, value[2]);
	}
}