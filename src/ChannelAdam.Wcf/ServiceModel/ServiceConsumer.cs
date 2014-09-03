//-----------------------------------------------------------------------
// <copyright file="ServiceConsumer.cs">
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.ServiceModel;

    using ChannelAdam.ServiceModel.Internal;

    using Microsoft.Practices.TransientFaultHandling;
    
    /// <summary>
    /// A class that correctly consumes a WCF service and handles the close/abort pattern.
    /// </summary>
    /// <typeparam name="TServiceInterface">The type of the service interface.</typeparam>
    /// <remarks>
    /// Manages the lifecycle of a DisposableServiceChannelProxy.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "IServiceConsumer must be be IDisposable and we want to inherit our Disposable functionality. This is intentional.")]
    public class ServiceConsumer<TServiceInterface> : DisposableWithDestructor, IServiceConsumer<TServiceInterface>
    {
        private Func<ICommunicationObject, DisposableServiceChannelProxy> disposableServiceChannelProxyFactoryMethod;
        private Func<ICommunicationObject> serviceChannelFactoryMethod;
        private IServiceConsumerExceptionBehaviourStrategy exceptionStrategy;
        private IServiceChannelCloseTriggerStrategy channelCloseTriggerStrategy;

        private DisposableServiceChannelProxy disposableChannelProxy;
        private TServiceInterface operations;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceConsumer{TServiceInterface}"/> class.
        /// </summary>
        /// <param name="serviceChannelFactoryMethod">The service channel factory method.</param>
        public ServiceConsumer(Func<ICommunicationObject> serviceChannelFactoryMethod) 
            : this(
                (ICommunicationObject serviceChannel) => new DisposableServiceChannelProxy<TServiceInterface>(serviceChannel), 
                serviceChannelFactoryMethod)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceConsumer{TServiceInterface}" /> class.
        /// </summary>
        /// <param name="disposableServiceChannelProxyFactoryMethod">The disposable service channel proxy factory method.</param>
        /// <param name="serviceChannelFactoryMethod">The service channel factory method.</param>
        public ServiceConsumer(
            Func<ICommunicationObject, DisposableServiceChannelProxy> disposableServiceChannelProxyFactoryMethod, 
            Func<ICommunicationObject> serviceChannelFactoryMethod)
        {
            this.disposableServiceChannelProxyFactoryMethod = disposableServiceChannelProxyFactoryMethod;
            this.serviceChannelFactoryMethod = serviceChannelFactoryMethod;

            this.CreateDisposableChannelProxy();
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
                return this.exceptionStrategy ?? NullServiceConsumerExceptionBehaviourStrategy.Instance;
            }

            set
            {
                this.exceptionStrategy = value;

                if (value == null)
                {
                    this.DestructorExceptionBehaviour = null;
                }
                else
                {
                    this.DestructorExceptionBehaviour = value.PerformDestructorExceptionBehaviour;
                }

                this.InitialiseExceptionStrategyInDisposableChannelProxy();
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
                return this.channelCloseTriggerStrategy ?? DefaultServiceChannelCloseTriggerStrategy.Instance;
            }

            set
            {
                this.channelCloseTriggerStrategy = value;

                this.InitialiseChannelCloseTriggerStrategyInDisposableChannelProxy();
            }
        }

        /// <summary>
        /// Gets or sets the default retry policy to use when using the <c>Call</c> method to call a service operation.
        /// </summary>
        /// <value>
        /// The retry policy.
        /// </value>
        /// <remarks>
        /// Warning: the retry policy is only used with the <c>Call</c> method. 
        /// If you use the <c>Operations</c> property to call your service operations, the retry policy is not applied.</remarks>
        public RetryPolicy RetryPolicy { get; set; }
 
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
        public TServiceInterface Operations
        {
            get
            {
                this.CreateDisposableChannelProxyIfNecessary();
                return this.operations;
            }
        }

        #endregion

        #region Private Properties

        private DisposableServiceChannelProxy DisposableChannelProxy
        {
            get
            {
                this.CreateDisposableChannelProxyIfNecessary();
                return this.disposableChannelProxy;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Consumes the specified service operation, using the default retry policy.
        /// </summary>
        /// <param name="serviceOperationExpression">The service operation expression.</param>
        /// <returns>
        /// An <see cref="OperationResult" />.
        /// </returns>
        public IOperationResult Consume(Expression<Action<TServiceInterface>> serviceOperationExpression)
        {
            return this.Consume(serviceOperationExpression, this.RetryPolicy);
        }

        /// <summary>
        /// Consumes the specified service operation, using the specified retry policy.
        /// </summary>
        /// <param name="serviceOperationExpression">The service operation expression.</param>
        /// <param name="retryPolicy">The retry policy to use.</param>
        /// <returns>
        /// An <see cref="OperationResult" />.
        /// </returns>
        public IOperationResult Consume(Expression<Action<TServiceInterface>> serviceOperationExpression, RetryPolicy retryPolicy)
        {            
            var result = new OperationResult();

            try
            {
                var expressionAdapter = ParseServiceOperationExpression(serviceOperationExpression);

                this.ExecuteServiceOperation(expressionAdapter, retryPolicy);
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
        /// An <see cref="OperationResult{TReturnValue}" /> with the return type specified by the method within the expression.
        /// </returns>
        public IOperationResult<TReturnValue> Consume<TReturnValue>(Expression<Func<TServiceInterface, TReturnValue>> serviceOperationExpression)
        {
            return this.Consume(serviceOperationExpression, this.RetryPolicy);
        }

        /// <summary>
        /// Consumes the specified service operation, using the specified retry policy.
        /// </summary>
        /// <typeparam name="TReturnValue">The type of the return value.</typeparam>
        /// <param name="serviceOperationExpression">The service operation expression.</param>
        /// <param name="retryPolicy">The retry policy to use.</param>
        /// <returns>
        /// An <see cref="OperationResult{TReturnValue}" /> with the return type specified by the method within the expression.
        /// </returns>
        public IOperationResult<TReturnValue> Consume<TReturnValue>(Expression<Func<TServiceInterface, TReturnValue>> serviceOperationExpression, RetryPolicy retryPolicy)
        {
            var result = new OperationResult<TReturnValue>();

            try
            {
                var expressionAdapter = ParseServiceOperationExpression(serviceOperationExpression);

                result.Value = (TReturnValue)this.ExecuteServiceOperation(expressionAdapter, retryPolicy);
            }
            catch (Exception ex)
            {
                result.Exception = ex;
            }

            return result;
        }

        /// <summary>
        /// Closes the service channel.
        /// </summary>
        public void Close()
        {
            this.DisposeDisposableChannelProxy();
        }

        #endregion

        #region Dispose Pattern Implementation

        protected override void DisposeUnmanagedResources()
        {
            this.DisposeDisposableChannelProxy();

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

        private void CreateDisposableChannelProxyIfNecessary()
        {
            if (this.disposableChannelProxy == null || this.disposableChannelProxy.IsDisposed)
            {
                this.CreateDisposableChannelProxy();
            }
        }

        private void CreateDisposableChannelProxy()
        {
            this.disposableChannelProxy = this.disposableServiceChannelProxyFactoryMethod.Invoke(this.serviceChannelFactoryMethod.Invoke());
            this.operations = (TServiceInterface)this.disposableChannelProxy.GetTransparentProxy();

            // If these were not WeakEvents then they would cause the garbage collector to not collect this ServiceConsumer ;)
            this.disposableChannelProxy.ClosingChannelEvent.Subscribe(this.DisposableChannelProxy_ChannelNowUnusable);
            this.disposableChannelProxy.AbortingChannelEvent.Subscribe(this.DisposableChannelProxy_ChannelNowUnusable);
            this.disposableChannelProxy.DisposedChannelEvent.Subscribe(this.DisposableChannelProxy_ChannelNowUnusable);

            this.InitialiseExceptionStrategyInDisposableChannelProxy();
            this.InitialiseChannelCloseTriggerStrategyInDisposableChannelProxy();
        }

        private void DisposableChannelProxy_ChannelNowUnusable(object sender, EventArgs e)
        {
            this.DisposeDisposableChannelProxy();
        }

        private void DisposeDisposableChannelProxy()
        {
            try
            {
                if (this.disposableChannelProxy != null)
                {
                    this.disposableChannelProxy.ClosingChannelEvent.Unsubscribe(this.DisposableChannelProxy_ChannelNowUnusable);
                    this.disposableChannelProxy.AbortingChannelEvent.Unsubscribe(this.DisposableChannelProxy_ChannelNowUnusable);
                    this.disposableChannelProxy.DisposedChannelEvent.Unsubscribe(this.DisposableChannelProxy_ChannelNowUnusable);

                    this.disposableChannelProxy.Dispose();
                }
            }
            finally
            {
                this.disposableChannelProxy = null;
                this.operations = default(TServiceInterface);
            }
        }

        private void InitialiseExceptionStrategyInDisposableChannelProxy()
        {
            if (this.disposableChannelProxy != null)
            {
                this.disposableChannelProxy.ExceptionBehaviourStrategy = this.ExceptionBehaviourStrategy;
            }
        }

        private void InitialiseChannelCloseTriggerStrategyInDisposableChannelProxy()
        {
            if (this.disposableChannelProxy != null)
            {
                this.disposableChannelProxy.ChannelCloseTriggerStrategy = this.ChannelCloseTriggerStrategy;
            }
        }

        private object ExecuteServiceOperation(ServiceOperationExpressionAdapter<TServiceInterface> expressionAdapter, RetryPolicy retryPolicy)
        {
            if (retryPolicy == null)
            {
                return ExecuteServiceOperation(this.Operations, expressionAdapter.OperationMethodInfo, expressionAdapter.OperationMethodArguments);
            }
            else
            {
                return retryPolicy.ExecuteAction(() => ExecuteServiceOperation(this.Operations, expressionAdapter.OperationMethodInfo, expressionAdapter.OperationMethodArguments));
            }
        }

        #endregion
    }
}