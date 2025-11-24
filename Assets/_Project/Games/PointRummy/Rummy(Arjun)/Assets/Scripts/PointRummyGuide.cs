using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System;
using System.Collections.Generic;
using System.Collections;

public class PointRummyGuide : MonoBehaviour
{
    public GameObject arrowObj, bgObj;
    public TMP_Text textObj;
    public Sprite arrowSP, handSP;
    public List<GuideTableData> guideData = new List<GuideTableData>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(StartGuide());
    }

    IEnumerator StartGuide()
    {
        yield return new WaitForSeconds(5);
        for (int i = 0; i < guideData.Count; i++)
        {
            arrowObj.SetActive(true);
            bgObj.SetActive(true);
            if (guideData[i].obj.name == "FinishDesk")
            {
                FinishDeskArrowAnimation(guideData[i]);
            }
            else
            {
                ShowArrow(guideData[i]);
            }
            yield return new WaitForSeconds(4);
            arrowObj.SetActive(false);
            bgObj.SetActive(false);

        }

        yield return new WaitForSeconds(1);
        arrowObj.transform.DOKill();
        CachetaConnection con = FindAnyObjectByType<CachetaConnection>();
        con.guideBool = true;
        con.CustomStartAfterGuide();
        gameObject.SetActive(false);
    }

    void ShowArrow(GuideTableData data)
    {
        arrowObj.transform.DOKill();
        arrowObj.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = arrowSP;
        Vector3 pos;
        if (data.obj.GetComponent<RectTransform>() == null)
        {
            pos = data.obj.transform.position + data.offset;
        }
        else
        {
            Vector3 pos1 = Camera.main.ScreenToWorldPoint(data.obj.transform.position);
            pos = new Vector3(pos1.x, pos1.y, 0) + data.offset;
        }
        transform.position = pos;
        textObj.text = data.message;
        arrowObj.transform.localPosition = Vector3.zero;
        arrowObj.transform.DOLocalMove(new Vector3(0.5f, 0.5f, 0), 0.3f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
    }

    //different animation created only for Declare
    void FinishDeskArrowAnimation(GuideTableData data)
    {
        arrowObj.transform.DOKill();
        arrowObj.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = handSP;
        transform.position = data.obj.transform.position + data.offset;
        textObj.text = data.message;
        arrowObj.transform.localPosition = new Vector3(-2f, -3f, 0);
        arrowObj.transform.DOLocalMove(new Vector3(0,-0.5f, 0), 1f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart);
    }

}

[Serializable]
public class GuideTableData
{
    public GameObject obj;
    public Vector3 offset;
    public string message;
}
