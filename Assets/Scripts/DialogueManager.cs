using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Ink.Runtime;
using UnityEngine.EventSystems;

public class DialogueManager : MonoBehaviour
{
    [Header("DialogueTriggerRef")]
    [SerializeField] DialogueTrigger trigger;
    [Header("Params")]
    [SerializeField] private float typingSpeed = 0.04f;

    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI displayNameText;
    [SerializeField] private GameObject senkuNameTag;
    [SerializeField] private GameObject genNameTag;

    [Header("Choices UI")]
    [SerializeField] private GameObject[] choices;
    private TextMeshProUGUI[] choicesText;

    [Header("Sound Effects")]
    [SerializeField] private AudioSource SFXSource;
    [SerializeField] private AudioClip SFX_RunForest, SFX_WaterFlow, SFX_Boom;

    private Story currentStory;
    public bool dialogueIsPlaying { get; private set; }
    private bool canContinueToNextLine = false;
    private Coroutine displayLineCoroutine;
    private static DialogueManager instance;

    //Animations
    [Header("Animations")]
    [SerializeField] private Animator BackgroundUIAnim;
    [SerializeField] private Animator SenkuPortrait, GenPortrait;
    [SerializeField] private Animator SenkuHead, SenkuFace, SenkuLeftArm, SenkuRightArm;
    [SerializeField] private Animator GenHead, GenFace, GenLeftArm, GenRightArm;
    //inky tags
    private const string SPEAKER_TAG = "speaker";
    private const string BG_EFFECT_TAG = "bg_effect";
    private const string SOUND_EFFECT_TAG = "sound_effect";
    private const string SENKU_TAG = "senku";
    private const string SENKU_HEAD_TAG = "senku_head";
    private const string SENKU_FACE_TAG = "senku_face";
    private const string SENKU_LEFT_ARM_TAG = "senku_left_arm";
    private const string SENKU_RIGHT_ARM_TAG = "senku_right_arm";
    private const string GEN_TAG = "gen";
    private const string GEN_HEAD_TAG = "gen_head";
    private const string GEN_FACE_TAG = "gen_face";
    private const string GEN_LEFT_ARM_TAG = "gen_left_arm";
    private const string GEN_RIGHT_ARM_TAG = "gen_right_arm";


    private void Awake() 
    {
        //hide all nametags
        senkuNameTag.SetActive(false);
        genNameTag.SetActive(false);

        if (instance != null)
        {
            Debug.LogWarning("Found more than one Dialogue Manager in the scene");
        }
        instance = this;
    }

    public static DialogueManager GetInstance() 
    {
        return instance;
    }

    private void Start() 
    {
        dialogueIsPlaying = false;
        dialoguePanel.SetActive(false);

        // get all of the choices text 
        choicesText = new TextMeshProUGUI[choices.Length];
        int index = 0;
        foreach (GameObject choice in choices) 
        {
            choicesText[index] = choice.GetComponentInChildren<TextMeshProUGUI>();
            index++;
        }
    }

    private void Update() 
    {
        // return right away if dialogue isn't playing
        if (!dialogueIsPlaying) 
        {
            return;
        }
        // handle continuing to the next line in the dialogue when submit is pressed
        if (canContinueToNextLine && currentStory.currentChoices.Count == 0 && Input.GetKeyDown("space"))
        {
            ContinueStory();
        }
    }

    public void EnterDialogueMode(TextAsset inkJSON) 
    {
        currentStory = new Story(inkJSON.text);
        dialogueIsPlaying = true;
        dialoguePanel.SetActive(true);

        ContinueStory();
    }

    private IEnumerator ExitDialogueMode() 
    {
        yield return new WaitForSeconds(0.2f);

        dialogueIsPlaying = false;
        dialoguePanel.SetActive(false);
        dialogueText.text = "";
    }

    private void ContinueStory() 
    {
        if (currentStory.canContinue) 
        {
            // set text for the current dialogue line
            if (displayLineCoroutine != null) 
            {
                StopCoroutine(displayLineCoroutine);
            }
            string nextLine = currentStory.Continue();
            // handle case where the last line is an external function
            if (nextLine.Equals("") && !currentStory.canContinue)
            {
                StartCoroutine(ExitDialogueMode());
            }
            // otherwise, handle the normal case for continuing the story
            else 
            {
                // handle tags
                HandleTags(currentStory.currentTags);
                displayLineCoroutine = StartCoroutine(DisplayLine(nextLine));
            }
        }
        else 
        {
            trigger.IsDialogueEnd = true;
            //StartCoroutine(ExitDialogueMode());
        }
    }

    private IEnumerator DisplayLine(string line) 
    {
        // set the text to the full line, but set the visible characters to 0
        dialogueText.text = line;
        dialogueText.maxVisibleCharacters = 0;
        // hide items while text is typing
        //continueIcon.SetActive(false);
        HideChoices();

        canContinueToNextLine = false;

        bool isAddingRichTextTag = false;

        // display each letter one at a time
        foreach (char letter in line.ToCharArray())
        {
            // if the submit button is pressed, finish up displaying the line right away
            //if (InputManager.GetInstance().GetSubmitPressed()) 
            /*if(Input.GetKeyDown("space"))
            {
                dialogueText.maxVisibleCharacters = line.Length;
                break;
            }*/

            // check for rich text tag, if found, add it without waiting
            if (letter == '<' || isAddingRichTextTag) 
            {
                isAddingRichTextTag = true;
                if (letter == '>')
                {
                    isAddingRichTextTag = false;
                }
            }
            // if not rich text, add the next letter and wait a small time
            else 
            {
                dialogueText.maxVisibleCharacters++;
                yield return new WaitForSeconds(typingSpeed);
            }
        }

        // actions to take after the entire line has finished displaying
        //continueIcon.SetActive(true);
        DisplayChoices();

        canContinueToNextLine = true;
    }

