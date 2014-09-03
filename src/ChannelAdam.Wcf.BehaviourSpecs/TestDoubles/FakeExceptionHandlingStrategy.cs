//-----------------------------------------------------------------------
// <copyright file="FakeExceptionHandlingStrategy.cs">
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
    using System.ServiceModel;

    using ChannelAdam.ServiceModel;
    
    /// <summary>
    /// An exception handling strategy test double.
    /// </summary>
    public class FakeExceptionHandlingStrategy : IServiceConsumerExceptionBehaviourStrategy
    {
        /// <summary>
        /// The behaviour to perform when an exception occurs during an Abort.
        /// </summary>
        public void PerformAbortExceptionBehaviour(Exception exception)
        {

        }

        /// <summary>
        /// The behaviour to perform when a communication exception occurs during a Close.
        /// </summary>
        public void PerformCloseCommunicationExceptionBehaviour(CommunicationException exception)
        {

        }

        /// <summary>
        /// The behaviour to perform when a timeout exception occurs during a Close.
        /// </summary>
        public void PerformCloseTimeoutExceptionBehaviour(TimeoutException exception)
        {

        }

        /// <summary>
        /// The behaviour to perform when an unexpected exception occurs during a Close.
        /// </summary>
        public void PerformCloseUnexpectedExceptionBehaviour(Exception exception)
        {

        }

        /// <summary>
        /// The behaviour to perform when a communication exception occurs while the service operation is called.
        /// </summary>
        public void PerformCommunicationExceptionBehaviour(CommunicationException exception)
        {

        }

        /// <summary>
        /// The behaviour to perform when a fault exception occurs while the service operation is called.
        /// </summary>
        public void PerformFaultExceptionBehaviour(FaultException exception)
        {

        }

        /// <summary>
        /// The behaviour to perform when a timeout exception occurs while the service operation is called.
        /// </summary>
        public void PerformTimeoutExceptionBehaviour(TimeoutException exception)
        {

        }

        /// <summary>
        /// The behaviour to perform when an unexpected exception occurs while the service operation is called.
        /// </summary>
        public void PerformUnexpectedExceptionBehaviour(Exception exception)
        {

        }

        #region IDestructorExceptionHandler Implementation
        
        /// <summary>
        /// The behaviour to perform when an exception occurs during the destructor/finalize.
        /// </summary>
        public void PerformDestructorExceptionBehaviour(Exception exception)
        {

        }

        #endregion
    }
}
