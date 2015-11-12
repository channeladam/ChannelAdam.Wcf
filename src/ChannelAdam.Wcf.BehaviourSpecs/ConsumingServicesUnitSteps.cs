//-----------------------------------------------------------------------
// <copyright file="ConsumingServicesUnitSteps.cs">
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

namespace ChannelAdam.Wcf.BehaviourSpecs
{
    using ChannelAdam.ServiceModel;
    using ChannelAdam.TestFramework.MSTest;
    using ChannelAdam.TransientFaultHandling;
    using ChannelAdam.Wcf.BehaviourSpecs.TestDoubles;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.ServiceModel;
    using System.Text;
    using System.Threading.Tasks;

    using TechTalk.SpecFlow;
    using ChannelAdam.ServiceModel.Internal;
    using System.Threading;
    using Microsoft.Practices.Unity;
    using Autofac;
    using Microsoft.Practices.TransientFaultHandling;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable"), Binding]
    [Scope(Feature = "Consuming Services")]
    public class ConsumingServicesUnitSteps : MoqTestFixture
    {
        Func<ICommunicationObject, DisposableServiceChannelProxy<IFakeService>> disposableServiceChannelProxyFactoryMethod;
        Func<ICommunicationObject> serviceChannelFactoryMethod;
        IServiceConsumer<IFakeService> serviceConsumer;
        WeakReference<ServiceConsumer<IFakeService>> weakRefServiceConsumer = null;
        int actualResult = 0;
        int expectedResult = 0;
        Mock<IFakeService> mockService;
        DisposableServiceChannelProxy<IFakeService> disposableServiceChannelProxy;
        Mock<IServiceConsumerExceptionBehaviourStrategy> mockExceptionStrategy;
        Exception exceptionToThrowOnClosing = null;
        bool abortThreadOnClosing = false;
        Exception exceptionToThrowOnAborting = null;
        int serviceChannelCreatedCount = 0, proxyCreatedCount = 0,
            proxyClosingCount = 0, proxyClosedCount = 0,
            proxyAbortingCount = 0, proxyAbortedCount = 0,
            proxyDisposedCount = 0,
            operationCallCount = 0;
        long memoryBefore;
        IUnityContainer unityContainer;
        ConstructorInjectedController controller;
        Autofac.IContainer autofacContainer;
        SimpleInjector.Container simpleContainer;

        [BeforeTestRun]
        public static void BeforeTestRun()
        {
            // Do this if you want to analyse heap allocations to track down possible leaks - it helps reduce noise ;)
            // FakeServiceClient.CacheSetting = CacheSetting.AlwaysOff;
        }

        [AfterScenario]
        public void CleanUp()
        {
            Logger.Log("About to verify mock objects");
            base.MyMockRepository.Verify();
        }

        #region Given

        [Given(@"a factory method to create a new service channel")]
        public void GivenAFactoryMethodToCreateANewServiceChannel()
        {
            this.serviceChannelFactoryMethod = () =>
                {
                    serviceChannelCreatedCount++;
                    Logger.Log("Created new Service Channel");

                    return new FakeServiceClient();
                };
        }

        [Given(@"a service consumer is created with the factory method")]
        public void GivenAServiceConsumerIsCreatedWithTheFactoryMethod()
        {
            GivenAFactoryMethodToCreateANewServiceChannel();
            WhenTheServiceConsumerIsCreatedWithTheFactoryMethod();
        }


        [Given(@"a configured Unity IoC container")]
        public void GivenAConfiguredUnityIoCContainer()
        {
            this.unityContainer = new UnityContainer();

            this.unityContainer

                .RegisterType<IServiceConsumer<IFakeService>>(
                    new TransientLifetimeManager(),
                    new InjectionFactory(c => ServiceConsumerFactory.Create<IFakeService>("BasicHttpBinding_IFakeService")))
                //
                // OR
                //
                //.RegisterType<IServiceConsumer<IFakeService>>(
                //    new InjectionFactory(c => ServiceConsumerFactory.Create<IFakeService>(() => new FakeServiceClient())))
                //
                // OR more 'correctly'
                //
                .RegisterType<IFakeService, FakeServiceClient>(new TransientLifetimeManager())
                .RegisterType<IServiceConsumer<IFakeService>>(
                    new InjectionFactory(c =>
                        ServiceConsumerFactory.Create<IFakeService>(() => (ICommunicationObject)c.Resolve<IFakeService>())))
                ;

        }

