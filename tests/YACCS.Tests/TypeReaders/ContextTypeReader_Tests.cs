using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Results;
using YACCS.TypeReaders;

namespace YACCS.Tests.TypeReaders
{
	[TestClass]
	public class ContextTypeReader_Tests : TypeReader_Tests<OtherContext>
	{
		public override Type ExpectedInvalidResultType => typeof(InvalidContextResult);
		public override ITypeReader<OtherContext> Reader { get; }
			= new ContextTypeReader<OtherContext>();

		[TestMethod]
		public async Task DirectInvalid_Test()
		{
			var reader = (ContextTypeReader<OtherContext>)Reader;
			var result = await reader.ReadAsync(null!, new[] { Invalid }).ConfigureAwait(false);
			Assert.IsFalse(result.InnerResult.IsSuccess);
		}

		[TestMethod]
		public async Task Valid_Test()
		{
			var context = new OtherContext();
			var value = await AssertSuccessAsync(Array.Empty<string>(), context).ConfigureAwait(false);
			Assert.AreEqual(context, value);
		}
	}
}