#### 1.1.0 - April 21 2023

Added:
- an action for each entry replaced in cache (I use it for disposing tensors).
- added a parameter reversed order Yoda style for  *insertItem*, **insertSequence*, *getSequence* since usually in F# flow you first process the item/items then you insert them into cache.

Changed behaviour:
- the sampling method when the user ask for more samples than 75% of the cache capacity 


#### 1.0.0 - April 17 2023

- Initial release
