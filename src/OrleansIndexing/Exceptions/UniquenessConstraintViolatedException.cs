
namespace Orleans.Indexing
{
    /// <summary>
    /// This exception is thrown when a uniqueness constraint defined on an index is violated.
    /// </summary>
    public class UniquenessConstraintViolatedException : System.Exception
    {
        public UniquenessConstraintViolatedException(string message) : base(message)
        {
        }
    }
}
