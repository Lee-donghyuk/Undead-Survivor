using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public Transform[] spawnPoint;
    
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
        if(timer > 0.9f)
        {
            timer = 0f;
            Spawn();
        }
    }

    void Spawn()
    {   
        //poolManager 스크립트 안에 있는 pools안에 있는 게임 오브젝트 [0 , 1] 중 랜덤으로 가져올거다 
        //poolManager는 GameManager 스크립트안에서 pool로 부르고 있음 
        GameObject enemy = GameManager.instance.pool.Get(Random.Range(0,2));
        enemy.transform.position = spawnPoint[Random.Range(1,spawnPoint.Length)].position;
    }
}
