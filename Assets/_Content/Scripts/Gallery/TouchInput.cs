using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TouchInput : MonoBehaviour
{
    public UnityEvent OnScaleStart;
    public UnityEvent<float, Vector2> OnDeltaScaleChanged;
    public UnityEvent OnScaleEnd;

    public UnityEvent<Vector2> OnDragStart;
    public UnityEvent<Vector2> OnDrag;
    public UnityEvent<Vector2> OnDragEnd;

    public UnityEvent<Vector2> OnDoubleTap;

    public UnityEvent<SwipeDirection> OnSwipe;

    public bool m_IsSwiping { get; private set; }
    protected Vector2 m_StartingTouch;
    private float m_SwipeTimeThreshold = 0.1f;
    private float m_SwipeTimeDelta = 0;
    private float m_SwipeDistanceThreshold = 0.02f;

    private float scaleDelta;
    private Vector2 scaleCenter;
    private float previousTouchDistance = -1;
    private float currentTouchDistance;

    private bool isDraging = false;
    private Vector3 dragStartPos;
    private Vector3 dragDeltaPos;

    private float mouseLastTime = -1;
    private Vector3 mouseLastPosition = -Vector3.one;
    private float mouseDeltaTime = -1;
    private float mouseDeltaPosMagnitude = -1;

    private void Awake()
    {
        m_IsSwiping = false;
    }

    void Update()
    {
        Swipe();
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {

            if (Input.touchCount == 2)
            {
                Touch firstTouch = Input.GetTouch(0);
                Touch secondTouch = Input.GetTouch(1);

                if (firstTouch.phase == TouchPhase.Began || secondTouch.phase == TouchPhase.Began)
                {
                    OnScaleStart?.Invoke();
                    previousTouchDistance = Vector2.Distance(firstTouch.position, secondTouch.position);
                }
                else
                {
                    scaleCenter = (firstTouch.position + secondTouch.position) / 2;
                    currentTouchDistance = Vector2.Distance(firstTouch.position, secondTouch.position);

                    scaleDelta = currentTouchDistance - previousTouchDistance;
                    previousTouchDistance = currentTouchDistance;

                    OnDeltaScaleChanged?.Invoke(scaleDelta, scaleCenter);
                }

                if (firstTouch.phase == TouchPhase.Ended || secondTouch.phase == TouchPhase.Ended ||
                    firstTouch.phase == TouchPhase.Canceled || secondTouch.phase == TouchPhase.Canceled)
                {
                    OnScaleEnd?.Invoke();
                }
            }

            if (Input.touchCount == 1)
            {
                if (Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    isDraging = true;
                    //  dragStartPos = Input.GetTouch(0).position;
                    OnDragStart?.Invoke(Input.GetTouch(0).position);
                }
                if (Input.GetTouch(0).phase == TouchPhase.Ended)
                {
                    isDraging = false;
                    OnDragEnd?.Invoke(Input.GetTouch(0).position);
                }
            }

        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (mouseDeltaTime < 0)
                {
                    mouseDeltaTime = Time.time;
                }
                else
                {
                    mouseDeltaTime = Time.time - mouseLastTime;
                }
                if (mouseDeltaPosMagnitude < 0)
                {
                    mouseDeltaPosMagnitude = Input.mousePosition.magnitude;
                }
                else
                {
                    mouseDeltaPosMagnitude = Mathf.Abs((Input.mousePosition - mouseLastPosition).magnitude);
                }
                mouseLastPosition = Input.mousePosition;
                mouseLastTime = Time.time;
            }

            if (Input.GetMouseButtonDown(0))
            {
                isDraging = true;
                dragStartPos = Input.mousePosition;
                OnDragStart?.Invoke(Input.mousePosition);
            }
            if (Input.GetMouseButtonUp(0))
            {
                isDraging = false;
                OnDragEnd?.Invoke(Input.mousePosition);
            }

            scaleCenter = Input.mousePosition;
            if (Input.mouseScrollDelta.y != 0)
            {
                OnDeltaScaleChanged?.Invoke(Input.mouseScrollDelta.y * 10, Input.mousePosition);
            }
        }
        Drag();
        IsDoubleTap();
    }

    private void Swipe()
    {
        m_SwipeTimeDelta += Time.deltaTime;
        if (Input.touchCount == 1)
        {
            if (m_IsSwiping)
            {
                if (m_SwipeTimeDelta > m_SwipeTimeThreshold)
                {
                    m_IsSwiping = false;
                }

                Vector2 diff = Input.GetTouch(0).position - m_StartingTouch;

                diff = new Vector2(diff.x / Screen.width, diff.y / Screen.width);

                if (diff.magnitude > m_SwipeDistanceThreshold) //we set the swip distance to trigger movement to 1% of the screen width
                {
                    if (Mathf.Abs(diff.y) > Mathf.Abs(diff.x))
                    {
                        if (diff.y < 0)
                        {
                            // StartCoroutine(Slide(2));
                            OnSwipe?.Invoke(SwipeDirection.Down);
                            Debug.Log("slide down");
                        }
                        else
                        {
                            OnSwipe?.Invoke(SwipeDirection.Up);
                            Debug.Log("slide up");
                        }
                    }
                    else
                    {
                        if (diff.x < 0)
                        {
                            OnSwipe?.Invoke(SwipeDirection.Left);
                            Debug.Log("slide left");
                        }
                        else
                        {
                            OnSwipe?.Invoke(SwipeDirection.Right);
                            Debug.Log("slide right");
                        }
                    }
                    m_IsSwiping = false;
                }
            }
            else
            {
             
                // playerFacade.entity.Set(new EntityTags.OnPlayerInput(InputType.Tap));
            }
            // Input check is AFTER the swip test, that way if TouchPhase.Ended happen a single frame after the Began Phase
            // a swipe can still be registered (otherwise, m_IsSwiping will be set to false and the test wouldn't happen for that began-Ended pair)
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                m_StartingTouch = Input.GetTouch(0).position;
                m_SwipeTimeDelta = 0;
                m_IsSwiping = true;
            }
            else if (Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                m_IsSwiping = false;
            }
        }
    }

    private void IsDoubleTap()
    {
        float MaxTimeWait = 0.2f;
        float VariancePosition = 5;

        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                float DeltaTime = Input.GetTouch(0).deltaTime;
                float DeltaPositionLenght = Input.GetTouch(0).deltaPosition.magnitude;

                if (DeltaTime > 0 && DeltaTime < MaxTimeWait && DeltaPositionLenght < VariancePosition)
                    OnDoubleTap?.Invoke(Input.GetTouch(0).position);
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (mouseDeltaTime > 0 && mouseDeltaTime < MaxTimeWait && mouseDeltaPosMagnitude < VariancePosition)
                    OnDoubleTap?.Invoke(Input.mousePosition);
            }
        }
    }

    private void Drag()
    {
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            if (!isDraging || Input.touchCount != 1) return;
            Touch touch = Input.GetTouch(Input.touchCount - 1);
            if (touch.phase == TouchPhase.Began)
            {
                dragDeltaPos = Vector2.zero;
            }
            else
            {
                dragDeltaPos = touch.position - (Vector2)dragStartPos;
            }
            dragStartPos = touch.position;
        }
        else
        {
            if (!isDraging) return;
            dragDeltaPos = Input.mousePosition - dragStartPos;
            dragStartPos = Input.mousePosition;
        }

        OnDrag?.Invoke(dragDeltaPos);
    }

    public enum SwipeDirection
    {
        Left, Right, Up, Down
    }
}
