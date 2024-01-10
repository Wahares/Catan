using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class Recorder : MonoBehaviour
{
    GameObjectRecorder rec;
    public AnimationClip clip;

    public Rigidbody rb;

    [SerializeField]
    private Transform pivot;
    public float force,angForce;

    [ContextMenu("roll")]
    private void roll()
    {
        rb.transform.position = pivot.position;
        rb.transform.rotation = pivot.rotation;
        rb.AddForce(Vector3.forward * force, ForceMode.Impulse);
        rb.angularVelocity = Random.rotation.eulerAngles*angForce;
    }


    void LateUpdate()
    {
        if (clip == null)
            return;

        // Take a snapshot and record all the bindings values for this frame.
        rec.TakeSnapshot(Time.deltaTime);
    }
    private void OnEnable()
    {
        // Create recorder and record the script GameObject.
        rec = new GameObjectRecorder(rb.gameObject);

        // Bind all the Transforms on the GameObject and all its children.
        rec.BindComponentsOfType<Transform>(rb.gameObject, true);
        if(rec.isRecording)
        rec.ResetRecording();
    }
    void OnDisable()
    {
        if (clip == null)
            return;

        if (rec.isRecording)
        {
            // Save the recorded session to the clip.
            rec.SaveToClip(clip);
        }
    }





}
