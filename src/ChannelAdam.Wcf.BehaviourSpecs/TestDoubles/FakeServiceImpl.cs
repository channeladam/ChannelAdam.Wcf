using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ChannelAdam.Wcf.BehaviourSpecs.TestDoubles
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class FakeServiceImpl : IFakeService
    {
        public void DoOneWayStuff()
        {
            // pretend to do stuff
        }

        public Task DoOneWayStuffAsync()
        {
            // pretend to do stuff
            return Task.FromResult(0);
        }

        public int AddIntegers(int first, int second)
        {
            return first + second;
        }

        public Task<int> AddTwoIntegersAsync(int first, int second)
        {
            return Task.FromResult(first + second);
        }

        public Task<int> AddTwoIntegersWithExceptionsToRetryAsync(int first, int second)
        {
            throw new NotImplementedException();
        }
    }
}
