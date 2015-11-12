//-----------------------------------------------------------------------
// <copyright file="ExtensionMethods.cs">
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

namespace ChannelAdam.ServiceModel
{
    using ChannelAdam.TransientFaultHandling;

    using Microsoft.Practices.TransientFaultHandling;

    public static class ExtensionMethods
    {
        /// <summary>
        /// Converts the Microsoft <see cref="RetryPolicy"/> to a <see cref="RetryPolicyAdapter"/>.
        /// </summary>
        /// <param name="retryPolicy">The retry policy.</param>
        /// <returns>A <see cref="IRetryPolicyFunction"/>.</returns>
        public static IRetryPolicyFunction ForServiceConsumer(this RetryPolicy retryPolicy)
        {
            return new RetryPolicyAdapter(retryPolicy);
        }

        /// <summary>
        /// Sets the retry policy.
        /// </summary>
        /// <typeparam name="TServiceInterface">The type of the service interface.</typeparam>
        /// <param name="serviceConsumer">The service consumer.</param>
        /// <param name="retryPolicy">The retry policy.</param>
        public static void SetRetryPolicy<TServiceInterface>(this IServiceConsumer<TServiceInterface> serviceConsumer, RetryPolicy retryPolicy)
        {
            serviceConsumer.RetryPolicy = new RetryPolicyAdapter(retryPolicy);
        }
    }
}
