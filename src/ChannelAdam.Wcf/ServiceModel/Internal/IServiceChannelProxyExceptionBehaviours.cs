//-----------------------------------------------------------------------
// <copyright file="IServiceChannelProxyExceptionBehaviours.cs">
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

namespace ChannelAdam.ServiceModel.Internal
{
    using System;
    using System.ServiceModel;

    /// <summary>
    /// Interface for a class that performs behaviours when exceptions occur from a service channel proxy.
    /// </summary>
    public interface IServiceChannelProxyExceptionBehaviours
    {
        /// <summary>
        /// The behaviour to perform when an exception occurs during an Abort.
        /// </summary>
        /// <param name="exception">The exception.</param>
        void PerformAbortExceptionBehaviour(Exception exception);

        /// <summary>
        /// The behaviour to perform when a communication exception occurs during a Close.
        /// </summary>
        /// <param name="exception">The exception.</param>
        void PerformCloseCommunicationExceptionBehaviour(CommunicationException exception);

        /// <summary>
        /// The behaviour to perform when a timeout exception occurs during a Close.
        /// </summary>
        /// <param name="exception">The exception.</param>
        void PerformCloseTimeoutExceptionBehaviour(TimeoutException exception);

        /// <summary>
        /// The behaviour to perform when an unexpected exception occurs during a Close.
        /// </summary>
        /// <param name="exception">The exception.</param>
        void PerformCloseUnexpectedExceptionBehaviour(Exception exception);

        /// <summary>
        /// The behaviour to perform when a communication exception occurs while the service operation is called.
        /// </summary>
        /// <param name="exception">The exception.</param>
        void PerformCommunicationExceptionBehaviour(CommunicationException exception);

        /// <summary>
        /// The behaviour to perform when a fault exception occurs while the service operation is called.
        /// </summary>
        /// <param name="exception">The exception.</param>
        void PerformFaultExceptionBehaviour(FaultException exception);

        /// <summary>
        /// The behaviour to perform when a timeout exception occurs while the service operation is called.
        /// </summary>
        /// <param name="exception">The exception.</param>
        void PerformTimeoutExceptionBehaviour(TimeoutException exception);

        /// <summary>
        /// The behaviour to perform when an unexpected exception occurs while the service operation is called.
        /// </summary>
        /// <param name="exception">The exception.</param>
        void PerformUnexpectedExceptionBehaviour(Exception exception);
    }
}
