namespace ASiNet.IO;
public static class StreamExtensions
{

    /// <summary>
    /// Write bytes to start of stream.
    /// </summary>
    /// <param name="bytes">Write bytes.</param>
    /// <param name="bufferSize"> Default size: 0.5Mb (524288 bytes) </param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static void WriteStart(this Stream stream, byte[] bytes, int bufferSize = 524288)
    {
        if (bufferSize <= 0)
            throw new ArgumentOutOfRangeException("bufferSize < 0");
        if (bytes.Length <= 0)
            return;

        MoveBase(stream, 0, bytes.Length, bufferSize);
        stream.Position = 0;
        stream.Write(bytes);
    }

    /// <summary>
    /// Insert bytes.
    /// </summary>
    /// <param name="start">Insert position.</param>
    /// <param name="bytes">Insert bytes.</param>
    /// <param name="bufferSize"> Default size: 0.5Mb (524288 bytes) </param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static void Insert(this Stream stream, long start, byte[] bytes, int bufferSize = 524288)
    {
        if (start > stream.Length)
            throw new ArgumentException("start > stream.Length");
        if (bufferSize <= 0)
            throw new ArgumentOutOfRangeException("bufferSize < 0");
        if (bytes.Length == 0)
            return;

        MoveBase(stream, start, bytes.Length, bufferSize);
        stream.Position = start;
        stream.Write(bytes);
    }

    /// <summary>
    /// Cut bytes.
    /// </summary>
    /// <param name="start">Position of first byte to cut.</param>
    /// <param name="length">Cut bytes count</param>
    /// <param name="bufferSize"> Default size: 0.5Mb (524288 bytes) </param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static void Cut(this Stream stream, long start, int length, int bufferSize = 524288)
    {
        if (start > stream.Length)
            throw new ArgumentException("start > stream.Length");
        if (bufferSize <= 0)
            throw new ArgumentOutOfRangeException("bufferSize < 0");
        if (length <= 0)
            return;

        CutBase(stream, start, length, bufferSize);
    }

    /// <summary>
    /// Cut bytes and return.
    /// </summary>
    /// <param name="start">Position of first byte to cut.</param>
    /// <param name="length">Cut bytes count.</param>
    /// <param name="result">Cut bytes.</param>
    /// <param name="bufferSize"> Default size: 0.5Mb (524288 bytes) </param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static void Cut(this Stream stream, long start, int length, out byte[] result, int bufferSize = 524288)
    {
        if (start > stream.Length)
            throw new ArgumentException("start > stream.Length");
        if (bufferSize <= 0)
            throw new ArgumentOutOfRangeException("bufferSize < 0");
        if (length <= 0)
        {
            result = Array.Empty<byte>();
            return;
        }

        result = new byte[length];
        stream.Position = start;
        stream.Read(result);

        CutBase(stream, start, length, bufferSize);
    }

    /// <summary>
    /// Move bytes.
    /// </summary>
    /// <param name="start">Position of first byte to moved.</param>
    /// <param name="offset">Move distance.</param>
    /// <param name="bufferSize">Default size: 0.5Mb (524288 bytes)</param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static void Move(this Stream stream, long start, long offset, int bufferSize = 524288)
    {
        if (start > stream.Length)
            throw new ArgumentException("start > stream.Length");
        if (bufferSize <= 0)
            throw new ArgumentOutOfRangeException("bufferSize < 0");
        if (offset <= 0)
            throw new ArgumentOutOfRangeException("offset < 0");
        MoveBase(stream, start, offset, bufferSize);
        stream.Position = start;
        stream.Write(new byte[offset]);
    }