        [Given(@"a configured Autofac IoC container")]
        public void GivenAConfiguredAutofacIoCContainer()
        {
            var builder = new Autofac.ContainerBuilder();

            builder.Register(c => ServiceConsumerFactory.Create<IFakeService>("BasicHttpBinding_IFakeService")).InstancePerDependency();

            builder.RegisterType<ConstructorInjectedController>();

            this.autofacContainer = builder.Build();
        }

        [Given(@"a configured Simple Injector IoC container")]
        public void GivenAConfiguredSimpleInjectorIoCContainer()
        {
            this.simpleContainer = new SimpleInjector.Container();

            this.simpleContainer.Register(() => ServiceConsumerFactory.Create<IFakeService>("BasicHttpBinding_IFakeService"), SimpleInjector.Lifestyle.Transient);

            this.simpleContainer.Verify();
        }

        [Given(@"a service consumer is created with an operation that throws a '(.*)' exception")]
        public void GivenAServiceConsumerIsCreatedWithAnOperationThatThrowsAException(string typeOfException)
        {
            InitialiseFactoryMethodsAndMockServiceAndDisposableProxyWithEvents();

            SetupMockServiceToThrowException(typeOfException);

            CreateServiceConsumerWithMocks();
        }

        [Given(@"the service consumer has a default retry policy")]
        public void GivenTheServiceConsumerHasADefaultRetryPolicy()
        {
            var retryStrategy = new FixedInterval(1, TimeSpan.FromSeconds(2));
            this.serviceConsumer.SetRetryPolicy(
                new RetryPolicy<SoapFaultWebServiceTransientErrorDetectionStrategy>(retryStrategy));
        }

        [Given(@"the service consumer has a default retry policy with a retry policy attempt exception behaviour")]
        public void GivenTheServiceConsumerHasADefaultRetryPolicyWithARetryExceptionBehaviour()
        {
            GivenTheServiceConsumerHasADefaultRetryPolicy();
            CreateAndSetupMockExceptionStrategy("retry");
            this.serviceConsumer.ExceptionBehaviourStrategy = this.mockExceptionStrategy.Object;
        }

        [Given(@"the service consumer has a custom service channel close trigger strategy that does not ever trigger the closing of the service channnel")]
        public void GivenTheServiceConsumerHasACustomServiceChannelCloseTriggerStrategyThatDoesNotEverTriggerTheClosingOfTheServiceChannnel()
        {
            this.serviceConsumer.ChannelCloseTriggerStrategy = new NullServiceChannelCloseTriggerStrategy();
        }

        [Given(@"a service consumer is created with an operation aborts the thread")]
        public void GivenAServiceConsumerIsCreatedWithAnOperationAbortsTheThread()
        {
            GivenAServiceConsumerIsCreatedWithAnOperationThatThrowsAException("thread abort");
        }

        [Given(@"a service consumer with a channel that will fault")]
        public void GivenAServiceConsumerWithAChannelThatWillFault()
        {
            InitialiseFactoryMethodsAndMockServiceAndDisposableProxyWithEvents();

            SetupMockServiceToSayStateIsFault();

            CreateServiceConsumerWithMocks();
        }

        [Given(@"a service consumer that will throw an '(.*)' exception when the service channel is closing")]
        public void GivenAServiceConsumerThatWillThrowAnExceptionWhenTheServiceChannelIsClosing(string typeOfException)
        {
            InitialiseFactoryMethodsAndMockServiceAndDisposableProxyWithEvents();

            MakeExceptionToThrowOnClosing(typeOfException);

            CreateServiceConsumerWithMocks();
        }

        [Given(@"a service consumer that will throw an exception when the service channel is aborting")]
        public void GivenAServiceConsumerThatWillThrowAnExceptionWhenTheServiceChannelIsAborting()
        {
            InitialiseFactoryMethodsAndMockServiceAndDisposableProxyWithEvents();

            SetupMockServiceToSayStateIsFault();
            MakeExceptionToThrowOnAborting();

            CreateServiceConsumerWithMocks();
        }

        [Given(@"a service consumer that will abort the thread when the service channel is closing")]
        public void GivenAServiceConsumerThatWillAbortTheThreadWhenTheServiceChannelIsClosing()
        {
            InitialiseFactoryMethodsAndMockServiceAndDisposableProxyWithEvents();

            this.abortThreadOnClosing = true;

            CreateServiceConsumerWithMocks();
        }

        [Given(@"a service consumer is created")]
        public void GivenAServiceConsumerIsCreated()
        {
            InitialiseFactoryMethodsAndMockServiceAndDisposableProxyWithEvents();
            CreateServiceConsumerWithMocks();
        }

