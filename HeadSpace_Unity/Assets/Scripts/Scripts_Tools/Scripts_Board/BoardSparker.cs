using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardSparker : MonoBehaviour
{
    public ParticleSystem sparker;
    public Transform[] lowPositions;
    public Transform[] allPositions;
    private int lowPosCount;
    private int allPosCount;
    private SlidableBoard _board;
    private int sparkChance;

    private void Awake()
    {
        lowPosCount = lowPositions.Length;
        allPosCount = allPositions.Length;
        _board = GetComponentInParent<SlidableBoard>();
    }

    private void Start()
    {
        sparkChance = 3;
    }

    private void OnEnable()
    {
        MovableContract.contractAssigned += PlayRandomBurst;
    }

    private void OnDisable()
    {
        MovableContract.contractAssigned -= PlayRandomBurst;
    }

    private void PlayRandomBurst(MovableContract contract)
    {
        int randomRoll = UnityEngine.Random.Range(sparkChance, 4);
        if (randomRoll == 3)
        {
            if (_board.IsOpen)
            {
                int randomIndex = UnityEngine.Random.Range(0, allPosCount);
                sparker.transform.position = allPositions[randomIndex].position;
                sparker.Play();
            }
            else
            {
                int randomIndex = UnityEngine.Random.Range(0, lowPosCount);
                sparker.transform.position = lowPositions[randomIndex].position;
                sparker.Play();
            }
            sparkChance = 0;
        }
        else
        {
            sparkChance++;
        }

    }
}
