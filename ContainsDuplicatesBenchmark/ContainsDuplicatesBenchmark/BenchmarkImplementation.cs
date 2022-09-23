using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace ContainsDuplicatesBenchmark;

[SimpleJob(RuntimeMoniker.Net60)]
public class BenchmarkImplementation
{
    private int[] _array = Array.Empty<int>();
    private readonly Random _random = new();

    [Params(1, 10, 100, 1000, 10000)] 
    public int Length { get; set; }

    [Params(
        DuplicatesBehavior.NoDuplicates,
        DuplicatesBehavior.Random,
        DuplicatesBehavior.Near,
        DuplicatesBehavior.Far
    )]
    public DuplicatesBehavior DuplicatesBehavior { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        switch (DuplicatesBehavior)
        {
            case DuplicatesBehavior.NoDuplicates:
                _array = Enumerable.Range(0, Length).ToArray();
                break;
            case DuplicatesBehavior.Random:
                _array = Enumerable.Range(0, Length)
                    .Select(_ => _random.Next())
                    .ToArray();
                break;
            case DuplicatesBehavior.Near:
                _array = Enumerable.Range(0, Length).ToArray();
                if (Length >= 2) _array[0] = _array.Skip(1).First();
                break;
            case DuplicatesBehavior.Far:
                _array = Enumerable.Range(0, Length).ToArray();
                if (Length >= 2) _array[0] = _array.Last();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    [Benchmark(Baseline = true)]
    public bool ContainsDuplicatesUsingHashSetForEach() =>
        ContainsDuplicatesUsingHashSetForEach(_array);

    [Benchmark]
    public bool ContainsDuplicatesUsingHashSetLinqAny() =>
        ContainsDuplicatesUsingHashSetLinqAny(_array);

    [Benchmark]
    public bool ContainsDuplicatesUsingLinqGroupBy() =>
        ContainsDuplicatesUsingLinqGroupBy(_array);

    [Benchmark]
    public bool ContainsDuplicatesUsingLinqDistinct() =>
        ContainsDuplicatesUsingLinqDistinct(_array);

    private static bool ContainsDuplicatesUsingHashSetForEach<T>(
        IEnumerable<T> enumerable)
    {
        var knownElements = new HashSet<T>();
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var element in enumerable)
            if (!knownElements.Add(element))
                return true;
        return false;
    }

    private static bool ContainsDuplicatesUsingHashSetLinqAny<T>(
        IEnumerable<T> enumerable)
    {
        var knownElements = new HashSet<T>();
        return enumerable.Any(element => !knownElements.Add(element));
    }

    private static bool ContainsDuplicatesUsingLinqGroupBy<T>(
        IEnumerable<T> enumerable)
    {
        return enumerable.GroupBy(x => x).Any(x => x.Count() > 1);
    }

    private static bool ContainsDuplicatesUsingLinqDistinct<T>(
        IEnumerable<T> enumerable)
    {
        // ReSharper disable PossibleMultipleEnumeration
        return enumerable.Count() == enumerable.Distinct().Count();
        // ReSharper restore PossibleMultipleEnumeration
    }
}