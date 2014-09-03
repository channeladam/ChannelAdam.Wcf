//-----------------------------------------------------------------------
// <copyright file="DisposableServiceChannelProxy.cs">
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

namespace ChannelAdam.ServiceModel.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Proxies;
    using System.Security;
    using System.Security.Permissions;
    using System.ServiceModel;
    using System.Threading;

    using ChannelAdam.Events;
    using ChannelAdam.Runtime.Remoting.Proxies;

    using Microsoft.Practices.TransientFaultHandling;
    
    /// <summary>
    /// Proxies a WCF Service Client/Channel and correctly performs the Close/Abort pattern.
    /// </summary>
    /// <remarks>
    /// The object to proxy is the proxy channel returned from ChannelFactory.CreateChannel().
    /// </remarks>
    [SecurityCritical]
    public class DisposableServiceChannelProxy : DisposableObjectRealProxy
    {
        #region Fields

        private ICommunicationObject channel;
        private bool isClosing;

        private IServiceConsumerExceptionBehaviourStrategy exceptionStrategy;
        private IServiceChannelCloseTriggerStrategy channelCloseTriggerStrategy;

        private WeakEvent<EventArgs> closingChannelEvent = new WeakEvent<EventArgs>();
        private WeakEvent<EventArgs> closedChannelEvent = new WeakEvent<EventArgs>();
        private WeakEvent<EventArgs> abortingChannelEvent = new WeakEvent<EventArgs>();
        private WeakEvent<EventArgs> abortedChannelEvent = new WeakEvent<EventArgs>();
        private WeakEvent<EventArgs> disposedChannelEvent = new WeakEvent<EventArgs>();

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the DisposableServiceChannelProxy class.
        /// </summary>
        /// <param name="serviceInterfaceType">Type of the service interface.</param>
        /// <param name="serviceChannelProxy">The service channel proxy.</param>
        public DisposableServiceChannelProxy(Type serviceInterfaceType, ICommunicationObject serviceChannelProxy) : base(serviceInterfaceType)
        {
            this.InitialiseChannel(serviceChannelProxy);
        }
        
        #endregion

        #region Public Properties - Events

        /// <summary>
        /// Gets the event that occurs when a channel is about to have the Close() method called on it.
        /// </summary>
        /// <value>
        /// The closing channel event.
        /// </value>
        public IWeakEvent<EventArgs> ClosingChannelEvent
        {
            get { return this.closingChannelEvent; }
        }

        /// <summary>
        /// Gets the event that occurs when a channel has been closed successfully. Will not occur if the close fails.
        /// </summary>
        /// <value>
        /// The closed channel event.
        /// </value>
        public IWeakEvent<EventArgs> ClosedChannelEvent
        {
            get { return this.closedChannelEvent; }
        }

        /// <summary>
        /// Gets the event that occurs when a channel is about to have the Abort() method called on it.
        /// </summary>
        /// <value>
        /// The aborting channel event.
        /// </value>
        public IWeakEvent<EventArgs> AbortingChannelEvent
        {
            get { return this.abortingChannelEvent; }
        }

        /// <summary>
        /// Gets the event that occurs when a channel has been aborted successfully. Will not occur if the abort fails.
        /// </summary>
        /// <value>
        /// The aborted channel event.
        /// </value>
        public IWeakEvent<EventArgs> AbortedChannelEvent
        {
            get { return this.abortedChannelEvent; }
        }

        /// <summary>
        /// Gets the event that occurs after a channel has been either closed or aborted, successfully or not. The channel is now disposed.
        /// </summary>
        /// <value>
        /// The disposed channel event.
        /// </value>
        public IWeakEvent<EventArgs> DisposedChannelEvent
        {
            get { return this.disposedChannelEvent; }
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
            get { return this.channel; }
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

            try
            {
                result = this.InvokeServiceOperation(msg, out rootCauseException);
            }
            catch (Exception ex)
            {
                rootCauseException = ex.GetBaseException();
            }
            finally 
            {
                // A 'finally' block is never interrupted by a ThreadAbortException
                if (rootCauseException != null)
                {
                    this.TryToPerformExceptionBehavioursRelatedToInvokingServiceOperation(rootCauseException);

                    if (this.ChannelCloseTriggerStrategy.ShouldCloseChannel(this.channel, rootCauseException))
                    {
                        this.TryToCloseOrAbortServiceChannelAndPerformExceptionBehaviours();
                    }
   
                    result = new ReturnMessage(rootCauseException, (IMethodCallMessage)msg);
                }
            }

            return result;
        }

        #endregion

        #region Protected Static Methods

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Intentionally a FaultException.")]
        protected static Type FaultExceptionType(FaultException fe)
        {
            if (fe == null)
            {
                throw new ArgumentNullException("fe");
            }

            Type faultType = fe.GetType();

            if (faultType.IsGenericType)
            {
                return faultType.GetGenericArguments()[0];
            }
            else
            {
                return faultType;
            }
        }

        #endregion

        #region Protected Methods - Disposable Overrides

        /// <summary>
        /// Release unmanaged resources here.
        /// </summary>
        protected override void DisposeUnmanagedResources()
        {
            this.CloseOrAbortServiceChannelAndPerformExceptionBehaviours(this.channel);

            base.DisposeUnmanagedResources();
        }

        #endregion

        #region Events

        protected virtual void OnClosingChannel()
        {
            this.closingChannelEvent.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnClosedChannel()
        {
            this.closedChannelEvent.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnAbortingChannel()
        {
            this.abortingChannelEvent.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnAbortedChannel()
        {
            this.abortedChannelEvent.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnDisposedChannel()
        {
            this.disposedChannelEvent.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Protected Virtual Methods

        protected virtual void OnUnexpectedException(Exception ex)
        {
            this.ExceptionBehaviourStrategy.PerformUnexpectedExceptionBehaviour(ex);
        }

        protected virtual void OnCloseUnexpectedException(Exception ex)
        {
            this.ExceptionBehaviourStrategy.PerformCloseUnexpectedExceptionBehaviour(ex);
        }

        protected virtual void OnAbortException(Exception ex)
        {
            this.ExceptionBehaviourStrategy.PerformAbortExceptionBehaviour(ex);
        }

        protected virtual void OnTimeoutException(TimeoutException ex)
        {
            this.ExceptionBehaviourStrategy.PerformTimeoutExceptionBehaviour(ex);
        }

        protected virtual void OnCloseTimeoutException(TimeoutException ex)
        {
            this.ExceptionBehaviourStrategy.PerformCloseTimeoutExceptionBehaviour(ex);
        }

        protected virtual void OnCommunicationException(CommunicationException ex)
        {
            this.ExceptionBehaviourStrategy.PerformCommunicationExceptionBehaviour(ex);
        }

        protected virtual void OnCloseCommunicationException(CommunicationException ex)
        {
            this.ExceptionBehaviourStrategy.PerformCloseCommunicationExceptionBehaviour(ex);
        }

        protected virtual void OnFaultException(FaultException ex)
        {
            this.ExceptionBehaviourStrategy.PerformFaultExceptionBehaviour(ex);
        }

        #endregion

        #region Private Static Methods

        private static Exception ExtractRootCauseExceptionFromReturnMessage(IMessage result)
        {
            Exception rootCauseException = null;

            ReturnMessage returnMessage = result as ReturnMessage;
            if (returnMessage != null && returnMessage.Exception != null)
            {
                rootCauseException = returnMessage.Exception.GetBaseException();
            }

            return rootCauseException;
        }

        #endregion

        #region Private Initialisation / Uninitialsation

        private void InitialiseChannel(ICommunicationObject serviceChannelProxy)
        {
            this.channel = serviceChannelProxy;
            this.channel.Faulted += this.Channel_Faulted;
        }

        private void Channel_Faulted(object sender, EventArgs e)
        {
            // If the channel transitions to the faulted state at any point in time, take action to clean it up - this is non-negotiable!
            this.TryToCloseOrAbortServiceChannelAndPerformExceptionBehaviours();
        }

        private void DisposeChannel()
        {
            if (this.channel != null)
            {
                this.channel.Faulted -= this.Channel_Faulted;
                this.channel = null;

                this.OnDisposedChannel();
            }
        }

        #endregion

        #region Private Methods

        private void TryToCloseOrAbortServiceChannelAndPerformExceptionBehaviours()
        {
            try
            {
                this.CloseOrAbortServiceChannelAndPerformExceptionBehaviours(this.channel);
            }
            catch (Exception ex)
            {
                try
                {
                    this.OnUnexpectedException(ex);    // this may throw a new exception
                }
                catch (Exception again) 
                {
                    // Failsafe
                    Console.Error.WriteLine("Exception occurred while handling exception that occurred while trying to close or abort channel: " + again.ToString());
                }
            }
        }

        private IMessage InvokeServiceOperation(IMessage message, out Exception rootCauseExceptionOut)
        {
            IMessage result = null;
            rootCauseExceptionOut = null;

            try
            {
                // base.Invoke returns any exception in the ReturnMessage.Exception property.
                // There is the slim possibility that an exception can still happen before that, which is why this is in a try/catch.
                result = base.Invoke(message);
            }
            catch (Exception ex)
            {
                rootCauseExceptionOut = ex.GetBaseException();
            }
            finally
            {
                if (result != null && rootCauseExceptionOut == null)
                {
                    rootCauseExceptionOut = ExtractRootCauseExceptionFromReturnMessage(result);
                }
            }

            return result;
        }

        private void TryToPerformExceptionBehavioursRelatedToInvokingServiceOperation(Exception rootCauseException)
        {
            try
            {
                if (rootCauseException is FaultException)
                {
                    this.OnFaultException((FaultException)rootCauseException);    // this may throw a new exception
                }
                else if (rootCauseException is CommunicationException)
                {
                    this.OnCommunicationException((CommunicationException)rootCauseException);   // this may throw a new exception
                }
                else if (rootCauseException is TimeoutException)
                {
                    this.OnTimeoutException((TimeoutException)rootCauseException);    // this may throw a new exception
                }
                else
                {
                    this.OnUnexpectedException(rootCauseException);    // this may throw a new exception
                }
            }
            catch (Exception ex)
            {
                try
                {
                    this.OnUnexpectedException(ex);    // this may throw a new exception
                }
                catch (Exception again) 
                {
                    // Failsafe
                    Console.Error.WriteLine("Exception occurred while handling exception that occurred in exception behaviours for invoking a service operation: " + again.ToString());
                }
            }
        }

        private void CloseOrAbortServiceChannelAndPerformExceptionBehaviours(ICommunicationObject communicationObject)
        {
            // Stop recursive or multiple calls
            if (this.isClosing || communicationObject == null || communicationObject.State == CommunicationState.Closed)
            {
                return;
            }

            this.isClosing = true;

            bool isClosed = false;
            Exception closeException = null;
            try
            {
                if (communicationObject.State != CommunicationState.Faulted)
                {
                    this.OnClosingChannel();

                    communicationObject.Close();
                    isClosed = true;

                    this.OnClosedChannel();
                }
            }
            catch (Exception ex)
            {
                // Handle the race condition where it might have faulted just after the If above.
                closeException = ex;
            }
            finally
            {
                if (closeException != null)
                {
                    this.TryToPerformCloseExceptionBehaviours(closeException);
                }

                // If the channel has not been closed yet because:
                // - State was Faulted; or 
                // - An exception occurred while doing the Close()
                // Then do an Abort()
                if (!isClosed)
                {
                    this.AbortServiceChannel(communicationObject);
                }

                this.DisposeChannel();
            }
        }

        private void TryToPerformCloseExceptionBehaviours(Exception closeException)
        {
            try
            {
                if (closeException is CommunicationException)
                {
                    this.OnCloseCommunicationException((CommunicationException)closeException);
                }
                else if (closeException is TimeoutException)
                {
                    this.OnCloseTimeoutException((TimeoutException)closeException);
                }
                else
                {
                    // An unexpected exception that we don't know how to handle.
                    this.OnCloseUnexpectedException(closeException);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    this.OnCloseUnexpectedException(ex);    // this may throw a new exception
                }
                catch (Exception again) 
                {
                    // Failsafe
                    Console.Error.WriteLine("Exception occurred while handling exception that occurred in exception behaviours for closing the channel: " + again.ToString());
                }
            }
        }

        private void AbortServiceChannel(ICommunicationObject communicationObject)
        {
            Exception abortException = null;

            try
            {
                this.OnAbortingChannel();

                communicationObject.Abort();

                this.OnAbortedChannel();
            }
            catch (Exception ex)
            {
                abortException = ex;
            }
            finally
            {
                if (abortException != null)
                {
                    this.TryToPerformAbortExceptionBehaviours(abortException);
                }
            }
        }

        private void TryToPerformAbortExceptionBehaviours(Exception abortException)
        {
            try
            {
                this.OnAbortException(abortException);
            }
            catch (Exception ex)
            {
                try
                {
                    this.OnAbortException(ex);    // this may throw a new exception
                }
                catch (Exception again) 
                {
                    // Failsafe
                    Console.Error.WriteLine("Exception occurred while handling exception that occurred in exception behaviours for aborting the channel: " + again.ToString());
                }
            }
        }

        #endregion
    }
}
