using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans.Runtime;
using Orleans.Runtime.MembershipService;

namespace Orleans.Indexing
{
    /// <summary>
    /// A utility class for the low-level operations related to silos
    /// </summary>
    internal static class SiloUtils
    {
        #region copy & paste from ManagementGrain.cs

        internal static Task<Dictionary<SiloAddress, SiloStatus>> GetHosts(bool onlyActive = false)
        {
            var mgmtGrain = InsideRuntimeClient.Current.InternalGrainFactory.GetGrain<IManagementGrain>(0);

            return mgmtGrain.GetHosts(onlyActive);
        }

        internal static SiloAddress[] GetSiloAddresses(SiloAddress[] silos)
        {
            if (silos != null && silos.Length > 0)
                return silos;

            return InsideRuntimeClient.Current.Catalog.SiloStatusOracle
                .GetApproximateSiloStatuses(true).Select(s => s.Key).ToArray();
        }

        internal static ISiloControl GetSiloControlReference(SiloAddress silo)
        {
            return InsideRuntimeClient.Current.InternalGrainFactory.GetSystemTarget<ISiloControl>(Constants.SiloControlId, silo);
        }

        #endregion
    }
}
