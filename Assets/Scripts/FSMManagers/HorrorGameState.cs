using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HorrorGameState 
{
    Idle, //초기 상태
    ScanningRoom, //방 구조를 분석 (반복 감지 체크)
    GhostPlanted, //귀신 심어진 상태
    GhostAppearing, //시선 고정으로 등장
    GameOver // 손으로 화면을 덮는 연출


}
