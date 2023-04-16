namespace Mecu

module Sampler = 

    open System.Collections.Generic

    let sample capacity items (random: System.Random) =
        if items >= capacity then failwithf "Not enough items %d to sample %d" capacity items
        if items < 0 then failwithf "Cannot sample a negative number of items: %d" items

        let uniqueEnforcer = SortedSet<int>()
        let rec getNewItem taken remaining =
            if 0 = remaining 
            then taken
            else 
                let candidate = random.Next(capacity)
                if uniqueEnforcer.Add candidate
                then getNewItem (candidate :: taken) (remaining - 1)
                else getNewItem taken remaining
        getNewItem [] items

[<Measure>] type seed;

type 't Cache(capacity: int, defaultValue: 't) =
    let items = Array.replicate capacity defaultValue
    let mutable entry = 0

    member val Capacity = capacity
    member c.InsertItem(value: 't) =
        Array.set items entry value
        entry <- (entry + 1) % capacity
        c
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

    let insertItem item (cache: 't Cache) = cache.InsertItem item
    let insertSequence items (cache: 't Cache) = cache.InsertSequence items
    let get atIndex (cache: 't Cache) = cache.Get atIndex
    let getSequence indexes (cache: 't Cache) = cache.GetSequence indexes
    let sample items (cache: 't Cache) = cache.Sample(items, None) 
    let sampleWithSeed (seed: int<seed>) items (cache: 't Cache) = cache.Sample(items, Some <| System.Random(int seed)) 
    let sampleWithRandom random items (cache: 't Cache) = cache.Sample(items, Some random)
