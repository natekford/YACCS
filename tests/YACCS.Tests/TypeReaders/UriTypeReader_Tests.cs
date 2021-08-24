
using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.TypeReaders;

namespace YACCS.Tests.TypeReaders
{
	[TestClass]
	public class UriTypeReader_Tests : TypeReader_Tests<Uri>
	{
		public override ITypeReader<Uri> Reader { get; } = new UriTypeReader();

		[TestMethod]
		public async Task Escaped_Test()
		{
			var result = await Reader.ReadAsync(Context, new[] { "<https://www.google.com>" }).ConfigureAwait(false);
			Assert.IsTrue(result.InnerResult.IsSuccess);
			Assert.IsInstanceOfType(result.Value, typeof(Uri));
		}

		[TestMethod]
		public async Task Exception_Test()
		{
			var result = await Reader.ReadAsync(Context, new[] { "www.google.com" }).ConfigureAwait(false);
			Assert.IsFalse(result.InnerResult.IsSuccess);
		}

		[TestMethod]
		public async Task NullOrEmpty_Test()
		{
			var nullResult = await Reader.ReadAsync(Context, new[] { default(string)! }).ConfigureAwait(false);
			Assert.IsFalse(nullResult.InnerResult.IsSuccess);

			var emptyResult = await Reader.ReadAsync(Context, new[] { "" }).ConfigureAwait(false);
			Assert.IsFalse(emptyResult.InnerResult.IsSuccess);
		}

		[TestMethod]
		public async Task Valid_Test()
		{
			var result = await Reader.ReadAsync(Context, new[] { "https://www.google.com" }).ConfigureAwait(false);
			Assert.IsTrue(result.InnerResult.IsSuccess);
			Assert.IsInstanceOfType(result.Value, typeof(Uri));
		}
	}
}