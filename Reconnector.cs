using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using MasterServerToolkit.Bridges.MirrorNetworking;
using Vriendly.Backend;
using UnityEngine.Events;
using System;

[RequireComponent(typeof(MirrorNetworkManager))]
public class Reconnector : MonoBehaviour
{


    public static UnityEvent FailedToReconnect = new UnityEvent();
    public static bool Reconnection => reconnection;
    public static bool Disconnected { get => disconnected; set => disconnected = value; }


    [SerializeField]
    private int _attempts = 5;

    private static bool disconnected = false;
    private static bool reconnection = false;
    private MirrorNetworkManager _networkManager;


    void Start()
    {
        _networkManager = GetComponent<MirrorNetworkManager>();
        _networkManager.OnConnectedEvent += ResetReconnector;
        _networkManager.OnDisconnectedEvent += NetworkManager_OnDisconnectedEvent;
    }

    private void ResetReconnector(NetworkConnection con)
    {
        disconnected = false;
    }

    private void NetworkManager_OnDisconnectedEvent(NetworkConnection conn)
    {
        print(disconnected ? "Disconnected!" : "Lost Connection!");
        if (disconnected)
            return;
        reconnection = true;
        StartCoroutine(BackendAPI.CheckConnectivity(_attempts, (resultEnum, response) =>
         {
             if (resultEnum == ProcessState.Successful)
             {
                 if (!Vriendly_ConnectionHelper.Instance.Connection.IsConnected)
                 {
                     Vriendly_ConnectionHelper.Instance.StartConnection();
                 }
                 _networkManager.StartClient();
                 _networkManager.OnConnectedEvent += _networkManager_OnConnectedEvent;
                 print("Reconnected!");
             }
             else
             {
                 FailedToReconnect?.Invoke();
                 print("Failed to reconnect!");
             }
             reconnection = false;
         }));
    }

    private void _networkManager_OnConnectedEvent(NetworkConnection conn)
    {
        _networkManager.OnConnectedEvent -= _networkManager_OnConnectedEvent;
        NetworkClient.Send(new CreatePlayerMessage());
    }

    private void OnDestroy()
    {
        FailedToReconnect.RemoveAllListeners();
        _networkManager.OnConnectedEvent -= ResetReconnector;
        _networkManager.OnDisconnectedEvent -= NetworkManager_OnDisconnectedEvent;
    }
}
