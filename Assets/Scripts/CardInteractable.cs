using DG.Tweening;
using UnityEngine;

public class CardInteractable : MonoBehaviour
{
    public CardState cardState;
    public CardType cardType;
    public bool canBeFlipped = true;

    public Vector3 childPos = new Vector3(0, 0, -0.01f);
    public Vector3 childScale = new Vector3(0.7f, 1, 1);

    void Start()
    {
        transform.GetChild(0).localPosition = childPos;
        transform.GetChild(2).localScale = childScale;
    }

    void OnEnable()
    {
        cardState = CardState.Hidden;
    }

    void OnDisable()
    {
        cardState = CardState.Hidden;
    }

    public void Match()
    {
        cardState = CardState.Matched;
        canBeFlipped = false;
        transform.DOShakeScale(0.3f, 0.2f);
    }

    public void ForceMatch()
    {
        cardState = CardState.Matched;
        canBeFlipped = false;
        transform.eulerAngles = new Vector3(0, 180, 0); // Open immediately
        transform.localScale = Vector3.one;
    }

    public void CloseCardWait()
    {
        transform.DORotate(new Vector3(0, 0, 0), 0.1f).SetEase(Ease.Linear).OnComplete(() =>
        {
            cardState = CardState.Hidden;
            canBeFlipped = true;
        });
    }

    void PerformFlip()
    {
        canBeFlipped = false;
        transform.DORotate(new Vector3(0, 180, 0), 0.5f).SetEase(Ease.Linear).OnComplete(() =>
        {
            cardState = CardState.Revealed;
            CardgameManager.Instance.CardFlipped(this);
        });
    }

    void OnMouseDown()
    {
        if (canBeFlipped && cardState == CardState.Hidden)
        {
            PerformFlip();
        }
    }

    public void SetCardType(CardType type)
    {
        cardType = type;
    }

    public void InitializeCard()
    {
        cardState = CardState.Hidden;
        canBeFlipped = true;
        transform.localScale = Vector3.zero;
        transform.localRotation = Quaternion.Euler(0, 180, 0);

        transform.DOScale(Vector3.one, 1f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            transform.localRotation = Quaternion.Euler(0, 0, 0);
        });
    }
}
