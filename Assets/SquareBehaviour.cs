using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SquareBehaviour : MonoBehaviour
{
    public int row, column, ID;

    public bool isTaken;
    public string squareIcon;
    public bool diagonalLeftToRight;
    public bool diagonalRightToLeft;

    public delegate void SquarePressedDelegate(SquareBehaviour pickedSquare);
    public event SquarePressedDelegate OnSquarePressed;


    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnPressed);


        diagonalLeftToRight = row == column;
        diagonalRightToLeft = (row + column) == 2 ;
    }

    void OnPressed()
    {
        if (!isTaken)
        {
            OnSquarePressed.Invoke(this);
        }
            
    }
    public void squarePicked(string icon, bool taken)
    {
        this.squareIcon = icon;
            isTaken = taken;
        GetComponentInChildren<Text>().text = icon;

    }
    public void resetSquare()
    {
        squarePicked("", false);

    }


}
