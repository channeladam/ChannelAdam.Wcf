open ChannelAdam.ServiceModel
open ChannelAdam.Wcf.BehaviourSpecs.TestDoubles

open System.ServiceModel

[<EntryPoint>]

let main argv = 

    let service = ServiceConsumerFactory.Create<IFakeService>(fun () -> new FakeServiceClient() :> ICommunicationObject)

    let x = 1
    let y = 1

    let closure() =
        let result = service.Consume(fun operation -> operation.AddIntegers(x, y))
        result.Value

    let output = closure()

    System.Console.WriteLine(output)

    0 // return an integer exit code
