using UnityEngine;
using TMPro;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI matchNumberText;
    public TextMeshProUGUI turnNumberText;
    public TextMeshProUGUI movesText;
    
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI gameLevelText;

    public GameObject comboEffect;
    public GameObject winPanel;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        levelText.text = GameConstants.LevelPrefix + PlayerPrefs.GetInt("PlayerLevel", 1);
        gameLevelText.text = GameConstants.LevelPrefix + PlayerPrefs.GetInt("PlayerLevel", 1);
    }

    private void OnEnable()
    {
        GameEvents.OnScoreUpdate += UpdateScore;
        GameEvents.OnLevelUpdate += UpdateLevel;
        GameEvents.OnTurnNumberUpdated += UpdateTurnNumber;
        GameEvents.OnMatchNumberUpdated += UpdateMatchNumber;
        GameEvents.OnComboUpdate += UpdateCombo;
        GameEvents.OnGameWin += ShowWinPanel;
    }

    private void OnDisable()
    {
        GameEvents.OnScoreUpdate -= UpdateScore;
        GameEvents.OnLevelUpdate -= UpdateLevel;
        GameEvents.OnTurnNumberUpdated -= UpdateTurnNumber;
        GameEvents.OnMatchNumberUpdated -= UpdateMatchNumber;
        GameEvents.OnComboUpdate -= UpdateCombo;
        GameEvents.OnGameWin -= ShowWinPanel;       
    }

    private void UpdateScore(int score) 
    {
        scoreText.text = GameConstants.ScorePrefix + score;
    }

    private void UpdateLevel(int level)
    {
        levelText.text = GameConstants.LevelPrefix + level;
        gameLevelText.text = GameConstants.LevelPrefix + level;
    }

    private void UpdateMatchNumber(int matchNumber)
    {
        matchNumberText.text = GameConstants.MatchPrefix + matchNumber;
    }

    private void UpdateTurnNumber(int turnNumber)
    {
        turnNumberText.text = GameConstants.TurnPrefix + turnNumber;
    }

    private void UpdateMoves(int moves)
    {
        movesText.text = GameConstants.MovesPrefix + moves;
    }

    private void UpdateCombo(int combo)
    {
        comboEffect.SetActive(combo > 1);
        comboEffect.transform.localScale = Vector3.zero;
        comboEffect.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        comboText.text = "Combo X" + combo;
        DOVirtual.DelayedCall(0.75f, () => comboEffect.SetActive(false));
    }

    public void OnPlayButtonClicked()
    {
        CardgameManager.Instance.LoadGameOrInitialize();
    }

    void ShowWinPanel()
    {
        winPanel.SetActive(true);
        winPanel.transform.localScale = Vector3.zero;
        winPanel.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
    }

    public void OnClickNextLevel()
    {
        winPanel.SetActive(false);
        CardgameManager.Instance.NextLevel();
    }
}
