//-----------------------------------------------------------------------
// <copyright file="FakeServiceClient.cs">
//     Copyright (c) 2014 Adam Craven. All rights reserved.
// </copyright>
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-----------------------------------------------------------------------

namespace ChannelAdam.Wcf.BehaviourSpecs.TestDoubles
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.ServiceModel;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// A service client test double.
    /// </summary>
    public class FakeServiceClient : System.ServiceModel.ClientBase<IFakeService>, IFakeService
    {
        public virtual int AddIntegers(int first, int second)
        {
            // let's just pretend we called a service elsewhere - but don't tell anyone... ;)
            return first + second;
        }

        public virtual Task<int> AddTwoIntegersAsync(int first, int second)
        {
            // let's just pretend we called a service elsewhere - but don't tell anyone... ;)
            return Task.FromResult<int>(first + second);
        }

        public new void Abort()
        {
        }

        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginClose(AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginOpen(AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public void Close(TimeSpan timeout)
        {
        }

        public new void Close()
        {
        }

        ////public event EventHandler Closed;

        ////public event EventHandler Closing;

        public void EndClose(IAsyncResult result)
        {
        }

        public void EndOpen(IAsyncResult result)
        {
        }

        ////public event EventHandler Faulted;

        public void Open(TimeSpan timeout)
        {
        }

        public new void Open()
        {
        }

        ////public event EventHandler Opened;

        ////public event EventHandler Opening;

        public new CommunicationState State
        {
            get { return CommunicationState.Opened; }
        }
    }
}
