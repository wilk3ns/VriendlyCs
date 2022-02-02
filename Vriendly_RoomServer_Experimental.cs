using System;
using System.Collections;
using System.Collections.Generic;
using MasterServerToolkit.Networking;
using MasterServerToolkit.MasterServer;
using UnityEngine;
using MasterServerToolkit.Bridges.MirrorNetworking;
using MasterServerToolkit.Vriendly;

[AddComponentMenu("Vriendly/Vriendly Room Server Experimental")]
public class Vriendly_RoomServer_Experimental : BaseClientBehaviour
{
    /// <summary>
    /// The instance of the <see cref="Vriendly_RoomServer_Experimental"/>
    /// </summary>Ins
    public static Vriendly_RoomServer_Experimental Instance { get; protected set; }

    /// <summary>
    /// <see cref="RoomProfile"/> is a <see cref="ScriptableObject"/>
    /// that is holding room's custom data
    /// </summary>
    public RoomProfile _roomProfile;

    protected override void Awake()
    {
        base.Awake();

        // Only one room server can exist in scene
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        // Create simple singleton
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _roomProfile.InitializeProfile(this);
        _roomProfile.connectionToTheMaster.Connect();
    }

    /// <summary>
    /// 
    /// </summary>
    protected virtual void OnApplicationQuit()
    {
        _roomProfile.connectionToTheMaster.Disconnect();
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDestroy()
    {
        base.OnDestroy();

        //// Remove connection listeners
        Connection?.RemoveConnectionListener(OnConnectedToMasterServerEventHandler);
        Connection?.RemoveDisconnectionListener(OnDisconnectedFromMasterServerEventHandler);

        //// Start listenin to OnServerStartedEvent of our MirrorNetworkManager
        //if (MirrorNetworkManager is MirrorNetworkManager manager)
        //{
        //    manager.OnServerStartedEvent -= OnMirrorServerStartedEventHandler;
        //    manager.OnClientDisconnectedEvent -= OnMirrorClientDisconnectedEvent;
        //    manager.OnServerStoppedEvent -= OnMirrorServerStoppedEventHandler;
        //}

        //// Unregister handlers
        //NetworkServer.UnregisterHandler<ValidateRoomAccessRequestMessage>();
    }

    protected override void OnInitialize()
    {
        //if (Mst.Client.Rooms.ForceClientMode) return;

        //// Get mirror network manager
        //MirrorNetworkManager = NetworkManager.singleton;

        //// Start listening to OnServerStartedEvent of our MirrorNetworkManager
        //if (MirrorNetworkManager is MirrorNetworkManager manager)
        //{
        //    manager.OnServerStartedEvent += OnMirrorServerStartedEventHandler;
        //    manager.OnClientDisconnectedEvent += OnMirrorClientDisconnectedEvent;
        //    manager.OnServerStoppedEvent += OnMirrorServerStoppedEventHandler;
        //}
        //else
        //{
        //    logger.Error("We cannot register listeners of MirrorNetworkManager events because we cannot find it onscene");
        //}

        //// Set room oprions
        //roomOptions = SetRoomOptions();

        //// Set port of the Mirror server
        //SetPort(roomOptions.RoomPort);

        // Add master server connection and disconnection listeners
        Connection.AddConnectionListener(OnConnectedToMasterServerEventHandler, true);
        Connection.AddDisconnectionListener(OnDisconnectedFromMasterServerEventHandler, false);

        // If connection to master server is not established
        _roomProfile.connectionToTheMaster.Connect();
    }

    /// <summary>
    /// Invokes when room server is successfully connected to master server as client
    /// </summary>
    private void OnConnectedToMasterServerEventHandler()
    {
        //logger.Debug("Room server is successfully connected to master server");

        //// If this room was spawned
        //if (Mst.Server.Spawners.IsSpawnedProccess)
        //{
        //    // Try to register spawned process first
        //    RegisterSpawnedProcess();
        //}

        //// If we are testing our room in editor
        //if (IsAllowedToBeStartedInEditor())
        //{
        //    StartServerInEditor();
        //}
    }

    /// <summary>
    /// Fired when this room server is disconnected from master as client
    /// </summary>
    protected virtual void OnDisconnectedFromMasterServerEventHandler()
    {
        // Quit the room if we are not in editor
        if (!Mst.Runtime.IsEditor)
            Mst.Runtime.Quit();
    }


}
