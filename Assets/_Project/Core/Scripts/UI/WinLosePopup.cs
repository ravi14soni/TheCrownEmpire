using DG.Tweening;
using TMPro;
using UnityEngine;

public class WinLosePopup : MonoBehaviour
{
    public TextMeshProUGUI textMeshProUGUI;

    void OnEnable()
    {
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        Invoke(nameof(ClosePopup), 1.5f);
    }
    public void ClosePopup()
    {
        transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }

    public void SetText(string text)
    {
        textMeshProUGUI.text = text;
    }
}
