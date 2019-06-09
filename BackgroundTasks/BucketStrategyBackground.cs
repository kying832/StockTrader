using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace BackgroundTaskLibrary
{
    public sealed class BucketStrategyBackground : IBackgroundTask
    {
        BackgroundTaskDeferral _deferral;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();

            // Build the strategy to run by querying the database


            // Run the appropriate strategy given its type


            RunBucketStrategy();

            _deferral.Complete();
        }

        private void RunBucketStrategy()
        {

        }
    }
}
