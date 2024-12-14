using UnityEngine;
using DG.Tweening;
using System; // DOTween namespace

public class BallController : MonoBehaviour
{
    [SerializeField] private float forwardJumpDistance = 5f;
    [SerializeField] private float horizontalSpeed = 10f;
    [SerializeField] private float xAxisClamp = 2f;
    [SerializeField] private float jumpHeight = 2f;
    // [SerializeField] private float rayMaxDist = 5f;
    [SerializeField] private float landingSpeed = 0.8f;
    [SerializeField] private float landingOffset = 0.1f;
    [SerializeField] private float fallSpeed = 1.0f;
    [SerializeField] private LayerMask platformLayer;

    private Rigidbody rb;
    private AudioSource audioSource;
    private Vector3 startTouchPosition;
    private Vector3 currentTouchPosition;
    private Platform prevPlatform;
    private bool isDragging = false;
    private bool canJump = false, canMove = false;

    public static Action startPlayerAction;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        rb.isKinematic = true;
        startPlayerAction += StartMovement;
        UIManager.RetryGameAction += ResetBall;
    }

    private void StartMovement()
    {
        canMove = true;
        rb.isKinematic = false;
    }

    void Update()
    {
        if (canMove)
        {
            HandleHorizontalMovement();


            HandleLanding();
        }
    }

    void HandleHorizontalMovement()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startTouchPosition = Input.mousePosition;
            isDragging = true;
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            currentTouchPosition = Input.mousePosition;
            float dragDelta = currentTouchPosition.x - startTouchPosition.x;

            // Smooth horizontal movement using DOTween
            Vector3 targetPosition = transform.position + new Vector3(dragDelta * horizontalSpeed * Time.deltaTime, 0, 0);
            targetPosition.x = Mathf.Clamp(targetPosition.x, -xAxisClamp, xAxisClamp);
            transform.DOMoveX(targetPosition.x, 0.1f);

            startTouchPosition = currentTouchPosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    void HandleLanding()
    {
        Platform curPlatform;
        Vector3 raydirection = -transform.up;

        if (Physics.Raycast(transform.position, raydirection, out RaycastHit hit, 20, platformLayer))
        {
            curPlatform = hit.transform.GetComponent<Platform>();
            if (curPlatform != null && curPlatform != prevPlatform)
            {
                prevPlatform = curPlatform;
                Vector3 landPos = new Vector3(this.transform.position.x, hit.point.y + landingOffset, hit.transform.position.z);

                rb.linearVelocity = Vector3.zero;
                rb.isKinematic = true;

                this.transform.DOMove(landPos, landingSpeed * 9.7f).SetSpeedBased(true).SetEase(Ease.Linear).OnComplete(() =>
                {
                    CalculateJumpScore(hit.transform);
                    canJump = true;
                    if (canJump)
                    {
                        JumpForward();
                    }
                    audioSource.Play();
                });

            }
        }
        else
        {
            HandleFalling();
        }
    }

    void HandleFalling()
    {
        if (this.transform.position.y < -1f)
        {
            canMove = false;
            transform.DOKill();
            Vector3 fallPos = new Vector3(this.transform.position.x, -20f, this.transform.position.z);
            this.transform.DOMove(fallPos, fallSpeed).SetEase(Ease.InOutQuint);
            UIManager.GameOverAction?.Invoke();
        }
    }

    void JumpForward()
    {
        canJump = false;
        rb.isKinematic = false;
        Vector3 jumpForce = new Vector3(0, Mathf.Sqrt(2 * Physics.gravity.magnitude * jumpHeight), forwardJumpDistance);
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce.y, jumpForce.z);
    }

    private void CalculateJumpScore(Transform platform)
    {
        float distance = Vector3.Distance(transform.position, platform.position);
        int score = 100;

        if (distance > 0.5f)
        {
            score -= (int)(distance * 10);
        }
        UIManager.AddScoreAction?.Invoke(score);

    }

    private void ResetBall()
    {
        prevPlatform = null;
        rb.isKinematic = true;
        transform.position = new Vector3(0, 2.5f, 0);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, -transform.up * 20);
    }

    private void OnDestroy()
    {
        startPlayerAction -= StartMovement;
        UIManager.RetryGameAction -= ResetBall;
    }
}