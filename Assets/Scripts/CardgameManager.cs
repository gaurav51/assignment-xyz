using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[System.Serializable]
public class GameSaveData
{
    public int rows;
    public int columns;
    public int score;
    public int combo;
    public int moves;
    public int lives;
    public int turnNumber;
    public int matchNumber;
    public List<int> gridCardTypes;
    public List<bool> gridCardMatched;
}

public class CardgameManager : MonoBehaviour
{
    public List<CardInteractable> cardPrefabs;
    public List<CardInteractable> cards;
    public static CardgameManager Instance;

    [Header("Grid Settings")]
    [Range(2, 6)]
    public int rows = 2;
    [Range(2, 6)]
    public int columns = 2;
    public float spacing = 1.2f;

    [Header("Camera Settings")]
    public Camera mainCamera;
    public float cameraPadding = 1.0f;

    [Header("Gameplay & Audio")]
    public AudioSource audioSource;
    public AudioClip flipSound;
    public AudioClip matchSound;
    public AudioClip mismatchSound;
    public AudioClip gameOverSound;
    public AudioClip comboSound;

    public int score = 0;
    public int combo = 0;
    public int moves = 0;
    public int lives = 3;
    public int level = 1;
    public int matchNumber = 0;
    public int turnNumber = 1;

    private List<CardInteractable> currentlyFlipped = new List<CardInteractable>();
    private int matchedPairsCount = 0;
    private int totalPairs;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        level = PlayerPrefs.GetInt("PlayerLevel", 1);

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

    }

    public void LoadGameOrInitialize()
    {
        if (PlayerPrefs.HasKey("CardGameSaveData"))
        {
            string json = PlayerPrefs.GetString("CardGameSaveData");
            GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
            if (data != null && data.gridCardTypes.Count == data.rows * data.columns)
            {
                score = data.score;
                combo = data.combo;
                moves = data.moves;
                lives = data.lives;
                turnNumber = data.turnNumber;
                matchNumber = data.matchNumber;
                
                // If lives ever somehow dropped to <=0 in save, restore to 3
                if (lives <= 0) lives = 3; 

                RestoreGrid(data);
                return;
            }
        }
        InitializeCardSetup();
    }

    public void SaveGame()
    {
        GameSaveData data = new GameSaveData
        {
            rows = rows,
            columns = columns,
            score = score,
            combo = combo,
            moves = moves,
            lives = lives,
            turnNumber = turnNumber,
            matchNumber = matchNumber,
            gridCardTypes = new List<int>(),
            gridCardMatched = new List<bool>()
        };

        foreach (var c in cards)
        {
            data.gridCardTypes.Add((int)c.cardType);
            data.gridCardMatched.Add(c.cardState == CardState.Matched);
        }

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("CardGameSaveData", json);
        PlayerPrefs.Save();
    }

    public void ClearSave()
    {
        PlayerPrefs.DeleteKey("CardGameSaveData");
    }

    void InitializeCardSetup()
    {
        score = 0;
        combo = 0;
        moves = 0;
        turnNumber = 1;
        matchNumber = 0;
        
        UpdateGridSizeForLevel(level);

        UpdateAllUIEvents();
        GenerateGrid(rows, columns);
    }

    private void UpdateAllUIEvents()
    {
        GameEvents.OnScoreUpdate?.Invoke(score);
        GameEvents.OnMovesUpdate?.Invoke(moves);
        GameEvents.OnLivesUpdate?.Invoke(lives);
        GameEvents.OnLevelUpdate?.Invoke(level);
        GameEvents.OnMatchNumberUpdated?.Invoke(matchNumber);
        GameEvents.OnTurnNumberUpdated?.Invoke(turnNumber);
    }

    public void GenerateGrid(int r, int c)
    {
        float startX = -(c - 1) * spacing / 2f;
        float startY = (r - 1) * spacing / 2f;

        int totalExpectedCards = r * c;
        totalPairs = totalExpectedCards / 2;
        matchedPairsCount = 0;

        List<CardInteractable> cardsToInstantiate = new List<CardInteractable>();
        int pairsCount = totalExpectedCards / 2;

        for (int i = 0; i < pairsCount; i++)
        {
            if (cardPrefabs == null || cardPrefabs.Count == 0) break;
            CardInteractable prefab = cardPrefabs[i % cardPrefabs.Count];
            cardsToInstantiate.Add(prefab);
            cardsToInstantiate.Add(prefab);
        }

        if (cardsToInstantiate.Count < totalExpectedCards && cardPrefabs != null && cardPrefabs.Count > 0)
        {
            cardsToInstantiate.Add(cardPrefabs[0]);
        }

        for (int i = 0; i < cardsToInstantiate.Count; i++)
        {
            CardInteractable temp = cardsToInstantiate[i];
            int randomIndex = Random.Range(i, cardsToInstantiate.Count);
            cardsToInstantiate[i] = cardsToInstantiate[randomIndex];
            cardsToInstantiate[randomIndex] = temp;
        }

        if (cards == null) cards = new List<CardInteractable>();

        foreach (var card in cards)
            if (card != null) Destroy(card.gameObject);
        cards.Clear();

        for (int i = 0; i < totalExpectedCards; i++)
        {
            int row = i / c;
            int col = i % c;

            float posX = startX + col * spacing;
            float posY = startY - row * spacing;

            Vector3 cardPos = new Vector3(posX, posY, 0f);

            if (i < cardsToInstantiate.Count)
            {
                CardInteractable newCard = Instantiate(cardsToInstantiate[i], cardPos, Quaternion.identity);
                newCard.transform.parent = this.transform;
                newCard.InitializeCard();
                cards.Add(newCard);
            }
        }

        currentlyFlipped.Clear();
        SaveGame();
        AdjustCameraSize(r, c);
    }

    public void RestoreGrid(GameSaveData data)
    {
        float startX = -(data.columns - 1) * spacing / 2f;
        float startY = (data.rows - 1) * spacing / 2f;

        this.rows = data.rows;
        this.columns = data.columns;

        int totalExpectedCards = data.rows * data.columns;
        totalPairs = totalExpectedCards / 2;
        matchedPairsCount = 0;

        if (cards == null) cards = new List<CardInteractable>();
        foreach (var card in cards)
            if (card != null) Destroy(card.gameObject);
        cards.Clear();

        for (int i = 0; i < totalExpectedCards; i++)
        {
            int row = i / data.columns;
            int col = i % data.columns;

            float posX = startX + col * spacing;
            float posY = startY - row * spacing;

            Vector3 cardPos = new Vector3(posX, posY, 0f);

            CardInteractable prefab = null;
            if (cardPrefabs != null && cardPrefabs.Count > 0)
            {
                foreach (var p in cardPrefabs)
                {
                    if ((int)p.cardType == data.gridCardTypes[i])
                    {
                        prefab = p;
                        break;
                    }
                }
                if (prefab == null) prefab = cardPrefabs[0];
            }

            if (prefab != null)
            {
                CardInteractable newCard = Instantiate(prefab, cardPos, Quaternion.identity);
                newCard.transform.parent = this.transform;
                newCard.cardType = (CardType)data.gridCardTypes[i];

                if (data.gridCardMatched[i])
                {
                    newCard.ForceMatch();
                    matchedPairsCount++; // Note: this increments per card, not per pair
                }
                else
                {
                    newCard.InitializeCard();
                }

                cards.Add(newCard);
            }
        }

        matchedPairsCount /= 2;
        matchNumber = matchedPairsCount;
        currentlyFlipped.Clear();
        UpdateAllUIEvents();
        AdjustCameraSize(data.rows, data.columns);
    }

    public void AdjustCameraSize(int r, int c)
    {
        if (mainCamera == null) return;
        float targetHeight = r * spacing;
        float targetWidth = c * spacing;
        float screenAspect = (float)Screen.width / Screen.height;
        float sizeFromHeight = targetHeight / 2f;
        float sizeFromWidth = targetWidth / (2f * screenAspect);
        mainCamera.orthographicSize = Mathf.Max(sizeFromHeight, sizeFromWidth) + cameraPadding;
    }

    public void CardFlipped(CardInteractable card)
    {
        PlaySound(flipSound);
        currentlyFlipped.Add(card);

        if (currentlyFlipped.Count >= 2)
        {
            CardInteractable c1 = currentlyFlipped[0];
            CardInteractable c2 = currentlyFlipped[1];
            currentlyFlipped.RemoveAt(0);
            currentlyFlipped.RemoveAt(0);

            DOVirtual.DelayedCall(0.5f, () => CompareCards(c1, c2));
        }
    }

    private void CompareCards(CardInteractable c1, CardInteractable c2)
    {
        moves++;
        turnNumber++;
        GameEvents.OnMovesUpdate?.Invoke(moves);
        GameEvents.OnTurnNumberUpdated?.Invoke(turnNumber);

        if (c1.cardType == c2.cardType)
        {
            combo++;
            score += 10 * combo;
            matchNumber++;
            GameEvents.OnScoreUpdate?.Invoke(score);
            GameEvents.OnMatchNumberUpdated?.Invoke(matchNumber);

            if (combo > 1)
            {
                GameEvents.OnComboUpdate?.Invoke(combo);
                PlaySound(comboSound);
            }
            else
            {
                PlaySound(matchSound);
            }

            c1.Match();
            c2.Match();

            matchedPairsCount++;
            CheckWinCondition();
        }
        else
        {
            PlaySound(mismatchSound);
            combo = 0;
            score = Mathf.Max(0, score - 2);
            GameEvents.OnScoreUpdate?.Invoke(score);
            
            lives--;
            GameEvents.OnLivesUpdate?.Invoke(lives);

            c1.CloseCardWait();
            c2.CloseCardWait();
        }

        SaveGame();
    }

    private void CheckWinCondition()
    {
        if (matchedPairsCount >= totalPairs)
        {
            ClearSave();
            Debug.Log($"Level {level} Complete! Score: {score}, Final Combo: {combo}");
             UpdateDataForNextLevel(); 
            DOVirtual.DelayedCall(1.5f, () =>{ 
               
                GameEvents.OnGameWin?.Invoke();
                });
        }
    }

    public void UpdateDataForNextLevel()
    {
        level++;
        PlayerPrefs.SetInt("PlayerLevel", level);
        PlayerPrefs.Save();
        
        UpdateGridSizeForLevel(level);

        moves = 0;
        turnNumber = 1;
        combo = 0;
        matchNumber = 0;

        UpdateAllUIEvents();
    }

    public void NextLevel()
    {
        GenerateGrid(rows, columns);
    }

    private void UpdateGridSizeForLevel(int currentLevel)
    {
        // Define grid sizes (Rows, Columns). Must result in an even total number of cards.
        int[,] gridSizes = new int[,] 
        {
            {2, 2}, // Lvl 1: 4 cards
            {2, 2}, // Lvl 2: 4 cards
            {3, 2}, // Lvl 3: 6 cards
            {3, 2}, // Lvl 4: 6 cards
            {4, 2}, // Lvl 5: 8 cards
            {4, 2}, // Lvl 6: 8 cards
            {5, 2}, // Lvl 7: 10 cards
            {5, 2}, // Lvl 8: 10 cards
            {4, 3}, // Lvl 9: 12 cards
            {4, 3}, // Lvl 10: 12 cards
            {7, 2}, // Lvl 11: 14 cards
            {7, 2}, // Lvl 12: 14 cards
            {4, 4}, // Lvl 13: 16 cards
            {4, 4}, // Lvl 14: 16 cards
            {6, 3}, // Lvl 15: 18 cards
            {6, 3}, // Lvl 16: 18 cards
            {5, 4}, // Lvl 17: 20 cards
            {5, 4}, // Lvl 18: 20 cards
            {6, 4}, // Lvl 19: 24 cards
            {6, 4}, // Lvl 20: 24 cards
            {6, 5}, // Lvl 21: 30 cards
            {6, 6}  // Lvl 22+: 36 cards
        };

        int index = Mathf.Clamp(currentLevel - 1, 0, gridSizes.GetLength(0) - 1);
        rows = gridSizes[index, 0];
        columns = gridSizes[index, 1];
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }
}
