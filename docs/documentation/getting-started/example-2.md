# Displaying new connections  
## Overview  
In the [last example](example-1.md), we learnt how to create a simple server and client and connect them together. However, we lacked any meaningful way to exploit that connection once it was established.  
In this example, we will learn how to create two simple modules to print a message to the console when a new connection is established. 
This will require two additional classes that derive from ``DuneModule``.

As in the last example, we will first provide the whole code for the example and then we will explain the important aspects of it.

*Assets/Scripts/Server/Modules/ServerGreeter.cs:*  
```csharp
using DuneNet.Server;
using DuneNet.Shared;
using UnityEngine;

namespace Server.Modules
{
    public class ServerGreeter : DuneNet.Server.DuneModule
    {
        protected override void OnNetConnected(DuneConnection conn)
        {
            Debug.Log("A new client connected from: " + conn.address);
        }
    }
}
```  

*Assets/Scripts/Client/Modules/ClientGreeter.cs:*  
```csharp
using DuneNet.Client;
using DuneNet.Shared;
using UnityEngine;

namespace Client.Modules
{
    public class ClientGreeter : DuneNet.Client.DuneModule
    {
        protected override void OnNetConnected(DuneConnection conn)
        {
            Debug.Log("Connected to server at: " + conn.address);
        }

        protected override void OnHandshakeSuccessful(DuneConnection conn)
        {
            DuneClient.NetworkController.SetReady();
        }
    }
}
```  

## ServerGreeter.cs  
This module takes care of printing a message to the console whenever a client connects to the server. The message is printed before any kind of authentication is performed so we have no guarantee the client will stay connected after the authentication step. In future examples, we will dig down into the meaning of this concept but, for now, this message should allow us to verify that clients can, indeed, connect to our server.  

First of all, we define a new class that derives from [``DuneModule``](../../api/DuneNet.Server.DuneModule.html):  
```csharp
public class ServerGreeter : DuneNet.Server.DuneModule
```  
The [``DuneModule``](../../api/DuneNet.Server.DuneModule.html) class provides many overridable methods that we can use to extend the functionality of DuneNet beyond what the main controllers provide.  
In our case, we want to override the  ``OnNetConnected(DuneConnection conn)`` method, which is automatically called when a new connection is established.  
```csharp
protected override void OnNetConnected(DuneConnection conn)
{
    Debug.Log("A new client connected from: " + conn.address);
}
```  
Here, we leverage the ``address`` property of ``DuneConnection`` to print a message to the console showing where the connection came from when a client connects to the server.

## ClientGreeter.cs  
This module takes care of printing a message to the console whenever the client connects to a server. The message is printed before any kind of authentication is performed so we have no guarantee that we will not be kicked by the server for not authenticating properly. In future examples, we will dig down into the meaning of this concept but, for now, this message should allow us to verify the client can, indeed, connect to a server.  
The module also takes care of setting the client connection ready once the server has successfully authenticated the client.  

First of all, we define a new class that derives from [``DuneModule``](../../api/DuneNet.Client.DuneModule.html):  
```csharp
public class ClientGreeter : DuneNet.Client.DuneModule
```  
The [``DuneModule``](../../api/DuneNet.Client.DuneModule.html) class provides many overridable methods that we can use to extend the functionality of DuneNet beyond what the main controllers provide.  
In our case, we want to override the  ``OnNetConnected(DuneConnection conn)`` and ``OnHandshakeSuccessful(DuneConnection conn)`` methods.  
The ``OnNetConnected(DuneConnection conn)`` method is automatically called when a new connection is established, while the ``OnHandshakeSuccessful(DuneConnection conn)`` method is called when the server successfully authenticates the client.  
```csharp
protected override void OnNetConnected(DuneConnection conn)
{
    Debug.Log("Connected to server at: " + conn.address);
}
```  
Here, we leverage the ``address`` property of ``DuneConnection`` to print a message to the console showing the address of the server we just connected to.  
```csharp
protected override void OnHandshakeSuccessful(DuneConnection conn)
{
    DuneClient.NetworkController.SetReady();
}
```  
Here, we set our connection ready once the server has authenticated the client. Setting the connection ready is only possible after the server has authenticated the client and instructs the server that we are ready to receive further information about the game state. This is useful, for example, to be able to change the client scene between the connection to the server and being able to receive game state updates.  

## Registering the modules  
Now that we have created both modules, we need to register them with DuneNet. This easily accomplished by using the ``DunneClient.Use()`` and ``DuneServer.Use()`` methods. Please, do note that only one instance of each module type can be registered at the same time.
  
