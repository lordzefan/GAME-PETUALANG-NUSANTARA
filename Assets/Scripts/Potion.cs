using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Potion : MonoBehaviour
{
    public PotionType potionType;
    
    public int xIndex;
    public int yIndex;

    public bool isMatched;
    private Vector2 currentPos;
    private Vector2 targetPos;
    public bool isMoving;
    private Tween selectionTween;
    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }


    public void SetSelected(bool selected)
{
    if (selected)
    {
        // Start new selection animation
        selectionTween = transform.DOScale(0.7f, 0.3f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);

        sr.DOColor(Color.yellow, 0.2f);
    }
    else
    {
        // Stop selection animation
        if (selectionTween != null)
        {
            selectionTween.Kill();
            selectionTween = null;
        }

        // Reset scale and color to default
        transform.DOScale(0.5f, 0.2f);
        sr.DOColor(Color.white, 0.2f);
    }
}

    public Potion(int _x, int _y)
    {
        xIndex = _x;
        yIndex = _y;
    }

    public void SetIndicies(int _x, int _y)
    {
        xIndex = _x;
        yIndex = _y;
    }

    // move to target
    public void MoveToTarget(Vector2 _targetPos)
    {
        StartCoroutine(MoveCoroutine(_targetPos));
    }
    // move coroutine
    private IEnumerator MoveCoroutine(Vector2 _targetPos)
    {
        isMoving = true;
        float duration = 0.2f;

        Vector2 startPosition = transform.position;
        float elaspedTime = 0f;

        while (elaspedTime < duration)
        {
            float t = elaspedTime / duration;

            transform.position = Vector2.Lerp(startPosition, _targetPos, t);

            elaspedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = _targetPos;
        isMoving = false;
    }

}

public enum PotionType
{
    red,
    blue,
    purple,
    green,
    white
}