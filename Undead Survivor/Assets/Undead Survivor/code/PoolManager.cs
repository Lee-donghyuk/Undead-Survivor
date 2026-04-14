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

   //게임 오브젝트 반환 함수
   public GameObject Get(int index)
   {
      GameObject select = null;

      // 선택한 pool의 놀고 있는 게임 오브젝트에 접근 
      foreach(GameObject item in pools[index])
      {
         if (!item.activeSelf)
         {// 비활성화 오브젝트 발견하면 select 변수에 할당
          // activeSelf -> 오브젝트가 활성화 상태인지 알 수 있음 
            select = item;
            // 발견한 비활성화 오브젝트 활성화
            select.SetActive(true);
            break;
         }
      }

      // 못찾았으면?
      if(select == null)
      { //새롭게 생성하고 selct 변수에 할당
            //Instantiate  -> 원본 오브젝트를 복제하여 장면에 생성하는 함수
            select =Instantiate(prefabs[index], transform);
            pools[index].Add(select);
      }

      return select;
   }
}
