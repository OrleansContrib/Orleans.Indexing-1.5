using System;

namespace Orleans.Indexing
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class IIndexAttribute : IndexAttribute
    {
        public IIndexAttribute() : this(false)
        {
        }

        public IIndexAttribute(bool IsEager) : this(Indexing.IndexType.HashIndexSingleBucket, IsEager, false)
        {
        }

        public IIndexAttribute(IndexType type, bool IsEager = false, bool IsUnique = false, int MaxEntriesPerBucket = -1)
        {
            switch (type)
            {
                case Indexing.IndexType.HashIndexSingleBucket:
                    IndexType = typeof(IHashIndexSingleBucket<,>);
                    break;
                case Indexing.IndexType.HashIndexPartitionedByKeyHash:
                    IndexType = typeof(IHashIndexPartitionedPerKey<,>);
                    break;
                //I-indexes partitioned by silo are not supported
                case Indexing.IndexType.HashIndexPartitionedBySilo:
                    throw new Exception("PartitionedBySilo indexes are not supported for I-Indexes.");
                //    IndexType = typeof(IHashIndexPartitionedPerSilo<,>);
                //    break;
                default:
                    IndexType = typeof(IHashIndexSingleBucket<,>);
                    break;
            }
            this.IsEager = IsEager;
            //A-Index cannot be defined as unique
            //Suppose there's a unique A-index over persistent objects.
            //The activation of an initialized object could create a conflict in
            //the A-index. E.g., there's an active player PA with email foo and
            //a non-active persistent player PP with email foo. An attempt to
            //activate PP will cause a violation of the A-index on email.
            //This implies we should disallow such indexes.
            this.IsUnique = IsUnique;
            this.MaxEntriesPerBucket = MaxEntriesPerBucket;
        }
    }
}
