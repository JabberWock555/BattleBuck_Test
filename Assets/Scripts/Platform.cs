using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class Platform : MonoBehaviour
{
    [Header("Visual Feedback")]
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float shakeStrength = 0.1f;
    [SerializeField] private int shakeVibrato = 10;

    [Header("ShatterEffect")]
    [SerializeField] private GameObject shatterPrefab;
    [SerializeField] private float explosionForce = 300f;
    [SerializeField] private float explosionRadius = 4f;
    [SerializeField] private float downwardModifier = 2f;

    private List<Transform> cubePieces = new();
    private List<Vector3> originalPositions = new();
    private List<Quaternion> originalRotations = new();
    private List<Vector3> originalScale = new();
    private Collider _collider;
    private Renderer _renderer;
    private AudioSource audioSource;
    private bool isBroken = false;

    private void Start()
    {

        _collider = GetComponent<Collider>();
        _renderer = GetComponent<Renderer>();
        audioSource = GetComponent<AudioSource>();
        SetPieces();
    }

    private void SetPieces()
    {
        foreach (Transform child in shatterPrefab.transform)
        {
            cubePieces.Add(child);
            originalPositions.Add(child.localPosition);
            originalRotations.Add(child.localRotation);
            originalScale.Add(child.localScale);


            if (!child.TryGetComponent<Rigidbody>(out var rb))
            {
                rb = child.gameObject.AddComponent<Rigidbody>();
                rb.isKinematic = true;
            }
        }
    }

    private async void BreakCube()
    {
        await Task.Delay(250);
        if (isBroken) return;

        audioSource.Play();
        _renderer.enabled = false;
        Vector3 explosionPoint = transform.position;

        foreach (Transform piece in cubePieces)
        {
            Rigidbody rb = piece.GetComponent<Rigidbody>();
            rb.isKinematic = false;

            rb.AddTorque(Random.Range(-5f, 5f), Random.Range(-5f, 5f), Random.Range(-5f, 5f), ForceMode.Impulse);

            Vector3 randomOffset = new Vector3(
                Random.Range(-0.5f, 0.5f),
                Random.Range(-0.5f, 0.5f),
                Random.Range(-0.5f, 0.5f)
            );

            rb.AddExplosionForce(explosionForce, explosionPoint + randomOffset, explosionRadius, downwardModifier);
        }

        isBroken = true;
    }

    public void ResetPieces()
    {
        if (!isBroken) return;
        _renderer.enabled = true;
        for (int i = 0; i < cubePieces.Count; i++)
        {
            Transform piece = cubePieces[i];
            Rigidbody rb = piece.GetComponent<Rigidbody>();

            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            piece.localPosition = originalPositions[i];
            piece.localRotation = originalRotations[i];
            piece.localScale = originalScale[i];
        }

        isBroken = false;
    }



    private void OnCollisionEnter(Collision other)
    {
        PlatformSpawner.PlayerLanded_Action?.Invoke(this);
        PlayerLanded();
        BreakCube();
    }

    public void PlayerLanded()
    {
        _collider.enabled = false;
        transform.DOShakeScale(shakeDuration, shakeStrength, shakeVibrato, randomnessMode: ShakeRandomnessMode.Harmonic)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => _collider.enabled = true);
    }

    public void ResetPlatform()
    {
        isBroken = false;
        _collider.enabled = true;
        _renderer.enabled = true;
        ResetPieces();
    }

    public void ChangeColor(Color color)
    {
        if (_renderer != null)
        {
            _renderer.materials[0].color = color;
            _renderer.materials[0].SetColor("_EmissionColor", color);

            foreach (var piece in cubePieces)
            {
                Renderer pieceRenderer = piece.GetComponent<Renderer>();
                if (pieceRenderer != null)
                {
                    pieceRenderer.materials[0].color = color;
                    pieceRenderer.materials[0].SetColor("_EmissionColor", color);
                }
            }
        }
    }


    private void OnDestroy()
    {
        transform.DOKill();
    }
}