using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.TypeReaders;

namespace YACCS.Tests.TypeReaders
{
	[TestClass]
	public class EnumTypeReader_Tests : TypeReader_Tests<BindingFlags>
	{
		public override ITypeReader<BindingFlags> Reader { get; }
			= new EnumTypeReader<BindingFlags>();

		[TestMethod]
		public async Task Valid_Test()
		{
			var result = await Reader.ReadAsync(Context, new[] { nameof(BindingFlags.CreateInstance) }).ConfigureAwait(false);
			Assert.IsTrue(result.InnerResult.IsSuccess);
			Assert.IsInstanceOfType(result.Value, typeof(BindingFlags));
			Assert.AreEqual(BindingFlags.CreateInstance, result.Value);
		}
	}
}