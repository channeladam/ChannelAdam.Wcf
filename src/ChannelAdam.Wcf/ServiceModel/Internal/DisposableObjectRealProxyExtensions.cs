using System;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using ChannelAdam.Runtime.Remoting.Proxies;

namespace ChannelAdam.ServiceModel.Internal
{
    public static class DisposableObjectRealProxyExtensions
    {
        public static IMessage InvokeSupportingRefs(this DisposableObjectRealProxy proxy, IMessage msg, object proxiedObject)
        {
            IMessage returnMessage;

            var methodCallMessage = (IMethodCallMessage)msg;

            try
            {
                if (proxy.IsDisposed)
                {
                    throw new ObjectDisposedException($"{proxy.GetType().FullName} proxying {proxy.TypeName}");
                }

                var args = methodCallMessage.Args;
                var result = InvokeMethod(methodCallMessage.MethodBase, methodCallMessage.MethodName == "Dispose" ? proxy : proxiedObject, args);

                returnMessage = new ReturnMessage(
                    result,                                 // Operation result
                    args,                                   // Out arguments
                    args.Length,                            // Out arguments count
                    methodCallMessage.LogicalCallContext,   // Call context
                    methodCallMessage);                     // Original message
            }
            catch (Exception e)
            {
                returnMessage = new ReturnMessage(e, methodCallMessage);
            }

            return returnMessage;
        }

        private static object InvokeMethod(MethodBase methodInfo, object onThis, object[] args)
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }

            try
            {
                return methodInfo.Invoke(onThis, args);
            }
            catch (TargetInvocationException targetEx)
            {
                if (targetEx.InnerException != null)
                {
                    // Unwrap the real exception from the TargetInvocationException
                    throw new AggregateException(new[] { targetEx.InnerException });
                }

                throw;
            }
        }
    }
}