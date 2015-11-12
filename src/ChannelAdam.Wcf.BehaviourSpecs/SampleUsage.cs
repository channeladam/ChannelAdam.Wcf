//-----------------------------------------------------------------------
// <copyright file="SampleUsage.cs">
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

namespace ChannelAdam.Wcf.BehaviourSpecs
{
    using System;
    using System.Collections;
    using System.ServiceModel;
    using System.Threading;

    using ChannelAdam;
    using ChannelAdam.ServiceModel;
    using ChannelAdam.ServiceModel.Internal;
    using ChannelAdam.Wcf.BehaviourSpecs.SampleServiceReference;
    using ChannelAdam.Wcf.BehaviourSpecs.TestDoubles;

    using Microsoft.Practices.TransientFaultHandling;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Sample typical usage of the ServiceConsumer.
    /// </summary>
    [TestClass]
    public class SampleUsage
    {
        ////[TestMethod]
        ////public void Sample_Level100_BasicUsage_OperationsProperty_RealService()
        ////{
        ////    using (var service = ServiceConsumerFactory.Create<ISampleService>("BasicHttpBinding_ISampleService"))
        ////    {
        ////        try
        ////        {
        ////            string actual = service.Operations.SampleOperation(1);
        ////            Console.WriteLine("Actual: " + actual);
        ////            Assert.AreEqual("You entered: 1", actual);

        ////            return;
        ////        }
        ////        // catch (FaultException<MyBusinessLogicType> fe)
        ////        catch (FaultException fe)
        ////        {
        ////            Console.WriteLine("Service operation threw a fault: " + fe.ToString());
        ////        }
        ////        catch (Exception ex)
        ////        {
        ////            Console.WriteLine("Technical error occurred while calling the service operation: " + ex.ToString());
        ////        }

        ////        Assert.Fail("Service operation was not successfully called");
        ////    }
        ////}

