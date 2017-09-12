# Orleans Indexing

> THIS PROJECT IS NOT READY FOR PRODUCTION USE. It has undergone a modest amount of testing and code review. It is published to collect community feedback and attract others to make it production-ready. 

Enables grains to be indexed and queried by scalar properties. A research paper describing the interface and implementation can be found [here](http://cidrdb.org/cidr2017/papers/p29-bernstein-cidr17.pdf).

## Features

- Index all grains of a class 
- Fault-tolerant multi-step workflow for index update

### Configuration Options
- Store an index as a single grain
- Partition an index with a bucket for each key value
- Index only the activated grains of a class
- Physically parititon an index over activated grains, so grains and their index are on the same silo
- Allow index to have very large buckets, to handle highly-skewed distribution of values

## Source Code

See [src/OrleansIndexing](src/OrleansIndexing) and [test/Tester/IndexingTests](test/Tester/IndexingTests)

## Example Usage

In this example usage, we assume that we are indexing the location of players in a game and we want to query the players based on their location. First, we describe the steps for defining an index on a grain. Then, we explain the steps for using an index in the queries.

### Defining an index

All the indexable properties of a grain should be defined in a properties class. The type of index is declared by adding annotations on the indexed property. Currently, three index annotations are available: [ActiveIndex](src/OrleansIndexing/Core/Annotations/ActiveIndexAttribute.cs), [TotalIndex](src/OrleansIndexing/Core/Annotations/TotalIndexAttribute.cs), and [StorageManagedIndex](src/OrleansIndexing/Core/Annotations/StorageManagedIndexAttribute.cs). In this example, PlayerProperties contains the only indexed property of a player, which is its location. We want to index the location of all the players that are currently active.

```c#
    [Serializable]
    public class PlayerProperties
    {
        [ActiveIndex]
        string Location { get; set; }
    }
```

The grain interface for the player should implement the `IIndexableGrain<PlayerProperties>` interface. This is a marker interface that declares the IPlayerGrain as an indexed grain interface where its indexed properties are defined in the PlayerProperties class.

```c#
    public interface IPlayerGrain : IGrainWithIntegerKey, IIndexableGrain<PlayerProperties>
    {
        Task<string> GetLocation();
        Task SetLocation(string location);
    }
```

The grain implementation for the player should extend the IndexableGrain<PlayerProperties> class.

```c#
    public class PlayerGrain : IndexableGrain<PlayerProperties>, IPlayerGrain
    {
        public string Location { get { return State.Location; } }

        public Task<string> GetLocation()
        {
            return Task.FromResult(Location);
        }

        public async Task SetLocation(string location)
        {
            State.Location = location;
            // the call to base.WriteStateAsync() determines
            // when the changes to the grain are applied to its
            // corresponding indexes.
            await base.WriteStateAsync();
        }
    }
```

### Using an index to query the grains

The code below queries all the players based on their locations and prints the information related to all the player grains that are located in Zurich.

```c#
var q = from player in GrainClient.GrainFactory.GetActiveGrains<IPlayerGrain, PlayerProperties>()
        where player.Location == "Zurich"
        select player;
        
q.ObserveResults(new QueryResultStreamObserver<IPlayerGrain>(async entry =>
{
    output.WriteLine("primary key = {0}, location = {1}", entry.GetPrimaryKeyLong(), await entry.GetLocation());
}));
```

For more examples, please have a look at [test/Tester/IndexingTests](test/Tester/IndexingTests).

## To Do

- Range indexes
- Replace workflows by transactions
- Replace inheritance by dependency injection (like for transactions)

## License

- This project is licensed under the [MIT license](https://github.com/dotnet/orleans/blob/master/LICENSE).




