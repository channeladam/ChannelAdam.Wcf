//-----------------------------------------------------------------------
// <copyright file="SoapFaultWebServiceTransientErrorDetectionStrategy.cs">
//     Copyright (c) 2014-2016 Adam Craven. All rights reserved.
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

    using Microsoft.Practices.TransientFaultHandling;

    /// <summary>
    /// A transient error detection strategy for a SOAP-based web service - everything but a FaultException is a transient failure.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If a FaultException occurs, it means that the service on the server threw an exception - as opposed to:
    ///   - the service or server not being available, timing out, crashing, etc.
    ///   - an exception that occurred somewhere on the client side.
    /// </para>
    /// <para>
    /// This may not be very useful to the majority of users because they probably want to have a custom strategy checking for
    /// specific types of FaultException{Xyz} rather than any FaultException in general.
    /// </para>
    /// </remarks>
    public class SoapFaultWebServiceTransientErrorDetectionStrategy : ITransientErrorDetectionStrategy
    {
        /// <summary>
        /// Determines whether the specified exception is a transient failure that could be retried.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <returns>
        ///   <c>true</c> if the exception is a transient failure that could be retried; otherwise <c>false</c>.
        /// </returns>
        public bool IsTransient(Exception ex)
        {
            if (ex is FaultException)
            {
                return false;
            }

            return true;
        }
    }
}
