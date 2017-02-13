using Orleans.Runtime;
using Orleans.Runtime.Configuration;
using Orleans.TestingHost;

namespace TestExtensions
{
    public class DefaultClusterFixture : BaseTestClusterFixture
    {
        protected override TestCluster CreateTestCluster()
        {
            var options = new TestClusterOptions();
            options.ClusterConfiguration.AddMemoryStorageProvider("Default");
            options.ClusterConfiguration.AddMemoryStorageProvider("MemoryStore");

            //Required for IndexingTests
            options.ClusterConfiguration.AddMemoryStorageProvider("PubSubStore");
            options.ClusterConfiguration.AddSimpleMessageStreamProvider("IndexingStreamProvider");
            options.ClientConfiguration.AddSimpleMessageStreamProvider("IndexingStreamProvider");

            options.ClusterConfiguration.AddMemoryStorageProvider("GrainStore");
            options.ClusterConfiguration.AddMemoryStorageProvider("IndexingStorageProvider");
            options.ClusterConfiguration.AddMemoryStorageProvider("IndexingWorkflowQueueStorageProvider");
            //options.ClusterConfiguration.AddAzureTableStorageProvider("GrainStore", "DefaultEndpointsProtocol=https;AccountName=ACCOUNTNAME;AccountKey=ACCOUNTKEY");
            //options.ClusterConfiguration.AddAzureBlobStorageProvider("IndexingStorageProvider", "DefaultEndpointsProtocol=https;AccountName=ACCOUNTNAME;AccountKey=ACCOUNTKEY");
            //options.ClusterConfiguration.AddAzureBlobStorageProvider("IndexingWorkflowQueueStorageProvider", "DefaultEndpointsProtocol=https;AccountName=ACCOUNTNAME;AccountKey=ACCOUNTKEY");

            options.ClientConfiguration.TraceToConsole = true;
            options.ClientConfiguration.TraceFileName = "C:\\log.txt";
            options.ClientConfiguration.TraceFilePattern = @"Client_{0}-{1}.log";

            //options.ClusterConfiguration.Defaults.DefaultTraceLevel = Severity.Verbose;
            //options.ClientConfiguration.DefaultTraceLevel = Severity.Verbose;
            //options.ClusterConfiguration.GetOrCreateNodeConfigurationForSilo("Primary").DefaultTraceLevel = Severity.Verbose;
            return new TestCluster(options);
        }
    }
}