        [Given(@"a service consumer is created for testing garbage collection")]
        public void GivenAServiceConsumerIsCreatedForTestingGarbageCollection()
        {
            var svc = new ServiceConsumer<IFakeService>(CreateRetryEnabledDisposableServiceChannelProxy());
            svc.ExceptionBehaviourStrategy = new FakeExceptionHandlingStrategy();
            this.weakRefServiceConsumer = new WeakReference<ServiceConsumer<IFakeService>>(svc, true);
        }

        [Given(@"a service consumer is created with an operation that throws a '(.*)' exception and has a corresponding exception behaviour strategy")]
        public void GivenAServiceConsumerIsCreatedWithAnOperationThatThrowsAExceptionAndHasACorrespondingExceptionBehaviourStrategy(string typeOfException)
        {
            InitialiseFactoryMethodsAndMockServiceAndDisposableProxyWithEvents();

            SetupMockServiceToThrowException(typeOfException);

            CreateAndSetupMockExceptionStrategy(typeOfException);
            CreateServiceConsumerWithMocksAndExceptionStrategy();
        }

        [Given(@"a service consumer that will throw an '(.*)' exception when the service channel is closing and have a corresponding exception behaviour strategy")]
        public void GivenAServiceConsumerThatWillThrowAnExceptionWhenTheServiceChannelIsClosingAndHaveACorrespondingExceptionBehaviourStrategy(string typeOfException)
        {
            InitialiseFactoryMethodsAndMockServiceAndDisposableProxyWithEvents();

            MakeExceptionToThrowOnClosing(typeOfException);

            CreateAndSetupMockExceptionStrategyForClosing(typeOfException);
            CreateServiceConsumerWithMocksAndExceptionStrategy();
        }

        [Given(@"a service consumer that will throw an exception while aborting, and has a corresponding exception behaviour")]
        public void GivenAServiceConsumerThatWillThrowAnExceptionWhileAbortingAndHaveACorrespondingExceptionBehaviourStrategy()
        {
            InitialiseFactoryMethodsAndMockServiceAndDisposableProxyWithEvents();

            SetupMockServiceToSayStateIsFault();
            MakeExceptionToThrowOnAborting();

            CreateAndSetupMockExceptionStrategyForAborting();
            CreateServiceConsumerWithMocksAndExceptionStrategy();
        }

        [Given(@"a service consumer is created for testing garbage collection, and has a destructor exception behaviour")]
        public void GivenAServiceConsumerIsCreatedForTestingGarbageCollectionAndHaveADestructorExceptionBehaviour()
        {
            CreateAndSetupMockExceptionStrategyForDestructor();

            var consumer = new ServiceConsumer<IFakeService>(CreateRetryEnabledDisposableServiceChannelProxy());
            consumer.ExceptionBehaviourStrategy = this.mockExceptionStrategy.Object;

            consumer.Disposed += consumer_Disposed;
            this.weakRefServiceConsumer = new WeakReference<ServiceConsumer<IFakeService>>(consumer, true);
        }

        ////[Given(@"a factory method to create a new service channel with the iDesign InProcFactory")]
        ////public void GivenAFactoryMethodToCreateANewServiceChannelWithTheIDesignInProcFactory()
        ////{
        ////    this.serviceChannelFactoryMethod = () =>
        ////    {
        ////        serviceChannelCreatedCount++;
        ////        Logger.Log("Created new Service Channel");

        ////        return (ICommunicationObject) ServiceModelEx.InProcFactory.CreateInstance<FakeServiceImpl, IFakeService>();
        ////    };
        ////}

        #endregion

        #region When

        [Given(@"the service consumer is created with the factory method")]
        [When(@"the service consumer is created with the factory method")]
        public void WhenTheServiceConsumerIsCreatedWithTheFactoryMethod()
        {
            this.serviceConsumer = ServiceConsumerFactory.Create<IFakeService>(this.serviceChannelFactoryMethod);
        }

        [When(@"the service consumer is created with the Unity IoC container")]
        public void WhenTheServiceConsumerIsCreatedWithTheUnityIoCContainer()
        {
            this.controller = this.unityContainer.Resolve<ConstructorInjectedController>();
        }

        [When(@"the service consumer is created with the Autofac IoC container")]
        public void WhenTheServiceConsumerIsCreatedWithTheAutofacIoCContainer()
        {
            this.controller = this.autofacContainer.Resolve<ConstructorInjectedController>();
        }

