using UnityEngine;

public class CherryController : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject cherryPrefab;

    [Header("Timing")]
    public float spawnDelay = 5f;

    [Header("Movement")]
    [Tooltip("Tiles per second along the cherry's path.")]
    public float cherrySpeed = 3f;

    [Tooltip("How far outside the level to start/end (in tiles).")]
    public float outsideDistance = 30f;

    // Current cherry state
    private GameObject currentCherry;
    private Vector3 cherryStart;
    private Vector3 cherryEnd;
    private float cherryElapsed;
    private float cherryDuration;
    private bool cherryActive;

    void Start()
    {
        cherryActive = false;
        Invoke(nameof(SpawnCherry), spawnDelay);
    }

    void Update()
    {
        if (!cherryActive || currentCherry == null) return;

        // Advance time and compute the lerp fraction
        cherryElapsed += Time.deltaTime;
        float t = Mathf.Clamp01(cherryElapsed / cherryDuration);

        // Linear lerp from start to end
        currentCherry.transform.position = Vector3.Lerp(cherryStart, cherryEnd, t);

        // Reached the end — destroy and schedule the next cherry
        if (t >= 1f)
        {
            DestroyCherry();
        }
    }

    // -----------------------------------------------------------------
    // Spawn a cherry at a random angle around the level center,
    // pointing away from the center so the path passes through it.
    // -----------------------------------------------------------------
    private void SpawnCherry()
    {
        if (cherryPrefab == null) return;

        // Level center in world space
        Vector3 center = LevelGrid.Instance.GridToWorld(LevelGrid.Rows / 2, LevelGrid.Cols / 2);

        // Random angle → random direction
        float angleDeg = Random.Range(0f, 360f);
        float angleRad = angleDeg * Mathf.Deg2Rad;
        Vector3 dir = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0f);

        // Start far out in the direction, end far out in the opposite direction
        cherryStart = center + dir * outsideDistance;
        cherryEnd   = center - dir * outsideDistance;

        // Duration = distance / speed (in tiles)
        float totalDistance = Vector3.Distance(cherryStart, cherryEnd);
        cherryDuration = totalDistance / cherrySpeed;
        cherryElapsed = 0f;

        // Instantiate
        currentCherry = Instantiate(cherryPrefab, cherryStart, Quaternion.identity);
        currentCherry.name = "Cherry";
        cherryActive = true;
    }

    // -----------------------------------------------------------------
    // Destroy the cherry and schedule the next spawn.
    // Public so PacStudentController can call it on collision.
    // -----------------------------------------------------------------
    public void DestroyCherry()
    {
        if (currentCherry != null)
            Destroy(currentCherry);

        cherryActive = false;
        currentCherry = null;
        Invoke(nameof(SpawnCherry), spawnDelay);
    }
}