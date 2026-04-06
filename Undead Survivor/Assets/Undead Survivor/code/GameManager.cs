using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //GameManager 자체를 메모리에 올린다? 
    public static GameManager instance;
    public Player player;

    void Awake()
    {
        Debug.Log("게임 start");
        instance = this; //자기 자신을 넣는다 .
    }
}
