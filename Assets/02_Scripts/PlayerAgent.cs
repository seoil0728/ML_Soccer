using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;

[RequireComponent(typeof(DecisionRequester))]
public class PlayerAgent : Agent
{
    public enum Team
    {
        BLUE = 0, RED
    }

    public Team team = Team.BLUE;

    // 플레이어의 초기 위치
    public Vector3 initPosBlue = new Vector3(-5.5f, 0.5f, 0.0f);
    public Vector3 initPosRed = new Vector3(5.5f, 0.5f, 0.0f);

    // 플레이어의 초기 각도
    public Quaternion initRotBlue = Quaternion.Euler(Vector3.up * 90);
    public Quaternion initRotRed = Quaternion.Euler(Vector3.up * -90);

    // 플레이어 색상
    public Material[] materials;

    // Behaviour Parameters 컴포넌트
    [SerializeField] private BehaviorParameters bps;
    [SerializeField] private Rigidbody rb;

    // 플레이어 위치와 회전을 최기화
    public void InitPlayer()
    {
        transform.localPosition = (team == Team.BLUE) ? initPosBlue : initPosRed;
        transform.localRotation = (team == Team.BLUE) ? initRotBlue : initRotRed;
    }

    public override void Initialize()
    {
        // 팀 설정
        bps = GetComponent<BehaviorParameters>();
        bps.TeamId = (int)team;
        // 물리 설정
        rb = GetComponent<Rigidbody>();
        rb.mass = 10.0f;
        rb.constraints = RigidbodyConstraints.FreezePositionY
                        | RigidbodyConstraints.FreezeRotationX
                        | RigidbodyConstraints.FreezeRotationY
                        | RigidbodyConstraints.FreezeRotationZ;

        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // 플레이어 색상 설정
        GetComponent<Renderer>().material = materials[(int)team];
        // 플레이어의 위치 및 각도 설정
        InitPlayer();
        // 최대 스텝수 설정
        MaxStep = 10000;
    }

    public override void OnEpisodeBegin()
    {
        InitPlayer();
        rb.velocity = rb.angularVelocity = Vector3.zero;
    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // 이산 수치 처리
        var actions = actionsOut.DiscreteActions;
        actions.Clear();

        /*
            Branch[0] = 정지, 전진, 후진 (W, S)
            Branch[1] = 정지, 왼쪽 이동, 오른쪽 이동 (Q, E)
            Branch[2] = 정지, 왼쪽 회전, 오른쪽 회전 (A, D)

            => 각 Branch 3개, 브랜치마다 3개의 값 == Actions Size를 지정
        */

        // Branch[0]
        if (Input.GetKey(KeyCode.W)) actions[0] = 1;
        if (Input.GetKey(KeyCode.S)) actions[0] = 2;
        
        // Branch[1]
        if (Input.GetKey(KeyCode.Q)) actions[1] = 1;
        if (Input.GetKey(KeyCode.E)) actions[1] = 2;
        
        // Branch[2]
        if (Input.GetKey(KeyCode.A)) actions[2] = 1;
        if (Input.GetKey(KeyCode.D)) actions[2] = 2;


    }


    public override void OnActionReceived(ActionBuffers actions)
    {
        var action = actions.DiscreteActions;
        // Debug.Log($"[0] = {action[0]}, [1] = {action[1]}, [2] = {action[2]}");

        Vector3 dir = Vector3.zero;
        Vector3 rot = Vector3.zero;
        int forward = action[0];
        int right = action[1];
        int rotate = action[2];

        switch(forward)
        {
            case 1 : dir = transform.forward; break;
            case 2 : dir = -transform.forward; break;
        }

        switch(right)
        {
            case 1 : dir = -transform.right; break;
            case 2 : dir = transform.right; break;
        }

        switch(rotate)
        {
            case 1 : rot = -transform.up; break;
            case 2 : rot = transform.up; break;
        }

        transform.Rotate(rot, Time.fixedDeltaTime * 100f);
        rb.AddForce(dir * 1.2f, ForceMode.VelocityChange);
    }


    private void OnCollisionEnter(Collision other) 
    {
        if (other.collider.CompareTag("ball"))
        {
            // Ball 터치 시 + 리워드
            AddReward(0.2f);

            // Kick 방향 계산
            Vector3 kickDir = other.GetContact(0).point - transform.position;
            other.gameObject.GetComponent<Rigidbody>().AddForce(kickDir.normalized * 1500f);

        }
    }

    
}
