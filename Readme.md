# Cache sampler

A collection similar to a queue(FIFO) that allows to
- overwrite the oldest entries
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
 [11] 8975.0  
 [12] 9487.0  
 [13] 8582.0  
 [14] 9001.0  
 [15] 8452.0  
 [16] 9087.0  
 [17] 9941.0  
 [18] 9703.0  
 [19] 9271.0  
 [20] 8501.0  
 [21] 8193.0  
 [22] 8521.0  
 [23] 9593.0  
 [24] 9504.0  
 [25] 8518.0  
 [26] 9329.0  
 [27] 9462.0  
 [28] 8995.0  
 [29] 9804.0  
 [30] 8264.0  
 [31] 9086.0  
 [32] 8229.0  
 [33] 9411.0  
 [34] 9285.0  
 [35] 9080.0  
 [36] 8741.0  
 [37] 9776.0  
 [38] 9824.0  
 [39] 8159.0  
 [40] 8906.0  
 [41] 9488.0  
 [42] 8215.0  
 [43] 9543.0  
 [44] 8386.0  
 [45] 9492.0  
 [46] 8110.0  
 [47] 9337.0  
 [48] 8888.0  
 [49] 8111.0  
 [50] 8951.0  
 [51] 9297.0  
 [52] 8591.0  
 [53] 9527.0  
 [54] 8763.0  
 [55] 8234.0  
 [56] 9129.0  
 [57] 8770.0  
 [58] 8245.0  
 [59] 9974.0  
 [60] 8208.0  
 [61] 9787.0  
 [62] 8759.0  
 [63] 9644.0  