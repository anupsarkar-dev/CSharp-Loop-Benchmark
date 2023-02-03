High Performance Loop in C#
===========================

Loop is one of the most important feature in every programming language. C# provides various of ways to iterate a collection. Most common ways to loop through a list or a array is ΓÇ£For, While, ForEachΓÇ¥ loop. There are several others way to iterate a collection using Linq, Parallel ForEach & Span. Lets do some benchmarking and see the actual performance.

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

SpanΓÇÖs implementation limits its use in code, but conversely, it provides span useful properties.

The compiler allocates reference type objects on the heap which means **we cannot use spans as fields in reference types.** More specifically `ref struct` objects cannot be boxed like other value-type objects. For the same reason, lambda statements can not make use of spans either. Spans can not be used in asynchronous programming across `await` and `yield` boundaries.

Spans are not appropriate in all situations. **Because we are allocating memory on the stack using spans, we must remember that there is less stack memory than heap memory.** We must consider this when choosing to use spans over strings.

If we want to use a span-like class in asynchronous programming we could take advantage of `Memory<>` and `ReadOnlyMemory<>`. We can create a `Memory<>` object from an array and slice it as we will see, we can do with a span. Once we can synchronously run code, we can get a span from a `Memory<>` object.

Github Repo :

[https://github.com/anupsarkar-dev/LoopBenchmark/tree/main/Benchmark.loop](https://github.com/anupsarkar-dev/LoopBenchmark/tree/main/Benchmark.loop)
