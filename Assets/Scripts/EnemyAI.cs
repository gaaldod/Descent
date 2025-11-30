using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("AI Settings")]
    public Transform[] patrolPoints;
    public float patrolWaitTime = 2f;
    public float detectionRange = 10f;
    public float attackRange = 2f;
    public float chaseSpeed = 4f;
    public float patrolSpeed = 2f;
    [Range(10f, 360f)]
    public float viewAngle = 90f;
    public LayerMask visionObstacles = ~0;
    public Transform eyePoint;
    
    [Header("Audio Settings")]
    public AudioClip[] footstepSounds;
    public AudioClip detectionSound;
    public AudioClip attackSound;
    
    private NavMeshAgent agent;
    private Transform player;
    private AudioSource audioSource;
    private Animator animator;
    private int currentPatrolIndex = 0;
    private float lastFootstepTime;
    private float footstepInterval = 0.5f;
    private bool waitingAtPoint;
    private float waitTimer;
    
    public enum AIState
    {
        Patrolling,
        Chasing,
        Attacking,
        Searching
    }
    
    public AIState currentState = AIState.Patrolling;
    
    private bool isInitialized = false;
    
    void Awake()
    {
        // Start disabled - will be enabled when attic door opens
        enabled = false;
    }
    
    void OnEnable()
    {
        // Initialize when enabled (if not already initialized)
        if (!isInitialized)
        {
            InitializeAI();
        }
    }
    
    void Start()
    {
        // Initialize if already enabled at start
        if (enabled && !isInitialized)
        {
            InitializeAI();
        }
    }
    
    private void InitializeAI()
    {
        if (isInitialized) return;
        
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponentInChildren<Animator>();
        if (eyePoint == null)
        {
            eyePoint = transform;
        }
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        if (patrolPoints.Length > 0)
        {
            currentPatrolIndex = 0;
            if (agent != null)
            {
                agent.SetDestination(patrolPoints[0].position);
                waitingAtPoint = true;
                waitTimer = patrolWaitTime;
                agent.isStopped = true;
            }
        }
        
        isInitialized = true;
        Debug.Log("[EnemyAI] AI initialized and enabled.");
    }
    
    void Update()
    {
        UpdateAnimator();

        switch (currentState)
        {
            case AIState.Patrolling:
                Patrol();
                break;
            case AIState.Chasing:
                Chase();
                break;
            case AIState.Attacking:
                Attack();
                break;
            case AIState.Searching:
                Search();
                break;
        }
        
        PlayFootstepSounds();
    }
    
    void Patrol()
    {
        if (patrolPoints.Length == 0) return;
        agent.speed = patrolSpeed;

        if (CanSeePlayer())
        {
            currentState = AIState.Chasing;
            PlaySound(detectionSound);
            return;
        }

        if (waitingAtPoint)
        {
            agent.isStopped = true;
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                waitingAtPoint = false;
                agent.isStopped = false;
                agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            }
            return;
        }

        if (agent.remainingDistance < 0.5f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            float wait = patrolWaitTime;
            if (currentPatrolIndex == 0)
            {
                wait = 20f;
            }
            waitTimer = wait;
            waitingAtPoint = true;
            agent.isStopped = true;
            return;
        }
        
        // if reached destination but not waiting, keep moving (handled above)
    }
    
    void Chase()
    {
        if (player == null) return;
        if (IsPlayerHidden())
        {
            ReturnToPatrol();
            return;
        }

        agent.SetDestination(player.position);
        agent.speed = chaseSpeed;
        
        float distance = Vector3.Distance(transform.position, player.position);
        bool canSee = CanSeePlayer();
        if (canSee && distance < attackRange)
        {
            currentState = AIState.Attacking;
        }
        else if (!canSee && distance > detectionRange * 1.2f)
        {
            currentState = AIState.Searching;
        }
    }
    
    void Attack()
    {
        if (player == null) return;

        agent.ResetPath();
        transform.LookAt(player);
        
        // Play attack sound
        PlaySound(attackSound);
        animator?.SetTrigger("Attack");
        
        // Check if player is hiding
        HidingSpot hidingSpot = FindFirstObjectByType<HidingSpot>();
        if (hidingSpot != null && hidingSpot.IsPlayerHiding())
        {
            ReturnToPatrol();
            return;
        }

        // Player is caught - game over
        Debug.Log("Game Over - You were caught!");
        
        // Show death screen
        DeathScreenUI.Instance?.Show();
    }
    
    void Search()
    {
        if (player == null) return;
        if (IsPlayerHidden())
        {
            ReturnToPatrol();
            return;
        }

        // Move to last known player position
        agent.SetDestination(player.position);
        
        if (agent.remainingDistance < 1f)
        {
            ReturnToPatrol();
        }
    }
    
    void PlayFootstepSounds()
    {
        if (agent.velocity.magnitude > 0.1f && Time.time - lastFootstepTime > footstepInterval)
        {
            if (footstepSounds.Length > 0)
            {
                AudioClip randomFootstep = footstepSounds[Random.Range(0, footstepSounds.Length)];
                PlaySound(randomFootstep);
            }
            lastFootstepTime = Time.time;
        }
    }
    
    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    void UpdateAnimator()
    {
        if (animator == null || agent == null) return;

        animator.SetFloat("Speed", agent.velocity.magnitude);
        animator.SetBool("IsChasing", currentState == AIState.Chasing);
    }

    bool IsPlayerHidden()
    {
        HidingSpot[] spots = FindObjectsByType<HidingSpot>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < spots.Length; i++)
        {
            if (spots[i] != null && spots[i].IsPlayerHiding())
            {
                return true;
            }
        }
        return false;
    }

    void ReturnToPatrol()
    {
        currentState = AIState.Patrolling;
        waitingAtPoint = false;
        agent.isStopped = false;
        agent.speed = patrolSpeed;
        if (patrolPoints.Length > 0)
        {
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
    }
    private bool CanSeePlayer()
    {
        if (player == null || eyePoint == null)
        {
            return false;
        }

        Vector3 toPlayer = player.position - eyePoint.position;
        float distance = toPlayer.magnitude;
        if (distance > detectionRange)
        {
            return false;
        }

        float angle = Vector3.Angle(eyePoint.forward, toPlayer.normalized);
        if (angle > viewAngle * 0.5f)
        {
            return false;
        }

        if (Physics.Raycast(eyePoint.position, toPlayer.normalized, out RaycastHit hit, detectionRange, visionObstacles))
        {
            if (!hit.collider.CompareTag("Player"))
            {
                return false;
            }
        }

        HidingSpot hidingSpot = FindFirstObjectByType<HidingSpot>();
        if (hidingSpot != null && hidingSpot.IsPlayerHiding())
        {
            return false;
        }

        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Transform eye = eyePoint != null ? eyePoint : transform;
        if (eye == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(eye.position, detectionRange);

        Vector3 leftDir = Quaternion.Euler(0f, -viewAngle * 0.5f, 0f) * transform.forward;
        Vector3 rightDir = Quaternion.Euler(0f, viewAngle * 0.5f, 0f) * transform.forward;
        Gizmos.DrawLine(eye.position, eye.position + leftDir.normalized * detectionRange);
        Gizmos.DrawLine(eye.position, eye.position + rightDir.normalized * detectionRange);

        if (player != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(eye.position, player.position);
        }
    }
}
