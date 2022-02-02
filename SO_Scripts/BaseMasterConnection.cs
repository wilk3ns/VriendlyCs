using MasterServerToolkit.MasterServer;
using MasterServerToolkit.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MasterServerToolkit.Vriendly
{
    [CreateAssetMenu(fileName = "NewMasterConnection", menuName = "ScriptableObjects/MasterConnection", order = 1)]
    public class BaseMasterConnection : ScriptableObject
    {
        /// <summary>
        /// Master server IP address to connect room server to master server as client
        /// </summary>
        [Header("Master Connection Settings"), SerializeField, Tooltip("Master server IP address to connect room server to master server as client")]
        public string masterIp = "127.0.0.1";

        /// <summary>
        /// Master server port to connect room server to master server as client
        /// </summary>
        [SerializeField, Tooltip("Master server port to connect room server to master server as client")]
        public int masterPort = 5000;

        protected IClientSocket _connection;


        public virtual void Initialize(IClientSocket connection)
        {
            _connection = connection;
            // If master IP is provided via cmd arguments
            masterIp = Mst.Args.AsString(Mst.Args.Names.MasterIp, masterIp);

            // If master port is provided via cmd arguments
            masterPort = Mst.Args.AsInt(Mst.Args.Names.MasterPort, masterPort);
        }

        public virtual void Connect()
        {
            if (!_connection.IsConnected && !_connection.IsConnecting)
            {
                _connection.Connect(masterIp, masterPort);
            }
        }

        public virtual void Disconnect()
        {
            if (_connection != null)
                _connection.Disconnect();
        }

    }
}
