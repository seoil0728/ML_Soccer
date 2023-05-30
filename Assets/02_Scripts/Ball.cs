using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using TMPro;

public class Ball : MonoBehaviour
{
    public Agent[] players;

    private Rigidbody rb;
    public int blueScore, redScore;

    public TMP_Text blueText, redText;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision other) {
        // red 골대에 들어감
        if (other.collider.CompareTag("RED_GOAL"))
        {
            players[0].AddReward(1);
            players[1].AddReward(-1);
            InitBall();
            players[0].EndEpisode();
            players[1].EndEpisode();

            
            blueText.text = (++blueScore).ToString("000");
        }

        if (other.collider.CompareTag("BLUE_GOAL"))
        {
            players[0].AddReward(-1);
            players[1].AddReward(1);
            InitBall();
            players[0].EndEpisode();
            players[1].EndEpisode();

            redText.text = (++redScore).ToString("000");
        }
    }

    void InitBall()
    {
        rb.velocity = rb.angularVelocity = Vector3.zero;
        transform.localPosition = new Vector3(0, 1, 0);
    }
}
