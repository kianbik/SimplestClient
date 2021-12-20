using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
public class TicTacToeManager : MonoBehaviour
{

    GameObject playerSymbol, opponentSymbol, turnIndicator, characterSelection, xButton, oButton, roomNumber, previousButton, nextButton;
    NetworkedClient connectionToHost;
    List<SquareBehaviour> ticTacToeSquares;


    string playerIcon, opponentIcon;

    bool isPlayersTurn = false;
    bool isGameOver = false;
    bool isObserver = false;
    bool wasPlayerOne = false;


    int rn;

    string[] turns;

    int turnCount = 0;



    public void SetTurns(string[] data)
    {
        turns = data;
        turnCount = data.Length;
    }

    private void Awake()
    {
        ticTacToeSquares = new List<SquareBehaviour>(GetComponentsInChildren<SquareBehaviour>());
        foreach (SquareBehaviour square in ticTacToeSquares)
        {
            square.OnSquarePressed += OnTicTacToeSquarePressed;
        }


        foreach (GameObject go in FindObjectsOfType<GameObject>())
        {
            if (go.name == "PlayerSymbolText")
                playerSymbol = go;
            else if (go.name == "OpponentSymbolText")
                opponentSymbol = go;
            else if (go.name == "TurnIndicatorText")
                turnIndicator = go;
            else if (go.name == "CharacterSelection")
                characterSelection = go;
            else if (go.name == "XButton")
                xButton = go;
            else if (go.name == "OButton")
                oButton = go;
            else if (go.name == "RoomNumberText")
                roomNumber = go;
            else if (go.name == "PreviousButton")
                previousButton = go;
            else if (go.name == "NextButton")
                nextButton = go;

        }

        xButton.GetComponent<Button>().onClick.AddListener(XSelected);
        oButton.GetComponent<Button>().onClick.AddListener(OSelected);
        nextButton.GetComponent<Button>().onClick.AddListener(NextButtonPressed);
        previousButton.GetComponent<Button>().onClick.AddListener(PreviousButtonPressed);

    }

    private void OnTicTacToeSquarePressed(SquareBehaviour square)
    {
        if (playerIcon == "" || !isPlayersTurn) //player hasn't picked their symbol yet or it isn't their turn, they cant claim a square yet
        {
            
            return;
        }
        isPlayersTurn = false;
        turnIndicator.GetComponent<Text>().text = "It's your opponent's turn";

        square.squarePicked(playerIcon, true);
        if (connectionToHost != null)
            connectionToHost.SendMessageToHost(ClientToServerSignifiers.SelectedTicTacToeSquare + "," + square.ID);
       
        CheckForWin(square.row, square.column);
        CheckForTie();
    }

    void CheckForWin(int rowToCheck, int colToCheck)
    {
        int rowCount, colCount, diagonal1Count, diagonal2Count;
        rowCount = colCount = diagonal1Count = diagonal2Count = 0;

        foreach (SquareBehaviour s in ticTacToeSquares)
        {
            if (s.isTaken == false || s.squareIcon == opponentIcon)
                continue;

            if (s.row == rowToCheck)
                rowCount++;
            if (s.column == colToCheck)
                colCount++;
            if (s.diagonalLeftToRight)
                diagonal1Count++;
            if (s.diagonalRightToLeft)
                diagonal2Count++;
        }

        if (rowCount == 3 || colCount == 3 || diagonal1Count == 3 || diagonal2Count == 3)
        {
            
            OnGameOver("You Won!");
            connectionToHost.SendMessageToHost(ClientToServerSignifiers.EndingTheGame + "," + "Game over, you lost");
        }

    }
    public void OpponentTookTurn(int squareID)
    {
        foreach (SquareBehaviour s in ticTacToeSquares)
        {
            if (s.ID == squareID)
                s.squarePicked(opponentIcon, true);
        }

        if (!isObserver)
        {
            isPlayersTurn = true;
            turnIndicator.GetComponent<Text>().text = "It's your turn";
        }
        else
        {
            MatchSquareToTurnData(squareID);
        }
    }

    public void OnGameOver(string endingMsg)
    {
        turnIndicator.GetComponent<Text>().text = endingMsg;

        if (isObserver)
            turnIndicator.GetComponent<Text>().text = "the game has ended";
        //enable ui for replay
        ChangeState(TicTacToeStates.GameOver);
    }

    public void SetNetworkConnection(NetworkedClient networkClient)
    {
        connectionToHost = networkClient;
    }

    void NextButtonPressed()
    {
        if (turnCount < turns.Length)
        {
            MatchSquareToTurnData(int.Parse(turns[turnCount]));
        }
    }

