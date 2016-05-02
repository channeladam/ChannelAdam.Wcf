//-----------------------------------------------------------------------
// <copyright file="ServiceConsumer.cs">
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.ServiceModel;
    using System.Threading.Tasks;

    using ChannelAdam.ServiceModel.Internal;
    using ChannelAdam.TransientFaultHandling;

    /// <summary>
    /// A class that correctly consumes a WCF service and handles the close/abort pattern.
    /// </summary>
    /// <typeparam name="TServiceInterface">The type of the service interface.</typeparam>
    /// <remarks>
    /// Manages the lifetime of a RetryEnabledDisposableServiceChannelProxy.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "IServiceConsumer must be IDisposable and we want to inherit our Disposable functionality. This is intentional.")]
    public class ServiceConsumer<TServiceInterface> : DisposableWithDestructor, IServiceConsumer<TServiceInterface>
    {
        private RetryEnabledDisposableServiceChannelProxy<TServiceInterface> retryEnabledDisposableChannelProxy;
        private TServiceInterface operations;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceConsumer{TServiceInterface}" /> class.
        /// </summary>
        /// <param name="serviceChannelFactoryMethod">The service channel factory method.</param>
        public ServiceConsumer(Func<ICommunicationObject> serviceChannelFactoryMethod)
            : this(serviceChannelFactoryMethod, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceConsumer{TServiceInterface}" /> class.
        /// </summary>
        /// <param name="serviceChannelFactoryMethod">The service channel factory method.</param>
        /// <param name="retryPolicy">The retry policy.</param>
        public ServiceConsumer(Func<ICommunicationObject> serviceChannelFactoryMethod, IRetryPolicyFunction retryPolicy)
            : this(new RetryEnabledDisposableServiceChannelProxy<TServiceInterface>(serviceChannelFactoryMethod, retryPolicy))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceConsumer{TServiceInterface}" /> class.
        /// </summary>
        /// <param name="retryEnabledDisposableServiceChannelProxy">The retry enabled disposable service channel proxy.</param>
        public ServiceConsumer(RetryEnabledDisposableServiceChannelProxy<TServiceInterface> retryEnabledDisposableServiceChannelProxy)
        {
            if (retryEnabledDisposableServiceChannelProxy == null)
            {
                throw new ArgumentNullException("retryEnabledDisposableServiceChannelProxy");
            }

            this.retryEnabledDisposableChannelProxy = retryEnabledDisposableServiceChannelProxy;
            this.operations = (TServiceInterface)retryEnabledDisposableServiceChannelProxy.GetTransparentProxy();
        }

        #endregion

        #region IServiceConsumer Implementation - Public Properties

        /// <summary>
        /// Gets or sets the exception behaviour strategy.
        /// </summary>
        /// <value>
        /// The exception behaviour strategy.
        /// </value>
        public IServiceConsumerExceptionBehaviourStrategy ExceptionBehaviourStrategy
        {
            get
            {
                return this.retryEnabledDisposableChannelProxy.ExceptionBehaviourStrategy;
            }

            set
            {
                this.retryEnabledDisposableChannelProxy.ExceptionBehaviourStrategy = value;

                if (value == null)
                {
                    this.DestructorExceptionBehaviour = null;
                }
                else
                {
                    this.DestructorExceptionBehaviour = value.PerformDestructorExceptionBehaviour;
                }
            }
        }

        /// <summary>
        /// Gets or sets the service channel close trigger strategy.
        /// </summary>
        /// <value>
        /// The service channel close trigger strategy.
        /// </value>
        public IServiceChannelCloseTriggerStrategy ChannelCloseTriggerStrategy
        {
            get
            {
                return this.retryEnabledDisposableChannelProxy.ChannelCloseTriggerStrategy;
            }

            set
            {
                this.retryEnabledDisposableChannelProxy.ChannelCloseTriggerStrategy = value;
            }
        }

        /// <summary>
        /// Gets or sets the default retry policy to use when the service operation is called.
        /// </summary>
        /// <value>
        /// The retry policy.
        /// </value>
        public IRetryPolicyFunction RetryPolicy
        {
            get
            {
                return this.retryEnabledDisposableChannelProxy.RetryPolicy;
            }

            set
            {
                this.retryEnabledDisposableChannelProxy.RetryPolicy = value;
            }
        }

        /// <summary>
        /// Gets the service channel proxy.
        /// </summary>
        /// <value>
        /// The service channel proxy.
        /// </value>
        public TServiceInterface Operations
        {
            get
            {
                return this.operations;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Consumes the specified service operation, using the default retry policy.
        /// </summary>
        /// <param name="serviceOperationExpression">The service operation expression.</param>
        /// <returns>
        /// An <see cref="IOperationResult" />.
        /// </returns>
        public IOperationResult Consume(Expression<Action<TServiceInterface>> serviceOperationExpression)
        {
            var result = new OperationResult();

            try
            {
                var expressionAdapter = ParseServiceOperationExpression(serviceOperationExpression);
                this.ExecuteServiceOperation(expressionAdapter);
            }
            catch (Exception ex)
            {
                result.Exception = ex;
            }

            return result;
        }

        /// <summary>
        /// Consumes the specified service operation, using the default retry policy.
        /// </summary>
        /// <typeparam name="TReturnValue">The type of the return value.</typeparam>
        /// <param name="serviceOperationExpression">The service operation expression.</param>
        /// <returns>
        /// An <see cref="IOperationResult{TReturnValue}" /> with the return type specified by the method within the expression.
        /// </returns>
        /// <remarks>
        /// If the expression returns a Task, then the Task is executed immediately/synchronously before we return to the caller.
        /// </remarks>
        public IOperationResult<TReturnValue> Consume<TReturnValue>(Expression<Func<TServiceInterface, TReturnValue>> serviceOperationExpression)
        {
            var result = new OperationResult<TReturnValue>();

            try
            {
                var expressionAdapter = ParseServiceOperationExpression(serviceOperationExpression);
                result.Value = (TReturnValue)this.ExecuteServiceOperation(expressionAdapter);

                // Execute the task immediately/synchronously so that Consume() has control over the exception handling ;)
                var resultTask = result.Value as Task;
                if (resultTask != null)
                {
#if NET40
                    resultTask.Wait();
#else
                    resultTask.GetAwaiter().GetResult(); // Use GetAwaiter().GetResult() instead of Wait() because Wait() will wrap any exceptions inside an AggregateException
#endif
                }
            }
            catch (Exception ex)
            {
                result.Exception = ex;
            }

            return result;
        }

        /// <summary>
        /// Consumes the specified one-way service operation asynchronously, using the default retry policy.
        /// </summary>
        /// <param name="serviceOperationExpression">The service operation expression.</param>
        /// <returns>
        /// A <see cref="Task{IOperationResult}" />.
        /// </returns>
        public Task<IOperationResult> ConsumeAsync(Expression<Func<TServiceInterface, Task>> serviceOperationExpression)
        {
            return Task.Factory.StartNew(
                () =>
                {
                    IOperationResult result = new OperationResult();

                    try
                    {
                        var expressionAdapter = ParseServiceOperationExpression(serviceOperationExpression);
                        var task = (Task)this.ExecuteServiceOperation(expressionAdapter);
#if NET40
                        task.Wait();
#else
                        task.GetAwaiter().GetResult(); // Use GetAwaiter().GetResult() instead of Wait() because Wait() will wrap any exceptions inside an AggregateException
#endif
                    }
                    catch (AggregateException aex)    // Just in case, but this shouldn't happen ;)
                    {
                        // If there is a Fault or exception thrown on the server,
                        // then there should only be one inner/base exception - being the FaultException
                        // Ditch the AggregateException for the base exception.
                        result.Exception = aex.GetBaseException();
                    }
                    catch (Exception ex)
                    {
                        result.Exception = ex;
                    }

                    return result;
                },
                TaskCreationOptions.AttachedToParent);
        }

        /// <summary>
        /// Consumes the specified service operation asynchronously, using the default retry policy.
        /// </summary>
        /// <typeparam name="TReturnValue">The type of the return value.</typeparam>
        /// <param name="serviceOperationExpression">The service operation expression.</param>
        /// <returns>
        /// A <see cref="Task{IOperationResult{TReturnValue}}" /> with the return type specified by the method within the expression.
        /// </returns>
        public Task<IOperationResult<TReturnValue>> ConsumeAsync<TReturnValue>(Expression<Func<TServiceInterface, Task<TReturnValue>>> serviceOperationExpression)
        {
            return Task.Factory.StartNew(
                () =>
                {
                    IOperationResult<TReturnValue> result = new OperationResult<TReturnValue>();

                    try
                    {
                        var expressionAdapter = ParseServiceOperationExpression(serviceOperationExpression);
                        var task = (Task<TReturnValue>)this.ExecuteServiceOperation(expressionAdapter);
                        result.Value = task.Result;
                    }
                    catch (AggregateException aex)    // Just in case, but this shouldn't happen ;)
                    {
                        // If there is a Fault or exception thrown on the server,
                        // then there should only be one inner/base exception - being the FaultException
                        // Ditch the AggregateException for the base exception.
                        result.Exception = aex.GetBaseException();
                    }
                    catch (Exception ex)
                    {
                        result.Exception = ex;
                    }

                    return result;
                },
                TaskCreationOptions.AttachedToParent);
        }

        /// <summary>
        /// Closes the service channel.
        /// </summary>
        public void Close()
        {
            this.retryEnabledDisposableChannelProxy.Close();
        }

#endregion

#region Dispose Pattern Implementation

        protected override void DisposeUnmanagedResources()
        {
            this.DisposeRetryEnabledDisposableChannelProxy();

            base.DisposeUnmanagedResources();
        }

#endregion

#region Static Private Methods

        private static ServiceOperationExpressionAdapter<TServiceInterface> ParseServiceOperationExpression(Expression<Action<TServiceInterface>> serviceOperationExpression)
        {
            var parser = new ServiceOperationExpressionAdapter<TServiceInterface>();
            parser.Parse(serviceOperationExpression);
            return parser;
        }

        private static ServiceOperationExpressionAdapter<TServiceInterface> ParseServiceOperationExpression<TReturnValue>(Expression<Func<TServiceInterface, TReturnValue>> serviceOperationExpression)
        {
            var parser = new ServiceOperationExpressionAdapter<TServiceInterface>();
            parser.Parse(serviceOperationExpression);
            return parser;
        }

        private static object ExecuteServiceOperation(object service, MethodInfo serviceOperationMethod, IEnumerable<object> serviceOperationArguments)
        {
            try
            {
                return serviceOperationMethod.Invoke(service, serviceOperationArguments.ToArray());
            }
            catch (TargetInvocationException tex)
            {
                throw tex.GetBaseException();   // Throw the root cause exception
            }
        }

#endregion

#region Private Methods

        private void DisposeRetryEnabledDisposableChannelProxy()
        {
            try
            {
                if (this.retryEnabledDisposableChannelProxy != null)
                {
                    this.retryEnabledDisposableChannelProxy.Dispose();
                }
            }
            finally
            {
                this.retryEnabledDisposableChannelProxy = null;
                this.operations = default(TServiceInterface);
            }
        }

        private object ExecuteServiceOperation(ServiceOperationExpressionAdapter<TServiceInterface> expressionAdapter)
        {
            return ExecuteServiceOperation(this.Operations, expressionAdapter.OperationMethodInfo, expressionAdapter.OperationMethodArguments);
        }

#endregion
    }
}