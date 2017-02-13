using System;

namespace Orleans.Indexing
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IndexAttribute : Attribute
    {
        public Type IndexType { get; protected set; }
        public bool IsUnique { get; protected set; }
        public bool IsEager { get; protected set; }

        public IndexAttribute() : this(false)
        {
        }

        public IndexAttribute(bool IsEager) : this(typeof(AHashIndexSingleBucket<,>), IsEager, false)
        {
        }

        public IndexAttribute(Type IndexType, bool IsEager = false, bool IsUnique = false)
        {
            this.IndexType = IndexType;
            this.IsUnique = IsUnique;
            this.IsEager = IsEager;
        }
    }
}
