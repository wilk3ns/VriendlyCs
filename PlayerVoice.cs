using System.Collections;
using System.Collections.Generic;
using Vriendly.Player;
using UnityEngine;
using UnityEngine.Events;
using Mirror;
using Photon.Voice.Unity;
using Photon.Realtime;
using System.ComponentModel;
using System;
using Photon.Voice.Unity.UtilityScripts;
using Vriendly.Global.Settings;
using System.Threading.Tasks;

public class OnVoiceConnected : UnityEvent<PlayerVoice> { }
[Serializable]
public class OnVoicePropertyChanged : UnityEvent<bool> { }

public class PlayerVoice : NetworkBehaviour, IPlayerModule
{
    public bool CanVMic => _vMicAvailable;
    public bool Muted => _muted;

    public static OnVoiceConnected OnConnected = new OnVoiceConnected();
    public OnVoicePropertyChanged OnMuted = new OnVoicePropertyChanged();
    public OnVoicePropertyChanged OnBanned = new OnVoicePropertyChanged();
    public OnVoicePropertyChanged OnVMicrophone = new OnVoicePropertyChanged();
    public OnVoicePropertyChanged OnVMicrophoneAcess = new OnVoicePropertyChanged();
    private VoiceConnection _connection;
    private ConnectAndJoin _connectUtility;
    [SyncVar/*(hook = nameof(OnActorNumber))*/]
    private int _actorNumber;
    [SyncVar(hook = nameof(OnMute))]
    private bool _muted;
    [SyncVar(hook = nameof(OnBann))]
    public bool _blocked;
    [SyncVar(hook = nameof(OnVmic))]
    private bool _vMic;
    [SyncVar(hook = nameof(OnVmicAvailable))]
    private bool _vMicAvailable;
    private Speaker _remoteSpeaker;
    private PlayerSpeaker _playerSpeaker;

    //private void OnActorNumber(int oldAN, int newAN)
    //{
    //    if (!isLocalPlayer)
    //    {
    //        StartCoroutine(WaitForTheSpeaker(newAN));
    //    }
    //}

    private void OnMute(bool oldValue, bool newValue)
    {
        _muted = newValue;
        OnMuted?.Invoke(newValue);
    }

    private void OnBann(bool oldValue, bool newValue)
    {
        _blocked = newValue;
        OnBanned?.Invoke(newValue);
        if (isLocalPlayer)
            _connection.PrimaryRecorder.TransmitEnabled = !(newValue ? true : _muted);
    }
    private void OnVmic(bool oldValue, bool newValue)
    {
        _vMic = newValue;
        OnVMicrophone?.Invoke(newValue);
        if (!isLocalPlayer && _playerSpeaker != null)
            _playerSpeaker.VirtualMicrophone = newValue;
    }

    public void OnVmicAvailable(bool oldValue, bool newValue)
    {
        _vMicAvailable = newValue;
        if (isLocalPlayer)
        {
            if (!_vMicAvailable)
                TurnVMicON(false);
            OnVMicrophoneAcess?.Invoke(newValue);
        }
    }

    public void TurnVMicON(bool on)
    {
        CmdTurnVMic(on);
    }

    [Command]
    private void CmdTurnVMic(bool on)
    {
        _vMic = on;
    }

    //private IEnumerator WaitForTheSpeaker(Speaker speaker)
    //{
    //    while (!SpeakerFound(speaker))
    //    {
    //        yield return new WaitForSeconds(1f);
    //    }
    //}

    private void GiveMicAcessToUser(GameObject user, bool acess)
    {
        CmdAcessToUser(user, acess);
    }

    [Command]
    private void CmdAcessToUser(GameObject user, bool acess)
    {
        user.GetComponent<PlayerVoice>()._vMicAvailable = acess;
    }

    private void BannVoiceUser(GameObject user, bool bann)
    {
        CmdBannUser(user, bann);
    }

    [Command]
    private void CmdBannUser(GameObject user, bool bann)
    {
        user.GetComponent<PlayerVoice>()._blocked = bann;
    }

    public void MuteSelf(bool mute)
    {
        if (_blocked)
            return;
        _connection.PrimaryRecorder.TransmitEnabled = !mute;
        CmdPassMute(mute);
    }

    [Command]
    private void CmdPassMute(bool mute)
    {
        _muted = mute;
    }

    //private bool SpeakerFound(Speaker speaker)
    //{
    //    Speaker[] speakers = FindObjectsOfType<Speaker>();
    //    if (speakers.Length > 0)
    //    {

