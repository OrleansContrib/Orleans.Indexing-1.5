//using Orleans;
//using Orleans.Concurrency;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using System;
//using Orleans.Runtime;
//using System.Reflection;
//using System.Linq;
//using Orleans.Storage;
//using System.Runtime.CompilerServices;

//namespace Orleans.Indexing
//{
//    /// <summary>
//    /// IndexableGrainStorageManaged class is the super-class of all grains that
//    /// need to have storage-managed indexing capability but with fault-tolerance requirements provided bythe storage.
//    /// 
//    /// To make a grain indexable, two steps should be taken:
//    ///     1- the grain class should extend IndexableGrainStorageManaged
//    /// </summary>
//    public abstract class IndexableGrainStorageManaged<TState, TProperties> : IndexableGrainNonFaultTolerant<TState, TProperties> where TProperties : new()
//    {
//    }
//}
