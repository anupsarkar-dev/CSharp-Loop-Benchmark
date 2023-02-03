using BenchmarkDotNet.Attributes;
using System.Runtime.InteropServices;

namespace Benchmark.loop;

[MemoryDiagnoser(false)]
public class LoopBenchmark
{
    private static readonly Random random = new(999999);

    [Params(100,10000,100000,1000000)]
    public int size { get; set; } = 100;

    private List<int> items = new();

    [GlobalSetup]
    public void InitList()
    {
        items = Enumerable.Range(1, size).Select(x => random.Next()).ToList();
    }

    [Benchmark]
    public void For()
    {
        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
        }
    }

    [Benchmark]
    public void While()
    {
        var i = 0;
        while (i < items.Count)
        {
            var item = items[i];
            i++;
        }
    }

    [Benchmark]
    public void ForEach()
    {
        foreach (var item in items)
        {
        }
    }

    [Benchmark]
    public void Foreach_Linq()
    {
        items.ForEach(item =>
        {

        });
    }

    [Benchmark]
    public void Parallel_ForEach()
    {
        Parallel.ForEach(items, item =>
        {

        });
    }

    [Benchmark]
    public void Parallel_Linq()
    {
        items.AsParallel().ForAll(item =>
        {

        });
    }

    [Benchmark]
    public void For_Span()
    {
        var asSpanList = CollectionsMarshal.AsSpan(items);

        for (var i=0;i< asSpanList.Length;i++)
        {
            var item = asSpanList[i];
        }
    }

    [Benchmark]
    public void Foreach_Span()
    {
        foreach (var item in CollectionsMarshal.AsSpan(items))
        {

        }
    }

}