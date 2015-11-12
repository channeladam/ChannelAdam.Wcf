//-----------------------------------------------------------------------
// <copyright file="StandardErrorServiceConsumerExceptionBehaviourStrategy.cs">
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
    /// An exception behaviour strategy for a Service Consumer that writes the exception details to standard error.
    /// </summary>
    public class StandardErrorServiceConsumerExceptionBehaviourStrategy : IServiceConsumerExceptionBehaviourStrategy
    {
        private const string NullPhrase = "Exception is null";

        private static readonly StandardErrorServiceConsumerExceptionBehaviourStrategy SingleInstance = new StandardErrorServiceConsumerExceptionBehaviourStrategy();

        /// <summary>
        /// Gets a singleton instance of this class.
        /// </summary>
        /// <value>
        /// The singleton of this class.
        /// </value>
        public static StandardErrorServiceConsumerExceptionBehaviourStrategy Instance
        {
            get { return StandardErrorServiceConsumerExceptionBehaviourStrategy.SingleInstance; }
        }

        /// <summary>
        /// The behaviour to perform when an exception occurs during an Abort.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public void PerformAbortExceptionBehaviour(Exception exception)
        {
            Console.Error.WriteLine(exception == null ? NullPhrase : exception.ToString());
        }

        /// <summary>
        /// The behaviour to perform when a communication exception occurs during a Close.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public void PerformCloseCommunicationExceptionBehaviour(CommunicationException exception)
        {
            Console.Error.WriteLine(exception == null ? NullPhrase : exception.ToString());
        }

        /// <summary>
        /// The behaviour to perform when a timeout exception occurs during a Close.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public void PerformCloseTimeoutExceptionBehaviour(TimeoutException exception)
        {
            Console.Error.WriteLine(exception == null ? NullPhrase : exception.ToString());
        }

        /// <summary>
        /// The behaviour to perform when an unexpected exception occurs during a Close.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public void PerformCloseUnexpectedExceptionBehaviour(Exception exception)
        {
            Console.Error.WriteLine(exception == null ? NullPhrase : exception.ToString());
        }

        /// <summary>
        /// The behaviour to perform when a communication exception occurs while the service operation is called.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public void PerformCommunicationExceptionBehaviour(CommunicationException exception)
        {
            Console.Error.WriteLine(exception == null ? NullPhrase : exception.ToString());
        }

        /// <summary>
        /// The behaviour to perform when a fault exception occurs while the service operation is called.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public void PerformFaultExceptionBehaviour(FaultException exception)
        {
            Console.Error.WriteLine(exception == null ? NullPhrase : exception.ToString());
        }

        /// <summary>
        /// The behaviour to perform when a timeout exception occurs while the service operation is called.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public void PerformTimeoutExceptionBehaviour(TimeoutException exception)
        {
            Console.Error.WriteLine(exception == null ? NullPhrase : exception.ToString());
        }

        /// <summary>
        /// The behaviour to perform when an unexpected exception occurs while the service operation is called.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public void PerformUnexpectedExceptionBehaviour(Exception exception)
        {
            Console.Error.WriteLine(exception == null ? NullPhrase : exception.ToString());
        }

        #region IRetryEnabledServiceChannelProxyExceptionBehaviour Implementation

        /// <summary>
        /// The behaviour to perform when an exception occurs when a retry policy is in use.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="attemptCount">The attempt count. One is the first attempt, not the first retry.</param>
        public void PerformRetryPolicyAttemptExceptionBehaviour(Exception exception, int attemptCount)
        {
            Console.Error.WriteLine($"RetryPolicy Attempt {attemptCount}:" + exception == null ? NullPhrase : exception.ToString());
        }

        #endregion

        #region IDestructorExceptionHandler Implementation

        /// <summary>
        /// The behaviour to perform when an exception occurs during the destructor/finalize.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public void PerformDestructorExceptionBehaviour(Exception exception)
        {
            Console.Error.WriteLine(exception == null ? NullPhrase : exception.ToString());
        }

        #endregion
    }
}
