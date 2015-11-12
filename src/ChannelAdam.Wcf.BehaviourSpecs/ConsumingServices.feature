Feature: Consuming Services

### Creating the service consumer

@UnitTest
Scenario: ConsumingServices - 001 - Positive - Should use a factory method to create a new service channel
Given a factory method to create a new service channel
When the service consumer is created with the factory method
Then the service consumer used the factory method to create a new service channel

@UnitTest
Scenario: ConsumingServices - 002 - Positive - Should automatically create a new service channel as needed when the previous one has been closed
Given a factory method to create a new service channel
When the service consumer is created with the factory method
And the service channel is closed
And the service channel is accessed
Then the service consumer used the factory method to create a new service channel again


### Using an IoC container to create the service consumer

@UnitTest
Scenario: ConsumingServices - 005 - Positive - Should be created successfully from Unity IoC
Given a configured Unity IoC container
When the service consumer is created with the Unity IoC container
Then the service consumer was created successfully

@UnitTest
Scenario: ConsumingServices - 006 - Positive - Should be created successfully from AutoFac IoC
Given a configured Autofac IoC container
When the service consumer is created with the Autofac IoC container
Then the service consumer was created successfully

@UnitTest
Scenario: ConsumingServices - 007 - Positive - Should be created successfully from Simple Injector IoC
Given a configured Simple Injector IoC container
When the service consumer is created with the Simple Injector IoC container
Then the service consumer was created successfully


### Invoking the service operation

@UnitTest
Scenario: ConsumingServices - 010 - Positive - Should successfully synchronously invoke a service operation on a service channel proxy
Given a factory method to create a new service channel
When the service consumer is created with the factory method
And an operation is invoked synchronously
Then the operation was invoked

@UnitTest
Scenario: ConsumingServices - 011 - Positive - Should successfully asynchronously invoke a service operation on a service channel proxy
Given a factory method to create a new service channel
When the service consumer is created with the factory method
And an operation is invoked asynchronously
Then the operation was invoked

@UnitTest
Scenario: ConsumingServices - 015 - Positive - Should by default keep the service channel open and reusable when a FaultException occurs while invoking a service operation
Given a service consumer is created with an operation that throws a 'fault' exception
When the operation is called via the Consume method and a 'fault' exception occurs
Then the service channel was not closed or aborted, and remains open and usable

### Retry Policy

@UnitTest
Scenario: ConsumingServices - 016 - Positive - Should use the default retry policy specified on the service consumer to perform retries with the Call method
Given a service consumer is created with an operation that throws a 'communication' exception
And the service consumer has a default retry policy
When the operation is called via the Consume method and a 'communication' exception occurs
Then the operation was invoked multiple times due to the retry policy
Then the exception behaviour was invoked

@UnitTest
Scenario: ConsumingServices - 017 - Positive - Should use the default retry policy specified on the service consumer to perform retries with the Operations property
Given a service consumer is created with an operation that throws a 'communication' exception
And the service consumer has a default retry policy with a retry policy attempt exception behaviour
When the operation is called via the Operations property and a 'communication' exception occurs
Then the operation was invoked multiple times due to the retry policy
Then the exception behaviour was invoked


### Closing the channel ###

@UnitTest
Scenario: ConsumingServices - 020 - Negative - Should by default close the service channel when a CommunicationException occurs while invoking a service operation
Given a service consumer is created with an operation that throws a 'communication' exception
When the operation is called via the Consume method and a 'communication' exception occurs
Then the service channel was closed and disposed

@UnitTest
Scenario: ConsumingServices - 021 - Negative - Should by default close the service channel when a TimeoutException occurs while invoking a service operation
Given a service consumer is created with an operation that throws a 'timeout' exception
When the operation is called via the Consume method and a 'timeout' exception occurs
Then the service channel was closed and disposed

@UnitTest
Scenario: ConsumingServices - 022 - Negative - Should by default close the service channel when an unexpected Exception occurs while invoking a service operation
Given a service consumer is created with an operation that throws a 'unexpected' exception
When the operation is called via the Consume method and a 'unexpected' exception occurs
Then the service channel was closed and disposed

Scenario: ConsumingServices - 023 - Negative - Should by default close the service channel when a ThreadAbortedException occurs while invoking a service operation
Given a service consumer is created with an operation aborts the thread
When the operation is invoked and the thread is aborted
Then the service channel was closed and disposed

Scenario: ConsumingServices - 024 - Positive - Should have a customisable trigger strategy for when to close the service channel when any exception occurs while invoking a service operation
Given a service consumer is created with an operation that throws a 'communication' exception
And the service consumer has a custom service channel close trigger strategy that does not ever trigger the closing of the service channnel
When the operation is called via the Consume method and a 'communication' exception occurs
Then the service channel was not closed or aborted, and remains open and usable



