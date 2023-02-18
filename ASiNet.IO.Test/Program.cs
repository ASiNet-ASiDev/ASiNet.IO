using ASiNet.IO;

using (var stream = new MemoryStream())
{
    stream.Write(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 1, 2, 3, 4, 5 });

    Console.WriteLine($"Created: {string.Join(" ", stream.ToArray())}");

    var testBytes = new byte[] { 33, 33, 33, 33 };

    stream.Insert(5, testBytes);

    Console.WriteLine($"Insert: [{string.Join(" ", testBytes.ToArray())}] result: [{string.Join(" ", stream.ToArray())}]");

    byte[] cutResult = null!;
    stream.Cut(5, testBytes.Length, out cutResult);

    Console.WriteLine($"Cut: [{string.Join(" ", cutResult.ToArray())}] result: [{string.Join(" ", stream.ToArray())}]");

    var findBytes = new byte[] { 3, 4, 5 };
    var findBytesResult = stream.Find(findBytes);
    Console.WriteLine($"Find: [{string.Join(" ", findBytes.ToArray())}] in: [{string.Join(" ", stream.ToArray())}] result: [{findBytesResult}]");

    foreach (var item in stream.FindAll(findBytes))
    {
        Console.WriteLine($"Find All Bytes: [{string.Join(" ", findBytes.ToArray())}] in: [{string.Join(" ", stream.ToArray())}] result: [{item}]");
    }

    stream.MoveTo(4, 0, 4);
    Console.WriteLine($"Move To:  result: [{string.Join(" ", stream.ToArray())}]");
}

tt(x => x < 10);

Console.ReadKey();


void tt(Func<int, bool> act) { }