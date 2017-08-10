using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Orleans.Runtime
{
    /// <summary>
    /// Client gateway interface for obtaining the grain interface/type map.
    /// </summary>
    internal interface IClusterTypeManager : ISystemTarget
    {
        /// <summary>
        /// Acquires grain interface map for all grain types supported across the entire cluster
        /// </summary>
        /// <returns></returns>
        Task<IGrainTypeResolver> GetClusterTypeCodeMap();

        /// <summary>
        /// Acquires the dictionary of all indexes defined on a grain interface type.
        /// </summary>
        /// <returns>A dictionary of grain interface types to their
        /// corresponding index information. The index information is
        /// a dictionary from index IDs defined on a grain interface to
        /// a triple. The triple consists of: 1) the index object (that
        /// implements IndexInterface, 2) the IndexMetaData object for
        /// this index, and 3) the IndexUpdateGenerator instance for this index.
        /// This triple is untyped, because IndexInterface, IndexMetaData
        /// and IndexUpdateGenerator types are not visible in this project.
        /// 
        /// This method returns an empty dictionary if the OrleansIndexing 
        /// project is not available.</returns>
        Task<IDictionary<Type, IDictionary<string, Tuple<object, object, object>>>> GetIndexes();

        Task<Streams.ImplicitStreamSubscriberTable> GetImplicitStreamSubscriberTable(SiloAddress silo);
    }

    internal interface ISiloTypeManager : ISystemTarget
    {
        /// <summary>
        /// Acquires grain interface map for all grain types supported by hosted silo.
        /// </summary>
        /// <returns></returns>
        Task<GrainInterfaceMap> GetSiloTypeCodeMap();
    }
}
