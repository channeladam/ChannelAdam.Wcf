//-----------------------------------------------------------------------
// <copyright file="IRetryEnabledServiceChannelProxyExceptionBehaviours.cs">
//     Copyright (c) 2015 Adam Craven. All rights reserved.
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

    /// <summary>
    /// Interface for a class that performs behaviours when exceptions occur from a retry enabled service channel proxy.
    /// </summary>
    public interface IRetryEnabledServiceChannelProxyExceptionBehaviours
    {
        /// <summary>
        /// The behaviour to perform when an exception occurs when a retry policy is in use.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="attemptCount">The attempt count. One is the first attempt, not the first retry.</param>
        void PerformRetryPolicyAttemptExceptionBehaviour(Exception exception, int attemptCount);
    }
}
