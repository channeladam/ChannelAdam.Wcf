//-----------------------------------------------------------------------
// <copyright file="OperationResult{TResult}.cs">
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
    /// Holds the result of calling a service operation.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public class OperationResult<TResult> : OperationResult, IOperationResult<TResult>
    {
        /// <summary>
        /// Gets or sets the return value from the service operation call.
        /// </summary>
        /// <value>
        /// The return value.
        /// </value>
        public TResult Value { get; set; }
    }
}
