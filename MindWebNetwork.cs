using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mirror;
using Vriendly.Player;
using System;

public class MindWebNetwork : NetworkBehaviour, IPlayerModule
{
    private static MindWebData _mindWebData;
    public static event Action<NodeData> OnNetworkCreateNode;
    public static event Action<string> OnNetworkDeleteNode;
    public static event Action<string, string> OnNetworkTextChange;
    public static event Action<string, Color> OnNetworkColorChange;
    public static event Action<string, NodeForm> OnNetworkShapeChange;
    public static event Action<string, Vector3, Quaternion> OnNetworkPositionChange;
    public static event Action<MindWebData> OnWebReceived;


    public void Initialize(PlayerUnit unit)
    {
        CmdGetWebData();
        MindWeb.OnCreateNode += OnNodeCreated;
        MindWeb.OnDeleteNode += OnNodeDeleted;
        MindWeb.OnTextChange += OnNodeTextChanged;
        MindWeb.OnColorChange += OnNodeColorChanged;
        MindWeb.OnPositionChange += OnNodePositionChanged;
        MindWeb.OnShapeChange += OnNodeShapeChanged;
    }

    [Command]
    private void CmdGetWebData()
    {
        if (_mindWebData != null)
        {
            RpcSetWebData(JsonUtility.ToJson(_mindWebData));
        }
        else
        {
            print($"MindWebData is set!");
            _mindWebData = new MindWebData();
        }
    }

    [ClientRpc]
    private void RpcSetWebData(string data)
    {
        if (isLocalPlayer)
        {
            OnWebReceived?.Invoke(JsonUtility.FromJson<MindWebData>(data));
        }
    }

    public void Deinitialize()
    {
        MindWeb.OnCreateNode -= OnNodeCreated;
        MindWeb.OnDeleteNode -= OnNodeDeleted;
        MindWeb.OnTextChange -= OnNodeTextChanged;
        MindWeb.OnColorChange -= OnNodeColorChanged;
        MindWeb.OnPositionChange -= OnNodePositionChanged;
        MindWeb.OnShapeChange -= OnNodeShapeChanged;
    }

    private void OnNodeCreated(NodeData node, bool master)
    {
        OnNodeCreatedCmd(JsonUtility.ToJson(node), master);
    }

    [Command]
    private void OnNodeCreatedCmd(string jNode, bool master)
    {
        print($"MindWebData is here : {_mindWebData != null}");
        var data = JsonUtility.FromJson<NodeData>(jNode);
        if (master)
        {
            if (_mindWebData._masterNode == null)
            {
                _mindWebData._masterNode = data;
            }
            return;
        }
        if (!_mindWebData._nodesList.Contains(data))
        {
            _mindWebData._nodesList.Add(data);
        }
        if (!master)
            OnNodeCreatedRpc(jNode);
    }

    [ClientRpc]
    private void OnNodeCreatedRpc(string jNode)
    {
        if (!isLocalPlayer)
            OnNetworkCreateNode?.Invoke(JsonUtility.FromJson<NodeData>(jNode));
    }

    private void OnNodeDeleted(string nodeID)
    {
        OnNodeDeletedCmd(nodeID);
    }

    [Command]
    private void OnNodeDeletedCmd(string nodeID)
    {
        NodeData nodeToRemove = null;
        foreach (NodeData node in _mindWebData._nodesList)
        {
            if (node._iD == nodeID)
            {
                nodeToRemove = node;
            }
        }
        //string jNodeToRmove = JsonUtility.ToJson(nodeToRemove);
        foreach (NodeData node in _mindWebData._nodesList)
        {
            if (JsonUtility.FromJson<NodeData>(node._parentNode)._iD == nodeToRemove._iD)
            {
                node._parentNode = nodeToRemove._parentNode;
            }
        }
        if (nodeToRemove != null)
            _mindWebData._nodesList.Remove(nodeToRemove);
        OnNodeDeletedRpc(nodeID);
    }

    [ClientRpc]
    private void OnNodeDeletedRpc(string nodeID)
    {
        if (!isLocalPlayer)
            OnNetworkDeleteNode?.Invoke(nodeID);
    }

    private void OnNodeTextChanged(string nodeID, string text)
    {
        OnNodeTextChangedCmd(nodeID, text);
    }

    [Command]
    private void OnNodeTextChangedCmd(string nodeID, string text)
    {
        if (string.IsNullOrEmpty(nodeID))
        {
            _mindWebData._masterNode._text = text;
        }
        else
            foreach (NodeData node in _mindWebData._nodesList)
            {
                if (node._iD == nodeID)
                {
                    node._text = text;
                }
            }
        OnNodeTextChangedRpc(nodeID, text);
    }

    [ClientRpc]
    private void OnNodeTextChangedRpc(string nodeID, string text)
    {
        if (!isLocalPlayer)
            OnNetworkTextChange?.Invoke(nodeID, text);
    }

    private void OnNodeColorChanged(string nodeID, Color color)
    {
        OnNodeColorChangedCmd(nodeID, color);
    }

    [Command]
    private void OnNodeColorChangedCmd(string nodeID, Color color)
    {
        if (string.IsNullOrEmpty(nodeID))
        {
            _mindWebData._masterNode._color = color;
        }
        else
            foreach (NodeData node in _mindWebData._nodesList)
            {
                if (node._iD == nodeID)
                {
                    node._color = color;
                }
            }
        OnNodeColorChangedRpc(nodeID, color);
    }

    [ClientRpc]
    private void OnNodeColorChangedRpc(string nodeID, Color color)
    {
        if (!isLocalPlayer)
            OnNetworkColorChange?.Invoke(nodeID, color);
    }

    private void OnNodeShapeChanged(string nodeID, NodeForm form)
    {
        OnNodeShapeChangedCmd(nodeID, form);
    }

    [Command]
    private void OnNodeShapeChangedCmd(string nodeID, NodeForm form)
    {
        if (string.IsNullOrEmpty(nodeID))
        {
            _mindWebData._masterNode._shape = form;
        }
        else
            foreach (NodeData node in _mindWebData._nodesList)
            {
                if (node._iD == nodeID)
                {
                    node._shape = form;
                }
            }
        OnNodeShapeChangedRpc(nodeID, form);
    }

    [ClientRpc]
    private void OnNodeShapeChangedRpc(string nodeID, NodeForm form)
    {
        if (!isLocalPlayer)
            OnNetworkShapeChange?.Invoke(nodeID, form);
    }

    private void OnNodePositionChanged(string nodeID, Vector3 position, Quaternion rotation)
    {
        OnNodePositionChangedCmd(nodeID, position, rotation);
    }

    [Command]
    private void OnNodePositionChangedCmd(string nodeID, Vector3 position, Quaternion rotation)
    {
        if (string.IsNullOrEmpty(nodeID))
        {
            _mindWebData._masterNode._position = position;
            _mindWebData._masterNode._rotation = rotation;
        }
        else
            foreach (NodeData node in _mindWebData._nodesList)
            {
                if (node._iD == nodeID)
                {
                    node._position = position;
                    node._rotation = rotation;
                }
            }
        OnNodePositionChangedRpc(nodeID, position, rotation);
    }

    [ClientRpc]
    private void OnNodePositionChangedRpc(string nodeID, Vector3 position, Quaternion rotation)
    {
        if (!isLocalPlayer)
            OnNetworkPositionChange?.Invoke(nodeID, position, rotation);
    }

}
