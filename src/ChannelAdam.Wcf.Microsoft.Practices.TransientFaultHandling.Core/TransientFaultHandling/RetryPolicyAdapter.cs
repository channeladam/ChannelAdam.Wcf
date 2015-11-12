//-----------------------------------------------------------------------
// <copyright file="RetryPolicyAdapter.cs">
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

namespace ChannelAdam.TransientFaultHandling
{
    using Microsoft.Practices.TransientFaultHandling;
    using System;

    /// <summary>
    /// An adapter for the Microsoft Transient Fault Handling Core <see cref="RetryPolicy"/>.
    /// </summary>
    public class RetryPolicyAdapter : IRetryPolicyFunction
    {
        private RetryPolicy retryPolicy;

        #region Constructors

        public RetryPolicyAdapter(RetryPolicy retryPolicy)
        {
            this.retryPolicy = retryPolicy;
        }

        #endregion Constructors

        #region Public Static Methods

        public static RetryPolicyAdapter CreateFrom(RetryPolicy retryPolicy)
        {
            return new RetryPolicyAdapter(retryPolicy);
        }

        #endregion Public Static Methods

        #region Operators

        /// <summary>
        /// Performs an implicit conversion from <see cref="RetryPolicy"/> to <see cref="RetryPolicyAdapter"/>.
        /// </summary>
        /// <param name="retryPolicy">The retry policy.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator RetryPolicyAdapter(RetryPolicy retryPolicy)
        {
            return new RetryPolicyAdapter(retryPolicy);
        }

        #endregion Operators

        public TResult Execute<TResult>(Func<TResult> func)
        {
            if (this.retryPolicy != null)
            {
                return this.retryPolicy.ExecuteAction(func);
            }

            return func.Invoke();
        }
    }
}