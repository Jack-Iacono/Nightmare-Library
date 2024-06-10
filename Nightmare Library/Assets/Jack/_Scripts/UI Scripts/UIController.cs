using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public TMP_InputField inputField;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public async void JoinGame()
    {
        NetworkConnectionController.connectionType = NetworkConnectionController.ConnectionType.JOIN;
        NetworkConnectionController.joinCode = inputField.text;

        if (!NetworkConnectionController.connectedToLobby)
        {
            if (await NetworkConnectionController.ConnectToLobby())
                await NetworkConnectionController.StartConnection();
        }

        gameObject.SetActive(false);
    }
    public async void CreateGame()
    {
        NetworkConnectionController.connectionType = NetworkConnectionController.ConnectionType.CREATE;

        if (!NetworkConnectionController.connectedToLobby)
        {
            if (await NetworkConnectionController.ConnectToLobby())
                await NetworkConnectionController.StartConnection();
        }

        Debug.Log(NetworkConnectionController.joinCode);
        gameObject.SetActive(false);
    }
}
