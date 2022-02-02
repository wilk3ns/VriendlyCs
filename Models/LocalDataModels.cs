using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vriendly.Backend.Models

{

    [Serializable]
    public class AvatarModel
    {
        public PlayerGender _gender;
        public Color _skinColor, _roboColor, _hairColor;
        public Material _skinMaterial;
        public Texture _shirtTextureM, _shirtTextureF;

        public AvatarModel(SerializableAvatarModel model)
        {
            _gender = model._gender;
            _skinColor = model._skinColor;
            _roboColor = model._roboColor;
            _hairColor = model._hairColor;
            _skinMaterial = (Material)Resources.Load($"Customization/BodyMaterials/Materials/{model._skinMaterial}");
            _shirtTextureM = (Texture)Resources.Load($"Customization/TshirtTextures/Male/{model._shirtTextureM}");
            if (_shirtTextureM == null) _shirtTextureM = (Texture)Resources.Load($"Customization/TshirtTextures/Unisex/{model._shirtTextureM}");
            _shirtTextureF = (Texture)Resources.Load($"Customization/TshirtTextures/Female/{model._shirtTextureF}");
            if (_shirtTextureF == null) _shirtTextureF = (Texture)Resources.Load($"Customization/TshirtTextures/Unisex/{model._shirtTextureF}");
        }

        public void Copy(AvatarModel profile)
        {
            _gender = profile._gender;
            _skinColor = profile._skinColor;
            _roboColor = profile._roboColor;
            _hairColor = profile._hairColor;
            _skinMaterial = profile._skinMaterial;
            _shirtTextureM = profile._shirtTextureM;
            _shirtTextureF = profile._shirtTextureF;
        }
    }

    [Serializable]
    public class Credentials
    {
        public string _email;
        public string _password;
    }

    [Serializable]
    public class SerializableAvatarModel
    {
        public PlayerGender _gender;
        public Color _skinColor, _roboColor, _hairColor;
        public string _skinMaterial;
        public string _shirtTextureM, _shirtTextureF;

        public SerializableAvatarModel(AvatarModel model)
        {
            _gender = model._gender;
            _skinColor = model._skinColor;
            _roboColor = model._roboColor;
            _hairColor = model._hairColor;
            _skinMaterial = model._skinMaterial.name;
            _shirtTextureM = model._shirtTextureM.name;
            _shirtTextureF = model._shirtTextureF.name;
        }
    }

    [Serializable]
    public class Place
    {
        public string ID;
        public string placeTitle;
        public string placeDescription;
        public string placeThumbnail;
    }

    [Serializable]
    public class Avatar
    {
        public int Head;
        public int Body;
        public int Sex;
    }

    [Serializable]
    public class Event
    {
        public string eventName;
        public string eventCode;
        public string placeTitle;
        public int PlaceID;
    }

    [Serializable]
    public class User
    {
        public string fullname;
        public bool activated;
        public string email;
        public string token;
        public string userType;
        public string planType;
        public DateTime expiry;
        public int hostCapacity;
        public string avatar;
        public List<Event> events;
    }

    [Serializable]
    public class AnalyticsResponse
    {
        public bool status;
        public string message;
    }


    [Serializable]
    public class LoginDataRoot
    {
        public bool status;
        public string message;
        public List<Place> places;
        public User user;
    }

    [Serializable]
    public class Version
    {
        public bool forceUpdate;
        public string version;
        public int buildNumber;
        public string windwosLink;
        public string linuxLink;
        public string macLink;
        public string masterServerIP;
    }

    [Serializable]
    public class VersionDataRoot
    {
        public bool status;
        public string message;
        public List<Version> versions;
    }

    [Serializable]
    public class EventResponse
    {
        public bool status;
        public string message;
        public string eventCode;
    }

    [Serializable]
    public class GetEventResponse
    {
        public bool status;
        public string message;
        public List<Event> events;
    }

    [Serializable]
    public class DelEventResponse
    {
        public bool status;
        public string message;
    }
}