        [When(@"the service consumer is created with the Simple Injector IoC container")]
        public void WhenTheServiceConsumerIsCreatedWithTheSimpleInjectorIoCContainer()
        {
            this.controller = this.simpleContainer.GetInstance<ConstructorInjectedController>();
        }

        [When(@"the service channel is closed")]
        public void WhenTheServiceChannelIsClosed()
        {
            this.serviceConsumer.Close();
        }

        [When(@"there is an attempt to close the service channel")]
        public void WhenThereIsAnAttemptToCloseTheServiceChannel()
        {
            Try(() =>
            {
                this.serviceConsumer.Close();
            });
        }

        [When(@"the service channel is accessed")]
        public void WhenTheServiceChannelIsAccessed()
        {
            var ops = this.serviceConsumer.Operations.ToString();
        }

        [When(@"an operation is invoked synchronously")]
        public void WhenAnOperationIsInvokedSynchronously()
        {
            expectedResult = 3;
            actualResult = this.serviceConsumer.Operations.AddIntegers(1, 2);
        }

        [When(@"an operation is invoked asynchronously")]
        public async void WhenAnOperationIsInvokedAsynchronously()
        {
            expectedResult = 3;
            actualResult = await this.serviceConsumer.Operations.AddTwoIntegersAsync(1, 2);
        }

        [When(@"the operation is called via the Consume method and a '(.*)' exception occurs")]
        public void WhenTheOperationIsCalledViaTheConsumeMethodAndAExceptionOccurs(string typeOfException)
        {
            var result = this.serviceConsumer.Consume(op => op.AddIntegers(1, 2));

            if (result.HasNoException)
            {
                this.actualResult = result.Value;
                LogAssert.Fail(typeOfException + " exception did not occur");
            }

            this.ActualException = result.Exception;

            AssertExceptionWasThrown(typeOfException);
        }

        [When(@"the operation is called and a '(.*)' exception occurs")]
        [When(@"the operation is called via the Operations property and a '(.*)' exception occurs")]
        public void WhenTheOperationIsCalledViaTheOperationsPropertyAndAExceptionOccurs(string typeOfException)
        {
            base.Try(() =>
            {
                this.actualResult = this.serviceConsumer.Operations.AddIntegers(1, 2);
                LogAssert.Fail(typeOfException + " exception did not occur");
            });

            AssertExceptionWasThrown(typeOfException);
        }

        [When(@"the operation is invoked and the thread is aborted")]
        public void WhenTheOperationIsInvokedAndTheThreadIsAborted()
        {
            Try(() =>
            {
                this.actualResult = Task.Factory.StartNew(() =>
                {
                    return this.serviceConsumer.Operations.AddIntegers(1, 2);
                })
                .GetAwaiter().GetResult();
            });
        }

        [When(@"the operation is invoked and the exception occurs")]
        public void WhenTheOperationIsInvokedAndAnyExceptionOccurs()
        {
            Try(() =>
            {
                this.actualResult = this.serviceConsumer.Operations.AddIntegers(1, 2);
            });
        }

        [When(@"the service channel faults")]
        public void WhenTheServiceChannelFaults()
        {
            this.mockService.As<ICommunicationObject>().Raise(m => m.Faulted += null, EventArgs.Empty);
        }

        [When(@"the service channel is closed and the thread is aborted")]
        public void WhenTheServiceChannelIsClosedAndTheThreadIsAborted()
        {
            Try(
            () =>
            {
                Task.Factory.StartNew(() =>
                {
                    this.serviceConsumer.Close();
                })
                .GetAwaiter().GetResult();
            });
        }

        [When(@"a service consumer is disposed at the end of a using block")]
        public void WhenAServiceConsumerIsDisposedAtTheEndOfAUsingBlock()
        {
            using (this.serviceConsumer)
            {
            }
        }

        [When(@"a service consumer is finalised by the garbage collector")]
        [When(@"a service consumer is finalised by the garbage collector and an exception occurs in the destructor of the service channel")]
        [When(@"garbage collection is performed")]
        public void WhenGarbageCollectionIsPerformed()
        {
            GC.Collect(2, GCCollectionMode.Forced, true);
            GC.WaitForPendingFinalizers();
        }

        [When(@"many service consumers are created and used in a tight loop and go out of scope immediately over '(.*)' seconds")]
        public void WhenManyServiceConsumersAreCreatedInATightLoopAndGoOutOfScopeImmediatelyOverSeconds(int seconds)
        {
            WhenGarbageCollectionIsPerformed();

            this.memoryBefore = GC.GetTotalMemory(true);
            Console.WriteLine("Total memory before: " + memoryBefore);

            var startTime = DateTime.Now;
            var endTime = startTime.AddSeconds(seconds);

            while (DateTime.Now < endTime)
            {
                var consumer = ServiceConsumerFactory.Create<IFakeService>(() => new FakeServiceClient(), (IServiceConsumerExceptionBehaviourStrategy)null);
                consumer.Operations.AddIntegers(1, 2);

                Thread.Sleep(1);
            }
        }

