using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public Transform[] spawnPoint;
    public SpawnData[] spawnData;
    
    int level;
    float timer;

    void Awake()
    {
        //GetComponents여러 개의 컴포넌트를 가져오는 함수
        spawnPoint = GetComponentsInChildren<Transform>();

    }

     void Update()
    {
        // timer에 한 프레임마다의 시간을 계속 더함 ㄴ
        timer += Time.deltaTime; 
        level = Mathf.Min(Mathf.FloorToInt(GameManager.instance.gameTime / 10f), spawnData.Length - 1);
        if(timer > spawnData[level].spawnTime)
        {
            timer = 0;
            Spawn();
        }
    }

    void Spawn()
    {
        if (!GameManager.instance.isLive)
            return;

        GameObject enemy = GameManager.instance.pool.Get(0);
        enemy.transform.position = spawnPoint[Random.Range(1, spawnPoint.Length)].position;
        enemy.GetComponent<Enemy>().Init(spawnData[level]);
    }

}

//SpawnData 직렬화 = Unity 인스펙터에서 확인할 수 있게 함 
[System.Serializable]
public class SpawnData
{
    public int spriteType;  
    public float spawnTime;
    public int health;
    public float speed;
}
