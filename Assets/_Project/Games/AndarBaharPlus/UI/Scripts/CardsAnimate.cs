using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CardsAnimate : MonoBehaviour
{
    public RectTransform[] cards; // Assign your 3 images' RectTransforms in the inspector
    public float animationDuration = 2f; // Total duration for the animation loop
    public int loopCount = 10; // Number of loops to perform

    public Sprite main_card;

    private float[] targetPositions = { 160f, 0f, -160f }; // The target positions for each card

    void Start() { }

    public void StartSlotAnimation()
    {

        Sequence slotSequence = DOTween.Sequence();

        for (int i = 0; i < loopCount; i++)
        {
            foreach (var card in cards)
            {
                slotSequence.Append(
                    card.DOLocalMoveY(170f, animationDuration / (loopCount * 3))
                        .OnComplete(() =>
                        {
                            card.localPosition = new Vector3(
                                card.localPosition.x,
                                -170f,
                                card.localPosition.z
                            );
                        })
                );
            }
        }

        slotSequence.OnComplete(() => ResetCardsToFinalPositions());
    }

    public void SetMiddleSprite()
    {
        cards[1].GetComponent<Image>().sprite = main_card;
    }
    
    void ResetCardsToFinalPositions()
    {
        cards[0].localPosition = new Vector3(
            cards[0].localPosition.x,
            160f,
            cards[0].localPosition.z
        );
        cards[1].GetComponent<Image>().sprite = main_card;
        cards[1].localPosition = new Vector3(
            cards[1].localPosition.x,
            0f,
            cards[1].localPosition.z
        );
        cards[2].localPosition = new Vector3(
            cards[2].localPosition.x,
            -160f,
            cards[2].localPosition.z
        );
    }
}
