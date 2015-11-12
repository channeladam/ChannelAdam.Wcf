//-----------------------------------------------------------------------
// <copyright file="FakeExceptionHandlingStrategy.cs">
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
        private const string Prefix = "#### EXCEPTION STRATEGY BEHAVIOUR ###: ";
        private const string NullPhrase = Prefix + "Exception is null";

        /// <summary>
        /// The behaviour to perform when an exception occurs during an Abort.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public void PerformAbortExceptionBehaviour(Exception exception)
        {
            Console.Out.WriteLine(exception == null ? NullPhrase : Prefix + "Abort: " + exception.ToString());
        }

        /// <summary>
        /// The behaviour to perform when a communication exception occurs during a Close.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public void PerformCloseCommunicationExceptionBehaviour(CommunicationException exception)
        {
            Console.Out.WriteLine(exception == null ? NullPhrase : Prefix + "CloseCommunication: " + exception.ToString());
        }

        /// <summary>
        /// The behaviour to perform when a timeout exception occurs during a Close.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public void PerformCloseTimeoutExceptionBehaviour(TimeoutException exception)
        {
            Console.Out.WriteLine(exception == null ? NullPhrase : Prefix + "CloseTimeout: " + exception.ToString());
        }

        /// <summary>
        /// The behaviour to perform when an unexpected exception occurs during a Close.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public void PerformCloseUnexpectedExceptionBehaviour(Exception exception)
        {
            Console.Out.WriteLine(exception == null ? NullPhrase : Prefix + "CloseUnexpected: " + exception.ToString());
        }

        /// <summary>
        /// The behaviour to perform when a communication exception occurs while the service operation is called.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public void PerformCommunicationExceptionBehaviour(CommunicationException exception)
        {
            Console.Out.WriteLine(exception == null ? NullPhrase : Prefix + "Communication: " + exception.ToString());
        }

        /// <summary>
        /// The behaviour to perform when a fault exception occurs while the service operation is called.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public void PerformFaultExceptionBehaviour(FaultException exception)
        {
            Console.Out.WriteLine(exception == null ? NullPhrase : Prefix + "Fault: " + exception.ToString());
        }

        /// <summary>
        /// The behaviour to perform when a timeout exception occurs while the service operation is called.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public void PerformTimeoutExceptionBehaviour(TimeoutException exception)
        {
            Console.Out.WriteLine(exception == null ? NullPhrase : Prefix + "Timeout: " + exception.ToString());
        }

        /// <summary>
        /// The behaviour to perform when an unexpected exception occurs while the service operation is called.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public void PerformUnexpectedExceptionBehaviour(Exception exception)
        {
            Console.Out.WriteLine(exception == null ? NullPhrase : Prefix + "Unexpected: " + exception.ToString());
        }

        #region IRetryEnabledServiceChannelProxyExceptionBehaviour Implementation

        /// <summary>
        /// The behaviour to perform when an exception occurs when a retry policy is in use.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="attemptCount">The attempt count.</param>
        public void PerformRetryPolicyAttemptExceptionBehaviour(Exception exception, int attemptCount)
        {
            Console.Out.WriteLine(exception == null ? NullPhrase : Prefix + $"RetryPolicy: Attempt {attemptCount}");
        }

        #endregion

        #region IDestructorExceptionHandler Implementation

        /// <summary>
        /// The behaviour to perform when an exception occurs during the destructor/finalize.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public void PerformDestructorExceptionBehaviour(Exception exception)
        {
            Console.Out.WriteLine(exception == null ? NullPhrase : Prefix + "Destructor: " + exception.ToString());
        }

        #endregion
    }
}
