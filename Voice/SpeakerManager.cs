using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SpeakerManager : Updatable
{
    public static SpeakerManager instance;
    private List<PlayerSpeaker> _speakers = new List<PlayerSpeaker>();


    private void Awake()
    {
        if (instance != null)
            Destroy(this);
        else
            instance = this;
    }

    public void AddSpeaker(PlayerSpeaker speaker)
    {
        _speakers.Add(speaker);
    }

    public void RemoveSpeaker(PlayerSpeaker speaker)
    {
        _speakers.Remove(speaker);
    }

    public override void Tick(LocalPlayer player)
    {
        for (int i = 0; i < _speakers.Count; i++)
        {
            float distance = Vector3.Distance(player._cameraSetupData.CurrentCamera.position, _speakers[i].transform.position);
            if (!_speakers[i].VirtualMicrophone)
            {
                if (distance < _speakers[i].Audio.maxDistance)
                {
                    if (!_speakers[i].Audio.enabled)
                        _speakers[i].Audio.enabled=true;
                    _speakers[i].UpdateValues(distance);
                }
                else
                {
                    if (_speakers[i].Audio.enabled)
                        _speakers[i].Audio.enabled=false;
                }
            }
            else
            {
                if (!_speakers[i].Audio.enabled)
                    _speakers[i].Audio.enabled=true;
                _speakers[i].UpdateValues(1);
            }
        }
    }


}
