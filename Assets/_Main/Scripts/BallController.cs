using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hitSound;

    private float lastPlayTime = 0f;

    private void OnCollisionEnter(Collision collision)
    {
        float minVelocity = 2f;
        float soundMaxVelocity = 20f;
        if (collision.relativeVelocity.magnitude < minVelocity)
            return;
        if (this.lastPlayTime + 0.1f > Time.time)
            return;
        float velocity = collision.relativeVelocity.magnitude;
        float volume = Mathf.InverseLerp(minVelocity, soundMaxVelocity, velocity);
        this.audioSource.PlayOneShot(this.hitSound, volume);
        this.lastPlayTime = Time.time;
    }

}
