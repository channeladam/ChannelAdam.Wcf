//-----------------------------------------------------------------------
// <copyright file="IServiceChannelProxy.cs">
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
    using System;

    using ChannelAdam.Events;

    public interface IServiceChannelProxy
    {
        /// <summary>
        /// Gets or sets the exception behaviour strategy.
        /// </summary>
        /// <value>
        /// The exception behaviour strategy.
        /// </value>
        IServiceConsumerExceptionBehaviourStrategy ExceptionBehaviourStrategy { get; set; }

        /// <summary>
        /// Gets or sets the service channel close trigger strategy.
        /// </summary>
        /// <value>
        /// The service channel close trigger strategy.
        /// </value>
        IServiceChannelCloseTriggerStrategy ChannelCloseTriggerStrategy { get; set; }
    }
}