    private void HideChoices() 
    {
        foreach (GameObject choiceButton in choices) 
        {
            choiceButton.SetActive(false);
        }
    }

    private void HandleTags(List<string> currentTags)
    {
        //loop through each tag
        foreach (string tag in currentTags)
        {
            //parse the tag
            string[] splitTag = tag.Split(':');
            if (splitTag.Length != 2)
            {
                Debug.LogError("Tag could not be appropraitely parsed" + tag);
            }
            string tagKey = splitTag[0].Trim();
            string tagValue = splitTag[1].Trim();

            switch (tagKey)
            {
                case GEN_TAG:
                    ManageGenPortrait(tagValue);
                    break;
                case GEN_HEAD_TAG:
                    GenHead.Play(tagValue);
                    break;
                case GEN_FACE_TAG:
                    GenFace.Play(tagValue);
                    break;
                case GEN_LEFT_ARM_TAG:
                    GenLeftArm.Play(tagValue);
                    break;
                case GEN_RIGHT_ARM_TAG:
                    GenRightArm.Play(tagValue);
                    break;
                case SENKU_TAG:
                    ManageSenkuPortrait(tagValue);
                    break;
                case SENKU_HEAD_TAG:
                    SenkuHead.Play(tagValue);
                    break;
                case SENKU_FACE_TAG:
                    SenkuFace.Play(tagValue);
                    break;
                case SENKU_LEFT_ARM_TAG:
                    SenkuLeftArm.Play(tagValue);
                    break;
                case SENKU_RIGHT_ARM_TAG:
                    SenkuRightArm.Play(tagValue);
                    break;
                case SPEAKER_TAG:
                    ManageSpeaker(tagValue);
                    break;
                case BG_EFFECT_TAG:
                    BackgroundUIAnim.Play(tagValue);
                    break;
                case SOUND_EFFECT_TAG:
                    ManageSFX(tagValue);
                    SFXSource.Play();
                    break;
                default:
                    Debug.LogWarning("Tag came in but is not currently being handled" + tag);
                    break;
            }
        }
    }

    private void ManageGenPortrait(string key){
        if(key == "null"){
            GenPortrait.Play("Gen_null");
        } else if (key == "active"){
            GenPortrait.Play("Gen_default");
        }
        if (key == "default"){
            GenHead.Play("default");
            GenLeftArm.Play("0");
            GenRightArm.Play("0");
        } else if (key == "think"){
            GenHead.Play("default");
            GenLeftArm.Play("1");
            GenRightArm.Play("0");
        } else if (key == "bingo"){
            GenHead.Play("default");
            GenLeftArm.Play("0");
            GenRightArm.Play("1");
        } else if (key == "bingo2"){
            GenHead.Play("default");
            GenLeftArm.Play("0");
            GenRightArm.Play("2");
        } else if (key == "reach_out"){
            GenHead.Play("default");
            GenLeftArm.Play("2");
            GenRightArm.Play("0");
        }
    }

    private void ManageSenkuPortrait(string key){
        if(key == "null"){
            SenkuPortrait.Play("Senku_null");
        } else if (key == "active"){
            SenkuPortrait.Play("Senku_default");
        }
        if (key == "default"){
            SenkuHead.Play("default");
            SenkuLeftArm.Play("0");
            SenkuRightArm.Play("0");
        } else if (key == "think"){
            SenkuHead.Play("nod_right");
            SenkuLeftArm.Play("0");
            SenkuRightArm.Play("1");
        }
    }

    private void ManageSpeaker(string speaker){
        if (speaker == "gen"){
            genNameTag.SetActive(true);
            senkuNameTag.SetActive(false);
        }
        if (speaker == "senku"){
            senkuNameTag.SetActive(true);
            genNameTag.SetActive(false);
        }
        if (speaker == "null"){
            senkuNameTag.SetActive(false);
            genNameTag.SetActive(false);
        }
    }

    private void ManageSFX(string key){
        if (key == "water_flow"){
            SFXSource.clip = SFX_WaterFlow;
        } else if (key == "boom"){
            SFXSource.clip = SFX_Boom;
        } else if (key == "run_forest"){
            SFXSource.clip = SFX_RunForest;
        }
    }

    // dialogue choice. not used
    // will furthr implement them when needed
    private void DisplayChoices() 
    {
        List<Choice> currentChoices = currentStory.currentChoices;

        // defensive check to make sure our UI can support the number of choices coming in
        if (currentChoices.Count > choices.Length)
        {
            Debug.LogError("More choices were given than the UI can support. Number of choices given: " 
                + currentChoices.Count);
        }
        int index = 0;
        // enable and initialize the choices up to the amount of choices for this line of dialogue
        foreach(Choice choice in currentChoices) 
        {
            choices[index].gameObject.SetActive(true);
            choicesText[index].text = choice.text;
            index++;
        }
        // go through the remaining choices the UI supports and make sure they're hidden
        for (int i = index; i < choices.Length; i++) 
        {
            choices[i].gameObject.SetActive(false);
        }

        StartCoroutine(SelectFirstChoice());
    }

    private IEnumerator SelectFirstChoice() 
    {
        // Event System requires clear it first, then wait for at least one frame before setting the current selected object.
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(choices[0].gameObject);
    }

    public void MakeChoice(int choiceIndex)
    {
        if(canContinueToNextLine){
            currentStory.ChooseChoiceIndex(choiceIndex);
            ContinueStory();
        }
        
    }

}