using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PlatformSpawner : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private Platform platformPrefab;
    [SerializeField] private int poolSize = 6;
    [SerializeField] private float initialZSpacing = 5f;
    [SerializeField] private float yOffset = -1.5f;
    [SerializeField] private float[] possibleXPositions = { -4f, 0f, 4f };


    [Header("\nAnimation Settings")]
    [SerializeField] private float fallHeight = 5f;    // How high above target position the platform starts
    [SerializeField] private float fallDuration = 1f;   // How long the falling animation takes
    [SerializeField] private Ease fallEase = Ease.OutBounce;

    [SerializeField] private Color[] platformColors = { Color.yellow, Color.red, Color.green, Color.blue, Color.magenta, Color.cyan };

    private List<Platform> activePlatforms = new List<Platform>();
    private int currentPlatformIndex = 0;
    private int currentColorIndex = 0;

    public static Action<Platform> PlayerLanded_Action;
    private void Awake()
    {
        InitializePlatformPool();

        PlayerLanded_Action += OnPlatformReached;
    }

    private void Start()
    {
        UIManager.RetryGameAction += ResetAllPlatforms;
    }

    private void InitializePlatformPool()
    {

        for (int i = 0; i < poolSize; i++)
        {
            Vector3 position = new Vector3(
                0f,
                yOffset,
                i * initialZSpacing
            );

            if (i > 0)
            {
                position.x = possibleXPositions[UnityEngine.Random.Range(0, possibleXPositions.Length)];
            }

            Platform platform = Instantiate(platformPrefab, position, Quaternion.identity, transform);

            activePlatforms.Add(platform);
            platform.gameObject.SetActive(true);
        }
    }

    private void ResetAllPlatforms()
    {
        currentPlatformIndex = 0;
        currentColorIndex = 0;
        Time.timeScale = 1;
        for (int i = 0; i < poolSize; i++)
        {
            Vector3 position = new Vector3(
                0f,
                yOffset,
                i * initialZSpacing
            );

            if (i > 0)
            {
                position.x = possibleXPositions[UnityEngine.Random.Range(0, possibleXPositions.Length)];
            }

            activePlatforms[i].transform.position = position;
            activePlatforms[i].gameObject.SetActive(true);
            activePlatforms[i].ResetPlatform();
            activePlatforms[i].ChangeColor(platformColors[currentColorIndex]);

        }
    }

    private void OnPlatformReached(Platform platform)
    {
        currentPlatformIndex++;

        Debug.Log(" OnPlatformReached: " + currentPlatformIndex % 10);
        if (currentPlatformIndex % 10 == 0)
        {
            ChangePlatformColor();
        }

        if (currentPlatformIndex >= 2)
        {
            ReusePlatform();
        }
    }


    private void ChangePlatformColor()
    {
        if (currentColorIndex >= platformColors.Length - 1)
        {
            currentColorIndex = 0;
        }
        else
        {
            currentColorIndex++;
        }

        Time.timeScale += 0.05f;

        for (int i = 0; i < activePlatforms.Count; i++)
        {
            activePlatforms[i].ChangeColor(platformColors[currentColorIndex]);
        }
    }

    private void ReusePlatform()
    {

        Platform platformToRecycle = activePlatforms[0];
        activePlatforms.RemoveAt(0);
        platformToRecycle.ResetPieces();

        Vector3 lastPlatformPos = activePlatforms[activePlatforms.Count - 1].transform.position;
        Vector3 targetPosition = new Vector3(
            possibleXPositions[UnityEngine.Random.Range(0, possibleXPositions.Length)],
            lastPlatformPos.y,
            lastPlatformPos.z + initialZSpacing
        );


        Vector3 startPosition = targetPosition + Vector3.up * fallHeight;
        platformToRecycle.transform.position = startPosition;

        platformToRecycle.transform
            .DOMove(targetPosition, fallDuration)
            .SetEase(fallEase);

        activePlatforms.Add(platformToRecycle);
    }

    private void OnDestroy()
    {
        PlayerLanded_Action -= OnPlatformReached;
        UIManager.RetryGameAction -= InitializePlatformPool;
    }


}