        #endregion

        #region Then

        [Given(@"the service consumer used the factory method to create a new service channel")]
        [Then(@"the service consumer used the factory method to create a new service channel")]
        public void ThenTheServiceConsumerUsedTheFactoryMethodToCreateANewServiceChannel()
        {
            LogAssert.AreEqual("Factory method invoke count", 1, this.serviceChannelCreatedCount);
        }

        [Then(@"the service consumer used the factory method to create a new service channel again")]
        public void ThenTheServiceConsumerUsedTheFactoryMethodToCreateANewServiceChannelAgain()
        {
            LogAssert.AreEqual("Factory method invoke count", 2, this.serviceChannelCreatedCount);
        }

        [Then(@"the service consumer was created successfully")]
        public void ThenTheServiceConsumerWasCreatedSuccessfully()
        {
            LogAssert.IsNotNull("Controller", this.controller);
            LogAssert.IsNotNull("Constructor injected service instance", this.controller.FakeService);
        }

        [Then(@"the operation was invoked")]
        public void ThenTheOperationWasInvoked()
        {
            LogAssert.AreEqual("Operation was invoked", expectedResult, actualResult);
        }

        [Then(@"the service channel was not closed or aborted, and remains open and usable")]
        public void ThenTheServiceChannelWasNotClosedOrAbortedAndRemainsOpenAndUsable()
        {
            LogAssert.AreEqual("Created count", 1, this.proxyCreatedCount);
            LogAssert.AreEqual("Disposed count", 0, this.proxyDisposedCount);
            LogAssert.AreEqual("Closing count", 0, this.proxyClosingCount);
            LogAssert.AreEqual("Closed count", 0, this.proxyClosedCount);
            LogAssert.AreEqual("Aborting count", 0, this.proxyAbortingCount);
            LogAssert.AreEqual("Aborted count", 0, this.proxyAbortedCount);
        }

        [Then(@"the service channel was closed and disposed")]
        public void ThenTheServiceChannelWasClosedAndDisposed()
        {
            LogAssert.AreEqual("Created count", 1, this.proxyCreatedCount);
            LogAssert.AreEqual("Disposed count", 1, this.proxyDisposedCount);
            LogAssert.AreEqual("Closing count", 1, this.proxyClosingCount);
            LogAssert.AreEqual("Closed count", 1, this.proxyClosedCount);
            LogAssert.AreEqual("Aborting count", 0, this.proxyAbortingCount);
            LogAssert.AreEqual("Aborted count", 0, this.proxyAbortedCount);
        }

        [Then(@"the service channel was aborted and disposed")]
        public void ThenTheServiceChannelWasAbortedAndDisposed()
        {
            LogAssert.AreEqual("Created count", 1, this.proxyCreatedCount);
            LogAssert.AreEqual("Disposed count", 1, this.proxyDisposedCount);
            LogAssert.AreEqual("Closing count", 0, this.proxyClosingCount);
            LogAssert.AreEqual("Closed count", 0, this.proxyClosedCount);
            LogAssert.AreEqual("Aborting count", 1, this.proxyAbortingCount);
            LogAssert.AreEqual("Aborted count", 1, this.proxyAbortedCount);
        }

        [Then(@"the service channel started closing, then aborted and disposed")]
        public void ThenTheServiceChannelStartedClosingThenAbortedAndDisposed()
        {
            LogAssert.AreEqual("Created count", 1, this.proxyCreatedCount);
            LogAssert.AreEqual("Disposed count", 1, this.proxyDisposedCount);
            LogAssert.AreEqual("Closing count", 1, this.proxyClosingCount);
            LogAssert.AreEqual("Closed count", 0, this.proxyClosedCount);
            LogAssert.AreEqual("Aborting count", 1, this.proxyAbortingCount);
            LogAssert.AreEqual("Aborted count", 1, this.proxyAbortedCount);
        }

        [Then(@"the operation was invoked multiple times due to the retry policy")]
        public void ThenTheOperationWasInvokedMultipleTimesDueToTheRetryPolicy()
        {
            LogAssert.AreEqual("Operation call count", 2, this.operationCallCount);
        }

