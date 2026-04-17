using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public PoolManager pool;
    public Player player;

    public float gameTime;
    public float maxGameTime = 20f;
    public bool isLive;

    // 0~9초: 레벨 0 / 10~20초: 레벨 1
    public int Level => Mathf.Min(Mathf.FloorToInt(gameTime / 10f), 1);

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        isLive = true;
    }

    void Update()
    {
        if (!isLive)
            return;

        gameTime += Time.deltaTime;

        if (gameTime >= maxGameTime)
        {
            gameTime = maxGameTime;
            isLive = false;
            Debug.Log("게임 종료");
        }
    }
}
