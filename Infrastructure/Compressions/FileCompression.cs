using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using static System.StringComparison;

namespace WTW.MdpService.Infrastructure.Compressions;

public static class FileCompression
{
    public static async Task<Stream> Zip(IEnumerable<StreamFile> files)
    {
        var result = new MemoryStream();

        // Do not use new using syntax. Throws: System.InvalidOperationException: Response Content-Length mismatch: too few bytes written (310899 of 311067).
        // https://stackoverflow.com/questions/61351582/response-content-length-mismatch-too-few-bytes-written
        using (var archive = new ZipArchive(result, ZipArchiveMode.Create, true))
        {
            foreach (var file in files)
            {
                var entry = archive.CreateEntry(file.Name);
                using var entryStream = entry.Open();
                await file.Stream.CopyToAsync(entryStream);
            }
        }

        result.Seek(0, SeekOrigin.Begin);
        return result;
    }

    public static async IAsyncEnumerable<InMemoryFile> Unzip(
        Stream zip,
        Func<ZipArchiveEntry, bool> filter = null)
    {
        using var archive = new ZipArchive(zip, ZipArchiveMode.Read, true);

        foreach (var file in archive.Entries.Where(filter ?? new(e => true)))
        {
            var result = new MemoryStream();
            using var fileStream = file.Open();
            await fileStream.CopyToAsync(result);
            result.Seek(0, SeekOrigin.Begin);

            yield return new InMemoryFile(file.Name, result);
        }
    }
}

public static class FileFilter
{
    public static readonly Func<ZipArchiveEntry, bool> Pdf = (file) => file.FullName.EndsWith(".pdf", OrdinalIgnoreCase);
}

public record InMemoryFile(string Name, MemoryStream Stream);

public record StreamFile(string Name, Stream Stream);