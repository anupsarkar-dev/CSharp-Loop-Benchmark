High Performance Loop in C#
===========================

Loop is one of the most important feature in every programming language. C# provides various of ways to iterate a collection. Most common ways to loop through a list or a array is  "For, While, ForEach" loop. There are several others way to iterate a collection using Linq, Parallel ForEach & Span. Lets do some benchmarking and see the actual performance.

To test the performance , I am going to use a list of integers. Size of this interger list will be 100,10000,100000,1000000 elements. We will benchmark each sizes using BenchmarkDotNet.

```
\[Params(100,10000,100000,1000000)\]  
public int size { get; set; }  
  
private List<int> items = new();  
  
\[GlobalSetup\]  
public void InitList()  
{  
    items = Enumerable.Range(1, size).Select(x => random.Next()).ToList();  
}
```

Here I have defined individual methods for each looping.

For

```
\[Benchmark\]  
public void For()  
{  
    for (int i = 0; i < items.Count; i++)  
    {  
        var item = items\[i\];  
    }  
}
```

While

```
\[Benchmark\]  
public void While()  
{  
    var i = 0;  
    while (i < items.Count)  
    {  
        var item = items\[i\];  
        i++;  
    }  
}
```

ForEach

```
  
\[Benchmark\]  
public void ForEach()  
{  
    foreach (var item in items)  
    {  
    }  
}
```

ForEach Linq

```
\[Benchmark\]  
public void Foreach\_Linq()  
{  
    items.ForEach(item =>  
    {  
  
    });  
}
```

Parallel ForEach

```
 \[Benchmark\]  
    public void Parallel\_ForEach()  
    {  
        Parallel.ForEach(items, item =>  
        {  
  
        });  
    }
```

Parallel Linq

```
 \[Benchmark\]  
    public void Parallel\_Linq()  
    {  
        items.AsParallel().ForAll(item =>  
        {  
  
        });  
    }
```

For Span

```
\[Benchmark\]  
public void For\_Span()  
{  
    var asSpanList = CollectionsMarshal.AsSpan(items);  
  
    for (var i=0;i< asSpanList.Length;i++)  
    {  
        var item = asSpanList\[i\];  
    }  
}
```

ForEach Span

```
\[Benchmark\]  
public void Foreach\_Span()  
{  
    foreach (var item in CollectionsMarshal.AsSpan(items))  
    {  
  
    }  
}
```

**Benchmark Results :**
=======================

Running the result in following hardware :

```
BenchmarkDotNet=v0.13.4, OS=Windows 11 (10.0.22000.1455/21H2)  
AMD Ryzen 7 5800H with Radeon Graphics, 1 CPU, 16 logical and 8 physical cores  
.NET SDK=7.0.102  
\[Host\]     : .NET 6.0.13 (6.0.1322.58009), X64 RyuJIT AVX2 \[AttachedDebugger\]  
DefaultJob : .NET 6.0.13 (6.0.1322.58009), X64 RyuJIT AVX2
```

Lets see the result which is very interesting.

**List Size : 100 Elements**
============================

Lets analyze the results. We can see from the result the lowest time taken to iterate 100 elements is 28.44 ns by **For Span**. After then comes for loop which took almost 46.67. Also very interesting to see **Parallel Foreach** takes so much longer 5,731.73 ns (5.7 seconds) . Also in case of **Parallel Linq** took almost 32 seconds. Furthermore they allocated some memory also.

**Winner : For Span (**28.44 ns)

**List Size : 10000 Elements**
==============================

This time the winner is ForEach Span.

**Winner : ForEach Span (**2,338.86 ns)

List Size : 100000 Elements
===========================

One things is noticible here is that the performance of For/ForEach span over the collection size quite predictable and dependable.

**Again the Winner : ForEach Span (**23,353.36 ns)

List Size : 1000000 Elements
============================

**Again the Winner : For Span (**233,805.18 ns)

In every aspect For Span is the winner.

Why is so faster than any other method? Lets deep drive.

Understanding Span<T> in C#
===========================

A `Span<>` is an allocation-free representation of contiguous regions of arbitrary memory. `Span<>` is implemented as a `ref struct` object that contains a `ref` to an object `T` and a length. This means that a Span in C# will always be allocated to stack memory, not heap memory. LetΓÇÖs consider this simplified implementation of `Span<>`:

```
public readonly ref struct Span<T>  
{  
    private readonly ref T \_pointer;  
    private readonly int \_length;  
}
```

Using `Span<>` leads to performance increases because they are always allocated on the stack. Since garbage collection does not have to suspend execution to clean up objects with no references on the heap as often the application runs faster. Pausing an application to collect garbage is always an expensive operation and should be avoided if possible. `Span<>` operations can be as efficient as operations on arrays. Indexing into a span does not require computation to determine the memory address to index to.

Another implementation of a Span in C# is `ReadOnlySpan<>`. It is a struct exactly like `Span<>` other than that its indexer returns a `readonly ref T,` not a `ref T`. This allows us to use `ReadOnlySpan<>` to represent immutable data types such as `String`.

Spans can use other value types such as `int`, `byte`, `ref structs`, `bool`, and `enum`. Spans can not use types like `object`, `dynamic`, or `interfaces`.

