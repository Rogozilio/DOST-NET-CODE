using System.Collections.Generic;
using System.IO;
using kcp2k;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMenuNetwork : MonoBehaviour
{
    public Button buttonHost;
    public Button buttonClient;
    public Button buttonServer;
    public TMP_InputField inputField;
    
    private NetworkManager _networkManager;
    private KcpTransport _kcpTransport;
    private Dictionary<string, string> _config;

    private void Awake()
    {
        _networkManager = FindObjectOfType<NetworkManager>();
        _kcpTransport = _networkManager.GetComponent<KcpTransport>();
        _config = new Dictionary<string, string>();
        
        inputField.text = _networkManager.networkAddress;
        buttonHost.onClick.AddListener(() => _networkManager.StartHost());
        buttonClient.onClick.AddListener(() => _networkManager.StartClient());
        buttonServer.onClick.AddListener(() => _networkManager.StartServer());
        //inputField.onValueChanged.AddListener((text) => _networkManager.networkAddress = text);
        loadStreamingAsset("Config.txt");
        _networkManager.networkAddress = _config["IP"];
        if(ushort.TryParse(_config["PORT"], out ushort port))
            _kcpTransport.port = port;
        else
            Debug.LogError("Not get parse port");
    }
    
    
    private void loadStreamingAsset(string fileName)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);

        string result = File.ReadAllText(filePath);

        foreach (var field in result.Split('\n'))
        {
            var key = field.Split(':')[0].Trim();
            var value = field.Split(':')[1].Trim();
            _config.Add(key, value);
        }
    }

    private void Update()
    {
        if (NetworkClient.isConnected || NetworkServer.active) return;
        
        if (Application.platform == RuntimePlatform.WindowsServer 
            || Application.platform == RuntimePlatform.LinuxServer
            || Application.platform == RuntimePlatform.OSXServer)
        {
            buttonServer.onClick.Invoke();
        }    
        
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