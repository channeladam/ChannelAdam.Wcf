//-----------------------------------------------------------------------
// <copyright file="ServiceOperationExpressionAdapter.cs">
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
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// Adapts a service operation expression for usage with the reflection classes.
    /// </summary>
    /// <typeparam name="TServiceInterface">The type of the service interface.</typeparam>
    public class ServiceOperationExpressionAdapter<TServiceInterface>
    {
        private const BindingFlags MethodBindingFlags = BindingFlags.ExactBinding | BindingFlags.Instance |
            BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.Public;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceOperationExpressionAdapter&lt;TServiceInterface&gt;"/> class.
        /// </summary>
        public ServiceOperationExpressionAdapter()
        {
            this.ServiceInterfaceType = typeof(TServiceInterface);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the operation name to execute.
        /// </summary>
        /// <value>The operation name to execute.</value>
        public string OperationNameToExecute { get; private set; }

        /// <summary>
        /// Gets the operation method arguments.
        /// </summary>
        /// <value>The operation method arguments.</value>
        public IEnumerable<object> OperationMethodArguments { get; private set; }

        /// <summary>
        /// Gets the type of the service interface.
        /// </summary>
        /// <value>The type of the service interface.</value>
        public Type ServiceInterfaceType { get; private set; }

        /// <summary>
        /// Gets the service operation method information.
        /// </summary>
        /// <value>
        /// The operation method information.
        /// </value>
        /// <exception cref="System.InvalidOperationException">If the given expression is not a public method.</exception>
        public MethodInfo OperationMethodInfo
        {
            get 
            {
                var methodInfoToExecute = this.ServiceInterfaceType.GetMethod(this.OperationNameToExecute, MethodBindingFlags);
                if (methodInfoToExecute == null)
                {
                    throw new InvalidOperationException(string.Format("Could not get the method info for Operation '{0}' from type '{1}'", this.OperationNameToExecute, this.ServiceInterfaceType.FullName));
                }

                return methodInfoToExecute;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Parses the given service operation expression.
        /// </summary>
        /// <param name="serviceOperationExpression">The service operation expression.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "We want the more derived type.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This rule was created before Expressions.")]
        public void Parse(Expression<Action<TServiceInterface>> serviceOperationExpression)
        {
            this.ParseExpression(serviceOperationExpression);
        }

        /// <summary>
        /// Parses the given service operation expression.
        /// </summary>
        /// <typeparam name="TReturnValue">The type of the return value.</typeparam>
        /// <param name="serviceOperationExpression">The service operation expression.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "We want the more derived type.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This rule was created before Expressions.")]
        public void Parse<TReturnValue>(Expression<Func<TServiceInterface, TReturnValue>> serviceOperationExpression)
        {
            this.ParseExpression(serviceOperationExpression);
        }

        #endregion

        #region Private Methods

        private static MethodCallExpression CastAsMethodCallExpression(LambdaExpression serviceOperationExpression)
        {
            var method = serviceOperationExpression.Body as MethodCallExpression;
            if (method == null)
            {
                throw new ArgumentException("Argument must be a System.Linq.Expressions.MethodCallExpression", "serviceOperationExpression");
            }

            return method;
        }

        private static object[] GetMethodArguments(MethodCallExpression method)
        {
            return method.Arguments.Select(arg => GetArgumentValue(arg)).ToArray();
        }
        
        private static object GetArgumentValue(Expression argumentExpression)
        {
            return Expression.Lambda(argumentExpression).Compile().DynamicInvoke();
        }

        private void ParseExpression(LambdaExpression serviceOperationExpression)
        {
            MethodCallExpression method = CastAsMethodCallExpression(serviceOperationExpression);
            this.OperationNameToExecute = method.Method.Name;
            this.OperationMethodArguments = GetMethodArguments(method);
        }

        #endregion
    }
}
