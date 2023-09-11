using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// this script is for debugging
public class senkuController : MonoBehaviour
{
    [Header("Game Data")]
    [SerializeField] private float moveSpeed = 5f; //500 before
    [SerializeField] private int stepsLeft = 10;

    [Header("Animation")]
    [SerializeField] private Animator Anim;
    [SerializeField] private AnimatorOverrideController rightController;
    [SerializeField] private AnimatorOverrideController leftController;
    [SerializeField] private string JumpAnimation = "gen_jump";

    [Header("Text Display")]
    [SerializeField] private TextMeshProUGUI stepText;

    private Vector3 targetPosition;
    private bool isMoving = false;
    private Vector2 facingDirection = Vector2.right;
    
    private void Update()
    {
        // detect input
        if (!isMoving)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow)||Input.GetKeyDown(KeyCode.D))
            {
                MoveToNextGrid(Vector2.right);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow)||Input.GetKeyDown(KeyCode.A))
            {
                MoveToNextGrid(Vector2.left);
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow)||Input.GetKeyDown(KeyCode.W))
            {
                MoveToNextGrid(Vector2.up);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow)||Input.GetKeyDown(KeyCode.S))
            {
                MoveToNextGrid(Vector2.down);
            }
        }
    }

    private void MoveToNextGrid(Vector2 direction)
    {
        // lock the input
        isMoving = true;
        //modify left steps
        stepsLeft--;
        stepText.text = "Steps left: " + stepsLeft;
        //update facing direction
        facingDirection = direction;
        //play jump animation
        Anim.Play(JumpAnimation);
    }

    //animation event, start moving when charged
    public void AnimationFinished()
    {
        StartMovingTowardsTarget();
    }

    //animation event, unlock input when jump animation ends
    public void ResetAnimationStatus(){
        isMoving = false;
    }

    //move the player
    private void StartMovingTowardsTarget()
    {
        Vector2 currentPosition = transform.position;
        Vector2 nextPosition = currentPosition + facingDirection * 1.6f;

        if (IsValidGridPosition(nextPosition))
        {
            targetPosition = nextPosition;
            StartCoroutine(MoveCoroutine()); //start the coroutine for smooth movement
        } else {
            Debug.Log("invalid grid position");
        }
    }

    private IEnumerator MoveCoroutine()
    {
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            Vector3 newPosition = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            if (facingDirection == Vector2.left)
            {
                transform.localScale = new Vector3(0.27f, 0.27f, 0.27f);
            }
            else if (facingDirection == Vector2.right)
            {
                transform.localScale = new Vector3(-0.27f, 0.27f, 0.27f);
            }

            transform.position = newPosition;

            yield return null;
        }
    }

    //detect barriers
    private bool IsValidGridPosition(Vector2 position)
    {
        Collider2D[] colliders = Physics2D.OverlapBoxAll(position, new Vector2(1.6f, 1.6f), 0f); //160f all before

        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("barrier"))
            {
                return false;
            }
        }
        return true;
    }
}
