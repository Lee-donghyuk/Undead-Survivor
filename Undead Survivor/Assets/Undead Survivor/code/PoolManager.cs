using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
   //..프리팹을 보관할 변수
   public GameObject[] prefabs;

   //.. 풀 담당을 하는 리스트
   List<GameObject>[] pools;

   void Awake()
   {
      pools = new List<GameObject>[prefabs.Length];

      for(int index = 0; index < pools.Length; index++)
      {
         //모든 풀리스트 초기화
         pools[index] = new List<GameObject>();
      }
      Debug.Log(pools.Length);
   }
}
