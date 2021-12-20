using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GameSystemManager : MonoBehaviour
{
    GameObject submitButton, userNameInput, passwordInput, createToggle, logInToggle;

    GameObject networkClient;
    GameObject joinGameRoom;


    GameObject gameRoomButton, observerButton, titleText,loginCanvas, ticTacToeCanvas, mainMenuCanvas, roomNumInput, leaveRoomButton,gameTable;
    //static GameObject instance;


    // Start is called before the first frame update
    void Start()
    {

        //instance = this.gameObject;

        GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
        foreach (GameObject go in allObjects)
        {
            if (go.name == "LoginCanvas")
                loginCanvas = go;
            if (go.name == "UserNameInputField")
                userNameInput = go;
            else if (go.name == "PasswordInputField")
                passwordInput = go;
            else if (go.name == "SubmitButton")
                submitButton = go;
            else if (go.name == "LogInToggle")
                logInToggle = go;
            else if (go.name == "CreateToggle")
                createToggle = go;
            else if (go.name == "NetworkedClient")
                networkClient = go;
            else if (go.name == "JoinGameRoom")
                joinGameRoom = go;
            else if (go.name == "GameRoomButton")
                gameRoomButton = go;
            else if (go.name == "Panel Title")
                titleText = go;
            else if (go.name == "TicTacToeCanvas")
                ticTacToeCanvas = go;
            else if (go.name == "MainMenuCanvas")
                mainMenuCanvas = go;
            else if (go.name == "ObserverButton")
                observerButton = go;
            else if (go.name == "RoomNumInputField")
                roomNumInput = go;
            else if (go.name == "LeaveRoomButton")
                leaveRoomButton = go;
            else if (go.name == "GameTable")
                gameTable = go;

        }

        submitButton.GetComponent<Button>().onClick.AddListener(SubmitButtonPressed);

        logInToggle.GetComponent<Toggle>().onValueChanged.AddListener(LoginToggleChanged);
       
        createToggle.GetComponent<Toggle>().onValueChanged.AddListener(CreateToggleChanged);

        observerButton.GetComponent<Button>().onClick.AddListener(GameRoomAsObserverButtonPressed);
        gameRoomButton.GetComponent<Button>().onClick.AddListener(GameRoomButtonPressed);
        joinGameRoom.GetComponent<Button>().onClick.AddListener(GameRoomButtonPressed);
        

        leaveRoomButton.GetComponent<Button>().onClick.AddListener(LeaveRoomButtonPressed);

        ChangeState(GameStates.LoginMenu);


    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SubmitButtonPressed()
    {
        //Send Login Data To Server
        string p = passwordInput.GetComponent<InputField>().text;
        string n = userNameInput.GetComponent<InputField>().text;
        string msg;


        if(createToggle.GetComponent<Toggle>().isOn)
        msg = ClientToServerSignifiers.CreateAccount + "," + n + "," + p;

        else
            msg = ClientToServerSignifiers.Login + "," + n + "," + p;

        networkClient.GetComponent<NetworkedClient>().SendMessageToHost(msg);
    
    
    
    }  
    public void LoginToggleChanged(bool newValue)
    {
            createToggle.GetComponent<Toggle>().SetIsOnWithoutNotify(!newValue);
        
    }
    public void CreateToggleChanged(bool newValue)
    {
        logInToggle.GetComponent<Toggle>().SetIsOnWithoutNotify(!newValue);
    }

    private void GameRoomButtonPressed()
    {
        networkClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.JoinGameRoomQueue + "");
        ChangeState(GameStates.WaitingInQueue);
    }
    private void GameRoomAsObserverButtonPressed()
    {
        InputField input = roomNumInput.GetComponent<InputField>();
        string roomNum = input.textComponent.text;

        if (roomNum == "")
        {
            networkClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.JoinAnyRoomAsObserver + "");
        }
        else if (int.TryParse(roomNum, out int temp))
        {
            networkClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.JoinSpecificRoomAsObserver + "," + roomNum);
        }

        input.text = "";
    }

    void LeaveRoomButtonPressed()
    {
        if (ticTacToeCanvas.activeInHierarchy && ticTacToeCanvas.GetComponent<TicTacToeManager>().CanLeave() == false)
            networkClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.EndingTheGame + "," + "Opponent Left Early");

        networkClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.LeaveTheRoom + "");
        ChangeState(GameStates.MainMenu);
    }

    public void ChangeState(int newState)
    {

        submitButton.SetActive(false);
        userNameInput.SetActive(false);
        passwordInput.SetActive(false);
        createToggle.SetActive(false);
        logInToggle.SetActive(false);
        joinGameRoom.SetActive(false);
       
        gameRoomButton.SetActive(false);
        titleText.SetActive(false);
        observerButton.SetActive(false);
        roomNumInput.SetActive(false);
        leaveRoomButton.SetActive(false);
        mainMenuCanvas.SetActive(false);
        gameTable.SetActive(false);


        if (newState == GameStates.LoginMenu)
        {
            loginCanvas.SetActive(true);
            submitButton.SetActive(true);
            userNameInput.SetActive(true);
            passwordInput.SetActive(true);
            createToggle.SetActive(true);
            logInToggle.SetActive(true);

        }
        else if(newState == GameStates.MainMenu)
        {
            mainMenuCanvas.SetActive(true);
            joinGameRoom.SetActive(true);
            observerButton.SetActive(true);
            roomNumInput.SetActive(true);
        }
        else if (newState == GameStates.WaitingInQueue)
        {
            leaveRoomButton.SetActive(true);
            gameTable.SetActive(true);
            ticTacToeCanvas.GetComponent<TicTacToeManager>().SetNetworkConnection(networkClient.GetComponent<NetworkedClient>());
        }
        else if(newState == GameStates.TicTacToe)
        {
            gameTable.SetActive(true);
            ticTacToeCanvas.GetComponent<TicTacToeManager>().SetNetworkConnection(networkClient.GetComponent<NetworkedClient>());
            leaveRoomButton.SetActive(true);
        }
    }
}
public static class GameStates
{

    public const int LoginMenu = 1;
    public const int MainMenu = 2;
    public const int WaitingInQueue = 3;
    public const int TicTacToe = 4;
   
}