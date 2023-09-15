using System.Collections;

namespace compression;

public class CompressedString : IEnumerable<char>
{
    private DoubleEndedArrayQueue<char> encoded = new DoubleEndedArrayQueue<char>();
    private DoubleEndedArrayQueue<int> countsToTakeFromOriginal = new DoubleEndedArrayQueue<int>();
    private DoubleEndedArrayQueue<int> amountsToBacktrackInEncoded = new DoubleEndedArrayQueue<int>();
    private DoubleEndedArrayQueue<int> countsToCopyFromEncoded = new DoubleEndedArrayQueue<int>();

    public CompressedString(IEnumerable<char> original)
    {
        int forCalulatingBackTrack = 0;
        foreach (char c in original)
        {
            if (encoded.Contains(c))
            {
                for (int x = 0; x < original.Count(); x++)
                {
                    if (original.ElementAt(x) == c)
                    {
                        countsToTakeFromOriginal.Add(0);
                        amountsToBacktrackInEncoded.Add(forCalulatingBackTrack - x);
                        countsToCopyFromEncoded.Add(1);
                        break;
                    }
                }
            }
            else
            {
                countsToTakeFromOriginal.Add(1);
                amountsToBacktrackInEncoded.Add(0);
                countsToCopyFromEncoded.Add(0);
                encoded.Add(c);
            }
            forCalulatingBackTrack++;
        }
        Console.WriteLine();
    }

    public static IEnumerable<T> Take<T>(IEnumerator<T> enumerator, int count)
    {
        for (int i = 0; i < count; i++)
        {
            enumerator.MoveNext();
            yield return enumerator.Current;
        }
    }

    public static IEnumerable<char> Decompress(IEnumerable<char> encoded, IEnumerable<int> take, IEnumerable<int> backtrack, IEnumerable<int> copy)
    {
        DoubleEndedArrayQueue<char> result = new DoubleEndedArrayQueue<char>();

        var orig = encoded.GetEnumerator();
        var backs = backtrack.GetEnumerator();
        var copies = copy.GetEnumerator();

        foreach (int numberToTake in take)
        {
            backs.MoveNext();
            copies.MoveNext();

            foreach (char c in Take(orig, numberToTake))
            {
                result.Add(c);
            }
            int startIndex = result.Size() - backs.Current;
            int copyCount = copies.Current;
            for (int i = 0; i < copyCount; i++)
            {
                result.Add(result.Get(startIndex + i));
            }
        }
        return result;
    }

    public IEnumerator<char> GetEnumerator()
    {
        return Decompress(encoded, countsToTakeFromOriginal, amountsToBacktrackInEncoded, countsToCopyFromEncoded).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int EncodedLength()
    {
        return encoded.Size() + countsToCopyFromEncoded.Size();
    }
}