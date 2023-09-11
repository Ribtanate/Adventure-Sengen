using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Ink JSON")]
    [SerializeField] private TextAsset[] inkJSON;
    [SerializeField] private Animator titleAnim;
    [SerializeField] private TextMeshProUGUI title;

    [Header("Stats")]
    [SerializeField] private float titleDisplayDuration = 5f;
    [SerializeField] private string titleText = "Episode 1";
    [Header("Audio")]
    [SerializeField] private AudioSource OpeningSrc;
    [SerializeField] private AudioSource BgmSrc;
    [SerializeField] private AudioClip opening;
    [SerializeField] private AudioClip[] bgm;

    private int currentDialogueIndex = 0;
    public bool IsDialogueEnd = false;

    private void Start()
    {
        //play opening animation
        DisplayTitle();
        // Start the first dialogue
        //StartNextDialogue();
    }

    private void Update(){
        //display dialogue list
        if(IsDialogueEnd == true){
            currentDialogueIndex++;
            if(currentDialogueIndex < inkJSON.Length)
            {
                StartNextDialogue();
            }
            else
            {
                IsDialogueEnd = false;
                Debug.Log("All dialogues have been played");
                StartChallenge();
            }
        }
    }

    private void StartNextDialogue()
    {
        IsDialogueEnd = false;
        /* TO BE IMPLEMENTED:
        background reference
        background animation
        */
        // Call DialogueManager to start the dialogue from the current index
        if (currentDialogueIndex < inkJSON.Length)
        {
            //assign and play bgm
            BgmSrc.clip = bgm[currentDialogueIndex];
            BgmSrc.Play();
            // play dialogue
            DialogueManager.GetInstance().EnterDialogueMode(inkJSON[currentDialogueIndex]);
        }
    }

    private void DisplayTitle(){
        //play audio
        OpeningSrc.clip = opening;
        OpeningSrc.Play();
        //set text
        title.text = titleText;
        StartCoroutine(DeactivateTextAfterDelay(titleDisplayDuration));
        //play title animation
        titleAnim.Play("start_title");
        // Start the Coroutine to wait for animation completion
        StartCoroutine(WaitForAnimationThenDialogue());
        return;
    }


    private IEnumerator WaitForAnimationThenDialogue()
    {
        // Wait for the animation to finish
        while (titleAnim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }

        // Animation has finished, now start the next dialogue
        StartNextDialogue();
    }

    private IEnumerator DeactivateTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        // Deactivate the TextMeshPro component or the GameObject containing it
        title.gameObject.SetActive(false);
    }

    private void StartChallenge(){
        /*TO BE IMPLEMENTED
        display start challenge animation
        start challenge sound effect
        load challenge scene
        */
        return;
    }

}

    