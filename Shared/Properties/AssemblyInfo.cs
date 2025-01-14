using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("DuneNet.Shared")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Dune Interactive")]
[assembly: AssemblyProduct("DuneNet")]
[assembly: AssemblyCopyright("Copyright © 2018 Dune Interactive")]
[assembly: AssemblyTrademark("DuneNet")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("58A07951-2845-42B1-AAE4-B01AAE8BA26D")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

//Obfuscation
[assembly: Obfuscation(Exclude = false, Feature = "generate debug symbol: true")]
[assembly: Obfuscation(Exclude = false, Feature = "random seed: DuneNet1")]
[assembly: Obfuscation(Exclude = false, Feature = "not member-type('type') and not member-type('module') and not is-public():preset(normal)")]
[assembly: Obfuscation(Exclude = false, Feature = "match('Awake|FixedUpdate|LateUpdate|OnAnimatorIK|OnAnimatorMove|OnApplicationFocus|OnApplicationPause|OnApplicationQuit|OnAudioFilterRead|OnBecameInvisible|OnBecameVisible|OnCollisionEnter|OnCollisionEnter2D|OnCollisionExit|OnCollisionExit2D|OnCollisionStay|OnCollisionStay2D|OnConnectedToServer|OnControllerColliderHit|OnDestroy|OnDisable|OnDisconnectedFromServer|OnDrawGizmos|OnDrawGizmosSelected|OnEnable|OnFailedToConnect|OnFailedToConnectToMasterServer|OnGUI|OnJointBreak|OnLevelWasLoaded|OnMasterServerEvent|OnMouseDown|OnMouseDrag|OnMouseEnter|OnMouseExit|OnMouseOver|OnMouseUp|OnMouseUpAsButton|OnNetworkInstantiate|OnParticleCollision|OnPlayerConnected|OnPlayerDisconnected|OnPostRender|OnPreCull|OnPreRender|OnRenderImage|OnRenderObject|OnSerializeNetworkView|OnServerInitialized|OnTriggerEnter|OnTriggerEnter2D|OnTriggerExit|OnTriggerExit2D|OnTriggerStay|OnTriggerStay2D|OnValidate|OnWillRenderObject|Reset|Start|Update'):-rename")]