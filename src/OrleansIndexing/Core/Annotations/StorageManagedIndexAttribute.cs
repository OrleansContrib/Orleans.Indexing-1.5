using System;

namespace Orleans.Indexing
{
    /// <summary>
    /// Direct Storage-Managed Index (i.e., without caching the results)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class StorageManagedIndexAttribute : IndexAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        public StorageManagedIndexAttribute() : base(typeof(DirectStorageManagedIndex<,>), true, false)
        {
        }
    }
}
