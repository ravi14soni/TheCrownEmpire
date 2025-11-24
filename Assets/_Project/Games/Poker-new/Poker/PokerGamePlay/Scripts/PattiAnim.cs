using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PattiAnim : MonoBehaviour
{
    public Transform startPosition;
    public Transform[] endPositions;
    public float moveDuration = 1f;
    public float delayBetweenCards = 0.5f;
    public List<Transform> cards;
    public Sprite backcard;
    public List<Transform> middlecards;
    public Transform[] middlecardsendPositions;
    public List<Transform> fourthcard;
    public Transform[] fourthcardendPositions;
    public List<Transform> fifthcard;
    public Transform[] fifthendPositions;

    private void OnEnable()
    {
        StartCoroutine(MoveAllCards(cards, endPositions));
    }

    private void Start()
    {

    }

    public IEnumerator MoveAllCards(List<Transform> cards, Transform[] endpos)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].gameObject.GetComponent<SpriteRenderer>().sprite = backcard;
        }

        for (int i = 0; i < cards.Count; i++)
        {
            Debug.Log("Called");
            yield return new WaitForSeconds(delayBetweenCards);
            // if (PlayerPrefs.GetString("sound") == "on")
            // {
            //     NewAudioManager.instance.gamesource.clip = NewAudioManager.instance.Pattisoundclip;
            //     NewAudioManager.instance.gamesource.Play();
            // }
            Transform card = cards[i];
            card.position = startPosition.position;
            card.rotation = startPosition.rotation;
            card.localScale = startPosition.localScale;
            cards[i].gameObject.SetActive(true);
            StartCoroutine(MoveCard(card, i, endpos));
        }
    }

    private IEnumerator MoveCard(Transform card, int index, Transform[] endpos)
    {
        float elapsedTime = 0f;
        card.gameObject.SetActive(true);
        while (elapsedTime < moveDuration)
        {
            float t = elapsedTime / moveDuration;

            card.position = Vector3.Lerp(startPosition.position, GetEndPosition(index, endpos), t);

            card.rotation = Quaternion.Slerp(startPosition.rotation, GetEndRotation(index, endpos), t);

            card.localScale = Vector3.Lerp(startPosition.localScale, GetEndScale(index), t);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        card.position = GetEndPosition(index, endpos);
        card.rotation = GetEndRotation(index, endpos);
        card.localScale = GetEndScale(index);
    }

    private Vector3 GetEndPosition(int index, Transform[] endPositions)
    {
        return endPositions[index].position;
    }

    private Quaternion GetEndRotation(int index, Transform[] endPositions)
    {
        return endPositions[index].rotation;
    }

    private Vector3 GetEndScale(int index)
    {
        return Vector3.one;
    }
}