        [Then(@"the exception behaviour was invoked")]
        public void ThenTheExceptionBehaviourWasInvoked()
        {
            base.MyMockRepository.Verify();
        }

        [Then(@"a '(.*)' exception bubbled up to the calling code")]
        public void ThenAExceptionBubbledUpToTheCallingCode(string typeOfException)
        {
            AssertExceptionWasThrown(typeOfException);
        }

        [Then(@"the exception is not bubbled up to the calling code")]
        public void ThenTheExceptionIsNotBubbledUpToTheCallingCode()
        {
            AssertNoExceptionOccurred();
        }

        [Then(@"there is no significant amount of memory loss")]
        public void ThenThereIsNoSignificantAmountOfMemoryLoss()
        {
            long memoryAfter = GC.GetTotalMemory(true);
            Console.WriteLine("Total memory after: " + memoryAfter);

            Assert.IsTrue(memoryAfter - this.memoryBefore < 450*1024, "allowed variance was ~450kb - primarily for System.Configuration and testing libraries");
        }

        [Then(@"the service consumer is explicitly closed")]
        public void ThenTheServiceConsumerIsExplicitlyClosed()
        {
            this.serviceConsumer.Close();
        }

        #endregion

        #region Private Methods

        private void InitialiseFactoryMethodsAndMockServiceAndDisposableProxyWithEvents()
        {
            this.mockService = MyMockRepository.Create<IFakeService>();

            this.mockService.Setup(m => m.AddIntegers(It.IsAny<int>(), It.IsAny<int>()))
                .Callback(() =>
                    {
                        this.operationCallCount++;
                        Logger.Log("************** AddIntegers(,) was called");
                    });

            this.serviceChannelFactoryMethod = () =>
            {
                serviceChannelCreatedCount++;
                Logger.Log("Created new Service Channel");

                return this.mockService.As<ICommunicationObject>().Object;
            };

            this.disposableServiceChannelProxyFactoryMethod = (ICommunicationObject serviceChannel) =>
            {
                this.disposableServiceChannelProxy = CreateDisposableServiceChannelProxy(serviceChannel);
                return this.disposableServiceChannelProxy;
            };
        }

        private RetryEnabledDisposableServiceChannelProxy<IFakeService> CreateRetryEnabledDisposableServiceChannelProxy()
        {
            return new RetryEnabledDisposableServiceChannelProxy<IFakeService>(
                    (channel) => CreateDisposableServiceChannelProxy(channel),
                    () => new FakeServiceClient(),
                    null);
        }

        private DisposableServiceChannelProxy<IFakeService> CreateDisposableServiceChannelProxy(ICommunicationObject serviceChannel)
        {
            var dsp = new DisposableServiceChannelProxy<IFakeService>(serviceChannel);
            dsp.ClosingChannelEvent.Subscribe(disposableServiceChannelProxy_ClosingChannel);
            dsp.ClosedChannelEvent.Subscribe(disposableServiceChannelProxy_ClosedChannel);
            dsp.AbortingChannelEvent.Subscribe(disposableServiceChannelProxy_AbortingChannel);
            dsp.AbortedChannelEvent.Subscribe(disposableServiceChannelProxy_AbortedChannel);
            dsp.DisposedChannelEvent.Subscribe(disposableServiceChannelProxy_DisposedChannel);

            proxyCreatedCount++;
            Logger.Log("Created new DisposableServiceChannelProxy");

            return dsp;
        }

        private void SetupMockServiceToThrowException(string typeOfException)
        {
            var mockSetup = this.mockService.Setup(m => m.AddIntegers(It.IsAny<int>(), It.IsAny<int>()));
            mockSetup.Callback(() =>
                {
                    this.operationCallCount++;
                    Logger.Log("################# AddIntegers(,) was called");
                });

            switch (typeOfException)
            {
                case "fault":
                    mockSetup.Throws<FaultException>().Verifiable();
                    break;

                case "communication":
                    mockSetup.Throws<CommunicationException>().Verifiable();
                    break;

                case "timeout":
                    mockSetup.Throws<TimeoutException>().Verifiable();
                    break;

                case "unexpected":
                    mockSetup.Throws<ApplicationException>().Verifiable();
                    break;

                case "thread abort":
                    mockSetup.Callback(() =>
                    {
                        Thread.CurrentThread.Abort();
                    });
                    break;

                default:
                    LogAssert.Fail("Unknown type of exception '{0}'", typeOfException);
                    break;
            }
        }

        private void SetupMockServiceToSayStateIsFault()
        {
            this.mockService.As<ICommunicationObject>()
                .SetupGet(m => m.State)
                .Returns(CommunicationState.Faulted)
                .Verifiable();
        }

