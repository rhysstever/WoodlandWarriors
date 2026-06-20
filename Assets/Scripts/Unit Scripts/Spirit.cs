using UnityEngine;
using UnityEngine.Splines;

public class Spirit : MonoBehaviour
{
    [SerializeField]
    private SplineContainer spline;
    [SerializeField]
    private float speed;

    private float splineLength, currentPosAlongSpline;
    Vector2 basePos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        basePos = transform.position;
        splineLength = spline.Spline.GetLength();
    }

    // Update is called once per frame
    void Update()
    {
        if(CanMove())
        {
            BobMovement();
        }
    }

    private bool CanMove()
    {
        return GameManager.instance.CurrentMenuState == MenuState.Game && GameManager.instance.CurrentGameState == GameState.Combat;
    }

    private void BobMovement()
    {
        currentPosAlongSpline = Mathf.Clamp(currentPosAlongSpline + speed * Time.deltaTime, 0f, splineLength);

        if(currentPosAlongSpline == splineLength)
        {
            currentPosAlongSpline = 0f;
        }

        float normalizedPos = currentPosAlongSpline / splineLength;
        spline.Spline.Evaluate(normalizedPos, out var pos, out _, out _);
        transform.position = basePos + new Vector2(pos.x, pos.y);
    }
}