    void PreviousButtonPressed()
    {
        if (turnCount > 0)
        {
            turnCount--;
            ticTacToeSquares[int.Parse(turns[turnCount])].resetSquare();
        }
    }

    void CharacterSelected(string symbol)
    {
        if (symbol == "X")
        {
            playerIcon = symbol;
            opponentIcon = "O";
            Debug.Log("X selected");
        }
        else if (symbol == "O")
        {
            playerIcon = symbol;
            opponentIcon = "X";
            Debug.Log("O selected");
        }
        playerSymbol.GetComponent<Text>().text = "You Are: " + symbol;
        opponentSymbol.GetComponent<Text>().text = "Opponent is: " + opponentIcon;

        oButton.SetActive(false);
        xButton.SetActive(false);
        turnIndicator.SetActive(true);

        //check if the other player made a choice before your icons were set
        foreach (SquareBehaviour s in ticTacToeSquares)
        {
            if (s.isTaken)
                s.squarePicked(opponentIcon , true);
        }
    }
    void OSelected()
    {
        CharacterSelected("O");
    }
    void XSelected()
    {
        CharacterSelected("X");
    }

    public void ChosenAsPlayerOne()
    {
        isPlayersTurn = true;
        turnIndicator.GetComponent<Text>().text = "It's your turn";
        wasPlayerOne = true;
    }
    private void CheckForTie()
    {
        int takenTileCount = 0;
        foreach (SquareBehaviour s in ticTacToeSquares)
        {
            if (s.isTaken)
                takenTileCount++;
        }

        if (takenTileCount >= 9 && isGameOver == false)
        {
            connectionToHost.SendMessageToHost(ClientToServerSignifiers.EndingTheGame + "," + "No Squares Left. You tied");
            OnGameOver("No squares left. You tied");
        }
    }
    public void SetRoomNumberText(string RoomNumber)
    {
        this.rn = int.Parse(RoomNumber);
        roomNumber.GetComponent<Text>().text = "Room: " + rn;
    }
    public void EnterGameAsObserver(string[] csv_TurnsSoFar)
    {
        ChangeState(TicTacToeStates.Observer);

        //update already taken squares
        foreach (string index in csv_TurnsSoFar)
        {
            int squareIndex = int.Parse(index);
            MatchSquareToTurnData(squareIndex);
        }

        if (isGameOver)
            ChangeState(TicTacToeStates.GameOver);
    }

    void MatchSquareToTurnData(int squareID)
    {
        if (wasPlayerOne)
        {
            if (turnCount++ % 2 == 0)
                ticTacToeSquares[squareID].squarePicked(playerIcon,true);
            else
                ticTacToeSquares[squareID].squarePicked(opponentIcon, true);
        }
        else
        {
            if (turnCount++ % 2 == 1)
                ticTacToeSquares[squareID].squarePicked(playerIcon, true);
            else
                ticTacToeSquares[squareID].squarePicked(opponentIcon, true);
        }
    }
    public bool CanLeave()
    {
        return (isObserver || isGameOver);
    }
    private void ResetGameState()
    {
        playerSymbol.GetComponent<Text>().text = "You Are: ";
        opponentSymbol.GetComponent<Text>().text = "Opponent is: ";

        foreach (SquareBehaviour s in ticTacToeSquares)
        {
            s.resetSquare();
        }
        turnIndicator.GetComponent<Text>().text = "It's your opponent's turn";
        turnIndicator.SetActive(false);
        turnCount = 0;
    }
    public void ChangeState(int state)
    {
        isPlayersTurn = false;
        nextButton.SetActive(false);
        previousButton.SetActive(false);

        if (state == TicTacToeStates.Game)
        {
            ResetGameState();

            isGameOver = false;
            opponentSymbol.SetActive(true);
            characterSelection.SetActive(true);

            isObserver = false;
        }
        else if (state == TicTacToeStates.Observer)
        {
            ResetGameState();
            playerIcon = "X";
            opponentIcon = "O";
            opponentSymbol.SetActive(false);
            characterSelection.SetActive(false);

            playerSymbol.GetComponent<Text>().text = "You Are: Observing";

            isObserver = true;
        }
        else if (state == TicTacToeStates.GameOver)
        {
            connectionToHost.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.RequestTurnData + "," + rn);
            isGameOver = true;
            turnIndicator.SetActive(true);
            //enable replay 
            nextButton.SetActive(true);
            previousButton.SetActive(true);
        }
    }
}
public class TicTacToeStates
{
    public const int Game = 1;
    public const int Observer = 2;
    public const int GameOver = 3;
}