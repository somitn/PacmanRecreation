using UnityEngine;

/// <summary>
/// Moves PacStudent on the grid using WASD input.
/// Uses linear lerping between grid cells (frame-rate independent).
/// No Rigidbody physics — only transform.position is modified.
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
    public AudioClip moveClip;       // moving, not eating
    public AudioClip eatClip;        // about to eat a pellet

    [Header("Dust Effect")]
    public ParticleSystem dustEffect;

    // ---------------------------------------------------------------
    // Movement state
    // ---------------------------------------------------------------
    private Vector3 lerpStart;
    private Vector3 lerpTarget;
    private float lerpProgress;      // 0 to 1
    private bool isLerping;

    private Vector2Int currentGrid;  // (col, row)
    private Direction currentInput;  // the direction we're actually moving
    private Direction lastInput;     // the last key the player pressed

    // Directions
    private enum Direction { None, Up, Down, Left, Right }

    // -----------------------------------------------------------------
    void Start()
    {
        // Determine starting grid cell from our world position
        currentGrid = LevelGrid.Instance.WorldToGrid(transform.position);

        // Snap to the exact center of the grid cell
        transform.position = LevelGrid.Instance.GridToWorld(currentGrid.y, currentGrid.x);

        // Brief: PacStudent starts facing right
        currentInput = Direction.Right;
        lastInput = Direction.Right;
    }

    // -----------------------------------------------------------------
    void Update()
    {
        // 1. Always read input — even while lerping
        Direction pressed = ReadInput();
        if (pressed != Direction.None)
        {
            lastInput = pressed;
        }

        // 2. If not currently lerping, decide what to do next
        if (!isLerping)
        {
            DecideMovement();
        }

        // 3. If lerping, advance the lerp
        if (isLerping)
        {
            AdvanceLerp();
        }
    }

    // -----------------------------------------------------------------
    // Read WASD input into a Direction
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
    // Called when PacStudent is at a grid cell and not lerping.
    // Decides which direction to move (or stop).
    // -----------------------------------------------------------------
    private void DecideMovement()
    {
        // Try lastInput first
        if (lastInput != Direction.None && CanMove(lastInput))
        {
            currentInput = lastInput;
            StartLerp(currentInput);
            return;
        }

        // Fallback: try currentInput
        if (currentInput != Direction.None && CanMove(currentInput))
        {
            StartLerp(currentInput);
            return;
        }

        // Blocked — stop moving
        StopMoving();
    }

    // -----------------------------------------------------------------
    // Can PacStudent move in this direction from currentGrid?
    // -----------------------------------------------------------------
    private bool CanMove(Direction dir)
    {
        Vector2Int target = GetTargetCell(currentGrid, dir);

        // Tunnel wrap: if we're at a tunnel row and stepping off the edge,
        // treat it as walkable (the level has tunnels on the left/right)
        if (LevelGrid.Instance.IsTunnel(target.y, target.x))
            return true;

        return LevelGrid.Instance.IsWalkable(target.y, target.x);
    }

    // -----------------------------------------------------------------
    // Get the grid cell that's one step in the given direction.
    // Note: world UP = grid row -1, world RIGHT = grid col +1, etc.
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
    // Start a lerp from current position to the target grid cell.
    // -----------------------------------------------------------------
    private void StartLerp(Direction dir)
    {
        Vector2Int target = GetTargetCell(currentGrid, dir);

        // Tunnel wrap: if the target is off the edge, wrap to the other side
        if (LevelGrid.Instance.IsTunnel(target.y, target.x))
        {
            if (target.y < 0)      target.y = LevelGrid.Cols - 1;
            else if (target.y >= LevelGrid.Cols) target.y = 0;
        }
        
        if (dustEffect != null && !dustEffect.isPlaying)
        
            dustEffect.Play();
        // Update grid tracking
        currentGrid = target;

        lerpStart = transform.position;
        lerpTarget = LevelGrid.Instance.GridToWorld(target.y, target.x);
        lerpProgress = 0f;
        isLerping = true;

        // Animation
        PlayAnimation(dir);

        // Audio — choose clip based on whether the target has a pellet
        PlayMovementAudio(target);
    }

    // -----------------------------------------------------------------
    // Advance the lerp by moveSpeed (tiles per second) * deltaTime
    // -----------------------------------------------------------------
    private void AdvanceLerp()
    {
        float step = moveSpeed * Time.deltaTime;
        lerpProgress += step;   // progress is in "tiles", not 0-1

        // The distance is 1 tile, so progress goes 0 -> 1
        float t = Mathf.Clamp01(lerpProgress);
        transform.position = Vector3.Lerp(lerpStart, lerpTarget, t);

        if (lerpProgress >= 1f)
        {
            // Snap to target to avoid drift
            transform.position = lerpTarget;
            isLerping = false;
            lerpProgress = 0f;
        }
    }

    // -----------------------------------------------------------------
    // Stop moving — called when blocked in both directions.
    // -----------------------------------------------------------------
    private void StopMoving()
    {
        isLerping = false;
        lerpProgress = 0f;

        if (movementAudio != null && movementAudio.isPlaying)
            movementAudio.Stop();

        if (dustEffect != null)
            dustEffect.Stop();
    }

    // -----------------------------------------------------------------
    // Play the correct walking animation for the direction.
    // Assumes your Animator has parameters or states:
    // WalkLeft, WalkRight, WalkUp, WalkDown
    // -----------------------------------------------------------------
    private void PlayAnimation(Direction dir)
    {
        if (animator == null) return;

        switch (dir)
        {
            case Direction.Up:    animator.Play("up"); break;
            case Direction.Down:  animator.Play("down"); break;
            case Direction.Left:  animator.Play("left"); break;
            case Direction.Right: animator.Play("right"); break;
        }

    }

    // -----------------------------------------------------------------
    // Play movement audio. If the target cell has a pellet, play the
    // eat clip; otherwise the move clip.
    // -----------------------------------------------------------------
    private void PlayMovementAudio(Vector2Int target)
    {
        if (movementAudio == null) return;

        int tile = LevelGrid.Instance.GetTile(target.y, target.x);
        AudioClip clipToPlay = (tile == 5 || tile == 6) ? eatClip : moveClip;

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

}