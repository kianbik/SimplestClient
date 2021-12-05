using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GameSystemManager : MonoBehaviour
{
    GameObject submitButton, userNameInput, passwordInput, createToggle, logInToggle;

    GameObject networkClient;

    // Start is called before the first frame update
    void Start()
    {
        GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
        foreach (GameObject go in allObjects)
        {
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

        }

        submitButton.GetComponent<Button>().onClick.AddListener(SubmitButtonPressed);

        logInToggle.GetComponent<Toggle>().onValueChanged.AddListener(LoginToggleChanged);
        createToggle.GetComponent<Toggle>().onValueChanged.AddListener(CreateToggleChanged);




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
}