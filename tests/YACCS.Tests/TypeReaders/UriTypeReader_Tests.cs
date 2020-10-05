using System;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.TypeReaders;

namespace YACCS.Tests.TypeReaders
{
	[TestClass]
	public class UriTypeReader_Tests : TypeReader_Tests<Uri>
	{
		public override TypeReader<Uri> Reader { get; } = new UriTypeReader();

		[TestMethod]
		public async Task Escaped_Test()
		{
			var result = await Reader.ReadAsync(Context, "<https://www.google.com>").ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.Value, typeof(Uri));
		}

		[TestMethod]
		public async Task Exception_Test()
		{
			var result = await Reader.ReadAsync(Context, "www.google.com").ConfigureAwait(false);
			AssertIsReaderResultFailure(result);
		}

		[TestMethod]
		public async Task NullOrEmpty_Test()
		{
			var nullResult = await Reader.ReadAsync(Context, null!).ConfigureAwait(false);
			AssertIsReaderResultFailure(nullResult);

			var emptyResult = await Reader.ReadAsync(Context, "").ConfigureAwait(false);
			AssertIsReaderResultFailure(emptyResult);
		}

		[TestMethod]
		public async Task Valid_Test()
		{
			var result = await Reader.ReadAsync(Context, "https://www.google.com").ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.Value, typeof(Uri));
		}
	}
}