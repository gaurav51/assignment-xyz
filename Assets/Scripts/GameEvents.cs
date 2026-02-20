using UnityEngine;
using System;

public static class GameEvents {
    public static Action OnGameStart;
    public static Action OnGameWin;
    public static Action OnGameLose;
    public static Action OnGamePause;
    public static Action OnGameResume;
    public static Action OnGameQuit;

    public static Action OnCardFlip;
    public static Action OnHitPlay;
    public static Action<int> OnCombo;

    public static Action<int> OnScoreUpdate;
    
    public static Action<int> OnMatchNumberUpdated;

    public static Action<int> OnTurnNumberUpdated;

    public static Action<int> OnLevelUpdate;
    public static Action<int> OnMovesUpdate;
    public static Action<int> OnLivesUpdate;
    public static Action<int> OnComboUpdate;
    
}