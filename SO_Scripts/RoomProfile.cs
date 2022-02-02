using MasterServerToolkit.MasterServer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MasterServerToolkit.Vriendly
{
    [CreateAssetMenu(fileName = "NewRoomProfile", menuName = "ScriptableObjects/RoomProfile", order = 1)]
    public class RoomProfile : ScriptableObject
    {

        #region Modules
        [Header("Modules of the room")]
        public BaseMasterConnection connectionToTheMaster;
        #endregion

        private BaseClientBehaviour _room;

        public void InitializeProfile(BaseClientBehaviour room)
        {
            _room = room;
            connectionToTheMaster.Initialize(_room.Connection);
        }

        public void ConnectToTheMaster()
        {
            connectionToTheMaster.Connect();
        }
    }
}
