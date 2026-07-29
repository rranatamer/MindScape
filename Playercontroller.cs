using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    public float moveSpeed  = 10f;
    public float jumpHeight = 6f;
    public float gravity    = -20f;

    
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private Vector2 moveInput;
    private bool jumpInput;
    private bool useCardInput;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * moveSpeed * Time.deltaTime);

        if (jumpInput && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpInput  = false;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (useCardInput)
        {
            UsePositiveCard();
            useCardInput = false;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
            jumpInput = true;
    }

    public void OnUseCard(InputAction.CallbackContext context)
    {
        if (context.performed)
            useCardInput = true;
    }

    public void UsePositiveCard()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.PositiveCardCount <= 0) return;

        GameManager.Instance.PositiveCardCount--;
        GameManager.Instance.ReduceStress(15);
        GameManager.Instance.AddConfidence(10);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayPositiveCardAudio();
    }
}