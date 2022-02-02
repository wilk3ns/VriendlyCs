using Photon.Voice.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrivateChatController : MonoBehaviour
{
    [SerializeField]
    private VoiceConnection _connection;



    public void SetInterestGroup(int group)
    {
        _connection.Client.GlobalInterestGroup = (byte)group;
    }
}
