using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    
    [Header("#GameObject")]
    public PoolManager pool;
    public Player player;
    public LevelUp uiLevelUp;
    [Header("#Game Control")]
    public bool isLive;
    public float gameTime;
    public float maxGameTime = 20f;
    [Header("#Player Info")]
    public int health;
    public int maxHealth = 100;
    public int level;
    public int kill;
    public int exp;
    public int[] nextExp = { 10, 30, 60, 100, 150, 210, 280, 360, 450, 600};



    // 0~9초: 레벨 0 / 10~20초: 레벨 1
    public int Level => Mathf.Min(Mathf.FloorToInt(gameTime / 10f), 1);

    void Awake()
    {
        instance = this;
    }

    public void GameStart()
    {
        health = maxHealth;
        uiLevelUp.Select(0);//임시 스크림트(첫번쨰 캐릭터 선택)
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

    public void GetExp()
    {
        exp++;
        if(exp == nextExp[Mathf.Min(level, nextExp.Length-1)])
        {
            level++;
            exp = 0;
            uiLevelUp.Show();
        }
    }

    public void Stop()
    {
        isLive = false;
        Time.timeScale = 0;
    }

    public void Resume()
    {
        
        isLive = true;
        Time.timeScale = 1;
    }
}
