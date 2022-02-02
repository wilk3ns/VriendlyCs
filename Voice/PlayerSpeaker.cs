using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Voice.Unity;
using System;

public interface ISoundVizualizable
{
    float VisualizerValue { get; set; }
}

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Speaker))]
public class PlayerSpeaker : MonoBehaviour
{
    public bool VirtualMicrophone
    {
        get => _virtualMicrophone;
        set
        {
            SetGlobal(value);
            _virtualMicrophone = value;
        }
    }

    public bool Mute
    {
        get => _source.mute;
        set => _source.mute = value;
    }

    public float VizualizerValue
    {
        get => _relatedBufferedValue;
        private set
        {
            if (_visuals.Count > 0)
            {
                foreach (ISoundVizualizable vis in _visuals)
                {
                    vis.VisualizerValue = value;
                }
            }
            _relatedBufferedValue = value;
        }
    }

    public AudioSource Audio => _source;

    [SerializeField, Header("Debug")]
    private bool _vMic;
    [SerializeField]
    private bool _mute;
    [SerializeField, Header("Settings")]
    private AnimationCurve _globalSpeakerCurve;
    private List<ISoundVizualizable> _visuals = new List<ISoundVizualizable>();

    private bool _virtualMicrophone;
    private Speaker _speaker;
    private AudioSource _source;
    private AnimationCurve _normalSpeakerCurve;

    //Vizualizer
    private float[] _meter = new float[64];
    private float _highestValue;
    private float _relatedBufferedValue;
    private float _bufferDecrease;

    private void Awake()
    {
        _speaker = GetComponent<Speaker>();
        _source = GetComponent<AudioSource>();
        _normalSpeakerCurve = _source.GetCustomCurve(AudioSourceCurveType.CustomRolloff);
    }

    //private void Start()
    //{
    //    SpeakerManager.instance?.AddSpeaker(this);
    //}

    private void OnDestroy()
    {
        SpeakerManager.instance?.RemoveSpeaker(this);
    }

    public void AddVisual(ISoundVizualizable visual)
    {
        _visuals.Add(visual);
    }

    public void RemoveVisual(ISoundVizualizable visual)
    {
        _visuals.Remove(visual);
    }

    public void UpdateValues(float distance)
    {
        _source.GetSpectrumData(_meter, 0, FFTWindow.Hamming);
        float valSum = 0;
        foreach (float val in _meter)
        {
            valSum += val;
        }
        float value = valSum / _meter.Length * (_virtualMicrophone ? 1 : distance);
        _highestValue = value > _highestValue ? value : _highestValue;
        float _relatedValue = value / _highestValue;
        _bufferDecrease = _relatedValue > _relatedBufferedValue ? 0.005f : _bufferDecrease *= 1.1f;
        VizualizerValue = _relatedValue > _relatedBufferedValue ? _relatedValue : Mathf.Clamp(_relatedBufferedValue - _bufferDecrease, 0, 10);
    }



#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying || !_source)
            return;
        VirtualMicrophone = _vMic;
        Mute = _mute;
    }
#endif

    private void SetGlobal(bool on)
    {
        if (_normalSpeakerCurve != null)
        {
            _highestValue = 0;
            _source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, on ? _globalSpeakerCurve : _normalSpeakerCurve);
        }
    }

}
