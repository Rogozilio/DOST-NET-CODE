using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIMenuNetwork : MonoBehaviour
{
    public Button buttonHost;
    public Button buttonClient;
    public Button buttonServer;
    public TMP_InputField inputField;
    
    private NetworkManager _networkManager;

    private void Awake()
    {
        _networkManager = FindObjectOfType<NetworkManager>();
        
        inputField.text = _networkManager.networkAddress;
        buttonHost.onClick.AddListener(() => _networkManager.StartHost());
        buttonClient.onClick.AddListener(() => _networkManager.StartClient());
        buttonServer.onClick.AddListener(() => _networkManager.StartServer());
        inputField.onValueChanged.AddListener((text) => _networkManager.networkAddress = text);
    }

    private void Update()
    {
        if (NetworkClient.isConnected || NetworkServer.active) return;
        
        if (Input.GetKeyUp(KeyCode.H))
        {
            buttonHost.onClick.Invoke();
        }
        
        if (Input.GetKeyUp(KeyCode.J))
        {
            buttonClient.onClick.Invoke();
        }
        
        if (Input.GetKeyUp(KeyCode.C))
        {
            buttonServer.onClick.Invoke();
        }
    }
}