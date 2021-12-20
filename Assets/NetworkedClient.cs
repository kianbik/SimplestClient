using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkedClient : MonoBehaviour
{

    int connectionID;

    int maxConnections = 1000;

    int reliableChannelID;

    int unreliableChannelID;

    int hostID;

    int socketPort = 5491;

    byte error;

    bool isConnected = false;

    int ourClientID;

    GameObject gameSystemManager, TicTacToeManager;

    // Start is called before the first frame update
    void Start()
    {
       
        GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
        foreach (GameObject go in allObjects)
        {
           if( go.GetComponent<GameSystemManager>() != null)
            {
                gameSystemManager = go;
                
            }
            if (go.GetComponent<TicTacToeManager>() != null)
            {
                TicTacToeManager = go;
               
            }
    
        }
            Connect();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateNetworkConnection();
    }

    private void UpdateNetworkConnection()
    {
        if (isConnected)
        {
            int recHostID;
            int recConnectionID;
            int recChannelID;
            byte[] recBuffer = new byte[1024];
            int bufferSize = 1024;
            int dataSize;
            NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recHostID, out recConnectionID, out recChannelID, recBuffer, bufferSize, out dataSize, out error);

            switch (recNetworkEvent)
            {
                case NetworkEventType.ConnectEvent:
                    Debug.Log("connected.  " + recConnectionID);
                    ourClientID = recConnectionID;
                    break;
                case NetworkEventType.DataEvent:
                    string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                    ProcessRecievedMsg(msg, recConnectionID);
                    Debug.Log("got msg = " + msg);
                    break;
                case NetworkEventType.DisconnectEvent:
                    isConnected = false;
                    Debug.Log("disconnected.  " + recConnectionID);
                    break;
            }
        }
    }
    
    private void Connect()
    {

        if (!isConnected)
        {
            Debug.Log("Attempting to create connection");

            NetworkTransport.Init();

            ConnectionConfig config = new ConnectionConfig();
            reliableChannelID = config.AddChannel(QosType.Reliable);
            unreliableChannelID = config.AddChannel(QosType.Unreliable);
            HostTopology topology = new HostTopology(config, maxConnections);
            hostID = NetworkTransport.AddHost(topology, 0);
            Debug.Log("Socket open.  Host ID = " + hostID);

            connectionID = NetworkTransport.Connect(hostID, "10.0.0.176", socketPort, 0, out error); // server is local on network

            if (error == 0)
            {
                isConnected = true;

                Debug.Log("Connected, id = " + connectionID);

            }
        }
    }
    
    public void Disconnect()
    {
        NetworkTransport.Disconnect(hostID, connectionID, out error);
    }
    
    public void SendMessageToHost(string msg)
    {
        byte[] buffer = Encoding.Unicode.GetBytes(msg);
        NetworkTransport.Send(hostID, connectionID, reliableChannelID, buffer, msg.Length * sizeof(char), out error);
    }

    private void ProcessRecievedMsg(string msg, int id)
    {
        Debug.Log("msg recieved = " + msg + ".  connection id = " + id);
       string[] csv = msg.Split(',');
        int signifier = int.Parse(csv[0]);
        if(signifier == ServerToClientSignifiers.AccountCreated)
        {
            gameSystemManager.GetComponent<GameSystemManager>().ChangeState(GameStates.MainMenu);
        }
       else if (signifier == ServerToClientSignifiers.LoginComplete)
        {
            gameSystemManager.GetComponent<GameSystemManager>().ChangeState(GameStates.MainMenu);
        }
        else if (signifier == ServerToClientSignifiers.GameStart)
        {
            gameSystemManager.GetComponent<GameSystemManager>().ChangeState(GameStates.TicTacToe);
            TicTacToeManager.GetComponent<TicTacToeManager>().ChangeState(TicTacToeStates.Game);
            TicTacToeManager.GetComponent<TicTacToeManager>().SetRoomNumberText(csv[1]);
        }
        else if (signifier == ServerToClientSignifiers.ChosenAsPlayerOne)
        {
            TicTacToeManager.GetComponent<TicTacToeManager>().ChosenAsPlayerOne();
        }
        else if (signifier == ServerToClientSignifiers.OpponentChoseASquare)
        {
            TicTacToeManager.GetComponent<TicTacToeManager>().OpponentTookTurn(int.Parse(csv[1]));
        }
        else if (signifier == ServerToClientSignifiers.GameIsOver)
        {
            TicTacToeManager.GetComponent<TicTacToeManager>().OnGameOver(csv[1]);
        }
      
        else if (signifier == ServerToClientSignifiers.EnteredGameRoomAsObserver)
        { // passes signifier, room number, then csv of all the turns so far
            TicTacToeManager ticTackToe = TicTacToeManager.GetComponent<TicTacToeManager>();
            gameSystemManager.GetComponent<GameSystemManager>().ChangeState(GameStates.TicTacToe);
            ticTackToe.SetRoomNumberText(csv[1]);

            string[] takenSquares = new string[csv.Length - 2];

            for (int i = 2; i < csv.Length; i++)
            {
                takenSquares[i - 2] = csv[i];
            }

            ticTackToe.EnterGameAsObserver(takenSquares);
        }
        else if (signifier == ServerToClientSignifiers.TurnData)
        {
            string[] turns = new string[csv.Length - 1];

            for (int i = 1; i < csv.Length; i++)
            {
                turns[i - 1] = csv[i];
            }
            TicTacToeManager.GetComponent<TicTacToeManager>().SetTurns(turns);
        }
    }

    public bool IsConnected()
    {
        return isConnected;
    }


}
public static class ClientToServerSignifiers {

    public const int CreateAccount = 1;
    public const int Login = 2;
    public const int JoinGameRoomQueue = 3;

    public const int SelectedTicTacToeSquare = 4;

    public const int ChatLogMessage = 8;

    public const int JoinAnyRoomAsObserver = 9;
    public const int JoinSpecificRoomAsObserver = 10;

    public const int EndingTheGame = 11;
    public const int LeaveTheRoom = 12;

    public const int RequestTurnData = 14;


}

public static class ServerToClientSignifiers
{

    public const int LoginComplete = 1;
    public const int LoginFailed = 2;

    public const int AccountCreated = 3;
    public const int AccountCreationFailed = 4;

    public const int GameStart = 5;

    public const int ChosenAsPlayerOne = 6;
    public const int OpponentChoseASquare = 7;

    public const int ChatLogMessage = 11;

    public const int EnteredGameRoomAsObserver = 12;

    public const int GameIsOver = 13;

    public const int TurnData = 14;
}