        private void MakeExceptionToThrowOnClosing(string typeOfException)
        {
            string message = "exception thrown on closing";

            switch (typeOfException)
            {
                case "communication":
                    this.exceptionToThrowOnClosing = new CommunicationException(message);
                    break;

                case "timeout":
                    this.exceptionToThrowOnClosing = new TimeoutException(message);
                    break;

                case "unexpected":
                    this.exceptionToThrowOnClosing = new ApplicationException(message);
                    break;

                default:
                    LogAssert.Fail("Unknown type of exception '{0}'", typeOfException);
                    break;
            }
        }

        private void MakeExceptionToThrowOnAborting()
        {
            string message = "exception thrown on aborting";
            this.exceptionToThrowOnAborting = new ApplicationException(message);
        }

        private void CreateServiceConsumerWithMocks()
        {
            var retryProxy = new RetryEnabledDisposableServiceChannelProxy<IFakeService>(
                this.disposableServiceChannelProxyFactoryMethod,
                this.serviceChannelFactoryMethod,
                null);
            this.serviceConsumer = new ServiceConsumer<IFakeService>(retryProxy);
            this.serviceConsumer.ExceptionBehaviourStrategy = new FakeExceptionHandlingStrategy();
        }

        private void CreateServiceConsumerWithMocksAndExceptionStrategy()
        {
            var retryProxy = new RetryEnabledDisposableServiceChannelProxy<IFakeService>(
                this.disposableServiceChannelProxyFactoryMethod,
                this.serviceChannelFactoryMethod,
                null);
            this.serviceConsumer = new ServiceConsumer<IFakeService>(retryProxy);
            this.serviceConsumer.ExceptionBehaviourStrategy = this.mockExceptionStrategy.Object;
        }

        void disposableServiceChannelProxy_DisposedChannel(object sender, EventArgs e)
        {
            this.proxyDisposedCount++;
            Logger.Log("disposableServiceChannelProxy_DisposedChannel");
        }

        void disposableServiceChannelProxy_AbortedChannel(object sender, EventArgs e)
        {
            this.proxyAbortedCount++;
            Logger.Log("disposableServiceChannelProxy_AbortedChannel");
        }

        void disposableServiceChannelProxy_AbortingChannel(object sender, EventArgs e)
        {
            this.proxyAbortingCount++;
            Logger.Log("disposableServiceChannelProxy_AbortingChannel");

            if (this.exceptionToThrowOnAborting != null)
            {
                throw this.exceptionToThrowOnAborting;
            }
        }

        void disposableServiceChannelProxy_ClosedChannel(object sender, EventArgs e)
        {
            this.proxyClosedCount++;
            Logger.Log("disposableServiceChannelProxy_ClosedChannel");
        }

        void disposableServiceChannelProxy_ClosingChannel(object sender, EventArgs e)
        {
            this.proxyClosingCount++;
            Logger.Log("disposableServiceChannelProxy_ClosingChannel");

            if (this.exceptionToThrowOnClosing != null)
            {
                throw this.exceptionToThrowOnClosing;
            }

            if (this.abortThreadOnClosing)
            {
                Thread.CurrentThread.Abort();
            }
        }

        private void CreateAndSetupMockExceptionStrategy(string typeOfException)
        {
            this.mockExceptionStrategy = MyMockRepository.Create<IServiceConsumerExceptionBehaviourStrategy>();

            switch (typeOfException)
            {
                case "fault":
                    this.mockExceptionStrategy
                        .Setup(m => m.PerformFaultExceptionBehaviour(It.IsAny<FaultException>()))
                        .Callback(
                            (FaultException fe) =>
                            {
                                Logger.Log("Fault exception handler here");
                            })
                        .Verifiable();
                    break;

                case "communication":
                    this.mockExceptionStrategy
                        .Setup(m => m.PerformCommunicationExceptionBehaviour(It.IsAny<CommunicationException>()))
                        .Callback(
                            (CommunicationException fe) =>
                            {
                                Logger.Log("Communication exception handler here");
                            })
                        .Verifiable();
                    break;

                case "timeout":
                    this.mockExceptionStrategy
                        .Setup(m => m.PerformTimeoutExceptionBehaviour(It.IsAny<TimeoutException>()))
                        .Callback(
                            (TimeoutException fe) =>
                            {
                                Logger.Log("Timeout exception handler here");
                            })
                        .Verifiable();
                    break;

                case "unexpected":
                    this.mockExceptionStrategy
                        .Setup(m => m.PerformUnexpectedExceptionBehaviour(It.IsAny<Exception>()))
                        .Callback(
                            (Exception fe) =>
                            {
                                Logger.Log("Unexpected exception handler here");
                            })
                        .Verifiable();
                    break;

                case "retry":
                    this.mockExceptionStrategy
                        .Setup(m => m.PerformRetryPolicyAttemptExceptionBehaviour(It.IsAny<Exception>(), It.IsAny<int>()))
                        .Callback(
                            (Exception re, int attempt) =>
                            {
                                Logger.Log("Retry policy attempt exception handler here - attempt: " + attempt);
                            })
                        .Verifiable();
                    break;

                default:
                    LogAssert.Fail("Unknown type of exception '{0}'", typeOfException);
                    break;
            }
        }

