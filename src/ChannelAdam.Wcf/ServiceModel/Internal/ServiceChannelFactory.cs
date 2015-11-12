//-----------------------------------------------------------------------
// <copyright file="ServiceChannelFactory.cs">
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
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    using ChannelAdam.ServiceModel.Internal;

    /// <summary>
    /// A WCF Service Channel Factory that creates service channels / <see cref="ICommunicationObject"/> instances.
    /// </summary>
    public static class ServiceChannelFactory
    {
        #region Fields

        private static readonly IDictionary<Type, IChannelFactory> ChannelFactories = new Dictionary<Type, IChannelFactory>();

        #endregion

        /// <summary>
        /// Creates the service channel using the given endpoint configuration name.
        /// </summary>
        /// <typeparam name="TServiceInterface">The type of the service interface.</typeparam>
        /// <param name="endpointConfigurationName">Name of the endpoint configuration.</param>
        /// <returns>
        /// The service channel / <see cref="ICommunicationObject"/> instance.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "This rule is more about type inference - but we specifically do not want type inference, as it is not the intent of this method.")] 
        public static ICommunicationObject CreateChannel<TServiceInterface>(string endpointConfigurationName)
        {
            return CreateChannel(typeof(TServiceInterface), endpointConfigurationName);
        }

        /// <summary>
        /// Creates the service channel / <see cref="ICommunicationObject" /> instance.
        /// </summary>
        /// <param name="serviceInterface">The service interface.</param>
        /// <param name="endpointConfigurationName">Name of the endpoint configuration.</param>
        /// <returns>An <see cref="ICommunicationObject"/>.</returns>
        /// <exception cref="System.ArgumentNullException">If serviceInterface or endpointConfigurationName is null.</exception>
        /// <exception cref="System.ArgumentException">If serviceInterface is not provided.</exception>
        public static ICommunicationObject CreateChannel(Type serviceInterface, string endpointConfigurationName)
        {
            if (serviceInterface == null)
            {
                throw new ArgumentNullException(nameof(serviceInterface));
            }

            if (!serviceInterface.IsInterface)
            {
                throw new ArgumentException(string.Format("Type '{0}' is not an interface.", serviceInterface.FullName), nameof(serviceInterface));
            }

            if (string.IsNullOrWhiteSpace(endpointConfigurationName))
            {
                throw new ArgumentNullException(nameof(endpointConfigurationName));
            }

            var channelFactoryType = typeof(ChannelFactory<>).MakeGenericType(serviceInterface);

            IChannelFactory channelFactory = GetChannelFactory(channelFactoryType, endpointConfigurationName);
            ICommunicationObject serviceChannel = CreateServiceChannel(channelFactory);

            return serviceChannel;
        }

        private static IChannelFactory GetChannelFactory(Type channelFactoryType, string endpointConfigurationName)
        {
            IChannelFactory channelFactory;

            ChannelFactories.TryGetValue(channelFactoryType, out channelFactory);

            if (channelFactory == null)
            {
                channelFactory = CreateChannelFactoryInstanceThreadSafe(channelFactoryType, endpointConfigurationName);
            }

            return channelFactory;
        }

        private static IChannelFactory CreateChannelFactoryInstanceThreadSafe(Type channelFactoryType, string endpointConfigurationName)
        {
            IChannelFactory channelFactory;

            lock (ChannelFactories)
            {
                ChannelFactories.TryGetValue(channelFactoryType, out channelFactory);
                if (channelFactory == null)
                {
                    channelFactory = (IChannelFactory)Activator.CreateInstance(channelFactoryType, endpointConfigurationName);
                    ChannelFactories.Add(channelFactoryType, channelFactory);
                }
            }

            return channelFactory;
        }

        private static ICommunicationObject CreateServiceChannel(IChannelFactory channelFactory)
        {
            // IChannelFactory<TChannel> has the method CreateChannel(), but IChannelFactory does not.
            // However, in a method above we activated the channel factory as type 'ChannelFactory<TServiceInterface>'
            // so we know that the CreateChannel() method is there...
            dynamic dynamicChannelFactory = channelFactory;
            var serviceChannel = (ICommunicationObject)dynamicChannelFactory.CreateChannel();
            return serviceChannel;
        }
    }
}