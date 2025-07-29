using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Integrations.Match3;
using UnityEngine.UIElements;
using System;
using Random = UnityEngine.Random;
using static Unity.MLAgents.Sensors.RayPerceptionOutput;
using Google.Protobuf.WellKnownTypes;
public class AgentController : Agent
{
    [SerializeField]
    private Transform target;
    [SerializeField]
    private float movement_speed = 2f;

    private float previous_distance;
    private float current_distance;
    private float k = 10f;

    private Rigidbody rb;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        previous_distance = Vector3.Distance(transform.position, target.position);
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(10.5f, 0.3f, 10.5f);
        target.localPosition = new Vector3((Random.Range(0, 2) == 0 ? 12f : -12f), 0.3f, (Random.Range(0, 2) == 0 ? 12f : -12f));
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        // sensor.AddObservation(target.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveRotate = actions.ContinuousActions[0];
        float moveForward = actions.ContinuousActions[1];

        rb.MovePosition(transform.position + transform.forward * moveForward * movement_speed * Time.deltaTime);
        transform.Rotate(0f, moveRotate * movement_speed, 0f, Space.Self);


        /*  Vector3 velocity = new Vector3(moveX, 0f, moveZ);
          velocity = velocity.normalized * Time.deltaTime * movement_speed;

          transform.localPosition += velocity; */
    }
    public void Update()
    {
        current_distance = Vector3.Distance(transform.position, target.position);

        float timePenalty = 0.01f;

        float distanceDelta = previous_distance - current_distance;
        float reward = (k * distanceDelta) - timePenalty;

        AddReward(reward);

        Debug.Log("Reward" + reward);

        previous_distance = current_distance;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> contiounousActions = actionsOut.ContinuousActions;
        contiounousActions[0] = Input.GetAxisRaw("Horizontal");
        contiounousActions[1] = Input.GetAxisRaw("Vertical");
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Target")
        {
            AddReward(100f);
            EndEpisode();
        }
        if (other.gameObject.tag == "Wall")
        {
            AddReward(-50f);
            EndEpisode();
        }
    }
}