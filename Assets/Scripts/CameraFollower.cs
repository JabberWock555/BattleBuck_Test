using System;
using System.Threading.Tasks;
using UnityEngine;

public class CameraFollower : MonoBehaviour
{

    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset;
    [SerializeField] private float smoothSpeed = 0.125f, clampXAxis = 4f;

    private bool canFollow = true;

    void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("CameraFollowZ: No target assigned.");
            return;
        }
        BallController.startPlayerAction += StartMovement;
        UIManager.GameOverAction += StopMovement;
        UIManager.RetryGameAction += ResetPosition;
    }

    private void StartMovement() => canFollow = true;
    private void StopMovement() => canFollow = false;

    void LateUpdate()
    {
        if (!canFollow) return;

        if (target == null)
        {
            Debug.LogWarning("CameraFollowZ: No target assigned.");
            return;
        }

        float clampedX = Mathf.Clamp(target.position.x + offset.x, -clampXAxis, clampXAxis);
        Vector3 desiredPosition = new Vector3(clampedX, transform.position.y, target.position.z + offset.z);

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        transform.position = smoothedPosition;
    }

    private async void ResetPosition()
    {
        await Task.Yield();
        float clampedX = Mathf.Clamp(target.position.x + offset.x, -clampXAxis, clampXAxis);
        Vector3 desiredPosition = new Vector3(clampedX, transform.position.y, target.position.z + offset.z);


        transform.position = desiredPosition;
    }

    private void OnDestroy()
    {
        BallController.startPlayerAction -= StartMovement;
        UIManager.GameOverAction -= StopMovement;
        UIManager.RetryGameAction -= ResetPosition;
    }
}
