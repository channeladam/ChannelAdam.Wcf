//-----------------------------------------------------------------------
// <copyright file="DisposableServiceChannelProxy{TServiceInterface}.cs">
//     Copyright (c) 2014-2015 Adam Craven. All rights reserved.
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

namespace ChannelAdam.ServiceModel.Internal
{
    using System.Security;
    using System.ServiceModel;

    /// <summary>
    /// Proxies a WCF Service Client/Channel and correctly performs the Close/Abort pattern.
    /// </summary>
    /// <typeparam name="TServiceInterface">The type of the service interface.</typeparam>
    /// <remarks>
    /// The object to proxy is the proxy channel returned from ChannelFactory.CreateChannel().
    /// </remarks>
    [SecurityCritical]
    public class DisposableServiceChannelProxy<TServiceInterface> : DisposableServiceChannelProxy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DisposableServiceChannelProxy{TServiceInterface}"/> class.
        /// </summary>
        /// <param name="serviceChannelProxy">The service channel proxy.</param>
        public DisposableServiceChannelProxy(ICommunicationObject serviceChannelProxy)
            : base(typeof(TServiceInterface), serviceChannelProxy)
        {
        }
    }
}
