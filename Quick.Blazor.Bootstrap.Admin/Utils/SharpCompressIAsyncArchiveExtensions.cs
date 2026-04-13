using SharpCompress.Common;
using SharpCompress.Readers;

namespace SharpCompress.Archives;

public struct OneAsyncArchiveEntry
{
    private IArchiveEntry archiveEntry;
    private IEntry readerEntry;
    private IAsyncReader reader;

    public IEntry Entry
    {
        get
        {
            if (archiveEntry != null)
                return archiveEntry;
            if (readerEntry != null)
                return readerEntry;
            return null;
        }
    }

    internal OneAsyncArchiveEntry(IArchiveEntry archiveEntry)
    {
        this.archiveEntry = archiveEntry;
    }
    internal OneAsyncArchiveEntry(IAsyncReader reader)
    {
        readerEntry = reader.Entry;
        this.reader = reader;
    }

    public async Task WriteToAsync(Stream writableStream,CancellationToken cancellationToken = default)
    {
        if (archiveEntry != null)
        {
            await archiveEntry.WriteToAsync(writableStream, null, cancellationToken);
            return;
        }
        if (reader != null)
        {
            if (reader.Entry != readerEntry)
                throw new IOException("Reader was moved to other entry.");
            await reader.WriteEntryToAsync(writableStream, cancellationToken);
            return;
        }
        throw new NotImplementedException();
    }

    public async Task WriteToDirectoryAsync(string destinationDirectory, ExtractionOptions options = null,CancellationToken cancellationToken = default)
    {
        if (archiveEntry != null)
        {
            await archiveEntry.WriteToDirectoryAsync(destinationDirectory, options, cancellationToken);
            return;
        }
        if (reader != null)
        {
            if (reader.Entry != readerEntry)
                throw new IOException("Reader was moved to other entry.");
            await reader.WriteEntryToDirectoryAsync(destinationDirectory, options, cancellationToken);
            return;
        }
        throw new NotImplementedException();
    }

    public async Task<Stream> OpenEntryStreamAsync(CancellationToken cancellationToken = default)
    {
        if (archiveEntry != null)
        {
            return await archiveEntry.OpenEntryStreamAsync(cancellationToken);
        }
        if (reader != null)
        {
            if (reader.Entry != readerEntry)
                throw new IOException("Reader was moved to other entry.");
            return await reader.OpenEntryStreamAsync(cancellationToken);
        }
        throw new NotImplementedException();
    }
}

public static class SharpCompressIAsyncArchiveExtensions
{
    public static async Task<int> GetEntriesCountAsync(this IAsyncArchive archive, CancellationToken cancellationToken = default)
    {
        var count = 0;
        if (await archive.IsSolidAsync())
            await using (var reader = await archive.ExtractAllEntriesAsync())
                while (await reader.MoveToNextEntryAsync(cancellationToken))
                    count++;
        else
        {
            count = await archive.EntriesAsync.CountAsync(cancellationToken);
        }
        return count;
    }

    public static async Task<long> GetEntriesTotalSizeAsync(this IAsyncArchive archive, CancellationToken cancellationToken = default)
    {
        long totalSize = 0;
        if (await archive.IsSolidAsync())
            await using (var reader = await archive.ExtractAllEntriesAsync())
                while (await reader.MoveToNextEntryAsync(cancellationToken))
                    totalSize += reader.Entry.Size;
        else
        {
            await foreach (var entry in archive.EntriesAsync)
                totalSize += entry.Size;
        }
        return totalSize;
    }
    public static async Task EntriesForEachAsync(this IAsyncArchive archive, Func<OneAsyncArchiveEntry, Task> function, CancellationToken cancellationToken = default)
    {
        if (await archive.IsSolidAsync())
            await using (var reader = await archive.ExtractAllEntriesAsync())
                while (await reader.MoveToNextEntryAsync(cancellationToken))
                    await function.Invoke(new(reader));
        else
            await foreach (var entry in archive.EntriesAsync)
                await function.Invoke(new(entry));
    }
}
