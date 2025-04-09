using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NetworkHeaderType {
    SYN, SYN_ACK, FIN, FIN_ACK, ACK,
    TEAM, MOVES, STATUS
}

public class NetworkHeader {
    public NetworkHeaderType Type;
    public string Message;
}
