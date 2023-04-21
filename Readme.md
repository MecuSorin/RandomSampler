# Cache sampler

A collection similar to a queue(FIFO) that allows to
- overwrite the oldest entries (with the extra ability to act on the discarded entries)
- sample for some unique(random or not) entries from the collection

### Reason to be

For a RL implementation, needed to sample from a cache of recent events, but didn't found something that will not shuffle the entire cache before spitting few items.

## Usage

It allows to preset the random generator with a custom seed.

See the tests, but in principle is simple as:

```fsharp
Cache.create 2000 [0.0]                         // similar to python zero(2000)
|> Cache.insertSequence [1.0 .. 5000.0]         // insert 5000 elements that are truncated to the last cache capacity 2000 in our case 
|> Cache.insertSequence [5001.0 .. 10_000.0]    // insert another 5000 elements that will retain the last 2000 entries (again the cache capacity)
|> Cache.sample 64                              // sample 64 unique entries from that current cache
```

> [00] 9352.0  
 [01] 8949.0  
 [02] 8030.0  
 [03] 8470.0  
 [04] 8151.0  
 [05] 8049.0  
 [06] 8882.0  
 [07] 9272.0  
 [08] 9430.0  
 [09] 8033.0  
 [10] 9354.0  
 ...  
 [60] 8208.0  
 [61] 9787.0  
 [62] 8759.0  
 [63] 9644.0  

 ```fsharp
use cartPole = new CartPoleEnv(AvaloniaEnvViewer.Factory)
let actionsCache = Cache.create actionsSize [0 .. actionsSize - 1]      // creates a cache for actions

let replayBuffer = 
    seq {
        let mutable observations = cartPole.Reset()
        for i = 1 to minReplaySize do
            let action = actionsCache |> Cache.sample 1 |> Seq.head     // sampling just 1 item
            let newObservations = cartPole.Step(action)
            yield {| fromState = observations
                     action = action
                     reward = newObservations.Reward
                     toState = newObservations.Observation |}
            if newObservations.Done
                then observations <- cartPole.Reset()
                else observations <- newObservations.Observation
    }
    |> Cache.create bufferSize      // creating a cache that initially is filled with the first transition from sequence, then the rest of the sequence is added to the cache

...

let newTransitions =
    replayBuffer
    |> Cache.sample 64              // take a batch of random 64 recorded transitions
    |> Seq.map actThenUpdateOnlineValueThenReturnTransition
replayBuffer
|> Cache.insertSequence newTransitions      // insert new transitions in the cache (overwriting the oldest transitions in the cache if the capacity is exceeded)
```

If you want to store disposable items (like tensors) in the cache, there is a way to dispose the discarded entries on insert operations. See *OnItemRemoved* property of the Cache class (you can invoke an action like dispose/log/etc. for each discarded entry).

If you are a C# user then explore the API using intellisense on:
- Mecu.Sampler.sample static method
- Mecu.Cache class
