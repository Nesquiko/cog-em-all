using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public UnityEvent OnHoldEvent;

    public event Action OnHold;

    [SerializeField] private float holdInterval = 0.02f;

    private bool isHolding;
    private Coroutine holdRoutine;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isHolding) return;
        isHolding = true;
        holdRoutine = StartCoroutine(HoldLoop());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StopHold();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopHold();
    }

    private void StopHold()
    {
        isHolding = false;
        if (holdRoutine != null)
        {
            StopCoroutine(holdRoutine);
            holdRoutine = null;
        }
    }

    private IEnumerator HoldLoop()
    {
        Fire();

        while (isHolding)
        {
            yield return new WaitForSeconds(holdInterval);
            Fire();
        }
    }

    private void Fire()
    {
        OnHold?.Invoke();
        OnHoldEvent?.Invoke();
    }
}
