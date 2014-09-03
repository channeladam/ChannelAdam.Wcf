//-----------------------------------------------------------------------
// <copyright file="OperationResult.cs">
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
    /// Holds the result of calling a service operation.
    /// </summary>
    public class OperationResult : ChannelAdam.ServiceModel.IOperationResult
    {
        #region Properties

        /// <summary>
        /// Gets a value indicating whether the service operation call did not throw an exception.
        /// </summary>
        /// <value>
        /// <c>true</c> if the service operation call did not throw an exception; otherwise, <c>false</c>.
        /// </value>
        public bool HasNoException 
        {
            get
            {
                return this.Exception == null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the service operation threw an exception.
        /// </summary>
        /// <value>
        /// <c>true</c> if the service operation threw an exception; otherwise, <c>false</c>.
        /// </value>
        public bool HasException
        {
            get
            {
                return this.Exception != null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the service operation threw a fault exception.
        /// </summary>
        /// <value>
        /// <c>true</c> if the service operation threw fault exception; otherwise, <c>false</c>.
        /// </value>
        public bool HasFaultException
        {
            get
            {
                return this.Exception != null && this.Exception is FaultException;
            }
        }

        /// <summary>
        /// Gets or sets the exception that occurred during the service operation call.
        /// </summary>
        /// <value>
        /// The exception.
        /// </value>
        public Exception Exception { get; set; }

        #endregion

        #region Public Method

        /// <summary>
        /// Gets a value indicating whether the service operation threw a fault exception of the specified type.
        /// </summary>
        /// <typeparam name="T">The generic type of the object inside the fault exception.</typeparam>
        /// <returns><c>true</c> if the service operation threw fault exception of the specified type; otherwise, <c>false</c>.</returns>
        public bool HasFaultExceptionOfType<T>()
        {
            return this.Exception != null && this.Exception is FaultException<T>;
        }

        #endregion
    }
}