    //        foreach (Speaker speaker in speakers)
    //        {
    //            if (speaker.Actor.ActorNumber == actorNumber)
    //            {
    //                _remoteSpeaker = speaker;
    //                _remoteSpeaker.transform.parent = this.transform;
    //                _remoteSpeaker.transform.localRotation = Quaternion.identity;
    //                _remoteSpeaker.transform.localPosition = Vector3.zero;
    //                _playerSpeaker = _remoteSpeaker.GetComponent<PlayerSpeaker>();
    //                SpeakerManager.instance?.AddSpeaker(_playerSpeaker);
    //                Nametag nametag = GetComponent<Nametag>();
    //                PlayerUnit unit = GetComponent<PlayerUnit>();
    //                PlayerIK ik = GetComponent<PlayerIK>();
    //                OnVMicrophoneAcess.Invoke(unit.LocalPlayer._profileData._admin);
    //                OnVMicrophone.AddListener((on) => nametag.State = on ? TagState.MIC : nametag.Admin ? TagState.ADMIN : TagState.USER);
    //                PlayerVoice localVoice = unit.LocalPlayer.LocalUnit.GetComponent<PlayerVoice>();
    //                nametag.MuteToggle.onValueChanged.AddListener((on) => _playerSpeaker.Mute = on);
    //                _playerSpeaker.AddVisual(nametag);
    //                _playerSpeaker.AddVisual(ik);
    //                nametag.BlockToggle.onValueChanged.AddListener((on) => localVoice.BannVoiceUser(gameObject, on));
    //                nametag.VirtualMicToggle.onValueChanged.AddListener((on) => localVoice.GiveMicAcessToUser(gameObject, on));
    //                nametag.VirtualMicToggle.transform.parent.gameObject.SetActive(unit.LocalPlayer._profileData._admin && !nametag.Admin);
    //                _playerSpeaker.VirtualMicrophone = _vMic;
    //                if (_vMic)
    //                    OnVMicrophone.Invoke(_vMic);
    //                return true;
    //            }
    //        }
    //    }
    //    return false;
    //}

    private void Awake()
    {
        if (!isServer)
        {
            _connection = FindObjectOfType<VoiceConnection>();
            if (_connection)
            {
                _connection.SpeakerLinked += _connection_SpeakerLinked;
                _connection.Client.StateChanged += Client_StateChanged;
            }
        }
    }

    private void Client_StateChanged(ClientState from, ClientState to)
    {
        if (!isLocalPlayer)
            return;
        if (to == ClientState.Joined)
        {
            CmdActorNumberSync(_connection.Client.LocalPlayer.ActorNumber);
        }
    }

    public void Initialize(PlayerUnit unit)
    {
        _connectUtility = FindObjectOfType<ConnectAndJoin>();
        _connection.PrimaryRecorder.PhotonMicrophoneDeviceId = (Microphone.devices.Length - 1) - InGameSettings.instance.Settings.Settings._usedMicrophoneIndex;
        _connectUtility.RoomName = unit.LocalPlayer._profileData._activeRoomHash;
        OnConnected?.Invoke(this);
        _connectUtility.ConnectNow();
    }


    [Command]
    private void CmdActorNumberSync(int actorNumber)
    {
        _actorNumber = actorNumber;
    }

    private async void _connection_SpeakerLinked(Speaker speaker)
    {
        if (isLocalPlayer)
            return;
        await Task.Delay(1000);
        print($"voice:  Speaker- {speaker.Actor.ActorNumber} Player({transform.GetInstanceID()}) - {_actorNumber}");
        if (speaker.Actor.ActorNumber == _actorNumber)
        {
            _remoteSpeaker = speaker;
            _remoteSpeaker.transform.parent = this.transform;
            _remoteSpeaker.transform.localRotation = Quaternion.identity;
            _remoteSpeaker.transform.localPosition = Vector3.zero;
            _playerSpeaker = _remoteSpeaker.GetComponent<PlayerSpeaker>();
            SpeakerManager.instance?.AddSpeaker(_playerSpeaker);
            Nametag nametag = GetComponent<Nametag>();
            PlayerUnit unit = GetComponent<PlayerUnit>();
            PlayerIK ik = GetComponent<PlayerIK>();
            OnVMicrophoneAcess.Invoke(unit.LocalPlayer._profileData._admin);
            OnVMicrophone.AddListener((on) => nametag.VMic = on);
            PlayerVoice localVoice = unit.LocalPlayer.LocalUnit.GetComponent<PlayerVoice>();
            nametag.MuteToggle.onValueChanged.AddListener((on) => _playerSpeaker.Mute = on);
            _playerSpeaker.AddVisual(nametag);
            _playerSpeaker.AddVisual(ik);
            nametag.BlockToggle.onValueChanged.AddListener((on) => localVoice.BannVoiceUser(gameObject, on));
            nametag.VirtualMicToggle.onValueChanged.AddListener((on) => localVoice.GiveMicAcessToUser(gameObject, on));
            nametag.VirtualMicToggle.transform.parent.gameObject.SetActive(unit.LocalPlayer._profileData._admin && !nametag.Admin);
            _playerSpeaker.VirtualMicrophone = _vMic;
            if (_vMic)
                OnVMicrophone.Invoke(_vMic);
        }
    }


    public void Deinitialize()
    {
    }

}
