using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchiveManager : MonoBehaviour
{
    public GameObject[] lockCharacter;
    public GameObject[] unLockCharacter;
    public GameObject uiNotice;

    enum Achive { UnlockPotato, unlockApple};
    Achive[] achives;
    WaitForSecondsRealtime wait;

    void Awake()
    {   //enum(열거형 데이터)에 데이터를 가져오는 함수
         achives = (Achive[])Enum.GetValues(typeof(Achive));
        wait = new WaitForSecondsRealtime(5);

        if (!PlayerPrefs.HasKey("MyData"))
        {
            init();
        }
    }

    void init()
    {   //유니티의 간단한 저장 기능을 제공하는 클래스
        PlayerPrefs.SetInt("MyData", 1);

        foreach(Achive achive in achives)
        {
            PlayerPrefs.SetInt(achive.ToString(), 0);
        }
    }
    
    void Start()
    {
        UnlockCharachter();
    }

    void UnlockCharachter()
    {
        for(int index = 0; index < lockCharacter.Length; index++)
        {
            string achiveName = achives[index].ToString();
            bool isUnlock = PlayerPrefs.GetInt(achiveName) == 1;
            lockCharacter[index].SetActive(!isUnlock);
            unLockCharacter[index].SetActive(isUnlock);
        }
    }

    void LateUpdate()
    {
           foreach(Achive achive in achives)
        {
            CheckAchive(achive);
        }
    }

    void CheckAchive(Achive achive)
    {
        bool isAchive = false;

        switch (achive)
        {
            case Achive.UnlockPotato:
                isAchive = GameManager.instance.kill >= 10;
                break;
            case Achive.unlockApple:
                isAchive = GameManager.instance.gameTime >= GameManager.instance.maxGameTime;
                break;
        }
        if(isAchive && PlayerPrefs.GetInt(achive.ToString()) == 0){
            PlayerPrefs.SetInt(achive.ToString(), 1);
            
            for(int index = 0; index < uiNotice.transform.childCount; index++)
            {
                bool isActive = index == (int)achive;
                uiNotice.transform.GetChild(index).gameObject.SetActive(isActive);
            }
            StartCoroutine(NoticeRountine());
        }
    }

    IEnumerator NoticeRountine()
    {
        uiNotice.SetActive(true);
        yield return wait;
        uiNotice.SetActive(false);
    }
}
