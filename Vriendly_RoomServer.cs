using UnityEngine;
using MasterServerToolkit.Bridges.MirrorNetworking;
using MasterServerToolkit.MasterServer;
using MasterServerToolkit.Networking;
using Mirror;

[AddComponentMenu("Vriendly/Vriendly Room Server")]
public class Vriendly_RoomServer : RoomServer
{


    protected override void Start()
    {
        base.Start();
        SetSceneName(Mst.Args.LoadScene);
        print($"Loaded Scene is: {Mst.Args.LoadScene}");
        if (Mst.Args.IsProvided(Mst.Args.Names.RoomMaxConnections))
        {
            print($"Max Connections are set to: {Mst.Args.RoomMaxConnections}");
            MirrorNetworkManager.maxConnections = Mst.Args.RoomMaxConnections > 0 ? Mst.Args.RoomMaxConnections : MirrorNetworkManager.maxConnections;
        }
        else
            print($"Max Connections are not provided!");
        print($"NetworkManager Max Connections: {MirrorNetworkManager.maxConnections}");
    }

    public void SetSceneName(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
            NetworkManager.singleton.onlineScene = sceneName;
    }


    /// <summary>
    /// Fires when client that wants to connect to this room made request to validate the access token
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="msg"></param>
    protected override void ValidateRoomAccessRequestHandler(NetworkConnection conn, ValidateRoomAccessRequestMessage msg)
    {
        logger.Debug($"Room client {conn.connectionId} asked to validate access token [{msg.Token}]");

        // Triying to validate given token
        Mst.Server.Rooms.ValidateAccess(RoomController.RoomId, msg.Token, (usernameAndPeerId, error) =>
        {
            // If token is not valid
            if (usernameAndPeerId == null)
            {
                logger.Error(error);

                conn.Send(new ValidateRoomAccessResultMessage()
                {
                    Error = error,
                    Status = ResponseStatus.Failed
                });

                MstTimer.WaitForSeconds(1f, () => conn.Disconnect());

                return;
            }

            logger.Debug($"Client {conn.connectionId} is successfully validated");
            logger.Debug("Getting his account info...");

            var player = new RoomPlayer
            {
                Username = $"Player {conn.connectionId}",
                MasterPeerId = usernameAndPeerId.PeerId,
                Profile = new ObservableServerProfile(usernameAndPeerId.Username),
                MirrorPeer = conn
            };

            roomPlayersByMsfPeerId.Add(usernameAndPeerId.PeerId, player);
            roomPlayersByMirrorPeerId.Add(conn.connectionId, player);
            roomPlayersByUsername.Add(player.Username, player);

            FinalizePlayerJoining(conn);
        });
    }

    public override void SetPort(int port)
    {
        if (Transport.activeTransport is kcp2k.KcpTransport transport)
        {
            transport.Port = (ushort)port;
        }
        else
        {
            logger.Error("You are trying to use KcpTransport. But it is not found on the scene. Try to override this method to create you own implementation");
        }
    }

    public override int GetPort()
    {
        if (Transport.activeTransport is kcp2k.KcpTransport transport)
        {
            return (int)transport.Port;
        }
        else
        {
            logger.Error("You are trying to use KcpTransport. But it is not found on the scene. Try to override this method to create you own implementation");
            return 0;
        }
    }

}
