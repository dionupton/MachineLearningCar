using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NNet))]
public class CarController : MonoBehaviour
{

    private Vector3 startPosition, startRotation;
    private NNet network;

    [Range(-1f, 1f)]
    public float a, t;

    public float timeSinceStart = 0f;

    [Header("Fitness")]
    public float overallFitness;
    public float distanceMultiplier = 1.4f;
    public float avgSpeedMultiplier = 0.2f;
    public float sensorMultiplier = 0.1f;

    [Header("Network Options")]
    public int LAYERS = 1;
    public int NEURONS = 10;

    private Vector3 lastPosition;
    private float totalDistanceTravelled;
    private float avgSpeed;

    private float aSensor, bSensor, cSensor;

    private void Awake()
    {
        startPosition = transform.position;
        startRotation = transform.eulerAngles;
        network = GetComponent<NNet>();

    }


    public void ResetWithNetwork(NNet net)
    {
        network = net;
        Reset();
    }

    public void Reset()
    {


        timeSinceStart = 0f;
        totalDistanceTravelled = 0f;
        avgSpeed = 0f;
        lastPosition = startPosition;
        overallFitness = 0f;
        transform.position = startPosition;
        transform.eulerAngles = startRotation;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.gameObject.tag == "Side")
            Death();
    }


    private void FixedUpdate()
    {
        InputSensors();
        lastPosition = transform.position;


        (a, t) = network.RunNetwork(aSensor, bSensor, cSensor);




        MoveCar(a, t);

        timeSinceStart += Time.deltaTime;

        CalculateFitness();


    }

    private void Death()
    {
        GameObject.FindObjectOfType<GeneticManager>().Death(overallFitness, network);
    }

    private void CalculateFitness()
    {
        totalDistanceTravelled += Vector3.Distance(transform.position, lastPosition);
        avgSpeed = totalDistanceTravelled / timeSinceStart;

        overallFitness = (totalDistanceTravelled * distanceMultiplier) + (avgSpeed * avgSpeedMultiplier) + (((aSensor + bSensor + cSensor) / 3) * sensorMultiplier);

        if(timeSinceStart > 20 && overallFitness < 40)
        {
            Death();
        }

        if(overallFitness >= 1000)
        {
            Death();
        }
    }

    private void InputSensors()
    {
        Vector3 a = (transform.forward + transform.right);
        Vector3 b = (transform.forward);
        Vector3 c = (transform.forward - transform.right);


        Ray r = new Ray(transform.GetChild(6).position, a);
        RaycastHit hit;

        if(Physics.Raycast(r, out hit))
        {
            aSensor = hit.distance / 30;
            if (aSensor < 0.05f)
            {
                Debug.DrawLine(r.origin, hit.point, Color.red);
            }
            else
            {
                Debug.DrawLine(r.origin, hit.point, Color.green);
            }
        }

        r.direction = b;

        if (Physics.Raycast(r, out hit))
        {
            bSensor = hit.distance / 30;
            if (bSensor < 0.05f)
            {
                Debug.DrawLine(r.origin, hit.point, Color.red);
            }
            else
            {
                Debug.DrawLine(r.origin, hit.point, Color.green);
            }
        }

        r.direction = c;

        if (Physics.Raycast(r, out hit))
        {
            cSensor = hit.distance / 30;

            if (cSensor < 0.05f)
            {
                Debug.DrawLine(r.origin, hit.point, Color.red);
            }
            else
            {
                Debug.DrawLine(r.origin, hit.point, Color.green);
            }
        }
    }

    private Vector3 inp;

    public void MoveCar(float v, float h)
    {
        inp = Vector3.Lerp(Vector3.zero, new Vector3(0, 0, v * 11.4f), 0.02f);
        inp = transform.TransformDirection(inp);
        transform.position += inp;

        transform.eulerAngles += new Vector3(0, (h*90)*0.02f,0);
    }
}
