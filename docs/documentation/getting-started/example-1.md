# Connecting a client to a server for the first time  
## Overview  
The first step required to start using DuneNet in a multiplayer game is connecting clients to a server.  
This requires two scenes, each of them with a ``Monobehaviour`` or ``DuneMonoBehaviour`` component attached to an active GameObject.  
In this example we will use two components that derive from DuneMonoBehaviour called ServerInit and ClientInit. These components will be attached to empty GameObjects in the ``Assets/Scenes/Server/init.unity`` and ``Assets/Scenes/Client/init.unity`` scenes respectively.  
We will first provide the whole code for the example and then we will explain the important aspects of it.

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

## ServerInit.cs  
This class takes care of initializing the serverside [``NetworkController``](../../api/DuneNet.Server.Controllers.NetworkController.html) and listening for incoming connections.  

First of all, we define a new class that derives from [``DuneMonoBehaviour``](../../api/DuneNet.Server.DuneMonoBehaviour.html):  
```csharp
public class ServerInit : DuneNet.Server.DuneMonoBehaviour
```  
This makes sure that the ``ServerInit`` class can listen for events.

The second thing we need to do is initialize the [``NetworkController``](../../api/DuneNet.Server.Controllers.NetworkController.html) with a configuration of our choice. This configuration is provided by two Unity objects: [``GlobalConfig``](https://docs.unity3d.com/ScriptReference/Networking.GlobalConfig.html) and [``ConnectionConfig``](https://docs.unity3d.com/ScriptReference/Networking.ConnectionConfig.html).  
We also need to tell the initializer how many connections should be opened at the same time. For the server, this is usually the same as the amount of clients we expect to handle at the same time, and should be the same number as [``GlobalConfig``](https://docs.unity3d.com/ScriptReference/Networking.GlobalConfig.html)'s ``MaxHosts`` variable.  
```csharp
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
```  
The next and final step is simply instructing the [``NetworkController``](../../api/DuneNet.Server.Controllers.NetworkController.html) to listen for connections at certain IP and Port.  
The IP should be ``127.0.0.1`` for the server to listen for connections in all the available IPs or a specific IP address the server can bind to. The [``NetworkController``](../../api/DuneNet.Server.Controllers.NetworkController.html) supports IPv6.  
The port should be any port of your choice that is available to userspace applications (more than 1024 and less than 65535).  
```csharp
if (!DuneClient.NetworkController.Connect("127.0.0.1", 7000))
{
    Debug.LogError("Error initializing network.");
}
```  
Here, we also take the opportunity to check if the [``NetworkController``](../../api/DuneNet.Server.Controllers.NetworkController.html) was able to listen on the provided endpoint by checking the return value of ``DuneClient.NetworkController.Connect()``.  
Please, do note that this is a very rudimentary check that does not specify which error actually occurred so it should only be used for simple operations like exiting the application if the server was unable to listen. For more complex operations, using a [``DuneModule``](../../api/DuneNet.Server.DuneModule.html) is recommended.

## ClientInit.cs  
This class takes care of initializing the clientside [``NetworkController``](../../api/DuneNet.Client.Controllers.NetworkController.html) and connecting to a server.  

First of all, we define a new class that derives from [``DuneMonoBehaviour``](../../api/DuneNet.Client.DuneMonoBehaviour.html):  
```csharp
public class ClientInit : DuneNet.Client.DuneMonoBehaviour
```  
This makes sure that the ``ClientInit`` class can listen for events.

The second thing we need to do is initialize the [``NetworkController``](../../api/DuneNet.Client.Controllers.NetworkController.html) with a configuration of our choice. This configuration is provided by two Unity objects: [``GlobalConfig``](https://docs.unity3d.com/ScriptReference/Networking.GlobalConfig.html) and [``ConnectionConfig``](https://docs.unity3d.com/ScriptReference/Networking.ConnectionConfig.html).  
We also need to tell the initializer how many connections should be opened at the same time. For the client, this is usually one, and should be the same number as [``GlobalConfig``](https://docs.unity3d.com/ScriptReference/Networking.GlobalConfig.html)'s ``MaxHosts`` variable.  
```csharp
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
```  
The next and final step is simply instructing the [``NetworkController``](../../api/DuneNet.Client.Controllers.NetworkController.html) to connect to a server that is listening at a certain IP and port.  
The IP should be ``127.0.0.1`` to connect to a server running on the same machine as the client or a valid IP the client machine can reach in the rest of the cases. The [``NetworkController``](../../api/DuneNet.Client.Controllers.NetworkController.html) supports IPv6.  
The port should be the port the server is listening on. This should be in the range reserved for userspace applications (more than 1024 and less than 65535).  
```csharp
if (!DuneClient.NetworkController.Connect("127.0.0.1", 7000))
{
    Debug.LogError("Error initializing network.");
}
```  
Here, we also take the opportunity to check if the [``NetworkController``](../../api/DuneNet.Client.Controllers.NetworkController.html) was able to initialize the client by checking the return value of ``DuneClient.NetworkController.Connect()``.  
Please, do note that this is a very rudimentary check that only verifies that the client was initialized properly. Connecting to a server is an asynchronous operation so any functionality that relies on checking the state of the connection should be handled using a ``DuneModule``.

## What's next?  
Now that we have an established connection, we should do something useful with it.  
In the [next example](example-2.md), we will create two simple modules to print a message to the console when a connection is established.