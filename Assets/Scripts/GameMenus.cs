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
        Debug.Log("[NETWORK-DEBUG]: Starting server");
        yield return new WaitUntil(() => task.IsCompleted);
        if(task.Result){
            Debug.Log("[NETWORK-DEBUG]: Server started");
            Task<bool> task2 = CombatManager.Instance.SubmitTeam(_localTeam, true);
            Debug.Log("[NETWORK-DEBUG]: Sending team");
            yield return new WaitUntil(() => task2.IsCompleted);
            if(!task2.Result){
                Debug.Log("[NETWORK-DEBUG]: Team send fail");
                GoToServerMenu();
                yield break;
            }

            Debug.Log("[NETWORK-DEBUG]: Team send success, awaiting enemy team submit ");
            
            Debug.Log($"[COMBATANTS]: Has enemy been received: {CombatManager.Instance.HasEnemyTeamBeenSubmitted()}");

            int awaitingTeamStartTime = DateTime.Now.Millisecond;
            while(!CombatManager.Instance.HasEnemyTeamBeenSubmitted()){
                Debug.Log($"[COMBATANTS]: Has enemy been received: {CombatManager.Instance.HasEnemyTeamBeenSubmitted()}");

                if(DateTime.Now.Millisecond - awaitingTeamStartTime > 6000){
                    Debug.Log("[NETWORK-DEBUG]: Enemy team send timeout");
                    GoToServerMenu();
                    yield break;
                }

                yield return new WaitForEndOfFrame();
            }
            Debug.Log("[NETWORK-DEBUG]: Enemy team received, starting combat");
            CloseAllMenus();
            CombatManager.Instance.StartCombat();
        } else {
            Debug.Log("[NETWORK-DEBUG]: Server failed to start");
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
        Debug.Log("[NETWORK-DEBUG]: Attempt to connect to server");
        yield return new WaitUntil(() => task.IsCompleted);
        if(task.Result){
            Debug.Log("[NETWORK-DEBUG]: Successfully connected to server");
            Task<bool> task2 = CombatManager.Instance.SubmitTeam(_localTeam, true);
            Debug.Log("[NETWORK-DEBUG]: Sending team");
            yield return new WaitUntil(() => task2.IsCompleted);
            if(!task2.Result){
                Debug.Log("[NETWORK-DEBUG]: Team sending fail");
                GoToClientMenu();
                yield break;
            }

            Debug.Log("[NETWORK-DEBUG]: Team sending success, waiting for enemy team");
            
            Debug.Log($"[COMBATANTS]: Has enemy been received: {CombatManager.Instance.HasEnemyTeamBeenSubmitted()}");

            int awaitingTeamStartTime = DateTime.Now.Millisecond;
            while(!CombatManager.Instance.HasEnemyTeamBeenSubmitted()){
                Debug.Log($"[COMBATANTS]: Has enemy been received: {CombatManager.Instance.HasEnemyTeamBeenSubmitted()}");
                
                if(DateTime.Now.Millisecond - awaitingTeamStartTime > 6000){
                    Debug.Log("[NETWORK-DEBUG]: Enemy team timeout");
                    GoToClientMenu();
                    yield break;
                }
                
                yield return new WaitForEndOfFrame();
            }
            Debug.Log("[NETWORK-DEBUG]: Enemy team received, starting combat");
            CloseAllMenus();
            CombatManager.Instance.StartCombat();
        } else {
            Debug.Log("[NETWORK-DEBUG]: Server failed to start");
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
