using UnityEngine;

/// <summary>
/// Moves PacStudent on the grid using WASD input.
/// Uses linear lerping between grid cells (frame-rate independent).
/// No Rigidbody physics for movement — only transform.position is modified.
/// Rigidbody2D is set to Kinematic purely for collision detection.
/// </summary>
public class PacStudentController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Tiles per second. Brief says ~5 for a fast feel.")]
    public float moveSpeed = 5f;

    [Header("References")]
    public Animator animator;
    public AudioSource movementAudio;

    [Header("Audio Clips")]
    public AudioClip moveClip;
    public AudioClip eatClip;

    [Header("One-Shot SFX")]
    public AudioSource effectsAudio;
    public AudioClip cherryClip;
    public AudioClip wallClip;
    public AudioClip deathClip;
    public AudioClip eatGhostClip;

    [Header("Other References")]
    public CherryController cherryController;
    public ParticleSystem dustEffect;
    public ParticleSystem wallBumpEffect;

    [Header("Tunnel")]
    [SerializeField] private int tunnelRow = 14;

    // ---------------------------------------------------------------
    // Movement state
    // ---------------------------------------------------------------
    private Vector3 lerpStart;
    private Vector3 lerpTarget;
    private float lerpProgress;
    private bool isLerping;

    private Vector2Int currentGrid;
    private Direction currentInput;
    private Direction lastInput;
    private bool hitWall;
    private Vector3 spawnPosition;

    private enum Direction { None, Up, Down, Left, Right }

    // -----------------------------------------------------------------
    void Start()
    {
        currentGrid = LevelGrid.Instance.WorldToGrid(transform.position);
        transform.position = LevelGrid.Instance.GridToWorld(currentGrid.y, currentGrid.x);

        spawnPosition = transform.position;

        currentInput = Direction.None;
        lastInput = Direction.None;
    }

    // -----------------------------------------------------------------
    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameManager.GameState.Playing)
            return;

        Direction pressed = ReadInput();
        if (pressed != Direction.None)
            lastInput = pressed;

        if (!isLerping)
            DecideMovement();

        if (isLerping)
            AdvanceLerp();
    }

    // -----------------------------------------------------------------
    private Direction ReadInput()
    {
        if (Input.GetKeyDown(KeyCode.W)) return Direction.Up;
        if (Input.GetKeyDown(KeyCode.S)) return Direction.Down;
        if (Input.GetKeyDown(KeyCode.A)) return Direction.Left;
        if (Input.GetKeyDown(KeyCode.D)) return Direction.Right;
        return Direction.None;
    }

    // -----------------------------------------------------------------
    private void DecideMovement()
    {
        if (lastInput != Direction.None)
        {
            if (IsTunnelExit(lastInput))
            {
                currentInput = lastInput;
                Teleport();
                StartLerp(currentInput);
                return;
            }

            if (CanMove(lastInput))
            {
                currentInput = lastInput;
                StartLerp(currentInput);
                return;
            }
        }

        if (currentInput != Direction.None)
        {
            if (IsTunnelExit(currentInput))
            {
                Teleport();
                StartLerp(currentInput);
                return;
            }

            if (CanMove(currentInput))
            {
                StartLerp(currentInput);
                return;
            }
        }

        StopMoving();

        if (!hitWall && currentInput != Direction.None)
        {
            hitWall = true;
            PlaySfx(wallClip);

            if (wallBumpEffect != null)
            {
                wallBumpEffect.transform.position =
                    transform.position + DirectionToVector(currentInput) * 0.5f;
                wallBumpEffect.Play();
            }
        }
    }

    // -----------------------------------------------------------------
    private bool CanMove(Direction dir)
    {
        Vector2Int target = GetTargetCell(currentGrid, dir);
        return LevelGrid.Instance.IsWalkable(target.y, target.x);
    }

    // -----------------------------------------------------------------
    private bool IsTunnelExit(Direction dir)
    {
        bool onLeft  = (currentGrid.y == tunnelRow && currentGrid.x == 0
                        && dir == Direction.Left);
        bool onRight = (currentGrid.y == tunnelRow && currentGrid.x == LevelGrid.Cols - 1
                        && dir == Direction.Right);
        return onLeft || onRight;
    }

    // -----------------------------------------------------------------
    private void Teleport()
    {
        int newCol = (currentGrid.x == 0) ? LevelGrid.Cols - 1 : 0;
        currentGrid = new Vector2Int(newCol, tunnelRow);
        transform.position = LevelGrid.Instance.GridToWorld(tunnelRow, newCol);
    }

    // -----------------------------------------------------------------
    private Vector2Int GetTargetCell(Vector2Int from, Direction dir)
    {
        switch (dir)
        {
            case Direction.Up:    return new Vector2Int(from.x, from.y - 1);
            case Direction.Down:  return new Vector2Int(from.x, from.y + 1);
            case Direction.Left:  return new Vector2Int(from.x - 1, from.y);
            case Direction.Right: return new Vector2Int(from.x + 1, from.y);
        }
        return from;
    }

    // -----------------------------------------------------------------
    private void StartLerp(Direction dir)
    {
        Vector2Int target = GetTargetCell(currentGrid, dir);

        if (dustEffect != null && !dustEffect.isPlaying)
            dustEffect.Play();

        currentGrid = target;

        lerpStart = transform.position;
        lerpTarget = LevelGrid.Instance.GridToWorld(target.y, target.x);
        lerpProgress = 0f;
        isLerping = true;
        hitWall = false;

        if (animator != null) animator.speed = 1f;

        PlayAnimation(dir);
        PlayMovementAudio(target);
    }

    // -----------------------------------------------------------------
    private void AdvanceLerp()
    {
        float step = moveSpeed * Time.deltaTime;
        lerpProgress += step;

        float t = Mathf.Clamp01(lerpProgress);
        transform.position = Vector3.Lerp(lerpStart, lerpTarget, t);

        if (lerpProgress >= 1f)
        {
            transform.position = lerpTarget;
            isLerping = false;
            lerpProgress = 0f;
        }
    }

    // -----------------------------------------------------------------
    private void StopMoving()
    {
        isLerping = false;
        lerpProgress = 0f;

        if (movementAudio != null && movementAudio.isPlaying)
            movementAudio.Stop();

        if (dustEffect != null)
            dustEffect.Stop();

        if (animator != null) animator.speed = 0f;
    }

    // -----------------------------------------------------------------
    private void PlayAnimation(Direction dir)
    {
        if (animator == null) return;

        switch (dir)
        {
            case Direction.Up:    animator.Play("up");    break;
            case Direction.Down:  animator.Play("down");  break;
            case Direction.Left:  animator.Play("left");  break;
            case Direction.Right: animator.Play("right"); break;
        }
    }

    // -----------------------------------------------------------------
    private void PlayMovementAudio(Vector2Int target)
    {
        if (movementAudio == null) return;

        Vector3 targetWorld = LevelGrid.Instance.GridToWorld(target.y, target.x);
        Collider2D hit = Physics2D.OverlapPoint(targetWorld);

        bool pelletAhead = false;
        if (hit != null)
            pelletAhead = hit.CompareTag("Pellet") || hit.CompareTag("PowerPellet");

        AudioClip clipToPlay = pelletAhead ? eatClip : moveClip;

        if (clipToPlay != null && movementAudio.clip != clipToPlay)
        {
            movementAudio.clip = clipToPlay;
            movementAudio.loop = true;
            movementAudio.Play();
        }
        else if (clipToPlay != null && !movementAudio.isPlaying)
        {
            movementAudio.Play();
        }
    }

    // -----------------------------------------------------------------
    private void PlaySfx(AudioClip clip)
    {
        if (effectsAudio != null && clip != null)
            effectsAudio.PlayOneShot(clip);
    }

    // -----------------------------------------------------------------
    private Vector3 DirectionToVector(Direction dir)
    {
        switch (dir)
        {
            case Direction.Up:    return Vector3.up;
            case Direction.Down:  return Vector3.down;
            case Direction.Left:  return Vector3.left;
            case Direction.Right: return Vector3.right;
        }
        return Vector3.zero;
    }

    // -----------------------------------------------------------------
    // Death + Respawn
    // -----------------------------------------------------------------
    public void PlayDeath()
    {
        isLerping = false;
        lerpProgress = 0f;

        if (movementAudio != null) movementAudio.Stop(); movementAudio.clip = null;
        if (dustEffect != null) dustEffect.Stop();

        if (animator != null)
        {
            animator.speed = 1f;
            animator.Play("dead");
        }
    }

    public void Respawn()
    {
        transform.position = spawnPosition;
        currentGrid = LevelGrid.Instance.WorldToGrid(spawnPosition);

        isLerping = false;
        lerpProgress = 0f;
        lastInput = Direction.None;
        currentInput = Direction.None;
        hitWall = false;

        if (animator != null)
        {
            animator.Play("right");
            animator.speed = 0f;
        }
    }

    // -----------------------------------------------------------------
    // Collision handling
    // -----------------------------------------------------------------
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameManager.GameState.Playing)
            return;

        if (other.CompareTag("Pellet"))
        {
            Destroy(other.gameObject);
            GameManager.Instance.AddScore(10);
            GameManager.Instance.PelletEaten();
            return;
        }

        if (other.CompareTag("Cherry"))
        {
            GameManager.Instance.AddScore(100);
            PlaySfx(cherryClip);
            if (cherryController != null)
                cherryController.DestroyCherry();
            return;
        }

        if (other.CompareTag("PowerPellet"))
        {
            Destroy(other.gameObject);
            GameManager.Instance.PelletEaten();
            GameManager.Instance.PowerPelletEaten();
            return;
        }

        if (other.CompareTag("Ghost"))
        {
            GhostState ghost = other.GetComponent<GhostState>();
            if (ghost == null) return;

            if (ghost.CurrentState == GhostState.State.Normal)
            {
                GameManager.Instance.LoseLife();
                return;
            }

            if (ghost.CurrentState == GhostState.State.Scared ||
                ghost.CurrentState == GhostState.State.Recovering)
            {
                PlaySfx(eatGhostClip);
                GameManager.Instance.EatGhost(ghost);
                return;
            }
        }
    }
}