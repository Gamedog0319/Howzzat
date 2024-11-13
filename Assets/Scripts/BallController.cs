using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    Rigidbody rb;
    public float minSpeed = 15f;
    public float maxSpeed = 30f;

    public float minHorizontalOffset = 5f;
    public float maxHorizontalOffset = 2f;

    public float minVerticalForce = 5f;
    public float maxVerticalForce = 10f;

    public float verticalOffset = -3f;
    public float verticalThreshold = 5f;

    private int ballTouchCount = 0;
    private int batTouchCount = 0;

    public float wicketDistanceThreshold = 500f;
    public float destructionTime = 3f;
    public float timeAfterBatHit = 5f;

    private Vector3 initialPosition;
    private bool isResolved = false;
    private bool isBoundary = false;
    private bool batHit = false;
    private float timeSinceBatHit = 0f;

    public Transform wicketTransform;

    private float timeSinceThrow = 0f;
    public float ballLifetime = 4f;
    private float wideCheckThresholdZ = -6f;
    private float noBallCheckThresholdY = -5f;

    private bool isValidBall = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        initialPosition = transform.position;

        if (wicketTransform == null)
        {
            GameObject wicket = GameObject.FindWithTag("HitWicket");
            if (wicket != null)
            {
                wicketTransform = wicket.transform;
            }
            else
            {
                Debug.LogError("HitWicket GameObject not found!");
            }
        }

        ThrowBall();
    }

    private void ThrowBall()
    {
        float randomSpeed = Random.Range(minSpeed, maxSpeed);
        float randomHorizontalOffset = Random.Range(minHorizontalOffset, maxHorizontalOffset);
        float randomVerticalForce = Random.Range(minVerticalForce, maxVerticalForce);

        Vector3 ballDirection = new Vector3(randomHorizontalOffset, randomVerticalForce + verticalOffset, -randomSpeed);
        rb.AddForce(ballDirection, ForceMode.Impulse);
    }

    private void DestroyBall()
    {
        if (!isResolved)
        {
            isResolved = true;
            if (GameManager.instance != null)
            {
                GameManager.instance.OnBallResolved(isValidBall);
            }
            Destroy(this.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Bat"))
        {
            batTouchCount = 1;
            batHit = true;
            timeSinceBatHit = 0f;

            float ballForce = Random.Range(50f, 60f);
            float ballHeight = Random.Range(5f, 15f);
            float ballPosition = Random.Range(-5f, 5f);

            Vector3 ballDirection = new Vector3(-ballForce, ballHeight + verticalOffset, ballPosition);
            rb.AddForce(ballDirection, ForceMode.Impulse);
        }

        if (other.gameObject.CompareTag("Boundary"))
        {
            isBoundary = true;

            if (batTouchCount == 1)
            {
                if (ballTouchCount <= 1)
                {
                    GameManager.instance.UpdateRuns(6);
                    Debug.Log("It's a six");
                }
                else
                {
                    GameManager.instance.UpdateRuns(4);
                    Debug.Log("It's a four");
                }
            }
            DestroyBall();
        }

        if (other.gameObject.CompareTag("HitWicket"))
        {
            GameManager.instance.UpdateRuns(0);
            Debug.Log("HitWicket");
            GameManager.instance.IncrementWicketCount();
            DestroyBall();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bat"))
        {
            batTouchCount = 1;
            batHit = true;
            timeSinceBatHit = 0f;
        }

        if (collision.gameObject.CompareTag("Ground"))
        {
            ballTouchCount++;

            if (!batHit && ballTouchCount > 0 && timeSinceThrow >= ballLifetime)
            {
                Debug.Log("Ball destroyed after touching ground and not hitting anything else");
                GameManager.instance.UpdateRuns(0);
                DestroyBall();
            }
        }

        if (collision.gameObject.CompareTag("Fielder"))
        {
            if (batTouchCount == 1)
            {
                CalculateAndAllotRuns();
                DestroyBall();
            }
        }

        // Handle collision with bowler
        if (collision.gameObject.CompareTag("Bowler"))
        {
            isValidBall = true; // Mark the ball as valid
            GameManager.instance.OnBallResolved(true); // Increment ball count only
            Debug.Log("Ball hit the bowler, no runs awarded");
            DestroyBall(); // Destroy the ball
        }
    }

    void Update()
    {
        timeSinceThrow += Time.deltaTime;

        if (batHit)
        {
            timeSinceBatHit += Time.deltaTime;

            if (timeSinceBatHit >= timeAfterBatHit && !isBoundary && !isResolved)
            {
                CalculateAndAllotRuns();
                DestroyBall();
            }
        }

        if (!batHit && timeSinceThrow >= ballLifetime && ballTouchCount == 0)
        {
            Debug.Log("Ball destroyed after 4 seconds with no bat hit and no wicket");
            GameManager.instance.UpdateRuns(0);
            DestroyBall();
        }

        if (transform.position.y > verticalThreshold && isBoundary)
        {
            if (ballTouchCount <= 1)
            {
                GameManager.instance.UpdateRuns(6);
                Debug.Log("Automatic Six due to height and boundary");
            }
            else
            {
                GameManager.instance.UpdateRuns(4);
                Debug.Log("Automatic Four due to height and boundary");
            }
            DestroyBall();
        }

        float distanceFromWicket = Vector3.Distance(transform.position, wicketTransform.position);
        bool isBehindWicket = transform.position.z < wicketTransform.position.z;

        if (!batHit && isBehindWicket && distanceFromWicket <= wicketDistanceThreshold)
        {
            GameManager.instance.UpdateRuns(0);
            Debug.Log("Ball destroyed after crossing behind the wicket without a bat hit");
            DestroyBall();
        }

        // Wide ball condition checked only after crossing z = -6
        if (!batHit && transform.position.z < wideCheckThresholdZ && transform.position.x > wicketTransform.position.x)
        {
            OnWideBall(); // Handle wide ball, but do not increment ball count
            DestroyBall();
        }

        // No Ball condition checked only after crossing y = -5
        if (!batHit && transform.position.y < noBallCheckThresholdY && transform.position.y > wicketTransform.position.y)
        {
            GameManager.instance.OnNoBall();
            DestroyBall();
        }
    }

    private void CalculateAndAllotRuns()
    {
        if (ballTouchCount == 0)
        {
            GameManager.instance.UpdateRuns(3);
            Debug.Log("Three runs due to no fielder touch");
        }
        else if (ballTouchCount == 1)
        {
            GameManager.instance.UpdateRuns(2);
            Debug.Log("Two runs due to one fielder touch");
        }
        else if (ballTouchCount == 2)
        {
            GameManager.instance.UpdateRuns(1);
            Debug.Log("One run due to two fielder touches");
        }
        else
        {
            GameManager.instance.UpdateRuns(0);
            Debug.Log("No runs due to more than two fielder touches");
        }
    }

    private void OnWideBall()
    {
        isValidBall = false; // Mark the ball as invalid
        GameManager.instance.UpdateRuns(1); // Add 1 run for the wide ball
        Debug.Log("Wide Ball!");
        StartCoroutine(GameManager.instance.ShowWideMessage());
        DestroyBall(); // Ensures the ball is destroyed but does not increment ball count
    }
}
