
using UnityEngine;
using DG.Tweening;

public class MovingBouncer : MonoBehaviour
{
    public Vector3 groundPosition;
    public Vector3 bouncePosition;
    public Vector3 squishPosition;

    public Vector3 stretchAmount;
    public Vector3 normAmount;
    public Vector3 sqishAmount;

    public float stageTime = 1f;

    private float m_timer;
    public int stage = 0;
    public DogMovement dM;
    private bool m_isBouncing = false;

    private Tween m_moveTween;
    private Tween m_rotateTween;
    private Tween m_scaleTween;

    void Start()
    {
        transform.localPosition = groundPosition;
        transform.localScale = normAmount;
    }

    void Update()
    {
        stageTime = Mathf.Max(0.05f, 0.5f - (Mathf.Abs(dM.currentSpeed) * 0.0075f));

        if (m_isBouncing)
        {
            m_timer += Time.deltaTime;

            if (m_timer >= stageTime)
            {
                stage++;
                m_timer = 0f;
                StartStage();
            }

            if ((stage % 4 == 1 || stage % 4 == 3) && (dM.currentSpeed <= 0.1f && dM.currentSpeed >= -0.1f))
            {
                m_isBouncing = false;
                stage = 0;
                m_timer = 0f;
                ResetBouncer();
            }
        }
        else if (dM.currentSpeed > 0.1f || dM.currentSpeed < -0.1f)
        {
            m_isBouncing = true;
            stage = 0;
            m_timer = 0f;
            StartStage();
        }
    }

    void StartStage()
    {
        m_moveTween?.Kill();
        m_rotateTween?.Kill();
        m_scaleTween?.Kill();

        if (stage % 4 == 0)
        {
            m_moveTween = transform.DOLocalMove(bouncePosition, stageTime);
            m_rotateTween = transform.DOLocalRotate(new Vector3(0f, 0f, 15f), stageTime);
            m_scaleTween = transform.DOScale(stretchAmount, stageTime);
        }
        else if (stage % 4 == 1)
        {
            m_moveTween = transform.DOLocalMove(squishPosition, stageTime);
            m_rotateTween = transform.DOLocalRotate(Vector3.zero, stageTime);
            m_scaleTween = transform.DOScale(sqishAmount, stageTime);
        }
        else if (stage % 4 == 2)
        {
            m_moveTween = transform.DOLocalMove(bouncePosition, stageTime);
            m_rotateTween = transform.DOLocalRotate(new Vector3(0f, 0f, -15f), stageTime);
            m_scaleTween = transform.DOScale(stretchAmount, stageTime);
        }
        else
        {
            m_moveTween = transform.DOLocalMove(squishPosition, stageTime);
            m_rotateTween = transform.DOLocalRotate(Vector3.zero, stageTime);
            m_scaleTween = transform.DOScale(sqishAmount, stageTime);
        }
    }

    void ResetBouncer()
    {
        m_moveTween?.Kill();
        m_rotateTween?.Kill();
        m_scaleTween?.Kill();

        m_moveTween = transform.DOLocalMove(groundPosition, 0.2f);
        m_rotateTween = transform.DOLocalRotate(Vector3.zero, 0.2f);
        m_scaleTween = transform.DOScale(normAmount, 0.2f);
    }

    void OnDestroy()
    {
        m_moveTween?.Kill();
        m_rotateTween?.Kill();
        m_scaleTween?.Kill();
    }
}
