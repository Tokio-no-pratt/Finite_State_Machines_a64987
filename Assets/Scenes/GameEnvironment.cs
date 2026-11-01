using System.Collections.Generic;
using UnityEngine;

public class GameEnvironment 
{
    private static GameEnvironment instance;
    private List<GameObject> checkpoint = new List<GameObject>();
    public List<GameObject> Checkpoints { get { return Checkpoints; } }

    public static GameEnvironment Singleton
    {
        get
        {
            if (instance == null)
            {
                instance = new GameEnvironment();
                instance.Checkpoints.AddRange(GameObject.FindGameObjectsWithTag("Checkpoint"));
            }
            return instance;
        }
    }
}
