using System;

namespace Orleans.Indexing
{
    //Direct Storage-Managed Index (i.e., without caching the results)
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DSMIndexAttribute : IndexAttribute
    {
        public DSMIndexAttribute() : base(typeof(DirectStorageManagedIndex<,>), true, false)
        {
        }
    }
}
