using UnityEngine;

public class GhostState : MonoBehaviour
{
    public enum State { Normal, Scared, Recovering, Dead }

    public State CurrentState { get; private set; }

    [SerializeField] private Animator animator;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
        SetState(State.Normal);
    }

    public void SetState(State newState)
    {
        CurrentState = newState;

        if (animator == null) return;

        switch (newState)
        {
            case State.Normal:     animator.Play("down");        break;
            case State.Scared:     animator.Play("scareddown");  break;
            case State.Recovering: animator.Play("Recovering");  break;
            case State.Dead:       animator.Play("dead");        break;
        }
    }

    public void ResetToStart()
    {
        transform.position = startPosition;
        SetState(State.Normal);
    }
}