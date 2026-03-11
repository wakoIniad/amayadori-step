using System;
using UnityEngine;

public class IntergrationManager : MonoBehaviour
{

    public static IntergrationManager instance;

    public enum GameState {
        TITLE,
        PLAYING
    }
    public GameState gameState = GameState.TITLE;
    public MapGenerator mapGenerator;
    void Awake() {
        if(instance)
        {
            Destroy(this);
            throw new Exception("[あ]: Already exsists IntergrationManager instance");
        } else instance = this;

        DontDestroyOnLoad(this);
    }

    [NonSerialized] public PlayerControl playerControl;
    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
