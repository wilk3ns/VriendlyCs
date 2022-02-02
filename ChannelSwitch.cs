using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using Vriendly.Player;
using UnityEngine;
using System;

public class ChannelSwitch : MonoBehaviour
{

    private PlayerVoice _player;

    private void Awake()
    {
        //PlayerVoice.OnPlayerVoiceConnect.AddListener(LocalPlayerConnected);
    }

    private void LocalPlayerConnected(PlayerVoice voice)
    {
        _player = voice;
    }

    //public void GoToTheGeneral()
    //{
    //    if (_player)
    //        _player.JoinAChannel();
    //}

    //public void SwitchChannel(string channelName)
    //{
    //    if (_player)
    //        _player.JoinAChannel(channelName);
    //}

}
