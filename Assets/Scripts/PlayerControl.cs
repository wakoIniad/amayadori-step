using System;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerControl : MonoBehaviour
{
    public GameObject playerObject;

    public UnityEngine.Vector2 moveVector;
    public float playerPosition;
    public enum PlayerState
    {
        UMBRELLA_BACK,
        UMBRELLA_FRONT,
        UMBRELLA_UP
    }
    public PlayerState playerState;
    void Start()
    {
        IntergrationManager.instance.playerControl = this;
        InputAction move = InputSystem.actions.FindAction("Move");
        move.performed += OnMove;
        move.canceled += OnMove;
    }
    void FixedUpdate()
    {
        //this.transform.Translate(moveVector * Time.fixedDeltaTime * 32);
        //jump up_umb fore_umb back_umb
    }
    public void OnMove(InputAction.CallbackContext context)
    {
         if (context.performed)
        {
            moveVector = context.ReadValue<Vector2>();
        }

        if (context.canceled)
        {
            moveVector = Vector2.zero;
        }
        Debug.Log("[あ]test");
    }
}
