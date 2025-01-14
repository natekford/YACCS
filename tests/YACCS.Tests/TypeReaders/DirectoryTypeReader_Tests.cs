using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Results;
using YACCS.TypeReaders;

namespace YACCS.Tests.TypeReaders;

[TestClass]
public class DirectoryTypeReader_Tests : TypeReader_Tests<DirectoryInfo>
{
	public override ITypeReader<DirectoryInfo> Reader { get; } = new DirectoryInfoTypeReader();

	[TestMethod]
	public async Task Empty_Test()
		=> await AssertFailureAsync<ParseFailed>([""]).ConfigureAwait(false);

	[TestMethod]
	public async Task Valid_Test()
	{
		var path = GetPath("valid");
		Directory.CreateDirectory(path);

		await AssertSuccessAsync([path]).ConfigureAwait(false);
	}

	private string GetPath(string name)
		=> Path.Combine(Directory.GetCurrentDirectory(), name);
}