For this example, we can simply modify the ``ServerInit.cs`` and ``ClientInit.cs`` classes from our [last example](example-1.md).

*Assets/Scripts/Server/ServerInit.cs:*  
```csharp
using DuneNet.Server;
using UnityEngine;
using UnityEngine.Networking;

namespace Server
{
    public class ServerInit : DuneNet.Server.DuneMonoBehaviour
    {
        protected override void OnAwake()
        {
            DuneServer.Use(new ServerGreeter());
        
            DuneServer.NetworkController.Init(new GlobalConfig
            {
                MaxHosts = 20,
                MaxNetSimulatorTimeout = 12000,
                MaxPacketSize = 2000,
                MaxTimerTimeout = 12000,
                MinNetSimulatorTimeout = 1,
                ReactorMaximumReceivedMessages = 8092,
                ReactorMaximumSentMessages = 8092,
                ReactorModel = ReactorModel.SelectReactor,
                ThreadAwakeTimeout = 1,
                ThreadPoolSize = 1
            }, new ConnectionConfig
            {
                AckDelay = 33,
                AcksType = ConnectionAcksType.Acks32,
                AllCostTimeout = 20,
                BandwidthPeakFactor = 2,
                ConnectTimeout = 2000,
                DisconnectTimeout = 2000,
                FragmentSize = 500,
                InitialBandwidth = 0,
                MaxCombinedReliableMessageCount = 10,
                MaxCombinedReliableMessageSize = 100,
                MaxConnectionAttempt = 10,
                MaxSentMessageQueueSize = 896,
                MinUpdateTimeout = 10,
                NetworkDropThreshold = 5,
                OverflowDropThreshold = 5,
                PacketSize = 1440,
                PingTimeout = 500,
                ReducedPingTimeout = 100,
                ResendTimeout = 1200,
                SendDelay = 0,
                SSLCAFilePath = "",
                SSLCertFilePath = "",
                SSLPrivateKeyFilePath = "",
                UdpSocketReceiveBufferMaxSize = 0,
                UsePlatformSpecificProtocols = false,
                WebSocketReceiveBufferMaxSize = 0
            }, 20);

            if (!DuneServer.NetworkController.Listen("127.0.0.1", 7000))
            {
                Debug.LogError("Error initializing network.");
            }
        }
    }
}  
```  

*Assets/Scripts/Client/ClientInit.cs:*  
```csharp
using DuneNet.Client;
using UnityEngine;
using UnityEngine.Networking;

namespace Client
{
    public class ClientInit : DuneNet.Client.DuneMonoBehaviour
    {
        protected override void OnAwake()
        {
            DuneClient.Use(new ClientGreeter());
        
            DuneClient.NetworkController.Init(new GlobalConfig
            {
                MaxHosts = 1,
                MaxNetSimulatorTimeout = 12000,
                MaxPacketSize = 2000,
                MaxTimerTimeout = 12000,
                MinNetSimulatorTimeout = 1,
                ReactorMaximumReceivedMessages = 8092,
                ReactorMaximumSentMessages = 8092,
                ReactorModel = ReactorModel.SelectReactor,
                ThreadAwakeTimeout = 1,
                ThreadPoolSize = 1
            }, new ConnectionConfig
            {
                AckDelay = 33,
                AcksType = ConnectionAcksType.Acks32,
                AllCostTimeout = 20,
                BandwidthPeakFactor = 2,
                ConnectTimeout = 2000,
                DisconnectTimeout = 2000,
                FragmentSize = 500,
                InitialBandwidth = 0,
                MaxCombinedReliableMessageCount = 10,
                MaxCombinedReliableMessageSize = 100,
                MaxConnectionAttempt = 10,
                MaxSentMessageQueueSize = 896,
                MinUpdateTimeout = 10,
                NetworkDropThreshold = 5,
                OverflowDropThreshold = 5,
                PacketSize = 1440,
                PingTimeout = 500,
                ReducedPingTimeout = 100,
                ResendTimeout = 1200,
                SendDelay = 0,
                SSLCAFilePath = "",
                SSLCertFilePath = "",
                SSLPrivateKeyFilePath = "",
                UdpSocketReceiveBufferMaxSize = 0,
                UsePlatformSpecificProtocols = false,
                WebSocketReceiveBufferMaxSize = 0
            }, 20);

            if (!DuneClient.NetworkController.Connect("127.0.0.1", 7000))
            {
                Debug.LogError("Error initializing network.");
            }
        }
    }
}
```  

## What's next?  
Now that we can easily verify that the client can connect to the server, we should be able to start using more advanced features of DuneNet.  
In the [next example](example-3.md), we will implement a basic authentication module to secure the server behind a password. 