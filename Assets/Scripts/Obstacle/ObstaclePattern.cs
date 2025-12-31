using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class PatternObject
{
    [Header("Basics")]
    public ObjectType objectType;

    public enum ObjectType
    {
        TallBranch,
        Coin,
        BigBranch,
        ShortBranch,
    }

    [Space]
    public float posOffset = 0f;
    [SerializeField] private float rotationOffset = 0f;
    [Space]
    [SerializeField] private float rotationRandomness = 0f;

    public float GetRotationOffset()
    {
        return rotationOffset + Random.Range(-rotationRandomness, rotationRandomness);
    }

    [Header("Repeating")]
    public int rotationRepeating = 0; //starts from 0, meaning no repeating
    public float rotationRepeatingUntil = 0f;

    public float GetRotationOffset(int index) //for repeating stuff
    {
        if (index == 0) return GetRotationOffset();

        float randValue = Random.Range(-rotationRandomness, rotationRandomness);
        float distancePerIndex = Mathf.DeltaAngle(rotationOffset, rotationRepeatingUntil) / (float)rotationRepeating;

        return distancePerIndex * index + randValue + rotationOffset;
    }
}

[CreateAssetMenu(menuName = "BunnyHop/Obstacle Pattern", fileName = "New Obstacle Pattern")]
public class ObstaclePattern : ScriptableObject
{
    public List<PatternObject> obstacles;

    public List<PatternObject> GetSortedList()
    {
        List<PatternObject> sortedList = new List<PatternObject>();
        sortedList = obstacles.OrderBy(i => i.posOffset).ToList();
        return sortedList;
    }

    [ContextMenu("Get Rotation Offsets")]
    public void GetRotationOffsets()
    {
        foreach (var obstacle in obstacles)
        {
            string log = $"{obstacle.objectType} at : ";

            if (obstacle.rotationRepeating < 1)
            {
                log += $"{obstacle.GetRotationOffset()}";
            }
            else
            {
                for (int i = 0; i <= obstacle.rotationRepeating; i++)
                {
                    log += $"{obstacle.GetRotationOffset(i)}, ";
                }
            }

            Debug.Log(log);
        }
    }
}