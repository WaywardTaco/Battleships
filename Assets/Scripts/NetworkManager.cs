using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;

public class NetworkManager : MonoBehaviour
{
    private Socket _connectionSocket;
    private Socket _welcomeSocket;
    private const string SYN = "<|SYN|>";
    private const string SYN_ACK = "<|SYN_ACK|>";
    private const string FIN = "<|FIN|>";
    private const string FIN_ACK = "<|FIN_ACK|>";
    private const string ACK = "<|ACK|>";
    public const string EOM = "<|EOM|>";
    public const string TEAM = "<|TEAM|>";
    public const string MOVES = "<|MOVES|>";
    public const string STATUS = "<|STATUS|>";

    private const int MAX_SEND_DATA_RETRIES = 3;
    private const int CONNECTION_TIMEOUT_LIMIT = 60000;

    public bool IsServer = false;
    private bool _shouldCloseConnection = false;
    private bool _appHandshakeVerified = false;
    private int _receivedAckCount = 0;
    private int _sentMessagesCount = 0;
    private int _failedMessagesCount = 0;

    /* GAMEPLAY LOGIC RELATED */
    public async Task<bool> SendTeamAsync(TeamStruct team){
        if(team == null) return false;
        string data = JsonConvert.SerializeObject(team, Formatting.None);

        if (string.IsNullOrEmpty(data)) return false;
        string toSend = TEAM + data + EOM;

        return await SendData(toSend);
    }

    public async Task SendMoves(MovesSubmissionStruct moves){
        if(moves == null) return;
        string data = JsonConvert.SerializeObject(moves, Formatting.None);
        Debug.Log($"[NETWORK-DEBUG]: Sending moves {data}");

        if (string.IsNullOrEmpty(data)) return;
        string toSend = MOVES + data + EOM;

        await SendData(toSend);
    }

    public async Task SendBattleStatusAsync(BattleStatusStruct battleStatus){
        if(battleStatus == null) return;
        string data = JsonConvert.SerializeObject(battleStatus, Formatting.None);

        if (string.IsNullOrEmpty(data)) return;
        string toSend = STATUS + data + EOM;

        await SendData(toSend);
    }

    /* CONNECTION RELATED */

    /// <summary>
    /// External interface to tell the network manager to close the connection
    /// </summary>
    public void CloseConnection(){
        if(IsConnectionStarted)
            _shouldCloseConnection = true;
    }

