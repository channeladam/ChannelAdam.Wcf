//-----------------------------------------------------------------------
// <copyright file="IOperationResult.cs">
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

    /// <summary>
    /// Interface for a service operation result.
    /// </summary>
    public interface IOperationResult
    {
        #region Properties

        /// <summary>
        /// Gets a value indicating whether the service operation call did not throw an exception.
        /// </summary>
        /// <value>
        /// <c>true</c> if the service operation call did not throw an exception; otherwise, <c>false</c>.
        /// </value>
        bool HasNoException { get; }

        /// <summary>
        /// Gets a value indicating whether the service operation threw an exception.
        /// </summary>
        /// <value>
        /// <c>true</c> if the service operation threw an exception; otherwise, <c>false</c>.
        /// </value>
        bool HasException { get; }

        /// <summary>
        /// Gets a value indicating whether the service operation threw a fault exception.
        /// </summary>
        /// <value>
        /// <c>true</c> if the service operation threw fault exception; otherwise, <c>false</c>.
        /// </value>
        bool HasFaultException { get; }

        /// <summary>
        /// Gets or sets the exception that occurred during the service operation call.
        /// </summary>
        /// <value>
        /// The exception.
        /// </value>
        Exception Exception { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a value indicating whether the service operation threw a fault exception of the specified type.
        /// </summary>
        /// <typeparam name="T">The generic type of the object inside the fault exception.</typeparam>
        /// <returns><c>true</c> if the service operation threw fault exception of the specified type; otherwise, <c>false</c>.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "This rule is more about type inference - but we specifically do not want type inference, as it is not the intent of this method.")]
        bool HasFaultExceptionOfType<T>();

        #endregion
    }
}
