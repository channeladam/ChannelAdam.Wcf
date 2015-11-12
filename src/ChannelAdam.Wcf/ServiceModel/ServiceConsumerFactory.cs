//-----------------------------------------------------------------------
// <copyright file="ServiceConsumerFactory.cs">
//     Copyright (c) 2014-2015 Adam Craven. All rights reserved.
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

    using ChannelAdam.ServiceModel.Internal;
    using ChannelAdam.TransientFaultHandling;

    /// <summary>
    /// A WCF Service Consumer Factory that creates <see cref="ServiceConsumer"/> instances.
    /// </summary>
    public static class ServiceConsumerFactory
    {
        private static IServiceConsumerExceptionBehaviourStrategy defaultExceptionBehaviourStrategy = StandardErrorServiceConsumerExceptionBehaviourStrategy.Instance;

        private static IRetryPolicyFunction defaultRetryPolicy = null;

        #region Public Static Properties

        /// <summary>
        /// Gets or sets the default retry policy, used only with the <c>Call</c> method, and NOT if you call a service operation directly through the <c>Operations</c> property.
        /// </summary>
        /// <value>
        /// The default retry policy.
        /// </value>
        /// <remarks>Out of the box, the default retry policy is not to retry.</remarks>
        public static IRetryPolicyFunction DefaultRetryPolicy
        {
            get
            {
                return defaultRetryPolicy;
            }

            set
            {
                defaultRetryPolicy = value;
            }
        }

        /// <summary>
        /// Gets or sets the default exception behaviour strategy to apply to all service consumers.
        /// </summary>
        /// <value>
        /// The default exception behaviour strategy.
        /// </value>
        /// <remarks>
        /// Out of the box, this is the <see cref="StandardErrorServiceConsumerExceptionBehaviourStrategy"/>.
        /// </remarks>
        public static IServiceConsumerExceptionBehaviourStrategy DefaultExceptionBehaviourStrategy
        {
            get
            {
                return defaultExceptionBehaviourStrategy;
            }

            set
            {
                defaultExceptionBehaviourStrategy = value;
            }
        }

        #endregion

        #region Public Static Methods - Create with a factory method

        /// <summary>
        /// Creates a <see cref="ServiceConsumer" /> from a factory method that returns a communication object / channel / Visual Studio Service Reference ServiceClient.
        /// </summary>
        /// <typeparam name="TServiceInterface">The type of the service interface.</typeparam>
        /// <param name="factoryMethod">The factory method.</param>
        /// <returns>
        /// A <see cref="ServiceConsumer" />.
        /// </returns>
        public static IServiceConsumer<TServiceInterface> Create<TServiceInterface>(Func<ICommunicationObject> factoryMethod)
        {
            return CreateServiceConsumer<TServiceInterface>(factoryMethod, defaultRetryPolicy, defaultExceptionBehaviourStrategy, null);
        }

        /// <summary>
        /// Creates a <see cref="ServiceConsumer" /> from a factory method that returns a communication object / channel / Visual Studio Service Reference ServiceClient.
        /// </summary>
        /// <typeparam name="TServiceInterface">The type of the service interface.</typeparam>
        /// <param name="factoryMethod">The factory method.</param>
        /// <param name="retryPolicy">The retry policy.</param>
        /// <returns>
        /// A <see cref="ServiceConsumer" />.
        /// </returns>
        public static IServiceConsumer<TServiceInterface> Create<TServiceInterface>(Func<ICommunicationObject> factoryMethod, IRetryPolicyFunction retryPolicy)
        {
            return CreateServiceConsumer<TServiceInterface>(factoryMethod, retryPolicy, defaultExceptionBehaviourStrategy, null);
        }

        /// <summary>
        /// Creates a <see cref="ServiceConsumer" /> from a factory method that returns a communication object / channel / Visual Studio Service Reference ServiceClient.
        /// </summary>
        /// <typeparam name="TServiceInterface">The type of the service interface.</typeparam>
        /// <param name="factoryMethod">The factory method.</param>
        /// <param name="exceptionStrategy">The exception strategy.</param>
        /// <returns>
        /// A <see cref="ServiceConsumer" />.
        /// </returns>
        public static IServiceConsumer<TServiceInterface> Create<TServiceInterface>(Func<ICommunicationObject> factoryMethod, IServiceConsumerExceptionBehaviourStrategy exceptionStrategy)
        {
            return CreateServiceConsumer<TServiceInterface>(factoryMethod, defaultRetryPolicy, exceptionStrategy, null);
        }

        /// <summary>
        /// Creates a <see cref="ServiceConsumer" /> from a factory method that returns a communication object / channel / Visual Studio Service Reference ServiceClient.
        /// </summary>
        /// <typeparam name="TServiceInterface">The type of the service interface.</typeparam>
        /// <param name="factoryMethod">The factory method.</param>
        /// <param name="retryPolicy">The retry policy.</param>
        /// <param name="exceptionStrategy">The exception strategy.</param>
        /// <returns>
        /// A <see cref="ServiceConsumer" />.
        /// </returns>
        public static IServiceConsumer<TServiceInterface> Create<TServiceInterface>(Func<ICommunicationObject> factoryMethod, IRetryPolicyFunction retryPolicy, IServiceConsumerExceptionBehaviourStrategy exceptionStrategy)
        {
            return CreateServiceConsumer<TServiceInterface>(factoryMethod, retryPolicy, exceptionStrategy, null);
        }

        /// <summary>
        /// Creates a <see cref="ServiceConsumer" /> from a factory method that returns a communication object / channel / Visual Studio Service Reference ServiceClient.
        /// </summary>
        /// <typeparam name="TServiceInterface">The type of the service interface.</typeparam>
        /// <param name="factoryMethod">The factory method.</param>
        /// <param name="retryPolicy">The retry policy.</param>
        /// <param name="exceptionStrategy">The exception strategy.</param>
        /// <param name="closeTriggerStrategy">The service channel close trigger strategy.</param>
        /// <returns>
        /// A <see cref="ServiceConsumer" />.
        /// </returns>
        public static IServiceConsumer<TServiceInterface> Create<TServiceInterface>(Func<ICommunicationObject> factoryMethod, IRetryPolicyFunction retryPolicy, IServiceConsumerExceptionBehaviourStrategy exceptionStrategy, IServiceChannelCloseTriggerStrategy closeTriggerStrategy)
        {
            return CreateServiceConsumer<TServiceInterface>(factoryMethod, retryPolicy, exceptionStrategy, closeTriggerStrategy);
        }

        #endregion

        #region Public Static Methods - Create with a an endpoint configuration name

        /// <summary>
        /// Creates a <see cref="ServiceConsumer" /> from the specified endpoint configuration name.
        /// </summary>
        /// <typeparam name="TServiceInterface">The type of the service interface.</typeparam>
        /// <param name="endpointConfigurationName">Name of the endpoint configuration.</param>
        /// <returns>
        /// A <see cref="ServiceConsumer" />
        /// </returns>
        public static IServiceConsumer<TServiceInterface> Create<TServiceInterface>(string endpointConfigurationName)
        {
            Func<ICommunicationObject> factoryMethod = () => ServiceChannelFactory.CreateChannel<TServiceInterface>(endpointConfigurationName);
            return ServiceConsumerFactory.Create<TServiceInterface>(factoryMethod);
        }

        /// <summary>
        /// Creates a <see cref="ServiceConsumer" /> from a factory method that returns a communication object / channel / Visual Studio Service Reference ServiceClient.
        /// </summary>
        /// <typeparam name="TServiceInterface">The type of the service interface.</typeparam>
        /// <param name="endpointConfigurationName">Name of the endpoint configuration.</param>
        /// <param name="retryPolicy">The retry policy.</param>
        /// <returns>
        /// A <see cref="ServiceConsumer" />.
        /// </returns>
        public static IServiceConsumer<TServiceInterface> Create<TServiceInterface>(string endpointConfigurationName, IRetryPolicyFunction retryPolicy)
        {
            Func<ICommunicationObject> factoryMethod = () => ServiceChannelFactory.CreateChannel<TServiceInterface>(endpointConfigurationName);
            return ServiceConsumerFactory.Create<TServiceInterface>(factoryMethod, retryPolicy);
        }

        /// <summary>
        /// Creates a <see cref="ServiceConsumer" /> from the specified endpoint configuration name.
        /// </summary>
        /// <typeparam name="TServiceInterface">The type of the service interface.</typeparam>
        /// <param name="endpointConfigurationName">Name of the endpoint configuration.</param>
        /// <param name="exceptionStrategy">The exception strategy.</param>
        /// <returns>
        /// A <see cref="ServiceConsumer" />
        /// </returns>
        public static IServiceConsumer<TServiceInterface> Create<TServiceInterface>(string endpointConfigurationName, IServiceConsumerExceptionBehaviourStrategy exceptionStrategy)
        {
            Func<ICommunicationObject> factoryMethod = () => ServiceChannelFactory.CreateChannel<TServiceInterface>(endpointConfigurationName);
            return ServiceConsumerFactory.Create<TServiceInterface>(factoryMethod, exceptionStrategy);
        }

        /// <summary>
        /// Creates a <see cref="ServiceConsumer" /> from a factory method that returns a communication object / channel / Visual Studio Service Reference ServiceClient.
        /// </summary>
        /// <typeparam name="TServiceInterface">The type of the service interface.</typeparam>
        /// <param name="endpointConfigurationName">Name of the endpoint configuration.</param>
        /// <param name="retryPolicy">The retry policy.</param>
        /// <param name="exceptionStrategy">The exception strategy.</param>
        /// <returns>
        /// A <see cref="ServiceConsumer" />.
        /// </returns>
        public static IServiceConsumer<TServiceInterface> Create<TServiceInterface>(string endpointConfigurationName, IRetryPolicyFunction retryPolicy, IServiceConsumerExceptionBehaviourStrategy exceptionStrategy)
        {
            Func<ICommunicationObject> factoryMethod = () => ServiceChannelFactory.CreateChannel<TServiceInterface>(endpointConfigurationName);
            return ServiceConsumerFactory.Create<TServiceInterface>(factoryMethod, retryPolicy, exceptionStrategy);
        }

        /// <summary>
        /// Creates a <see cref="ServiceConsumer" /> from a factory method that returns a communication object / channel / Visual Studio Service Reference ServiceClient.
        /// </summary>
        /// <typeparam name="TServiceInterface">The type of the service interface.</typeparam>
        /// <param name="endpointConfigurationName">Name of the endpoint configuration.</param>
        /// <param name="retryPolicy">The retry policy.</param>
        /// <param name="exceptionStrategy">The exception strategy.</param>
        /// <param name="closeTriggerStrategy">The service channel close trigger strategy.</param>
        /// <returns>
        /// A <see cref="ServiceConsumer" />.
        /// </returns>
        public static IServiceConsumer<TServiceInterface> Create<TServiceInterface>(string endpointConfigurationName, IRetryPolicyFunction retryPolicy, IServiceConsumerExceptionBehaviourStrategy exceptionStrategy, IServiceChannelCloseTriggerStrategy closeTriggerStrategy)
        {
            Func<ICommunicationObject> factoryMethod = () => ServiceChannelFactory.CreateChannel<TServiceInterface>(endpointConfigurationName);
            return ServiceConsumerFactory.Create<TServiceInterface>(factoryMethod, retryPolicy, exceptionStrategy, closeTriggerStrategy);
        }

        #endregion

        #region Private Static Methods

        private static IServiceConsumer<TServiceInterface> CreateServiceConsumer<TServiceInterface>(Func<ICommunicationObject> factoryMethod, IRetryPolicyFunction retryPolicy, IServiceConsumerExceptionBehaviourStrategy exceptionStrategy, IServiceChannelCloseTriggerStrategy closeTriggerStrategy)
        {
            var consumer = new ServiceConsumer<TServiceInterface>(factoryMethod);
            consumer.ExceptionBehaviourStrategy = exceptionStrategy;
            consumer.RetryPolicy = retryPolicy;
            consumer.ChannelCloseTriggerStrategy = closeTriggerStrategy;
            return consumer;
        }

        #endregion
    }
}