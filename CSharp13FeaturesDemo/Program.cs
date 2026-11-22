using System.Runtime.CompilerServices;
using static System.Console;

namespace CSharp13FeaturesDemo;

class Program
{
    static async Task Main(string[] args)
    {
        WriteLine("C# 13.0 Features Demo");

        ParamsCollections();
        LockType();
        await RefStructsEverywhere();
        OverloadResolutionPriority();
        PartialProperties();
        SmallerChanges();
    }

    // 1. params works with spans and collection interfaces, not only arrays
    static void ParamsCollections()
    {
        WriteLine("\n-- params collections --");

        WriteLine(Sum(1, 2, 3));                  // 6
        WriteLine(Sum());                         // 0
        WriteLine(Count("a", "b"));               // 2
    }

    static int Sum(params ReadOnlySpan<int> values) // no array allocation
    {
        int total = 0;
        foreach (int v in values) total += v;
        return total;
    }

    static int Count(params IEnumerable<string> values) => values.Count();

    // 2. System.Threading.Lock: the same lock statement compiles to EnterScope()
    static void LockType()
    {
        WriteLine("\n-- System.Threading.Lock --");

        var box = new Box();
        Parallel.For(0, 1000, _ => box.Increment());
        WriteLine(box.Value);                     // 1000
    }

    // 3. ref structs in async methods and as generic arguments (allows ref struct)
    static async Task RefStructsEverywhere()
    {
        WriteLine("\n-- ref struct everywhere --");

        WriteLine(await Measure("hello"));        // 5
        WriteLine(Length(new Reader(3)));         // 3
    }

    static async Task<int> Measure(string s)
    {
        await Task.Yield();
        ReadOnlySpan<char> part = s.AsSpan();     // ref struct in an async method
        return part.Length;                       // as long as it does not cross an await
    }

    static int Length<T>(T value) where T : IHasLength, allows ref struct => value.Length;

    // 4. [OverloadResolutionPriority] lets a library prefer a new overload
    static void OverloadResolutionPriority()
    {
        WriteLine("\n-- OverloadResolutionPriority --");

        Api.Log("a", "b");                        // span
    }

    // 5. Partial properties: declaration and implementation in separate parts
    static void PartialProperties()
    {
        WriteLine("\n-- partial properties --");

        var model = new Model { Name = "ok" };
        WriteLine(model.Name);                    // ok
    }

    // 6. The \e escape sequence and implicit index access (^) in object initializers
    static void SmallerChanges()
    {
        WriteLine("\n-- \\e and ^ in initializers --");

        WriteLine("\e[1mbold\e[0m");              // bold, using ESC instead of \x1b

        var timer = new Timer { Data = { [^1] = 9, [^2] = 8 } };
        WriteLine(timer.Data[2]);                 // 9
    }
}

class Box
{
    private readonly Lock _gate = new();
    public int Value { get; private set; }

    public void Increment()
    {
        lock (_gate)                              // migrating means changing the field type
        {
            Value++;
        }
    }
}

interface IHasLength { int Length { get; } }

ref struct Reader(int n) : IHasLength            // ref structs can implement interfaces
{
    public int Length => n;
}

static class Api
{
    public static void Log(params string[] values) => WriteLine("array");

    [OverloadResolutionPriority(1)]
    public static void Log(params ReadOnlySpan<string> values) => WriteLine("span");
}

public partial class Model
{
    public partial string Name { get; set; }      // declaration
}

public partial class Model
{
    private string _name = "";
    public partial string Name                    // implementation, usually from a source generator
    {
        get => _name;
        set => _name = value;
    }
}

class Timer { public int[] Data { get; } = new int[3]; }
