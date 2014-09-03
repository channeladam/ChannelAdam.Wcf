//-----------------------------------------------------------------------
// <copyright file="IServiceConsumer.cs">
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
    using System.Linq.Expressions;
    using System.ServiceModel;

    using ChannelAdam.ServiceModel.Internal;
    
    using Microsoft.Practices.TransientFaultHandling;

    /// <summary>
    /// Interface for a <see cref="IDisposable"/> WCF Service Consumer.
    /// </summary>
    /// <typeparam name="TServiceInterface">The type of the service interface.</typeparam>
    public interface IServiceConsumer<TServiceInterface> : IDisposable 
    {
        #region Properties

        /// <summary>
        /// Gets or sets the exception behaviour strategy.
        /// </summary>
        /// <value>
        /// The exception behaviour strategy.
        /// </value>
        IServiceConsumerExceptionBehaviourStrategy ExceptionBehaviourStrategy { get; set; }

        /// <summary>
        /// Gets or sets the service channel close trigger strategy.
        /// </summary>
        /// <value>
        /// The service channel close trigger strategy.
        /// </value>
        IServiceChannelCloseTriggerStrategy ChannelCloseTriggerStrategy { get; set; }

        /// <summary>
        /// Gets or sets the default retry policy to use when using the <c>Call</c> method to call a service operation.
        /// </summary>
        /// <value>
        /// The retry policy.
        /// </value>
        /// <remarks>
        /// Warning: the retry policy is only used with the <c>Call</c> method. 
        /// If you use the <c>Operations</c> property to call your service operations, the retry policy is not applied.</remarks>
        RetryPolicy RetryPolicy { get; set; } 

        /// <summary>
        /// Gets the service channel proxy.
        /// </summary>
        /// <value>
        /// The service channel proxy.
        /// </value>
        /// <remarks>
        /// Warning: the retry policy is NOT used if you call a service operation directly through this property!
        /// The retry policy is only used with the <c>Call</c> method. 
        /// </remarks>
        TServiceInterface Operations { get; }

        #endregion 

        #region Methods

        /// <summary>
        /// Consumes the specified service operation, using the default retry policy.
        /// </summary>
        /// <param name="serviceOperationExpression">The service operation expression.</param>
        /// <returns>
        /// An <see cref="OperationResult" />.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This rule was created before Expressions.")]
        IOperationResult Consume(Expression<Action<TServiceInterface>> serviceOperationExpression);

        /// <summary>
        /// Consumes the specified service operation, using the specified retry policy.
        /// </summary>
        /// <param name="serviceOperationExpression">The service operation expression.</param>
        /// <param name="retryPolicy">The retry policy to use.</param>
        /// <returns>
        /// An <see cref="OperationResult" />.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This rule was created before Expressions.")]
        IOperationResult Consume(Expression<Action<TServiceInterface>> serviceOperationExpression, RetryPolicy retryPolicy);

        /// <summary>
        /// Consumes the specified service operation, using the default retry policy.
        /// </summary>
        /// <typeparam name="TReturnValue">The type of the return value.</typeparam>
        /// <param name="serviceOperationExpression">The service operation expression.</param>
        /// <returns>
        /// An <see cref="OperationResult{TReturnValue}" /> with the return type specified by the method within the expression.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This rule was created before Expressions.")]
        IOperationResult<TReturnValue> Consume<TReturnValue>(Expression<Func<TServiceInterface, TReturnValue>> serviceOperationExpression);

        /// <summary>
        /// Consumes the specified service operation, using the specified retry policy.
        /// </summary>
        /// <typeparam name="TReturnValue">The type of the return value.</typeparam>
        /// <param name="serviceOperationExpression">The service operation expression.</param>
        /// <param name="retryPolicy">The retry policy to use.</param>
        /// <returns>
        /// An <see cref="OperationResult{TReturnValue}" /> with the return type specified by the method within the expression.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This rule was created before Expressions.")]
        IOperationResult<TReturnValue> Consume<TReturnValue>(Expression<Func<TServiceInterface, TReturnValue>> serviceOperationExpression, RetryPolicy retryPolicy);
        
        /// <summary>
        /// Closes the service channel.
        /// </summary>
        void Close();

        #endregion
    }
}