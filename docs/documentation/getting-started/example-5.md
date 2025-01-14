# Simple synchronized FSM with Events
## Overview  
Events are a great way to trigger global actions in your game and they can be used synchronize non-entity dependent information to the clients.  
In this example, we will use the power of DuneNet's events to create a simple Finite State Machine (FSM) to keep the game's round state synchronized between the server and the clients.  

As always, we will first provide the whole code for the example and then we will explain the important aspects of it.

*Assets/Scripts/Shared/RoundState.cs:*  
```csharp
namespace Shared
{
    public enum RoundState
    {
        OutRound,
        InRound
    }
}
```  

*Assets/Scripts/Server/Modules/RoundModule.cs:*  
```csharp
using DuneNet.Server;
using DuneNet.Shared;
using DuneNet.Shared.Events;
using Shared;

namespace Server.Modules
{
    public class RoundModule : DuneNet.Server.DuneModule
    {
        private RoundState _currentState = RoundState.OutRound;
        
        public RoundState GetCurrentState()
        {
            return _currentState;
        }
        
        private void NextState()
        {
            _currentState = _currentState == RoundState.OutRound ? RoundState.InRound : RoundState.OutRound;
            EventArguments args = new EventArguments();
            args.SetVariable("state", _currentState);
            DuneServer.EventController.InvokeEvent("OnChangeState", args, true);
        }
        
        protected override void OnNetReady(DuneConnection conn)
        {
            DuneServer.NetworkController.ForceServerReadiness(conn);
            if (DuneServer.NetworkController.GetConnectionAmount() > 1 && _currentState == RoundState.OutRound)
            {
                NextState();
            }
        }

        protected override void OnNetDisconnected(DuneConnection conn)
        {
            if (DuneServer.NetworkController.GetConnectionAmount() < 2)
            {
                NextState();
            }
        }
    }
}
```

*Assets/Scripts/Client/Entities/ClientStateController.cs:*  
```csharp
using System;
using DuneNet.Client;
using DuneNet.Shared.Enums;
using DuneNet.Shared.Events;
using Shared;
using UnityEngine;

namespace Client
{
    public class ClientStateController : DuneNet.Client.DuneMonoBehaviour
    {
        private RoundState _currentState;

        [EventReg("OnChangeState", NetworkContext.Client, true)]
        private void OnChangeState(EventArguments args)
        {
            _currentState = args.GetVariable<RoundState>("state");
        }

        private void OnGUI()
        {
            GUI.Label(new Rect(10, 10, 200, 20), "Current State: " + Enum.GetName(typeof(RoundState), _currentState));
        }
    }
}
```  

## RoundState.cs  
The first thing we need to do is define an enum that will represent all the possible round states.  
```csharp
namespace Shared
{
    public enum RoundState
    {
        OutRound,
        InRound
    }
}
```  
Since this enum will be used by both the server and the client, it is a good idea to declare it inside a shared namespace that both contexts can access.

## RoundModule.cs  
This module takes care of keeping track of the serverside round state and also provides a few methods to work with it.  
First of all, we define a new class that derives from [``DuneModule``](../../api/DuneNet.Server.DuneModule.html):  
```csharp
public class RoundModule : DuneNet.Server.DuneModule
```  
The [``DuneModule``](../../api/DuneNet.Server.DuneModule.html) class provides many overridable methods that we can use to extend the functionality of DuneNet beyond what the main controllers provide.  
In our case, we want to override the  ``OnNetReady(DuneConnection conn)`` and ``OnNetDisconnected(DuneConnection conn)`` methods. The ``OnNetReady(DuneConnection conn)`` method is called when a client is set ready, while the ``OnNetReady(DuneConnection conn)`` method is called when a client disconnects from the server.  
```csharp
protected override void OnNetReady(DuneConnection conn)
{
    DuneServer.NetworkController.ForceServerReadiness(conn);
    if (DuneServer.NetworkController.GetConnectionAmount() > 1 && _currentState == RoundState.OutRound)
    {
        NextState();
    }
}
```  
First, we use the ``DuneServer.NetworkController.ForceServerReadiness()`` method to tell DuneNet that from now on, the readiness of the provided connection will be handled by the server. This prevents the affected client from controlling its readiness state, which is useful to prevent the client from creating unintended behaviour on the server by, for example, changing rapidly between the ready and not ready states.  
After that, we check that the amount of connections is at least two by using the ``DuneServer.NetworkController.GetConnectionAmount()`` method and we also check that the current state is ``RoundState.OutRound``. If the conditions are met, we jump to the next state.  
```csharp
private void NextState()
{
    _currentState = _currentState == RoundState.OutRound ? RoundState.InRound : RoundState.OutRound;
    EventArguments args = new EventArguments();
    args.SetVariable("state", _currentState);
    DuneServer.EventController.InvokeEvent("OnChangeState", args, true);
}
```  
In this helper method, first we set the value of ``_currentState`` to the next state, which in our case will be the state we currently are not since there are only two possible states.  
Then, we create a new instance of [``EventArguments``](../../api/DuneNet.Shared.Events.EventArguments.html) and we set the variable ``state`` inside it to the value of ``_currentState``. The ``EventArguments`` class takes care of all the serialization and sanity checking required to propagate all the information of the event.  
Finally, we invoke the event on both the client and the server by using the overload of ``DuneServer.EventController.InvokeEvent()`` that takes the name of the event to invoke, a set of event arguments and a boolean value representing whether the event should be invoked on both the server and all authenticated clients.  
```csharp
protected override void OnNetDisconnected(DuneConnection conn)
{
    if (DuneServer.NetworkController.GetConnectionAmount() < 2)
    {
        NextState();
    }
}
```  
Here, we make sure to return to the base state by invoking the ``NextState()`` method if a client disconnects and the amount of remaining clients is less than two.  

