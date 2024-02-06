using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Results;
using YACCS.TypeReaders;

namespace YACCS.Tests.TypeReaders;

[TestClass]
public class UriTypeReader_Tests : TypeReader_Tests<Uri>
{
	public override ITypeReader<Uri> Reader { get; } = new UriTypeReader();

	[TestMethod]
	public async Task Empty_Test()
		=> await AssertFailureAsync("").ConfigureAwait(false);

	[TestMethod]
	public async Task Escaped_Test()
		=> await AssertSuccessAsync("<https://www.google.com>").ConfigureAwait(false);

	[TestMethod]
	public async Task Exception_Test()
		=> await AssertFailureAsync("www.google.com").ConfigureAwait(false);

	[TestMethod]
	public async Task Null_Test()
		=> await AssertFailureAsync(default(string)!).ConfigureAwait(false);

	[TestMethod]
	public async Task Valid_Test()
		=> await AssertSuccessAsync("https://www.google.com").ConfigureAwait(false);
}