Span Limitations
================

Span<T> implementation limits its use in code, but conversely, it provides span useful properties.

The compiler allocates reference type objects on the heap which means **we cannot use spans as fields in reference types.** More specifically `ref struct` objects cannot be boxed like other value-type objects. For the same reason, lambda statements can not make use of spans either. Spans can not be used in asynchronous programming across `await` and `yield` boundaries.

Spans are not appropriate in all situations. **Because we are allocating memory on the stack using spans, we must remember that there is less stack memory than heap memory.** We must consider this when choosing to use spans over strings.

If we want to use a span-like class in asynchronous programming we could take advantage of `Memory<>` and `ReadOnlyMemory<>`. We can create a `Memory<>` object from an array and slice it as we will see, we can do with a span. Once we can synchronously run code, we can get a span from a `Memory<>` object.

``` ini

BenchmarkDotNet=v0.13.4, OS=Windows 11 (10.0.22000.1455/21H2)
AMD Ryzen 7 5800H with Radeon Graphics, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.102
  [Host]     : .NET 6.0.13 (6.0.1322.58009), X64 RyuJIT AVX2 [AttachedDebugger]
  DefaultJob : .NET 6.0.13 (6.0.1322.58009), X64 RyuJIT AVX2


```
|           Method |    size |            Mean |         Error |        StdDev | Allocated |
|----------------- |-------- |----------------:|--------------:|--------------:|----------:|
|              **For** |     **100** |        **46.67 ns** |      **1.165 ns** |      **3.417 ns** |         **-** |
|            While |     100 |        47.51 ns |      1.217 ns |      3.588 ns |         - |
|          ForEach |     100 |        77.41 ns |      1.394 ns |      1.236 ns |         - |
|     Foreach_Linq |     100 |       173.88 ns |      1.106 ns |      1.034 ns |         - |
| Parallel_ForEach |     100 |     5,731.73 ns |     86.697 ns |     72.396 ns |    2586 B |
|    Parallel_Linq |     100 |    32,550.63 ns |    637.052 ns |    933.783 ns |    7544 B |
|         For_Span |     100 |        28.48 ns |      0.579 ns |      0.752 ns |         - |
|     Foreach_Span |     100 |        28.44 ns |      0.497 ns |      0.440 ns |         - |
|              **For** |   **10000** |     **3,624.66 ns** |     **71.397 ns** |     **76.394 ns** |         **-** |
|            While |   10000 |     3,629.78 ns |     51.335 ns |     42.867 ns |         - |
|          ForEach |   10000 |     7,193.93 ns |     72.673 ns |     67.978 ns |         - |
|     Foreach_Linq |   10000 |    17,036.08 ns |    301.575 ns |    267.338 ns |         - |
| Parallel_ForEach |   10000 |    38,179.41 ns |    199.711 ns |    177.039 ns |    4374 B |
|    Parallel_Linq |   10000 |    44,337.14 ns |    671.524 ns |    628.144 ns |    7544 B |
|         For_Span |   10000 |     2,347.31 ns |     12.294 ns |     11.500 ns |         - |
|     Foreach_Span |   10000 |     2,338.86 ns |      4.766 ns |      3.980 ns |         - |
|              **For** |  **100000** |    **35,951.16 ns** |    **235.119 ns** |    **208.427 ns** |         **-** |
|            While |  100000 |    35,876.83 ns |    336.323 ns |    314.597 ns |         - |
|          ForEach |  100000 |    71,685.42 ns |    932.394 ns |    872.162 ns |         - |
|     Foreach_Linq |  100000 |   168,568.65 ns |  1,120.527 ns |  1,048.142 ns |         - |
| Parallel_ForEach |  100000 |   150,327.02 ns |  2,552.659 ns |  2,387.759 ns |    5651 B |
|    Parallel_Linq |  100000 |   161,650.69 ns |  1,610.477 ns |  1,344.822 ns |    7568 B |
|         For_Span |  100000 |    23,767.82 ns |    463.648 ns |    496.099 ns |         - |
|     Foreach_Span |  100000 |    23,353.36 ns |    114.071 ns |     95.255 ns |         - |
|              **For** | **1000000** |   **358,105.68 ns** |  **1,191.449 ns** |  **1,056.188 ns** |         **-** |
|            While | 1000000 |   357,501.54 ns |  1,257.940 ns |    982.117 ns |         - |
|          ForEach | 1000000 |   713,361.81 ns |  1,544.057 ns |  1,289.358 ns |       1 B |
|     Foreach_Linq | 1000000 | 1,682,066.91 ns |  2,430.385 ns |  1,897.485 ns |       1 B |
| Parallel_ForEach | 1000000 |   875,492.44 ns | 16,617.031 ns | 26,356.371 ns |    5737 B |
|    Parallel_Linq | 1000000 | 1,128,220.79 ns | 20,107.288 ns | 17,824.587 ns |    7581 B |
|         For_Span | 1000000 |   233,805.18 ns |  1,804.816 ns |  1,599.923 ns |         - |
|     Foreach_Span | 1000000 |   236,692.77 ns |  4,434.790 ns |  4,148.305 ns |         - |
