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

    [Params(true, false)]
    public bool ContainsDuplicates { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var fixedRandom = _random.Next();
        _array = Enumerable.Range(0, Length)
            .Select(_ => ContainsDuplicates ? _random.Next() : fixedRandom)
            .ToArray();
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