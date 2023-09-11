using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class menuManager : MonoBehaviour
{
    [Header("Menu UI")]
    [SerializeField] private GameObject ep1Button;

    void Start()
    {
        
    }


    public void LoadDialogueScene()
    {
        // Load DialogueScene
        SceneManager.LoadScene("DialogueScene");
    }
}
