open Expecto

open Mecu

let testSampling = test "Sampling" {
    let cache = Cache.create 30 [-29 .. 30]
    // let first = [11; 2; 30; 12; 14; 21; 25; 16; 5; 9]
    let actual1 = cache |> Cache.sampleWithSeed 1<seed> 10 |> Seq.toList
    let actual2 = cache |> Cache.sampleWithSeed 1<seed> 10 |> Seq.toList
    let actualDifferentSeed = cache |> Cache.sampleWithSeed 2<seed> 10 |> Seq.toList
    Expect.equal actual1 actual2 "Should be the same"
    Expect.notEqual  actual1 actualDifferentSeed "Should be different"
}
let testCreating = test "Creating" {
    Expect.equal (Cache.create 10 [1..10] |> Cache.getSequence [0..9] |> Seq.toList) [1..10] "Should be identical"
    Expect.equal (Cache.create 10 [1..8] |> Cache.getSequence [0..6] |> Seq.toList) (1::1::[1..5]) "Should be identical"
}
let testOverwriting = test "Overwriting" {
    let cache =
        Cache.create 10 [1]
        |> Cache.insertSequence [2..10]
    Expect.equal (cache |> Cache.getSequence [0..9] |> Seq.toList) [1..10] "Should be the same"
    cache
    |> Cache.insertItem 11
    |> Cache.insertSequence [12..50]
    |> ignore
    Expect.sequenceEqual (cache.GetSequence [0..9]) [41..50] "Same values"
}
let testExample = test "Sample example" {
    let mySample =
        Cache.create 2000 [0.0]                         // similar to python zero(2000)
        |> Cache.insertSequence [1.0 .. 5000.0]         // insert 5000 elements that are truncated to the last cache capacity 2000 in our case
        |> Cache.insertSequence [5001.0 .. 10_000.0]    // insert another 5000 elements that will retain the last 2000 entries (again the cache capacity)
        |> Cache.sample 64                              // sample 64 unique entries from that current cache
    Expect.isNonEmpty mySample "Should not be empty :)"
}
let tests = testList "Cache" [
    testSampling
    testCreating
    testOverwriting
    testExample
]


[<EntryPoint>]
let main args =
    runTestsWithArgs
        { defaultConfig with
            verbosity = Expecto.Logging.LogLevel.Info
            printer = Expecto.Impl.TestPrinters.summaryPrinter defaultConfig.printer }
        [||]
        tests