### Aborting the channel ###

@UnitTest
Scenario: ConsumingServices - 030 - Negative - Should abort the service channel whenever it faults
Given a service consumer with a channel that will fault
When the service channel faults
Then the service channel was aborted and disposed

@UnitTest
Scenario: ConsumingServices - 031 - Negative - Should abort the service channel when there is an exception closing the service channel
Given a service consumer that will throw an 'unexpected' exception when the service channel is closing
When there is an attempt to close the service channel
Then the service channel started closing, then aborted and disposed

Scenario: ConsumingServices - 032 - Negative - Should abort the service channel when a ThreadAbortedException occurs while closing the service channel
Given a service consumer that will abort the thread when the service channel is closing
When the service channel is closed and the thread is aborted
Then the service channel started closing, then aborted and disposed


### Using Disposal & Finalizer/Destructor ###

@UnitTest
Scenario: ConsumingServices - 040 - Positive - Should close the service channel when the scope of a Using block for a service consumer finishes
Given a service consumer is created
When a service consumer is disposed at the end of a using block
Then the service channel was closed and disposed

@UnitTest
Scenario: ConsumingServices - 041 - Positive - Should close the service channel when the Garbage Collector finalises the service consumer
Given a service consumer is created for testing garbage collection
When a service consumer is finalised by the garbage collector
Then the service channel was closed and disposed


### Exception behaviours

@UnitTest
Scenario Outline: ConsumingServices - 051 - Negative - Should perform the corresponding exception behaviour action when there is a <Xyz> Exception - while invoking a service operation
Given a service consumer is created with an operation that throws a '<Type of Exception>' exception and has a corresponding exception behaviour strategy
When the operation is called and a '<Type of Exception>' exception occurs
Then the exception behaviour was invoked
Examples:
| Type of Exception |
| fault             |
| communication     |
| timeout           |
| unexpected        |

@UnitTest
Scenario Outline: ConsumingServices - 052 - Negative - Should perform the corresponding exception behaviour action when there is a <Xyz> Exception - while closing a service channel
Given a service consumer that will throw an '<Type of Exception>' exception when the service channel is closing and have a corresponding exception behaviour strategy
When there is an attempt to close the service channel
Then the exception behaviour was invoked
Examples:
| Type of Exception |
| communication     |
| timeout           |
| unexpected        |

@UnitTest
Scenario: ConsumingServices - 053 - Negative - Should perform the corresponding exception behaviour action when there is an exception while aborting a service channel
Given a service consumer that will throw an exception while aborting, and has a corresponding exception behaviour
When the service channel faults
Then the exception behaviour was invoked

@UnitTest
Scenario: ConsumingServices - 054 - Negative - Should perform the corresponding exception behaviour action when there is an exception while the Garbage Collector is finalising the service consumer
Given a service consumer is created for testing garbage collection, and has a destructor exception behaviour
When a service consumer is finalised by the garbage collector and an exception occurs in the destructor of the service channel
Then the exception behaviour was invoked


### Exceptions bubbling up to calling code

@UnitTest
Scenario Outline: ConsumingServices - 060 - Negative - Should bubble an <Xyz> exception back to the caller when there is an exception while invoking a service operation
Given a service consumer is created with an operation that throws a '<Type of Exception>' exception
When the operation is invoked and the exception occurs
Then a '<Type of Exception>' exception bubbled up to the calling code
Examples:
| Type of Exception |
| fault             |
| communication     |
| timeout           |
| unexpected        |

@UnitTest
Scenario Outline: ConsumingServices - 061 -  Negative - Should not bubble an exception back to the caller when there is a <Xyz> exception while closing a service channel
Given a service consumer that will throw an '<Type of Exception>' exception when the service channel is closing
When there is an attempt to close the service channel
Then the exception is not bubbled up to the calling code
Examples:
| Type of Exception |
| communication     |
| timeout           |
| unexpected        |

@UnitTest
Scenario: ConsumingServices - 062 - Negative - Should not bubble an exception back to the caller when there is an exception while aborting a service channel
Given a service consumer that will throw an exception when the service channel is aborting
When there is an attempt to close the service channel
Then the exception is not bubbled up to the calling code


### Memory Leak Testing

Scenario: ConsumingServices - 070 - Positive - Should not leak memory
When many service consumers are created and used in a tight loop and go out of scope immediately over '15' seconds
And garbage collection is performed
Then there is no significant amount of memory loss



### External library tests

#@UnitTest
#Scenario: ConsumingServices - 100 - Postive - Should work with iDesign InProcFactory
#Given a factory method to create a new service channel with the iDesign InProcFactory
#And the service consumer is created with the factory method
#And the service consumer used the factory method to create a new service channel
#When an operation is invoked synchronously
#Then the operation was invoked