    /// <summary>
    /// Move bytes from start position to new position.
    /// </summary>
    /// <param name="start">Position of first byte to moved.</param>
    /// <param name="to">New position of first byte.</param>
    /// <param name="bufferSize">Default size: 0.5Mb (524288 bytes)</param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static void MoveTo(this Stream stream, long start, long to, int length, int bufferSize = 524288)
    {
        if (start > stream.Length || to > stream.Length)
            throw new ArgumentException("start or end > stream.Length");
        if (bufferSize <= 0)
            throw new ArgumentOutOfRangeException("bufferSize < 0");
        if (length <= 0)
            throw new ArgumentOutOfRangeException("offset < 0");

        var data = new byte[length];

        stream.Position = start;
        var count = stream.Read(data);

        CutBase(stream, start, count, bufferSize);
        MoveBase(stream, to, count, bufferSize);
        stream.Position = to;
        stream.Write(data, 0, count);
    }

    /// <summary>
    /// Find bytes.
    /// </summary>
    /// <param name="bytes"> Pattern max length: <see cref="short.MaxValue"/></param>
    /// <returns>First byte position, or -1 if bytes not found.</returns>
    public static long Find(this Stream stream, byte[] bytes, long maxPosition = -1)
    {
        if (bytes.Length > short.MaxValue)
            return -1;
        if (maxPosition == -1)
            maxPosition = stream.Length;
        var result = -2L;

        stream.Position = 0;

        while (result != -1 && stream.Position < maxPosition)
        {
            result = FindBase(stream, 0, bytes);
            if (result >= 0)
                break;


        }
        return result == -2 ? -1 : result;
    }

    /// <summary>
    /// Find All bytes.
    /// </summary>
    /// <param name="bytes">Pattern max length: <see cref="short.MaxValue"/></param>
    /// <param name="maxCount">Max find count/</param>
    /// <returns>First byte position.</returns>
    public static IEnumerable<long> FindAll(this Stream stream, byte[] bytes, int maxCount = -1, long maxPosition = -1)
    {
        if (bytes.Length > short.MaxValue)
            yield break;
        if (maxPosition == -1)
            maxPosition = stream.Length;
        if (maxCount == -1)
            maxCount = int.MaxValue;
        var result = -2L;
        var count = 0;

        stream.Position = 0;

        while (result != -1 && stream.Position < maxPosition && count <= maxCount)
        {
            result = FindBase(stream, 0, bytes);
            if (result >= 0)
                yield return result;
        }
        yield break;
    }

    private static void MoveBase(Stream stream, long start, long offset, int bufferSize)
    {
        var isContinue = true;
        if (bufferSize > stream.Length)
        {
            bufferSize = (int)(stream.Length - start);
            isContinue = false;
        }
        var buffer = new byte[bufferSize];
        var readStartPos = stream.Length;
        do
        {
            readStartPos -= bufferSize;
            if (readStartPos < 0)
            {
                buffer = new byte[(int)Math.Abs(readStartPos)];
                readStartPos = start;
                isContinue = false;
            }

            stream.Position = readStartPos;
            var readBytes = stream.Read(buffer);
            stream.Position = readStartPos + offset;
            stream.Write(buffer, 0, readBytes);
        } while (isContinue);
    }

    private static void CutBase(Stream stream, long start, long offset, int bufferSize)
    {
        var buffer = new byte[bufferSize];
        var readStartPos = start + offset;
        var oldReadBytesCount = 0;
        do
        {
            stream.Position = readStartPos;
            oldReadBytesCount = stream.Read(buffer);
            stream.Position = readStartPos - offset;
            stream.Write(buffer, 0, oldReadBytesCount);
            readStartPos += bufferSize;
        } while (oldReadBytesCount == bufferSize);
        stream.SetLength(stream.Length - offset);
    }

    private static long FindBase(Stream stream, short offset, byte[] bytes)
    {
        if (offset == bytes.Length)
            return stream.Position - bytes.Length;

        var newByte = stream.ReadByte();
        if (newByte == bytes[offset])
            return FindBase(stream, ++offset, bytes);
        else if (newByte == -1)
            return -1;
        else
            return -2;
    }
}