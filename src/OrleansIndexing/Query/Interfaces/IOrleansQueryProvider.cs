using Orleans.Runtime;
using Orleans.Runtime.MembershipService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orleans.Indexing
{
    /// <summary>
    /// Extension for the built-in <see cref="IQueryProvider"/> allowing for Orleans specific operations
    /// </summary>
    public interface IOrleansQueryProvider : IQueryProvider
    {
    }
}
