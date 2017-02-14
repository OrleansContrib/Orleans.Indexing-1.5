using System;

namespace Orleans.Indexing
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class AIndexAttribute : IndexAttribute
    {
        public AIndexAttribute() : this(false)
        {
        }

        public AIndexAttribute(bool IsEager) : this(Indexing.IndexType.HashIndexSingleBucket, IsEager)
        {
        }

        public AIndexAttribute(IndexType type, bool IsEager = false, int MaxEntriesPerBucket = -1)
        {
            switch (type)
            {
                case Indexing.IndexType.HashIndexSingleBucket:
                    IndexType = typeof(AHashIndexSingleBucket<,>);
                    break;
                case Indexing.IndexType.HashIndexPartitionedByKeyHash:
                    IndexType = typeof(AHashIndexPartitionedPerKey<,>);
                    break;
                case Indexing.IndexType.HashIndexPartitionedBySilo:
                    IndexType = typeof(AHashIndexPartitionedPerSilo<,>);
                    break;
                default:
                    IndexType = typeof(AHashIndexSingleBucket<,>);
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
            this.IsUnique = false;
            this.MaxEntriesPerBucket = MaxEntriesPerBucket;
        }
    }
}
