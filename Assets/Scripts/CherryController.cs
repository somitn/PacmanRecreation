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

    [Header("Level Center")]
    [Tooltip("World position of the level's center. (7.5, -10, 0) for topLeftWorld (-6, 4).")]
    public Vector3 levelCenter = new Vector3(7.5f, -10f, 0f);

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
        // GameManager calls StartSpawning() after "GO!"
    }

    public void StartSpawning()
    {
        Invoke(nameof(SpawnCherry), spawnDelay);
    }

    void Update()
    {
        if (!cherryActive || currentCherry == null) return;

        cherryElapsed += Time.deltaTime;
        float t = Mathf.Clamp01(cherryElapsed / cherryDuration);

        currentCherry.transform.position = Vector3.Lerp(cherryStart, cherryEnd, t);

        if (t >= 1f)
        {
            DestroyCherry();
        }
    }

    private void SpawnCherry()
    {
        if (cherryPrefab == null) return;

        Vector3 center = levelCenter;

        float angleDeg = Random.Range(0f, 360f);
        float angleRad = angleDeg * Mathf.Deg2Rad;
        Vector3 dir = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0f);

        cherryStart = center + dir * outsideDistance;
        cherryEnd   = center - dir * outsideDistance;

        float totalDistance = Vector3.Distance(cherryStart, cherryEnd);
        cherryDuration = totalDistance / cherrySpeed;
        cherryElapsed = 0f;

        currentCherry = Instantiate(cherryPrefab, cherryStart, Quaternion.identity);
        currentCherry.name = "Cherry";
        cherryActive = true;
    }

    public void DestroyCherry()
    {
        if (currentCherry != null)
            Destroy(currentCherry);

        cherryActive = false;
        currentCherry = null;
        Invoke(nameof(SpawnCherry), spawnDelay);
    }
}