
namespace ChannelAdam.ServiceModel
{
    using ChannelAdam.TransientFaultHandling;

    using Microsoft.Practices.TransientFaultHandling;

    public static class ExtensionMethods
    {
        /// <summary>
        /// Converts the Microsoft <see cref="RetryPolicy"/> to a <see cref="RetryPolicyAdapter"/>.
        /// </summary>
        /// <param name="retryPolicy">The retry policy.</param>
        /// <returns></returns>
        public static IRetryPolicyFunction ForServiceConsumer(this RetryPolicy retryPolicy)
        {
            return new RetryPolicyAdapter(retryPolicy);
        }

        public static void SetRetryPolicy<TServiceInterface>(this IServiceConsumer<TServiceInterface> serviceConsumer, RetryPolicy retryPolicy)
        {
            serviceConsumer.RetryPolicy = new RetryPolicyAdapter(retryPolicy);
        }

        ///// <summary>
        ///// Converts the Microsoft <see cref="RetryPolicy{typeparam name="TErrorDetectionStrategy"}"/> to a <see cref="RetryPolicyAdapter{typeparam name="TErrorDetectionStrategy"}"/>.
        ///// </summary>
        ///// <param name="retryPolicy">The retry policy.</param>
        ///// <returns></returns>
        //public static RetryPolicyAdapter<TErrorDetectionStrategy> ForServiceConsumer<TErrorDetectionStrategy>(this RetryPolicy<TErrorDetectionStrategy> retryPolicy)
        //    where TErrorDetectionStrategy : ITransientErrorDetectionStrategy, new()
        //{
        //    return new RetryPolicyAdapter<TErrorDetectionStrategy>(retryPolicy);
        //}
    }
}
