// Round floating point numbers
import std.stdio;
import std.algorithm;
import std.range;

alias round = pipe!(to!real, std.math.round, to!string);
static reFloatingPoint = ctRegex!`[0-9]+\.[0-9]+`;

void main()
{
    // Let's get going!
    writeln("Hello World!");
    
    // An example for experienced programmers:
    // Take three arrays, and without allocating
    // any new memory, sort across all the
    // arrays inplace
    int[] arr1 = [4, 9, 7];
    int[] arr2 = [5, 2, 1, 10];
    int[] arr3 = [6, 8, 3];
    sort(chain(arr1, arr2, arr3));
    writefln("%s\n%s\n%s\n", arr1, arr2, arr3);
    // To learn more about this example, see the
    // "Range algorithms" page under "Gems"
}