As a last step, we just need to register the module we have created as in previous examples.  
```csharp
DuneServer.Use(new RoundModule());
```

## ClientStateController.cs  
This class takes care of keeping track of the client's round state and also displays a basic GUI label at the top left corner of the screen showing the current state.
First of all, we define a new class that derives from [``DuneMonoBehaviour``](../../api/DuneNet.Client.DuneMonoBehaviour.html):  
```csharp
public class ClientStateController : DuneNet.Client.DuneMonoBehaviour
```  
This makes sure that the ``ClientStateController`` class can listen for events.  

The first thing we need to do is declare a method that DuneNet will invoke when the event we define is invoked. This can be any static method declared anywhere or any instance method that is declared inside a [``DuneBehaviour``](../../api/DuneNet.Client.DuneBehaviour.html) or [``DuneMonoBehaviour``](../../api/DuneNet.Client.DuneMonoBehaviour.html). The method should only have one argument of type [``EventArguments``](../../api/DuneNet.Shared.Events.EventArguments.html).
```csharp
private void OnChangeState(EventArguments args)
{
    _currentState = args.GetVariable<GameState>("state");
}
```  
Here, we simply assign ``_currentState`` the value of the ``state`` Event Variable. As with entity Networked Variables and User Messages, we need to provide the generics type of the variable we are trying to retrieve. This type is known beforehand as it is the type of the variable we set when we invoked the event.  
In order to register the event handler with DuneNet, we attach the ``EventReg`` attribute to the method we want DuneNet to call when the event is invoked.  
```csharp
[EventReg("OnChangeState", NetworkContext.Client, true)]
private void OnChangeState(EventArguments args)
```  
The ``EventReg`` attribute takes three arguments in its constructor (the last one is optional and defaults to false): the name of the event we want to listen for, the context the event is valid for (either ``NetworkContext.Server`` or ``NetworkContext.Client``) and a boolean value representing whether the event should be called by the other Network Context (server -> client and client -> server).  
Events that have the ``networkInvoked`` parameter set to false will only be invoked on the same Network Context they were registered on (server -> server and client -> client), while those that have the ``networkInvoked`` parameter set to true will also be invoked on the other Network Context (server -> client and client -> server). Special attention must be paid to which events are to be invoked over the network and which are not, as a malicious client could abuse wrongly handled networked events to cause unintended behaviours on the server.

Finally, we want to display the current state on the screen so we implement the ``OnGUI()`` method. This is a method inherited from ``MonoBehaviour`` but since [``DuneMonoBehaviour``](../../api/DuneNet.Client.DuneMonoBehaviour.html) works exactly the same as a ``MonoBehaviour`` except for the fact that it can work with DuneNet events, we can easily use this method to display element son the screen.  
```csharp
private void OnGUI()
{
    GUI.Label(new Rect(10, 10, 200, 20), "Current State: " + Enum.GetName(typeof(RoundState), _currentState));
}
```  
Here, we display a label on the top left corner of the screen displaying the name of the round state we currently are in. Since ``RoundState`` is an enum, we can simply use ``Enum.GetName()`` to get the name of the current value of ``_currentState``.

## Using the FSM  
Right now, the round state should properly be synchronized between the server and the clients and it should transition between states when enough players are ready or not enough clients are connected anymore.  
However, this isn't of much utility on its own, so lets use it for something useful.  

In order to accomplish this, we will modify the ``WelcomeEntitySpawner`` module from the [previous example](example-4.md).  
```csharp
using DuneNet.Server;
using DuneNet.Shared;
using DuneNet.Shared.Enums;
using DuneNet.Shared.Events;
using Shared;

namespace Server.Modules
{
    public class WelcomeEntitySpawner : DuneNet.Server.DuneModule
    {
        [EventReg("OnChangeState", NetworkContext.Server)]
        private void OnChangeState(EventArguments args)
        {
            switch (args.GetVariable<RoundState>("state"))
            {
                case RoundState.InRound:
                    DuneServer.NetworkController.ForEachConnection(conn =>
                    {
                        DuneServer.EntityController.SpawnEntity("awesome_sphere", conn);
                    });
                    break;
                case RoundState.OutRound:
                    DuneServer.EntityController.DestroyAllEntities();
                    break;
            }
        }
    }
}
```  
First, we declare a new method and attach a serverside ``EventReg`` attribute to it. This way, the method will be called on the server at the same time as the on the clients (keep latency in consideration for time sensitive operations) when we invoke the event.  
```csharp
[EventReg("OnChangeState", NetworkContext.Server)]
private void OnChangeState(EventArguments args)
```  
After that, if the new state is ``RoundState.InRound``, we use the ``DuneServer.NetworkController.ForEachConnection()`` method to spawn an entity of type [``awesome_sphere``](example-4.md) for each of the clients. Each client will have authority over its own entity.  
If the new state is, instead, ``RoundState.OutRound``, we destroy all the spawned entities from the scene, which will also destroy them for all the ready clients.

## What's next?  
This concludes the *Getting Started* introductory guide to DuneNet. You should now be able to use modules, entities and events to make your game a multiplayer success.  
However, the journey has not ended yet and there is more functionality available for you to use.  
Therefore, we recommend that you take a look at the [Api Reference](../api/index.md) to learn all how to use DuneNet to its maximum.  

All the files that we have created in the course of this guide are available inside the ``Examples`` folder of the Unitypackage you downloaded from the Asset Store.