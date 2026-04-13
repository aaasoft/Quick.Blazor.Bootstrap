using SharpCompress.Common;
using SharpCompress.Readers;

namespace SharpCompress.Archives;

public struct OneArchiveEntry
{
    private IArchiveEntry archiveEntry;
    private IEntry readerEntry;
    private IReader reader;

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

    internal OneArchiveEntry(IArchiveEntry archiveEntry)
    {
        this.archiveEntry = archiveEntry;
    }
    internal OneArchiveEntry(Readers.IReader reader)
    {
        readerEntry = reader.Entry;
        this.reader = reader;
    }

    public void WriteTo(Stream writableStream)
    {
        if (archiveEntry != null)
        {
            archiveEntry.WriteTo(writableStream);
            return;
        }
        if (reader != null)
        {
            if (reader.Entry != readerEntry)
                throw new IOException("Reader was moved to other entry.");
            reader.WriteEntryTo(writableStream);
            return;
        }
        throw new NotImplementedException();
    }

    public void WriteToDirectory(string destinationDirectory, ExtractionOptions options = null)
    {
        if (archiveEntry != null)
        {
            archiveEntry.WriteToDirectory(destinationDirectory, options);
            return;
        }
        if (reader != null)
        {
            if (reader.Entry != readerEntry)
                throw new IOException("Reader was moved to other entry.");
            reader.WriteEntryToDirectory(destinationDirectory, options);
            return;
        }
        throw new NotImplementedException();
    }

    public Stream OpenEntryStream()
    {
        if (archiveEntry != null)
        {
            return archiveEntry.OpenEntryStream();
        }
        if (reader != null)
        {
            if (reader.Entry != readerEntry)
                throw new IOException("Reader was moved to other entry.");
            return reader.OpenEntryStream();
        }
        throw new NotImplementedException();
    }
}

public static class SharpCompressIArchiveExtensions
{    
    public static int GetEntriesCount(this IArchive archive)
    {
        var count = 0;

        if (archive.IsSolid)
            using (var reader = archive.ExtractAllEntries())
                while (reader.MoveToNextEntry())
                    count++;
        else
            count = archive.Entries.Count();
        return count;
    }

    public static long GetEntriesTotalSize(this IArchive archive)
    {
        long totalSize = 0;

        if (archive.IsSolid)
            using (var reader = archive.ExtractAllEntries())
                while (reader.MoveToNextEntry())
                    totalSize += reader.Entry.Size;
        else
            totalSize = archive.Entries.Sum(t => t.Size);
        return totalSize;
    }

    public static void EntriesForEach(this IArchive archive, Action<OneArchiveEntry> action, CancellationToken cancellationToken = default)
    {
        if (archive.IsSolid)
        {
            using (var reader = archive.ExtractAllEntries())
            {
                while (reader.MoveToNextEntry())
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;
                    action.Invoke(new OneArchiveEntry(reader));
                }
            }
        }
        else
        {
            foreach (var entry in archive.Entries)
            {
                action.Invoke(new OneArchiveEntry(entry));
            }
        }
    }
}