        [TestMethod]
        public void Sample_Level100_BasicUsage_OperationsProperty()
        {
            //using (var service = ServiceConsumerFactory.Create<IFakeService>("BasicHttpBinding_IFakeService"))
            using (var service = ServiceConsumerFactory.Create<IFakeService>(() => new FakeServiceClient()))
            {
                try
                {
                    int actual = service.Operations.AddIntegers(1, 1);
                    Console.WriteLine("Actual: " + actual);
                    Assert.AreEqual(2, actual);

                    return;
                }
                // catch (FaultException<MyBusinessLogicType> fe)
                catch (FaultException fe)
                {
                    Console.WriteLine("Service operation threw a fault: " + fe.ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Technical error occurred while calling the service operation: " + ex.ToString());
                }

                Assert.Fail("Service operation was not successfully called");
            }
        }

        [TestMethod]
        public void Sample_Level100_BasicUsage_CallMethod()
        {
            using (var service = ServiceConsumerFactory.Create<IFakeService>(() => new FakeServiceClient()))
            {
                IOperationResult<int> result = service.Consume(operation => operation.AddIntegers(1, 1));

                if (result.HasNoException)
                {
                    int actual = result.Value;
                    Console.WriteLine("Actual: " + actual);
                    Assert.AreEqual(2, actual);
                }
                else
                {
                    // if (result.HasFaultExceptionOfType<MyBusinessLogicException>())
                    if (result.HasFaultException)
                    {
                        Console.WriteLine("Service operation threw a fault: " + result.Exception.ToString());
                    }
                    else
                    {
                        Console.WriteLine("Technical error occurred while calling the service operation: " + result.Exception.ToString());
                    }

                    Assert.Fail("Service operation call threw an exception");
                }
            }
        }

        [TestMethod]
        public void Sample_Level100_BasicUsage_MultipleCalls()
        {
            using (var service = ServiceConsumerFactory.Create<IFakeService>(() => new FakeServiceClient()))
            {
                IOperationResult<int> result = service.Consume(operation => operation.AddIntegers(1, 1));
                AssertOperationResult(2, result);

                // Even if the channel had a communication exception and went into the fault state or was aborted,
                // you can still use the service consumer!

                result = service.Consume(operation => operation.AddIntegers(1, 3));
                AssertOperationResult(4, result);
            }
        }

        private void AssertOperationResult<T>(T expected, IOperationResult<T> result)
        {
            if (result.HasNoException)
            {
                T actual = result.Value;
                Console.WriteLine("Actual: " + actual);
                Assert.AreEqual(expected, actual);
            }
            else
            {
                // if (result.HasFaultExceptionOfType<MyBusinessLogicException>())
                if (result.HasFaultException)
                {
                    Console.WriteLine("Service operation threw a fault: " + result.Exception.ToString());
                }
                else
                {
                    Console.WriteLine("Technical error occurred while calling the service operation: " + result.Exception.ToString());
                }

                Assert.Fail("Service operation call threw an exception");
            }
        }

        [TestMethod]
        public void Sample_Level200_Static_DefaultExceptionBehaviourStrategy()
        {
            // Apply this exception behaviour strategy for all created service consumer instances.
            // By default, out of the box, the default is a StandardErrorServiceConsumerExceptionBehaviourStrategy.
            ServiceConsumerFactory.DefaultExceptionBehaviourStrategy = new StandardOutServiceConsumerExceptionBehaviourStrategy();

            using (var service = ServiceConsumerFactory.Create<IFakeService>(() => new FakeServiceClient()))
            {
                try
                {
                    int actual = service.Operations.AddIntegers(1, 1);

                    Console.WriteLine("Actual: " + actual);
                    Assert.AreEqual(2, actual);

                    return;
                }
                catch (FaultException fe)
                {
                    Console.WriteLine("Service operation threw a fault: " + fe.ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Technical error occurred while calling the service operation: " + ex.ToString());
                }

                Assert.Fail("Service operation was not successfully called");
            }
        }

        [TestMethod]
        public void Sample_Level200_Instance_ExceptionBehaviourStrategy()
        {
            // Apply the exception handling strategy only for this created service consumer instance
            using (var service = ServiceConsumerFactory.Create<IFakeService>(
                                    () => new FakeServiceClient(),
                                    new StandardOutServiceConsumerExceptionBehaviourStrategy()))
            {
                try
                {
                    int actual = service.Operations.AddIntegers(1, 1);

                    Console.WriteLine("Actual: " + actual);
                    Assert.AreEqual(2, actual);

                    return;
                }
                catch (FaultException fe)
                {
                    Console.WriteLine("Service operation threw a fault: " + fe.ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Technical error occurred while calling the service operation: " + ex.ToString());
                }

                Assert.Fail("Service operation was not successfully called");
            }
        }

        [TestMethod]
        public void Sample3_Level300_AutomaticRetry_Manual_UsingOperationProperty()
        {
            int retryCount = 1;
            Exception lastException = null;

            using (var service = ServiceConsumerFactory.Create<IFakeService>(() => new FakeServiceClient()))
            {
                while (retryCount > 0)
                {
                    Console.WriteLine("#### Retry count: " + retryCount);

                    try
                    {
                        int actual = service.Operations.AddIntegers(1, 1);

                        Console.WriteLine("Actual: " + actual);
                        Assert.AreEqual(2, actual);

                        return;
                    }
                    catch (FaultException fe)
                    {
                        lastException = fe;
                        Console.WriteLine("Service operation threw a fault: " + fe.ToString());
                    }
                    catch (Exception ex)
                    {
                        lastException = ex;
                        Console.WriteLine("Technical error occurred while calling the service operation: " + ex.ToString());
                    }

                    retryCount--;
                }
            }

            Assert.Fail("Service operation was not successfully called");
        }

        [TestMethod]
        public void Sample3_Level300_AutomaticRetry_TransientFaultHandling_UsingOperationsProperty()
        {
            Exception lastException;

            using (var service = ServiceConsumerFactory.Create<IFakeService>(() => new FakeServiceClient()))
            {
                try
                {
                    int actual = 0;

                    var retryStrategy = new Incremental(5, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2));
                    var retryPolicy = new RetryPolicy<SoapFaultWebServiceTransientErrorDetectionStrategy>(retryStrategy);

                    retryPolicy.ExecuteAction(() =>
                    {
                        actual = service.Operations.AddIntegers(1, 1);
                    });

                    Console.WriteLine("Actual: " + actual);
                    Assert.AreEqual(2, actual);

                    return;
                }
                catch (FaultException fe)
                {
                    lastException = fe;
                    Console.WriteLine("Service operation threw a fault: " + fe.ToString());
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    Console.WriteLine("Technical error occurred while calling the service operation: " + ex.ToString());
                }

                Assert.Fail("Service operation was not successfully called");
            }
        }

        [TestMethod]
        public void Sample3_Level300_AutomaticRetry_TransientFaultHandling_DefaultWithCallMethod()
        {
            var retryStrategy = new Incremental(5, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2));
            var retryPolicy = new RetryPolicy<SoapFaultWebServiceTransientErrorDetectionStrategy>(retryStrategy).ForServiceConsumer();

            using (var service = ServiceConsumerFactory.Create<IFakeService>(() => new FakeServiceClient(), retryPolicy))
            {
                var result = service.Consume(operation => operation.AddIntegers(1, 1));

                if (result.HasNoException)
                {
                    Console.WriteLine("Actual: " + result.Value);
                    Assert.AreEqual(2, result.Value);
                }
                else
                {
                    if (result.HasFaultException)
                    {
                        Console.WriteLine("Service operation threw a fault: " + result.Exception.ToString());
                    }
                    else if (result.HasException)
                    {
                        Console.WriteLine("Technical error occurred while calling the service operation: " + result.Exception.ToString());
                    }

                    Assert.Fail("Service operation was not successfully called");
                }
            }
        }

        //[TestMethod]
        //public void Sample3_Level300_AutomaticRetry_TransientFaultHandling_OnCallMethod()
        //{
        //    using (var service = ServiceConsumerFactory.Create<IFakeService>(() => new FakeServiceClient()))
        //    {
        //        var retryStrategy = new Incremental(5, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2));
        //        var retryPolicy = new RetryPolicy<SoapFaultWebServiceTransientErrorDetectionStrategy>(retryStrategy);

        //        var result = service.Consume(operation => operation.AddIntegers(1, 1), retryPolicy);

        //        if (result.HasNoException)
        //        {
        //            Console.WriteLine("Actual: " + result.Value);
        //            Assert.AreEqual(2, result.Value);
        //        }
        //        else
        //        {
        //            if (result.HasFaultException)
        //            {
        //                Console.WriteLine("Service operation threw a fault: " + result.Exception.ToString());
        //            }
        //            else if (result.HasException)
        //            {
        //                Console.WriteLine("Technical error occurred while calling the service operation: " + result.Exception.ToString());
        //            }

        //            Assert.Fail("Service operation was not successfully called");
        //        }
        //    }
        //}
    }
}
