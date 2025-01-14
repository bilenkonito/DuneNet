# Password Authentication  
One of the simplest methods of deciding which clients are able to connect to the server and which are not, is by using password authentication.  
This can easily be implemented in DuneNet by using a module.  

In this example, we will create two new classes that derive from ``DuneModule``: ``ServerPasswordAuth.cs`` and ``ClientPasswordAuth.cs``.  
We will first provide the whole code for the example and then we will explain the important aspects of it.  

*Assets/Scripts/Client/Modules/ClientPasswordAuth.cs:*  
```csharp
using System;
using System.Text;
using DuneNet.Client;
using DuneNet.Shared;
using DuneNet.Shared.Modules;
using UnityEngine;

namespace Client.Modules
{
    public class ClientPasswordAuth : DuneNet.Client.DuneModule
    {
        private string _password;

        public ClientPasswordAuth(string password)
        {
            _password = password;
        }
        
        public void ChangePassword(string newPassword)
        {
            _password = newPassword;
        }
        
        protected override HandshakeRequest OnSendHandshake(HandshakeRequest previous)
        {
            previous.IDToken = Guid.NewGuid().ToString();
            previous.Secret = Encoding.Unicode.GetBytes(_password);

            return previous;
        }

        protected override void OnHandshakeSuccessful(DuneConnection conn)
        {
            Debug.Log("Auth successful, setting ourselves ready.");
            DuneClient.NetworkController.SetReady();
        }

        protected override void OnHandshakeError(DuneConnection conn, string error)
        {
            Debug.LogError("Auth error: " + error);
        }
    }
}
```

*Assets/Scripts/Server/Modules/ServerPasswordAuth.cs:*  
```csharp
using System;
using System.Text;
using DuneNet.Server;
using DuneNet.Shared.Modules;

namespace Server.Modules
{
    public class ServerPasswordAuth : DuneNet.Server.DuneModule
    {
        private string _password;

        public ServerPasswordAuth(string password)
        {
            _password = password;
        }

        public void ChangePassword(string newPassword)
        {
            _password = newPassword;
        }
        
        protected override HandshakeResponse OnRespondHandshake(HandshakeResponse previous, string idToken, byte[] secret)
        {
            string clientPassword = Encoding.Unicode.GetString(secret);
            if (clientPassword == _password)
            {
                previous.Allowed = true;
                previous.AuthenticationToken = Guid.NewGuid().ToString();
            }
            else
            {
                previous.Allowed = false;
                previous.Error = "Wrong password";
            }

            return previous;
        }
    }
}
```  

## ClientPasswordAuth.cs  
This module takes care of generating a random identification token and encoding a password of our choice.  
First of all, we define a new class that derives from [``DuneModule``](../../api/DuneNet.Client.DuneModule.html):  
```csharp
public class ClientPasswordAuth : DuneNet.Client.DuneModule
```  
The [``DuneModule``](../../api/DuneNet.Client.DuneModule.html) class provides many overridable methods that we can use to extend the functionality of DuneNet beyond what the main controllers provide.  
In our case, we want to override the  ``OnSendHandshake(HandshakeRequest previous)`` method, which is called when the client is expected to make an authentication request. This happens after the client has precariously connected to the server but before the server responds with any kind of game state data.  
The ``previous`` argument of this method is the result from the previous module in the module chain and can be used to chain multiple authentication schemes such as Steam Authentication Ticket validation and password authentication. We might also use this argument to modify our authentication ticket or secret depending on what authentication schemes are currently registered.  
```csharp
protected override HandshakeRequest OnSendHandshake(HandshakeRequest previous)
{
    previous.IDToken = Guid.NewGuid().ToString();
    previous.Secret = Encoding.Unicode.GetBytes(_password);

    return previous;
}
```  
Here, we simply override the identification token from the previous module with a new random Guid and we set our authentication secret to a Unicode encoded byte array of our password. You might want to set the authentication token to something meaningful for your system, like a Steam Authentication Ticket if you are using Steam.  
We then return our modified ``HandshakeRequest`` object for the next module to process. If no additional modules are registered, the ``HandshakeRequest`` object will directly be handled by DuneNet.  
Please, do note that the ``previous`` argument cannot and will never be null, so null checking it is redundant and unnecessary.
```csharp
protected override void OnHandshakeSuccessful(DuneConnection conn)
{
    Debug.Log("Auth successful, setting ourselves ready.");
    DuneClient.NetworkController.SetReady();
}

protected override void OnHandshakeError(DuneConnection conn, string error)
{
    Debug.LogError("Auth error: " + error);
}
```  
As with the previous example, we also take the opportunity to set our connection ready once the server has authenticated the client.  
In this case, however, we go a step forward and override the ``OnHandshakeError(DuneConnection conn, string error)`` method in order to print a message to the console when our authentication request is rejected by the server. In this example, the server would reject our authentication request if the password we provided does not match the one being evaluated by the server.  

## ServerPasswordAuth.cs  
This module takes care of authenticating new clients by checking that the provided password matches the one we defined on the server.  
First of all, we define a new class that derives from [``DuneModule``](../../api/DuneNet.Server.DuneModule.html):  
```csharp
public class ServerPasswordAuth : DuneNet.Server.DuneModule
```  
The [``DuneModule``](../../api/DuneNet.Server.DuneModule.html) class provides many overridable methods that we can use to extend the functionality of DuneNet beyond what the main controllers provide.  
In our case, we want to override the  ``OnRespondHandshake(HandshakeResponse previous, string idToken, byte[] secret)`` method, which is called when the server is expected to respond to an authentication request. This happens after the client has precariously connected to the server but before the client is able to become ready.  
The ``previous`` argument of this method is the result from the previous module in the module chain and can be used to chain multiple authentication schemes such as Steam Authentication Ticket validation and password authentication.  
```csharp
protected override HandshakeResponse OnRespondHandshake(HandshakeResponse previous, string idToken, byte[] secret)
{
    string clientPassword = Encoding.Unicode.GetString(secret);
    if (clientPassword == _password)
    {
        previous.Allowed = true;
        previous.AuthenticationToken = Guid.NewGuid().ToString();
    }
    else
    {
        previous.Allowed = false;
        previous.Error = "Wrong password";
    }

    return previous;
}
```  
Here, we first decode the password the client provided us with using the same encoding (Unicode).  
Then, we check that the client's password matches the server's password. If this is the case, we set the ``Allowed`` property of the ``previous`` argument to true, telling the next modules that the client successfully authenticated against the current module. We also set the ``AuthenticationToken`` property to a new Guid. You might want to set this property to something that identifies the user in your system, like a SteamID if you are using Steam.    
If the client did not successfully authenticate, we set it to false and we also set the ``Error`` property to a descriptive error that the client will get when it gets kicked out of the server.  
We then return our modified ``HandshakeResponse`` object for the next module to process. If no additional modules are registered, the ``HandshakeResponse`` object will directly be handled by DuneNet.

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
            DuneServer.Use(new ServerPasswordAuth("123456"));
        
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
            DuneClient.Use(new ClientPasswordAuth("123456"));
        
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
You can try changing the password on the client and reconnecting multiple times to test that the password authentication is working properly. You might do so like this:
```csharp
DuneClient.NetworkController.Disconnect();
((ClientPasswordAuth) DuneClient.GetModule<ClientPasswordAuth>())?.ChangePassword("abcdef");
DuneClient.NetworkController.Connect("127.0.0.1", 7000);
```  

## What's next?  
In the [next example](example-4.md), we will create our first entity and we will learn how to synchronize movement and arbitrary data.