    /// <summary>
    /// Start this instance of the game's network manager as a server
    /// </summary>
    /// <param name="portNumber">The port to run the server on</param>
    /// <returns>Returns true if a server was successfully created</returns>
    public async Task<bool> StartServer(int portNumber){

        // Create endpoint then socket on this own system's Address, then bind
        IPEndPoint endpoint = await CreateEndpoint(Dns.GetHostName(), portNumber);
        if(endpoint == null){
            Debug.LogWarning($"[NETWORK-WARN]: Failed to create endpoint for {Dns.GetHostName()} (Port: {portNumber})");
            return false;
        }
        _welcomeSocket =  new (endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        try {
            _welcomeSocket.Bind(endpoint);
        } catch (Exception e) {
            Debug.LogWarning($"[NETWORK-WARN]: Welcome socket error: {e.ToString()}");
            CleanupConnections();
            return false;
        }

        // Set the network manager's status as a server
        IsServer = true;
        Debug.Log($"[NETWORK-DEBUG]: Established server at {_welcomeSocket.LocalEndPoint} (Port: {portNumber})");
        
        // Listen to one connection request, assign to connection socket, and verify application level handshake
        _welcomeSocket.Listen(1);
        Task<Socket> task = _welcomeSocket.AcceptAsync();
        try{
            await task;
        } catch {
            return false;
        }
        
        if(task.IsCompletedSuccessfully)
            _connectionSocket = task.Result;

        while(!_appHandshakeVerified){
            await ProcessServer(await ReadMessage());
        }

        // Run Server asyncronously
        _ = Task.Run(() => RunServer());

        return true;   
    }

    /// <summary>
    /// Start the instance of the game's network manager as a client to a server
    /// </summary>
    /// <param name="hostname">The host name of the server</param>
    /// <param name="portNumber">The port on the server to find the server</param>
    /// <returns></returns>
    public async Task<bool> ConnectBattle(string hostname, int portNumber){
        
        // Create an endpoint to the server and attach it to connection of the client
        IPEndPoint endpoint = await CreateEndpoint(hostname, portNumber);
        if(endpoint == null){
            Debug.LogWarning($"[NETWORK-WARN]: Failed to create endpoint for {Dns.GetHostName()} (Port: {portNumber})");
            return false;
        }
        _connectionSocket = new (endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        try {
            await _connectionSocket.ConnectAsync(endpoint);
        } catch (Exception e) {
            Debug.LogWarning($"[NETWORK-WARN]: Client connection error: {e.ToString()}");
            CleanupConnections();
            return false;
        }

        // Set the network manager's status as client
        IsServer = false;

        // Exchange Application Layer handshake
        _appHandshakeVerified = await SendHandshake(SYN);
        if(!_appHandshakeVerified){
            Debug.LogWarning($"[NETWORK-WARN]: Failed server connection to {_connectionSocket.RemoteEndPoint} (Port: {portNumber})");
            CleanupConnections();
            return false;
        }

        Debug.Log($"[NETWORK-DEBUG]: Established server connection to {_connectionSocket.RemoteEndPoint} (Port: {portNumber})");

        // Run Client asyncronously
        _ = Task.Run(() => RunClient());

        return true;   
    }

    /* PRIVATE FUNCTIONS */
    private async Task RunServer(){
        // Runs server until connection close flag is called
        // while(!_shouldCloseConnection){

            // Reads messages from the server and processes them one by one as long as connection should not close and there is a connection
            while(!_shouldCloseConnection && _connectionSocket != null){
                await ProcessServer(await ReadMessage());
            }
        // }
        
        // Closing server welcome socket
        Debug.Log($"[NETWORK-DEBUG]: Closing server at {_welcomeSocket.LocalEndPoint}");
        await SendCloseConnectionHandshake();
        _welcomeSocket.Close();
        CleanupConnections();
    }

    private async Task RunClient(){

        // Keep reading messages and processing the client as long as connection exists and connection close flag not set
        while(!_shouldCloseConnection && _connectionSocket != null){
            await ProcessClient(await ReadMessage());
        }

        // Closing connection socket
        Debug.Log($"[NETWORK-DEBUG]: Closing connection to server via {_connectionSocket.RemoteEndPoint}");
        await SendCloseConnectionHandshake();
        _connectionSocket.Close();
        CleanupConnections();
    }

    private async Task SendCloseConnectionHandshake(){
        if(_connectionSocket == null || !_appHandshakeVerified){
            Debug.Log("[NETWORK]: No connection attached");
            return;
        }
        

        if(await SendHandshake(FIN)){
            Debug.Log($"[NETWORK-DEBUG]: Connection properly terminated to {_connectionSocket.RemoteEndPoint}");
            await SendData(ACK, false);
        } else {
            Debug.Log($"[NETWORK-DEBUG]: Unable to properly close connection to {_connectionSocket.RemoteEndPoint}");

        }
    }

    /* PROCESSING MESSAGES RECEIVED */
    private async Task ProcessServer(string response){
        Debug.Log($"[NETWORK-DEBUG]: Processing server received: {response}");
        if(response.CompareTo(ACK) == 0){
            _receivedAckCount++;
            return;
        }

        if(!_appHandshakeVerified){

            Debug.Log($"[NETWORK-DEBUG]: App handshake not yet established");
            if(response.CompareTo(SYN) == 0){
                Task<bool> task = SendData(SYN_ACK);
                await ProcessServer(await ReadMessage());
                await task;
                _appHandshakeVerified = task.Result;
            }
            
            return;
        }

        if(response.CompareTo(FIN) == 0){
            await SendData(FIN_ACK);
            CloseConnection();
            return;
        }

        _ = SendData(ACK, false);

        /* SERVER SIDE RECEIVING GAME LOGIC */
        string prefix = GetMessagePrefix(response);
        Debug.Log($"[NETWORK-DEBUG]: Processing server received message type: {prefix}");
        if(prefix == null) return;
        
        string trimmedResponse = TrimMessagePrefix(response, prefix);
        Debug.Log($"[NETWORK-DEBUG]: Processing server received: {trimmedResponse}");
        switch(prefix){
            case TEAM:
                
                Debug.Log($"[NETWORK-DEBUG]: Response was \"{trimmedResponse}\"");
                TeamStruct team = null;
                team = JsonConvert.DeserializeObject<TeamStruct>(trimmedResponse);
                if(team == null) break;

                _ = CombatManager.Instance.SubmitTeam(team, false);
                break;
            case MOVES:

                Debug.Log($"[NETWORK-DEBUG]: Response was \"{trimmedResponse}\"");
                MovesSubmissionStruct moves = null;
                moves = JsonConvert.DeserializeObject<MovesSubmissionStruct>(trimmedResponse);
                if(moves == null) break;

                CombatManager.Instance.SubmitEnemyMoves(moves);
                break;
        }


        return;
    }
    
    private async Task ProcessClient(string response){
        Debug.Log($"[NETWORK-DEBUG]: Processing client received: {response}");

        if(response.CompareTo(FIN) == 0){
            await SendData(FIN_ACK);
            CloseConnection();
            return;
        }

        if(response.CompareTo(ACK) == 0){
            _receivedAckCount++;
            return;
        }

        if(!_appHandshakeVerified) return;
        _ = SendData(ACK, false);
        
        /* CLIENT SIDE RECEIVING GAME LOGIC */
        string prefix = GetMessagePrefix(response);
        if(prefix == null) return;
        
        string trimmedResponse = TrimMessagePrefix(response, prefix);
        switch(prefix){
            case TEAM:
                Debug.Log($"[NETWORK-DEBUG]: Response was \"{trimmedResponse}\"");
                
                TeamStruct team = null;
                team = JsonConvert.DeserializeObject<TeamStruct>(trimmedResponse);
                if(team == null) break;

                _ = CombatManager.Instance.SubmitTeam(team, false);
                break;
            case MOVES:
                Debug.Log($"[NETWORK-DEBUG]: Response was \"{trimmedResponse}\"");
                
                MovesSubmissionStruct moves = null;
                moves = JsonConvert.DeserializeObject<MovesSubmissionStruct>(trimmedResponse);
                if(moves == null) break;

                CombatManager.Instance.SubmitEnemyMoves(moves);
                break;
            case STATUS:
                Debug.Log($"[NETWORK-DEBUG]: Response was \"{trimmedResponse}\"");
                
                BattleStatusStruct battleStatus = null;
                battleStatus = JsonConvert.DeserializeObject<BattleStatusStruct>(trimmedResponse);
                if(battleStatus == null) break;

                CombatManager.Instance.UpdateStatus(battleStatus);
                break;
        }

        Debug.Log($"[NETWORK-DEBUG]: Response was \"{trimmedResponse}\"");

        return;
    }

    /* DATA SENDING FUNCTIONS */

    /// <summary>
    /// Sends data to connection
    /// </summary>
    /// <param name="data">The data itself in string format</param>
    /// <param name="awaitACK">Whether or not the data should check if successful ACK (if false the return is always true)</param>
    /// <returns>Whether or not the data was successfully acknowledged</returns>
    private async Task<bool> SendData(string data, bool awaitACK = true){
        if(_connectionSocket == null){
            Debug.LogWarning("[NETWORK-WARN]: Attempted to send data to a null connection");
            return false;
        }

        var dataBytes = Encoding.UTF8.GetBytes(data);

        _sentMessagesCount++;

        int sendAttempts = 1;
        while(sendAttempts <= MAX_SEND_DATA_RETRIES){
            _ = await _connectionSocket.SendAsync(dataBytes, SocketFlags.None);
            Debug.Log($"[NETWORK-DEBUG]: Attempt send message to connection \"{data}\"");

            if(!awaitACK){
                _receivedAckCount++;
                return true;
            }

            int dataSentTime = DateTime.Now.Millisecond;
            while(DateTime.Now.Millisecond - dataSentTime < CONNECTION_TIMEOUT_LIMIT){
                await Task.Delay(100); 
                // string response = await ReadMessage();

                // if(response.CompareTo(ACK) == 0){
                //     Debug.Log($"[NETWORK-DEBUG]: ACK Received for {data}");
                //     _receivedAckCount++;
                // } else {
                //     if(IsServer)    await ProcessServer(response);
                //     else            await ProcessClient(response);
                // }

                if(_receivedAckCount >= _sentMessagesCount - _failedMessagesCount){
                    Debug.Log($"[NETWORK-DEBUG]: Successful message (ACK: {_receivedAckCount}, SENT: {_sentMessagesCount}, FAIL: {_failedMessagesCount}) sent to connection \"{data}\"");
                    return true;
                }
            }

            sendAttempts++;
        }

        _failedMessagesCount++;
        return false;
    }

    private async Task<bool> SendHandshake(string handshakeType){

        int sendAttempts = 1;
        while(sendAttempts <= MAX_SEND_DATA_RETRIES){
            if(!await SendData(handshakeType, false))
                return false;

            int dataSentTime = DateTime.Now.Millisecond;
            while(DateTime.Now.Millisecond - dataSentTime < CONNECTION_TIMEOUT_LIMIT){
                string response = await ReadMessage();

                if(handshakeType.CompareTo(SYN) == 0 && response.CompareTo(SYN_ACK) == 0){
                    Debug.Log($"[NETWORK-DEBUG]: Successful connection made");
                    await SendData(ACK, false);
                    return true;
                } 

                if(handshakeType.CompareTo(FIN) == 0 && response.CompareTo(FIN_ACK) == 0){
                    Debug.Log($"[NETWORK-DEBUG]: Successful terminated connection");
                    await SendData(ACK, false);
                    return true;
                }
            }

            sendAttempts++;
        }

        return false;
    }

    /// <summary>
    /// Reads and returens a piece of data from its connection. Already removes EOM at the end of messages
    /// </summary>
    /// <returns>Returns the data read if valid, null if otherwise </returns>
    private async Task<string> ReadMessage(){
        Debug.Log("[NETWORK-DEBUG]: Awaiting some message begin");

        var buffer = new byte[1_024];
        var received = await _connectionSocket.ReceiveAsync(buffer, SocketFlags.None);
        var response = Encoding.UTF8.GetString(buffer, 0, received);

        if(response.CompareTo(ACK) == 0){
            Debug.Log("[NETWORK-DEBUG]: Received ACK message");
            return ACK;
        }

        if(response.CompareTo(SYN) == 0){
            Debug.Log("[NETWORK-DEBUG]: Received SYN message");
            return SYN;
        }

        if(response.CompareTo(SYN_ACK) == 0){
            Debug.Log("[NETWORK-DEBUG]: Received SYN_ACK message");
            return SYN_ACK;
        }

        if(response.CompareTo(FIN) == 0){
            Debug.Log("[NETWORK-DEBUG]: Received FIN message");
            return FIN;
        }

        if(response.CompareTo(FIN_ACK) == 0){
            Debug.Log("[NETWORK-DEBUG]: Received FIN_ACK message");
            return FIN_ACK;
        }

        if (response.IndexOf(EOM) <= -1){
            Debug.Log($"[NETWORK-DEBUG]: Received invalid response, include <|EOM|> to message: {response}");
            return null;
        }

        string modifiedResponse = response.Remove(response.IndexOf(EOM), EOM.Length);
        
        Debug.Log($"[NETWORK-DEBUG]: Read message: {response}");

        return modifiedResponse;
    }

    /* HELPER FUNCTIONS */
    private async Task<IPEndPoint> CreateEndpoint(string hostname, int portNumber){
        IPHostEntry ipHostInfo = await Dns.GetHostEntryAsync(hostname);
        IPAddress ipAddress = ipHostInfo.AddressList[1];
        IPEndPoint endpoint = new(ipAddress, portNumber);

        return endpoint;
    }

    public void CloseConnections(){
        _connectionSocket?.Close();
        _welcomeSocket?.Close();
    }

    private void CleanupConnections(){
        Debug.Log("[NETWORK-DEBUG]: Cleaning up connections");
        _connectionSocket = null;
        
        _welcomeSocket = null;
        
        IsServer = false;
        _appHandshakeVerified = false;
    
        _shouldCloseConnection = false;
        _receivedAckCount = 0;

        _failedMessagesCount = 0;
        _sentMessagesCount = 0;

    }

    private bool IsConnectionStarted{
        get {
            return 
                (IsServer && _welcomeSocket != null) ||
                _connectionSocket != null;
        }
    }

    private string GetMessagePrefix(string data){
        
        if (data.IndexOf(TEAM) > -1){
            Debug.Log("[NETWORK-DEBUG]: Team submit message received");
            return TEAM;
        }
        
        if (data.IndexOf(MOVES) > -1){
            Debug.Log("[NETWORK-DEBUG]: Moves submit message received");
            return MOVES;
        }
        
        if (data.IndexOf(STATUS) > -1){
            Debug.Log("[NETWORK-DEBUG]: Status submit message received");
            return STATUS;
        }

        return null;
    }

    private string TrimMessagePrefix(string data, string prefix){
        return data.Remove(data.IndexOf(prefix), prefix.Length);
    }

    /* SINGLETON CODE */
    public static NetworkManager Instance { get; private set;}
    void Start()
    {
        if(Instance == null){
            Instance = this;
        } else {
            Destroy(this);
        }

        CleanupConnections();
    }
    void OnDestroy()
    {
        Instance = this;
    }
}
