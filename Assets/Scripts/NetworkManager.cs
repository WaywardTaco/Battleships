using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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

    private bool _isServer = false;
    private bool _shouldCloseConnection = false;
    private bool _appHandshakeVerified = false;
    
    /* GAMEPLAY LOGIC RELATED */
    public void SendTeam(string data){
        if (string.IsNullOrEmpty(data)) return;
        string toSend = TEAM + data + EOM;

        SendData(toSend);
    }

    public void SendMoves(string data){
        if (string.IsNullOrEmpty(data)) return;
        string toSend = MOVES + data + EOM;

        SendData(toSend);
    }

    public void SendBattleStatus(string data){
        if (string.IsNullOrEmpty(data)) return;
        string toSend = STATUS + data + EOM;

        SendData(toSend);
    }

    /* CONNECTION RELATED */

    /// <summary>
    /// External interface to tell the network manager to close the connection
    /// </summary>
    public void CloseConnection(){
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
        _isServer = true;
        Debug.Log($"[NETWORK-DEBUG]: Established server at {_welcomeSocket.LocalEndPoint} (Port: {portNumber})");

        // Run Server asyncronously
        RunServer();

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
        _isServer = false;

        // Exchange Application Layer handshake
        _appHandshakeVerified = await SendHandshake(SYN);
        if(!_appHandshakeVerified){
            Debug.LogWarning($"[NETWORK-WARN]: Failed server connection to {_connectionSocket.RemoteEndPoint} (Port: {portNumber})");
            CleanupConnections();
            return false;
        }

        Debug.Log($"[NETWORK-DEBUG]: Established server connection to {_connectionSocket.RemoteEndPoint} (Port: {portNumber})");

        // Run Client asyncronously
        RunClient();

        return true;   
    }

    /* PRIVATE FUNCTIONS */
    private async Task RunServer(){
        // Runs server until connection close flag is called
        while(!_shouldCloseConnection){
            // Listen to one connection request, assign to connection socket, and verify application level handshake
            _welcomeSocket.Listen(1);
            _connectionSocket = await _welcomeSocket.AcceptAsync();

            // Reads messages from the server and processes them one by one as long as connection should not close and there is a connection
            while(!_shouldCloseConnection && _connectionSocket != null){
                await ProcessServer(await ReadMessage());
            }
        }
        
        // Closing server welcome socket
        Debug.Log($"[NETWORK-DEBUG]: Closing server at {_welcomeSocket.LocalEndPoint}");
        await SendCloseConnectionHandshake();
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
        _connectionSocket.Shutdown(SocketShutdown.Both);
        CleanupConnections();
    }

    private async Task SendCloseConnectionHandshake(){
        if(await SendHandshake(FIN)){
            Debug.Log($"[NETWORK-DEBUG]: Connection properly terminated to {_connectionSocket.RemoteEndPoint}");
            await SendData(ACK, false);
        } else {
            Debug.Log($"[NETWORK-DEBUG]: Unable to properly close connection to {_connectionSocket.RemoteEndPoint}");

        }
    }

    /* PROCESSING MESSAGES RECEIVED */
    private async Task ProcessServer(string response){
        if(!_appHandshakeVerified){
            if(response.CompareTo(SYN) == 0)
                _appHandshakeVerified = await SendData(SYN_ACK);
            
            return;
        }

        if(response.CompareTo(FIN) == 0){
            await SendData(FIN_ACK);
            CloseConnection();
            return;
        }

        if(response.CompareTo(ACK) == 0) return;
        SendData(ACK, false);

        /* SERVER SIDE RECEIVING GAME LOGIC */
        switch(GetMessagePrefix(ref response)){
            case TEAM:
                break;
            case MOVES:
                break;
        }

        Debug.Log($"[NETWORK-DEBUG]: Response was \"{response}\"");

        return;
    }
    
    private async Task ProcessClient(string response){
        if(!_appHandshakeVerified) return;

        if(response.CompareTo(FIN) == 0){
            await SendData(FIN_ACK);
            CloseConnection();
            return;
        }

        if(response.CompareTo(ACK) == 0) return;
        SendData(ACK, false);
        
        /* CLIENT SIDE RECEIVING GAME LOGIC */
        switch(GetMessagePrefix(ref response)){
            case TEAM:
                CombatManager.Instance.SubmitTeam(response, false);
                break;
            case MOVES:
                CombatManager.Instance.SubmitEnemyMoves(response);
                break;
            case STATUS:
                CombatManager.Instance.UpdateStatus(response);
                break;
        }

        Debug.Log($"[NETWORK-DEBUG]: Response was \"{response}\"");

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

        int sendAttempts = 1;
        while(sendAttempts <= MAX_SEND_DATA_RETRIES){
            _ = await _connectionSocket.SendAsync(dataBytes, SocketFlags.None);
            Debug.Log($"[NETWORK-DEBUG]: Attempt send message to connection \"{data}\"");

            if(!awaitACK) return true;

            int dataSentTime = DateTime.Now.Millisecond;
            while(DateTime.Now.Millisecond - dataSentTime < CONNECTION_TIMEOUT_LIMIT){
                string response = await ReadMessage();

                if(response.CompareTo(ACK) == 0){
                    Debug.Log($"[NETWORK-DEBUG]: Successful message sent to connection \"{data}\"");
                    return true;
                }

                if(_isServer)   await ProcessServer(response);
                else            await ProcessClient(response);
            }

            sendAttempts++;
        }

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
                    await SendData(ACK);
                    return true;
                } 
                if(handshakeType.CompareTo(FIN) == 0 && response.CompareTo(FIN_ACK) == 0){
                    Debug.Log($"[NETWORK-DEBUG]: Successful terminated connection");
                    await SendData(ACK);
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
            Debug.Log("[NETWORK-DEBUG]: Received invalid response, include <|EOM|> to message");
            return null;
        }

        response.Replace(EOM, "");

        return response;
    }

    /* HELPER FUNCTIONS */
    private async Task<IPEndPoint> CreateEndpoint(string hostname, int portNumber){
        IPHostEntry ipHostInfo = await Dns.GetHostEntryAsync(hostname);
        IPAddress ipAddress = ipHostInfo.AddressList[1];
        IPEndPoint endpoint = new(ipAddress, portNumber);

        return endpoint;
    }

    private void CleanupConnections(){
        _connectionSocket = null;
        
        _welcomeSocket = null;
        _isServer = false;
        _appHandshakeVerified = false;
    
        _shouldCloseConnection = false;
    }

    private string GetMessagePrefix(ref string data){
        
        if (data.IndexOf(TEAM) > -1){
            Debug.Log("[NETWORK-DEBUG]: Team submit message received");
            data.Replace(TEAM, "");
            return TEAM;
        }
        
        if (data.IndexOf(MOVES) > -1){
            Debug.Log("[NETWORK-DEBUG]: Moves submit message received");
            data.Replace(MOVES, "");
            return MOVES;
        }
        
        if (data.IndexOf(STATUS) > -1){
            Debug.Log("[NETWORK-DEBUG]: Status submit message received");
            data.Replace(STATUS, "");
            return STATUS;
        }

        return null;
    }

    /* SINGLETON CODE */
    public static NetworkManager Instance { get; private set;}
    async Task Start()
    {
        if(Instance == null){
            Instance = this;
        } else {
            Destroy(this);
        }

        var hostname = Dns.GetHostName();
        IPHostEntry localhost = await Dns.GetHostEntryAsync(hostname);
        IPAddress address = localhost.AddressList[0];

        Debug.Log($"[SERVER-DEBUG]: {hostname}, {localhost.HostName}, {address}, {localhost.AddressList[1]}");
    }
    void OnDestroy()
    {
        Instance = this;
    }
}
