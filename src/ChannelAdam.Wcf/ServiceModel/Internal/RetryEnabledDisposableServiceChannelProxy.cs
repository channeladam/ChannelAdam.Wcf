//-----------------------------------------------------------------------
// <copyright file="RetryEnabledDisposableServiceChannelProxy.cs">
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

namespace ChannelAdam.ServiceModel.Internal
{
    using System;
    using System.Runtime.Remoting.Messaging;
    using System.Security;
    using System.ServiceModel;
    using ChannelAdam.Runtime.Remoting.Proxies;
    using ChannelAdam.TransientFaultHandling;

    /// <summary>
    /// Proxies a <see cref="DisposableServiceChannelProxy" /> to provide retry capability.
    /// </summary>
    /// <typeparam name="TServiceInterface">The type of the service interface.</typeparam>
    [SecurityCritical]
    public class RetryEnabledDisposableServiceChannelProxy<TServiceInterface>
        : DisposableObjectRealProxy, IServiceChannelProxy
    {
        #region Fields

        private Func<ICommunicationObject, DisposableServiceChannelProxy<TServiceInterface>> disposableServiceChannelProxyFactoryMethod;
        private Func<ICommunicationObject> serviceChannelFactoryMethod;

        private IServiceConsumerExceptionBehaviourStrategy exceptionStrategy;
        private IServiceChannelCloseTriggerStrategy channelCloseTriggerStrategy;

        private DisposableServiceChannelProxy<TServiceInterface> disposableChannelProxy;
        private TServiceInterface operations;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryEnabledDisposableServiceChannelProxy{TServiceInterface}" /> class.
        /// </summary>
        /// <param name="serviceChannelFactoryMethod">The service channel factory method.</param>
        public RetryEnabledDisposableServiceChannelProxy(Func<ICommunicationObject> serviceChannelFactoryMethod)
            : this(serviceChannelFactoryMethod, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryEnabledDisposableServiceChannelProxy{TServiceInterface}" /> class.
        /// </summary>
        /// <param name="serviceChannelFactoryMethod">The service channel factory method.</param>
        /// <param name="retryPolicy">The retry policy.</param>
        public RetryEnabledDisposableServiceChannelProxy(Func<ICommunicationObject> serviceChannelFactoryMethod, IRetryPolicyFunction retryPolicy)
            : this(
                (ICommunicationObject serviceChannel) => new DisposableServiceChannelProxy<TServiceInterface>(serviceChannel),
                serviceChannelFactoryMethod,
                retryPolicy)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryEnabledDisposableServiceChannelProxy{TServiceInterface}" /> class.
        /// </summary>
        /// <param name="disposableServiceChannelProxyFactoryMethod">The disposable service channel proxy factory method.</param>
        /// <param name="serviceChannelFactoryMethod">The service channel factory method.</param>
        /// <param name="retryPolicy">The retry policy.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "As designed.")]
        public RetryEnabledDisposableServiceChannelProxy(
            Func<ICommunicationObject, DisposableServiceChannelProxy<TServiceInterface>> disposableServiceChannelProxyFactoryMethod,
            Func<ICommunicationObject> serviceChannelFactoryMethod,
            IRetryPolicyFunction retryPolicy) : base(typeof(TServiceInterface))
        {
            this.disposableServiceChannelProxyFactoryMethod = disposableServiceChannelProxyFactoryMethod;
            this.serviceChannelFactoryMethod = serviceChannelFactoryMethod;
            this.RetryPolicy = retryPolicy;

            this.CreateDisposableChannelProxy();
        }

        #endregion

        #region Public Properties

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
        /// Gets or sets the default retry policy to use when the service operation is called.
        /// </summary>
        /// <value>
        /// The retry policy.
        /// </value>
        public IRetryPolicyFunction RetryPolicy { get; set; }

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
                this.CreateDisposableChannelProxyIfNecessary();
                return (TServiceInterface)this.operations;
            }
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// Gets the actual object that is being proxied.
        /// </summary>
        /// <value>
        /// The proxied object.
        /// </value>
        protected override object ProxiedObject
        {
            get { return this.operations; }
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

        #region Public Methods - Invoke Override

        /// <summary>
        /// Invokes the specified message.
        /// </summary>
        /// <param name="msg">The message.</param>
        /// <returns>
        /// The IMessage.
        /// </returns>
        public override IMessage Invoke(IMessage msg)
        {
            IMessage result = null;
            Exception rootCauseException = null;

            if (this.RetryPolicy != null)
            {
                result = this.InvokeServiceOperationWithRetryPolicy(msg);
            }
            else
            {
                this.CreateDisposableChannelProxyIfNecessary();
                result = this.InvokeServiceOperation(msg, out rootCauseException);
            }

            return result;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Closes the service channel.
        /// </summary>
        public void Close()
        {
            this.DisposeDisposableChannelProxy();
        }

        #endregion

        #region Protected Methods - Disposable Overrides

        /// <summary>
        /// Release unmanaged resources here.
        /// </summary>
        protected override void DisposeUnmanagedResources()
        {
            this.DisposeDisposableChannelProxy();

            base.DisposeUnmanagedResources();
        }

        #endregion

        #region Events

        #endregion

        #region Protected Methods

        protected void CreateDisposableChannelProxyIfNecessary()
        {
            if (this.disposableChannelProxy == null || this.disposableChannelProxy.IsDisposed)
            {
                this.CreateDisposableChannelProxy();
            }
        }

        protected virtual void OnRetryPolicyAttemptException(Exception ex, int attempt)
        {
            if (this.ExceptionBehaviourStrategy != null)
            {
                this.ExceptionBehaviourStrategy.PerformRetryPolicyAttemptExceptionBehaviour(ex, attempt);
            }
        }

        #endregion

        #region Private Static Methods

        private static Exception ExtractRootCauseExceptionFromReturnMessage(IMessage result)
        {
            Exception rootCauseException = null;

            var returnMessage = result as ReturnMessage;
            if (returnMessage != null && returnMessage.Exception != null)
            {
                rootCauseException = returnMessage.Exception.GetBaseException();
            }

            return rootCauseException;
        }

        #endregion

        #region Private Methods

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

        private IMessage InvokeServiceOperationWithRetryPolicy(IMessage msg)
        {
            IMessage result;
            Exception rootCauseException = null;
            int attemptCount = 0;

            try
            {
                result = this.RetryPolicy.Execute(() =>
                {
                    attemptCount++;
                    rootCauseException = null;
                    this.CreateDisposableChannelProxyIfNecessary();

                    var innerResult = this.InvokeServiceOperation(msg, out rootCauseException);

                    if (rootCauseException != null)
                    {
                        OnRetryPolicyAttemptException(rootCauseException, attemptCount);
                        throw rootCauseException;  // make the retry policy activate
                    }

                    return innerResult;
                });
            }
            catch (Exception ex)
            {
                if (rootCauseException == null)
                {
                    rootCauseException = ex.GetBaseException();
                }

                result = new ReturnMessage(rootCauseException, (IMethodCallMessage)msg);
            }

            return result;
        }

        private IMessage InvokeServiceOperation(IMessage msg, out Exception rootCauseException)
        {
            IMessage result = null;
            rootCauseException = null;

            try
            {
                // base.Invoke returns any exception in the ReturnMessage.Exception property.
                // There is the slim possibility that an exception can still happen before that,
                // which is why this is in a try/catch.
                result = base.Invoke(msg);
            }
            catch (Exception ex)
            {
                rootCauseException = ex.GetBaseException();
            }
            finally
            {
                if (result != null)
                {
                    rootCauseException = ExtractRootCauseExceptionFromReturnMessage(result);
                }

                if (rootCauseException != null)
                {
                    result = new ReturnMessage(rootCauseException, (IMethodCallMessage)msg);
                }
            }

            return result;
        }

        #endregion
    }
}
