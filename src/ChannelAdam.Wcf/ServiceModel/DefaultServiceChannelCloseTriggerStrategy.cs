//-----------------------------------------------------------------------
// <copyright file="DefaultServiceChannelCloseTriggerStrategy.cs">
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

namespace ChannelAdam.ServiceModel
{
    using System;
    using System.ServiceModel;

    /// <summary>
    /// The default strategy for determining when to close the service channel.
    /// </summary>
    /// <remarks>
    /// This strategy is very simple, effective and leans towards the safe side.
    /// It triggers a close to occur if the exception is not a <see cref="FaultException"/>, regardless of the state of the service channel.
    /// It is highly probable that the channel is no longer usable in that case.
    /// </remarks>
    public class DefaultServiceChannelCloseTriggerStrategy : IServiceChannelCloseTriggerStrategy
    {
        private static readonly DefaultServiceChannelCloseTriggerStrategy SingleInstance = new DefaultServiceChannelCloseTriggerStrategy();

        /// <summary>
        /// Gets a singleton instance of this class.
        /// </summary>
        /// <value>
        /// The singleton of this class.
        /// </value>
        public static DefaultServiceChannelCloseTriggerStrategy Instance
        {
            get { return DefaultServiceChannelCloseTriggerStrategy.SingleInstance; }
        }

        /// <summary>
        /// Determines if the channel should be closed, based on the channel's state and the exception that occurred.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="exception">The exception.</param>
        /// <returns><c>true</c> if the channel should be closed, otherwise <c>false</c>.</returns>
        public bool ShouldCloseChannel(ICommunicationObject channel, Exception exception)
        {
            return exception != null && !(exception is FaultException);
        }
    }
}
