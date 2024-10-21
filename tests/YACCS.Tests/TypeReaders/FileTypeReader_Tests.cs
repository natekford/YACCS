using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Results;
using YACCS.TypeReaders;

namespace YACCS.Tests.TypeReaders;

[TestClass]
public class FileTypeReader_Tests : TypeReader_Tests<FileInfo>
{
	public override ITypeReader<FileInfo> Reader { get; } = new FileInfoTypeReader();

	[TestMethod]
	public async Task Empty_Test()
		=> await AssertFailureAsync<ParseFailed>("").ConfigureAwait(false);

	[TestMethod]
	public async Task Valid_Test()
	{
		var path = GetPath("valid");
		Directory.CreateDirectory(Path.GetDirectoryName(path)!);
		File.Create(path).Dispose();

		await AssertSuccessAsync(path).ConfigureAwait(false);
	}

	private string GetPath(string name)
		=> Path.Combine(Directory.GetCurrentDirectory(), "TEMP", name + ".txt");
}