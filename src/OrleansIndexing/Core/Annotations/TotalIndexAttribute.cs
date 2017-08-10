using System;

namespace Orleans.Indexing
{
    /// <summary>
    /// The attribute for declaring the property fields of an
    /// indexed grain interface to have a "Total Index", which
    /// is also known as "Initialized Index".
    /// 
    /// A "Total Index" indexes all the grains that have been
    /// created during the lifetime of the application.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class TotalIndexAttribute : IndexAttribute
    {
        /// <summary>
        /// The default constructor for TotalIndex.
        /// </summary>
        public TotalIndexAttribute() : this(false)
        {
        }

        /// <summary>
        /// The constructor for TotalIndex.
        /// </summary>
        /// <param name="IsEager">Determines whether the index should be
        /// updated eagerly upon any change in the indexed grains. Otherwise,
        /// the update propagation happens lazily after applying the update
        /// to the grain itself.</param>
        public TotalIndexAttribute(bool IsEager) : this(Indexing.TotalIndexType.HashIndexSingleBucket, IsEager, false)
        {
        }

        /// <summary>
        /// The full-option constructor for TotalIndex.
        /// </summary>
        /// <param name="type">The index type for the Total index</param>
        /// <param name="IsEager">Determines whether the index should be
        /// updated eagerly upon any change in the indexed grains. Otherwise,
        /// the update propagation happens lazily after applying the update
        /// to the grain itself.</param>
        /// <param name="IsUnique">Determines whether the index should maintain
        /// a uniqueness constraint.</param>
        /// <param name="MaxEntriesPerBucket">The maximum number of entries
        /// that should be stored in each bucket of a distributed index. This
        /// option is only considered if the index is a distributed index.
        /// Use -1 to declare no limit.</param>
        public TotalIndexAttribute(TotalIndexType type, bool IsEager = false, bool IsUnique = false, int MaxEntriesPerBucket = -1)
        {
            switch (type)
            {
                case Indexing.TotalIndexType.HashIndexSingleBucket:
                    IndexType = typeof(TotalHashIndexSingleBucket<,>);
                    break;
                case Indexing.TotalIndexType.HashIndexPartitionedByKeyHash:
                    IndexType = typeof(TotalHashIndexPartitionedPerKey<,>);
                    break;
                default:
                    IndexType = typeof(TotalHashIndexSingleBucket<,>);
                    break;
            }
            this.IsEager = IsEager;
            this.IsUnique = IsUnique;
            this.MaxEntriesPerBucket = MaxEntriesPerBucket;
        }
    }
}
