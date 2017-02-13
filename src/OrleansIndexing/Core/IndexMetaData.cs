using Orleans.Concurrency;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;
using Orleans.Runtime;

namespace Orleans.Indexing
{
    /// <summary>
    /// The meta data that is stored beside the index
    /// </summary>
    [Serializable]
    public class IndexMetaData
    {
        private Type _indexType;
        private bool _isUniqueIndex;
        private bool _isEager;

        /// <summary>
        /// Constructs an IndexMetaData, which currently only
        /// consists of the type of the index
        /// </summary>
        /// <param name="indexType">the type of the index</param>
        public IndexMetaData(Type indexType, bool isUniqueIndex, bool isEager)
        {
            _indexType = indexType;
            _isUniqueIndex = isUniqueIndex;
            _isEager = isEager;
        }
        
        /// <returns>the type of the index</returns>
        public Type getIndexType()
        {
            return _indexType;
        }

        /// <summary>
        /// Determines whether the index grain is a stateless worker
        /// or not. This piece of information can impact the relationship
        /// between index handlers and the index. 
        /// </summary>
        /// <returns>the result of whether the current index is
        /// a stateless worker or not</returns>
        public bool IsIndexStatelessWorker()
        {
            return IsStatelessWorker(Type.GetType(TypeCodeMapper.GetImplementation(_indexType).GrainClass));
        }

        /// <summary>
        /// A helper function that determines whether a given grain type
        /// is annotated with StatelessWorker annotation or not.
        /// </summary>
        /// <param name="grainType">the grain type to be tested</param>
        /// <returns>true if the grain type has StatelessWorker annotation,
        /// otherwise false.</returns>
        private static bool IsStatelessWorker(Type grainType)
        {
            return grainType.GetCustomAttributes(typeof(StatelessWorkerAttribute), true).Length > 0 ||
                grainType.GetInterfaces()
                    .Any(i => i.GetCustomAttributes(typeof(StatelessWorkerAttribute), true).Length > 0);
        }

        public bool IsUniqueIndex()
        {
            return _isUniqueIndex;
        }

        public bool IsEager()
        {
            return _isEager;
        }
    }
}
