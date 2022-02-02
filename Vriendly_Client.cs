using MasterServerToolkit.Bridges.MirrorNetworking;
using MasterServerToolkit.MasterServer;
using MasterServerToolkit.Networking;
using System.IO;
using Mirror;
using kcp2k;
using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class Vriendly_Client : BaseClientBehaviour
{

    public string _region = "international";



    //public Vriendly_RoomAccessPacket roomOptions;
    /// <summary>
    /// Mirror network manager
    /// </summary>
    public NetworkManager MirrorNetworkManager { get; set; }

    [SerializeField]
    private string _roomName = "NoName";

    [Scene, SerializeField]
    private string _sceneToLoad;

    private string SceneToLoad => Path.GetFileNameWithoutExtension(_sceneToLoad);

    [SerializeField]
    private int _maxPlayers;

    /// <summary>
    /// Room access data received when getting access to room
    /// </summary>
    protected RoomAccessPacket roomAccess;


    /// <summary>
    /// Time of waiting the connection to mirror server
    /// </summary>
    [SerializeField, Tooltip("Time of waiting the connection to mirror server"), Space]
    protected int roomConnectionTimeout = 10;


    /// <summary>
    /// Fires when room server has given an access to us
    /// </summary>
    public event Action OnAccessGrantedEvent;

    /// <summary>
    /// Fires when room server has rejected an access to us
    /// </summary>
    public event Action OnAccessDeniedEvent;

    protected MasterServerToolkit.Logging.Logger _log;

    /// <summary>
    /// Time to wait before match creation process will be aborted
    /// </summary>
    [SerializeField, Tooltip("Time to wait before match creation process will be aborted")]
    protected uint matchCreationTimeout = 60;

    [Space]
    public UnityEvent OnRoomStartedEvent;

    public UnityEvent OnRoomStartAbortedEvent;


    #region Initialization
    protected override void OnInitialize()
    {
        _log = Mst.Create.Logger(GetType().Name);
        _log.LogLevel = logLevel;
        // Get mirror network manager
        MirrorNetworkManager = NetworkManager.singleton;
        // Start listening to OnServerStartedEvent of our MirrorNetworkManager
        if (!(NetworkManager.singleton is MirrorNetworkManager manager))
        {
            _log.Error("Before using MirrorNetworkManager add it to scene");
        }
        else
        {
            OfflineManager.OnJoinEvent.AddListener(LookForRoom);
            OfflineManager.OnStartEvent.AddListener(StartARoom);
        }
    }
    #endregion

    #region Helper Functions

    public string GenerateUniqueID()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 8);
    }

    /// <summary>
    /// Sets an address 
    /// </summary>
    /// <param name="roomAddress"></param>
    public void SetAddress(string roomAddress)
    {
        NetworkManager.singleton.networkAddress = roomAddress;
    }

    public void SetSceneName(string sceneName)
    {
        NetworkManager.singleton.onlineScene = sceneName;
    }

    /// <summary>
    /// Gets an address
    /// </summary>
    /// <param name="roomIp"></param>
    public string GetAddress()
    {
        return NetworkManager.singleton.networkAddress;
    }

    /// <summary>
    /// Set network transport port
    /// </summary>
    /// <param name="port"></param>
    public virtual void SetPort(int port)
    {
        if (Transport.activeTransport is KcpTransport transport)
        {
            transport.Port = (ushort)port;
        }
        else
        {
            _log.Error("You are trying to use KcpTransport. But it is not found on the scene. Try to override this method to create you own implementation");
        }
    }

    public virtual void CreatePlayer()
    {
        NetworkClient.Send(new CreatePlayerMessage());
    }


    #endregion

    private GameInfoPacket FindMyGame(List<GameInfoPacket> list, string hash)
    {
        foreach (GameInfoPacket game in list)
        {
            if (game.Name == hash)
                return game;
        }
        return null;
    }

    #region Unity functions

    protected override void OnDestroy()
    {
        base.OnDestroy();

        NetworkClient.UnregisterHandler<ValidateRoomAccessResultMessage>();
    }

    #endregion

    public void LookForRoom(string Hash, UnityAction<ProcessState> result)
    {
        Mst.Client.Matchmaker.FindGames((games) =>
         {
             if (games.Count > 0)
             {
                 GameInfoPacket room = FindMyGame(games, Hash);
                 if (room != null)
                 {
                     GetRoomAccess(room.Id);
                     result?.Invoke(ProcessState.Successful);
                 }
                 else
                 {
                     result?.Invoke(ProcessState.Failed);
                 }
             }
             else
                 result?.Invoke(ProcessState.Failed);
         });
    }

    public void StartARoom(EventInfo info, int maxPlayers, UnityAction<ProcessState> startResult)
    {
        LookForRoom(info._code, (lookResult) =>
        {
            if (lookResult == ProcessState.Failed)
            {
                MstProperties props = new MstProperties();
                props.Add(MstDictKeys.ROOM_MAX_PLAYERS, maxPlayers);
                props.Add(MstDictKeys.ROOM_NAME, info._code);
                props.Add(MstDictKeys.ROOM_ONLINE_SCENE_NAME, info._placeTitle);
                props.Add(MstDictKeys.ROOM_IS_PUBLIC, true);
                AdministrativeEntrance(_region, info._code, props, startResult);
            }
            else
            {
                startResult?.Invoke(lookResult);
            }
        });
    }


    /// <summary>
    /// Tries to get access data for room we want to connect to
    /// </summary>
    /// <param name="roomId"></param>
    protected virtual void GetRoomAccess(int roomId)
    {
        _log.Debug($"Getting access to room {roomId}");
        Mst.Events.Invoke(MstEventKeys.showLoadingInfo, $"Getting access to room {roomId}... Please wait!");

        Mst.Client.Rooms.GetAccess(roomId, (access, error) =>
        {
            if (access == null)
            {
                _log.Debug($"We could not get access to room. Please try again later or contact to administrator {error}");
                return;
            }
            _log.Debug($"Access to room {roomId} received");
            _log.Debug(access);

            // Save gotten room access
            roomAccess = access;

            // Start joining the room
            JoinTheRoom();
        });
    }



    /// <summary>
    /// Sends request to master server to start new room process
    /// </summary>
    /// <param name="spawnOptions"></param>
    protected virtual void AdministrativeEntrance(string regionName, string hash, MstProperties spawnOptions, UnityAction<ProcessState> result)
    {
        // Custom options that will be given to room directly
        var customSpawnOptions = new MstProperties();
        customSpawnOptions.Add(Mst.Args.Names.RoomMaxConnections, spawnOptions.AsString(MstDictKeys.ROOM_MAX_PLAYERS));
        customSpawnOptions.Add(Mst.Args.Names.LoadScene, spawnOptions.AsString(MstDictKeys.ROOM_ONLINE_SCENE_NAME));


        Mst.Client.Spawners.RequestSpawn(spawnOptions, customSpawnOptions, regionName, (controller, error) =>
        {
            if (controller == null)
            {
                _log.Error(error);
                return;
            }

            _log.Info("Room started. Finalizing... Please wait!");

            // Wait for spawning status until it is finished
            MstTimer.WaitWhile(() =>
            {
                return controller.Status != SpawnStatus.Finalized;
            }, (isSuccess) =>
            {

                if (!isSuccess)
                {
                    result(ProcessState.Failed);
                    Mst.Client.Spawners.AbortSpawn(controller.SpawnTaskId);
                    _log.Error("Failed spawn new room. Time is up!");

                    OnRoomStartAbortedEvent?.Invoke();

                    return;
                }

                OnRoomStartedEvent?.Invoke();
                _log.Debug("You have successfully spawned new room");

            }, matchCreationTimeout);
            MstTimer.WaitForSeconds(4f, () =>
            {
                LookForRoom(hash, (sucess) => result.Invoke(sucess));
            });
        });


    }

    protected virtual void JoinTheRoom()
    {
        // Wait for connection to mirror server
        MstTimer.WaitWhile(() => !NetworkClient.isConnected, isSuccessful =>
        {

            if (!isSuccessful)
            {
                _log.Error("We could not connect to room. Please try again later or contact to administrator");
                MirrorNetworkManager.StopClient();
            }
            else
            {
                OnConnectedToMirrorServerEventHandler(NetworkClient.connection);
            }
        }, roomConnectionTimeout);

        // If we are not connected to mirror server
        if (!NetworkClient.isConnected)
        {
            // Let's set the IP before we start connection
            SetAddress(roomAccess.RoomIp);

            // Let's set the port before we start connection
            SetPort(roomAccess.RoomPort);

            SetSceneName(roomAccess.SceneName);

            print($"The Scene Is: {roomAccess.SceneName}");
            _log.Debug("Connecting to mirror server...");
            if (LoadingUI.Instance)
            {
                OfflineManager.instance.GetRoomThumbnail(roomAccess.SceneName,(sprite)=>
                {
                    LoadingUI.Instance.Thumbnail = sprite;
                });
                LoadingUI.Instance?.LoadingStarted(NetworkManager.loadingSceneAsync);
                // Start mirror client     
            }
            MirrorNetworkManager.StartClient();
        }
    }


    /// <summary>
    /// Invokes when room client is successfully connected to mirror server
    /// </summary>
    protected virtual void OnConnectedToMirrorServerEventHandler(NetworkConnection connection)
    {
        _log.Debug($"Validating access to room server with token [{roomAccess.Token}]");

        // Register listener for access validation message from mirror room server
        NetworkClient.RegisterHandler<ValidateRoomAccessResultMessage>(ValidateRoomAccessResultHandler, false);

        // Send validation message to room server
        connection.Send(new ValidateRoomAccessRequestMessage(roomAccess.Token));
    }

    /// <summary>
    /// Fired when this room client is disconnected from master as client.
    /// </summary>
    public virtual void OnDisconnectedFromMasterServerEventHandler()
    {
        _log.Debug("Room client was disconnected from master server");

        NetworkClient.UnregisterHandler<ValidateRoomAccessResultMessage>();

        // Stop mirror client
        MirrorNetworkManager.StopClient();
    }


    /// <summary>
    /// Fires when room server send message about access validation result
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="msg"></param>
    protected virtual void ValidateRoomAccessResultHandler(NetworkConnection conn, ValidateRoomAccessResultMessage msg)
    {
        if (msg.Status != ResponseStatus.Success)
        {
            _log.Error(msg.Error);

            OnAccessDenied(conn);
            OnAccessDeniedEvent?.Invoke();

            return;
        }

        _log.Debug("Access to server room is successfully validated");

        OnAccessGranted(conn);
        OnAccessGrantedEvent?.Invoke();
    }


    /// <summary>
    /// Fires when access to room server granted
    /// </summary>
    /// <param name="conn"></param>
    protected virtual void OnAccessGranted(NetworkConnection conn)
    {
        CreatePlayer();
    }


    /// <summary>
    /// Fires when access to room server denied
    /// </summary>
    /// <param name="conn"></param>
    protected virtual void OnAccessDenied(NetworkConnection conn) { }

    /// <summary>
    /// Create the network player in mirror networking
    /// </summary>


}
