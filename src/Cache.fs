namespace Mecu

module Sampler =

    open System.Collections.Generic

    let sample capacity items (random: System.Random) =
        if items >= capacity then failwithf "Not enough items %d to sample %d" capacity items
        if items < 0 then failwithf "Cannot sample a negative number of items: %d" items

        let uniqueEnforcer = SortedSet<int>()
        let pickAnUniqueCandidateUntilSuccess () =
            let candidate = random.Next(capacity)
            if uniqueEnforcer.Add candidate
                then candidate
                else
                    if items * 4 > capacity * 3 // a close one to the random is good enough for me when I sample more then 75% of total capacity
                    then
                        let mutable nextAttempt = (candidate + 1) % capacity
                        while not (uniqueEnforcer.Add nextAttempt) do
                            nextAttempt <- nextAttempt + 1
                        nextAttempt
                    else
                        let mutable nextAttempt = random.Next(capacity)
                        while not (uniqueEnforcer.Add nextAttempt) do
                            nextAttempt <- random.Next(capacity)
                        nextAttempt
        let rec getNewItem taken remaining =
            if 0 = remaining
            then taken
            else
                let candidate = pickAnUniqueCandidateUntilSuccess()
                getNewItem (candidate :: taken) (remaining - 1)
        getNewItem [] items

[<Measure>] type seed;

type 't Cache(capacity: int, defaultValue: 't) =
    let items = Array.replicate capacity defaultValue
    let mutable entry = 0
    /// When an item is removed from cache this function will be invoked. I use it to dispose the tensors. Default value is the action that doesn't do anything, the compiler should optimize it a noop.
    member val OnItemRemoved = (fun (_itemRemoved: 't) -> ()) with get, set
    member val Capacity = capacity
    /// As a side-effect will invoke OnItemRemoved on the replaced entry from the cache
    member c.InsertItem(value: 't) =
        c.Get entry |> c.OnItemRemoved
        Array.set items entry value
        entry <- (entry + 1) % capacity
        c
    /// As a side-effect will invoke OnItemRemoved on the replaced entries from the cache
    member c.InsertSequence(items: #seq<'t>) =
        Seq.iter (c.InsertItem >> ignore) items
        c
    member c.Get(atIndex: int) = Array.get items ((entry + atIndex + capacity) % capacity)
    member c.GetSequence(indexes: #seq<int>) = Seq.map c.Get indexes
    member c.Sample(items, random: System.Random option) =
        match random with
            | Some r -> r
            | None -> System.Random System.DateTime.Now.Millisecond
        |> Sampler.sample capacity items
        |> c.GetSequence

[<RequireQualifiedAccess>]
module Cache =
    let create capacity (items: #seq<'t>) =
        use enumerator = items.GetEnumerator()
        let cache =
            if enumerator.MoveNext()
                then
                    Cache(capacity, enumerator.Current).InsertItem enumerator.Current
                else failwith "Could not instantiate the Cache with an empty sequence"
        while enumerator.MoveNext() do
            cache.InsertItem enumerator.Current |> ignore
        cache

    /// See also itemInsert
    let insertItem item (cache: 't Cache) = cache.InsertItem item
    /// See also insertItem
    let itemInsert (cache: 't Cache) item = cache.InsertItem item
    /// See also sequenceInsert
    let insertSequence items (cache: 't Cache) = cache.InsertSequence items
    /// See also insertSequence
    let sequenceInsert (cache: 't Cache) items = cache.InsertSequence items
    let get atIndex (cache: 't Cache) = cache.Get atIndex
    /// See also sequenceGet
    let getSequence indexes (cache: 't Cache) = cache.GetSequence indexes
    /// See also getSequence
    let sequenceGet (cache: 't Cache) indexes = cache.GetSequence indexes
    let sample items (cache: 't Cache) = cache.Sample(items, None)
    let sampleWithSeed (seed: int<seed>) items (cache: 't Cache) = cache.Sample(items, Some <| System.Random(int seed))
    let sampleWithRandom random items (cache: 't Cache) = cache.Sample(items, Some random)