        private void CreateAndSetupMockExceptionStrategyForClosing(string typeOfException)
        {
            this.mockExceptionStrategy = MyMockRepository.Create<IServiceConsumerExceptionBehaviourStrategy>();

            switch (typeOfException)
            {
                case "communication":
                    this.mockExceptionStrategy
                        .Setup(m => m.PerformCloseCommunicationExceptionBehaviour(It.IsAny<CommunicationException>()))
                        .Callback(
                            (CommunicationException fe) =>
                            {
                                Logger.Log("Close Communication exception handler here");
                            })
                        .Verifiable();
                    break;

                case "timeout":
                    this.mockExceptionStrategy
                        .Setup(m => m.PerformCloseTimeoutExceptionBehaviour(It.IsAny<TimeoutException>()))
                        .Callback(
                            (TimeoutException fe) =>
                            {
                                Logger.Log("Close Timeout exception handler here");
                            })
                        .Verifiable();
                    break;

                case "unexpected":
                    this.mockExceptionStrategy
                        .Setup(m => m.PerformCloseUnexpectedExceptionBehaviour(It.IsAny<Exception>()))
                        .Callback(
                            (Exception fe) =>
                            {
                                Logger.Log("Close Unexpected exception handler here");
                            })
                        .Verifiable();
                    break;

                default:
                    LogAssert.Fail("Unknown type of exception '{0}'", typeOfException);
                    break;
            }
        }

        private void CreateAndSetupMockExceptionStrategyForAborting()
        {
            this.mockExceptionStrategy = MyMockRepository.Create<IServiceConsumerExceptionBehaviourStrategy>();

            this.mockExceptionStrategy
                .Setup(m => m.PerformAbortExceptionBehaviour(It.IsAny<Exception>()))
                .Callback(() => Logger.Log("Abort exception handler here"))
                .Verifiable();
        }

        private void CreateAndSetupMockExceptionStrategyForDestructor()
        {
            this.mockExceptionStrategy = MyMockRepository.Create<IServiceConsumerExceptionBehaviourStrategy>();

            this.mockExceptionStrategy
                .Setup(m => m.PerformDestructorExceptionBehaviour(It.IsAny<Exception>()))
                .Callback(() => Logger.Log("Destructor exception handler here"))
                .Verifiable();
        }

        void consumer_Disposed(object sender, EventArgs e)
        {
            Logger.Log("ServiceConsumer.Disposed event - about to throw application exception");
            throw new ApplicationException("Thrown in the ServiceConsumer disposed event.");
        }

        private void AssertExceptionWasThrown(string typeOfException)
        {
            LogAssert.IsNotNull("Actual exception exists", this.ActualException);

            bool isExpected = false;

            switch (typeOfException)
            {
                case "fault":
                    if (this.ActualException is FaultException)
                    {
                        isExpected = true;
                    }
                    break;

                case "communication":
                    if (this.ActualException is CommunicationException)
                    {
                        isExpected = true;
                    }
                    break;

                case "timeout":
                    if (this.ActualException is TimeoutException)
                    {
                        isExpected = true;
                    }
                    break;

                case "unexpected":
                    if (this.ActualException is ApplicationException)
                    {
                        isExpected = true;
                    }
                    break;

                case "thread abort":
                    if (this.ActualException is System.Threading.ThreadAbortException)
                    {
                        isExpected = true;
                    }
                    break;

                default:
                    LogAssert.Fail("Unknown type of exception '{0}'", typeOfException);
                    break;
            }

            if (isExpected)
            {
                Logger.Log("Expected '{0}' exception occurred - {1}", typeOfException, this.ActualException.ToString());
            }
            else
            {
                LogAssert.Fail("An exception occurred that was not expected: " + this.ActualException.ToString());
            }
        }

        #endregion
    }
}
