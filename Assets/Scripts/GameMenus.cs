using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameMenus : MonoBehaviour{
    [SerializeField] private TeamStruct _localTeam;

    [SerializeField] private GameObject _mainMenu;
    [SerializeField] private GameObject _serverMenu;
    [SerializeField] private TMP_InputField _serverPortNumField;
    [SerializeField] private Button _serverMenuButton;
    private int _serverPortNum = 9999;
    [SerializeField] private GameObject _clientMenu;
    [SerializeField] private TMP_InputField _clientServerNameField;
    [SerializeField] private TMP_InputField _clientPortField;
    [SerializeField] private Button _clientMenuButton;
    private string _clientServerName = "";
    private int _clientPortNum = 9999;
    [SerializeField] private GameObject _serverConnectMenu;
    [SerializeField] private TMP_Text _serverConnectHost;
    [SerializeField] private TMP_Text _serverConnectPort;
    [SerializeField] private GameObject _clientConnectMenu;
    [SerializeField] private TMP_Text _clientConnectHost;
    [SerializeField] private TMP_Text _clientConnectPort;
    [SerializeField] private GameObject _winMenu;
    [SerializeField] private GameObject _loseMenu;

    void Start()
    {
        GoToMainMenu();
    }

    private void CloseAllMenus(){
        _mainMenu.SetActive(false);
        _serverMenu.SetActive(false);
        _clientMenu.SetActive(false);
        _serverConnectMenu.SetActive(false);
        _clientConnectMenu.SetActive(false);
        _winMenu.SetActive(false);
        _loseMenu.SetActive(false);
    }

    public void GoToMainMenu(){
        if(NetworkManager.Instance != null)
            NetworkManager.Instance.CloseConnection();
        CloseAllMenus();
        _mainMenu.SetActive(true);
    }

    public void GoToServerMenu(){
        NetworkManager.Instance.CloseConnection();
        NetworkManager.Instance.CloseConnections();
        CloseAllMenus();
        _serverMenu.SetActive(true);
    }

    public void GoToClientMenu(){
        NetworkManager.Instance.CloseConnection();
        NetworkManager.Instance.CloseConnections();
        CloseAllMenus();
        _clientMenu.SetActive(true);
    }

    public void ServerPortUpdateCallback(){
        if(int.TryParse(_serverPortNumField.text, out int portNum))
            _serverPortNum = portNum;
        else {

            _clientPortNum = 9999;
        }
    }
    public void ClientServerNameUpdateCallback(){
        _clientServerName = _clientServerNameField.text;
    }

    public void ClientPortUpdateCallback(){
        if(int.TryParse(_clientPortField.text, out int portNum)){
            _clientPortNum = portNum;

        }
        else {
            _clientPortNum = 9999;
        } 
    }
    
    public void GoToServerConnectMenu(){
        StartCoroutine(GoToServerConnectMenuAsync());
    }

    private IEnumerator GoToServerConnectMenuAsync(){
        NetworkManager.Instance.CloseConnection();
        CloseAllMenus();
        _serverConnectHost.text = $"Host: {Dns.GetHostName()}";
        _serverConnectPort.text = $"Port: {_serverPortNum.ToString()}";
        _serverConnectMenu.SetActive(true);
        
        Task<bool> task = NetworkManager.Instance.StartServer(_serverPortNum);
        yield return new WaitUntil(() => task.IsCompleted);
        if(task.Result){
            Task<bool> task2 = CombatManager.Instance.SubmitTeam(_localTeam, true);
            yield return new WaitUntil(() => task2.IsCompleted);
            if(!task2.Result){
                GoToServerMenu();
                yield break;
            }

            while(!CombatManager.Instance.HasEnemyTeamBeenSubmitted()){
                yield return new WaitForEndOfFrame();
            }
            CloseAllMenus();
            CombatManager.Instance.StartCombat();
        } else {
            GoToServerMenu();
        }
    }
    public void GoToClientConnectMenu(){
        StartCoroutine(GoToClientConnectMenuAsync());
    }

    private IEnumerator GoToClientConnectMenuAsync(){
        NetworkManager.Instance.CloseConnection();
        CloseAllMenus();
        _clientConnectHost.text = $"Host: {_clientServerName}";
        _clientConnectPort.text = $"Port: {_clientPortNum.ToString()}";
        _clientConnectMenu.SetActive(true);

        Task<bool> task = NetworkManager.Instance.ConnectBattle(_clientServerName, _clientPortNum);
        yield return new WaitUntil(() => task.IsCompleted);
        if(task.Result){
            Task<bool> task2 = CombatManager.Instance.SubmitTeam(_localTeam, true);
            yield return new WaitUntil(() => task2.IsCompleted);
            if(!task2.Result){
                GoToClientMenu();
                yield break;
            }

            while(!CombatManager.Instance.HasEnemyTeamBeenSubmitted()){
                yield return new WaitForEndOfFrame();
            }
            CloseAllMenus();
            CombatManager.Instance.StartCombat();
        } else {
            GoToClientMenu();
        }
    }
    
    public void GoToWinMenu(){
        NetworkManager.Instance.CloseConnection();
        CloseAllMenus();
        _winMenu.SetActive(true);
    }
    
    public void GoToLoseMenu(){
        NetworkManager.Instance.CloseConnection();
        CloseAllMenus();
        _loseMenu.SetActive(true);
    }
}
