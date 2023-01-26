using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(-90)]
public class OVRHandProxy : OVRHand, OVRSkeleton.IOVRSkeletonDataProvider
{
    protected struct Record
    {
        public float time;
        public OVRSkeleton.SkeletonPoseData data;

        public Record(float t, OVRSkeleton.SkeletonPoseData d)
        {
            time = t;
            data = d;
        }
    }

    #region Members
    [Range(0.0f, 2.0f)]
    public float latency = 0.0f;

    [Range(0.0f, 0.02f)]
    public float noise = 0.0f;

    protected Queue<Record> queue;
    #endregion

    #region MonoBehaviour callbacks
    protected void Start()
    {
        queue = new Queue<Record>();
    }
    #endregion

    #region IOVRSkeletonDataProvider implementation
    OVRSkeleton.SkeletonPoseData OVRSkeleton.IOVRSkeletonDataProvider.GetSkeletonPoseData()
    {
        OVRSkeleton.SkeletonPoseData data = new OVRSkeleton.SkeletonPoseData();

        data.IsDataValid = IsDataValid;
        if (IsDataValid)
        {
            data.RootPose = Rand(_handState.RootPose);
            data.RootScale = Rand(_handState.HandScale);
            data.BoneRotations = _handState.BoneRotations.Select(q => Rand(q)).ToArray();
            data.IsDataHighConfidence = IsTracked && HandConfidence == TrackingConfidence.High;
        }

        queue.Enqueue(new Record(Time.realtimeSinceStartup, data));

        while (queue.Count > 1 && (Time.realtimeSinceStartup - queue.Peek().time) > latency)
        {
            queue.Dequeue();
        }

        return queue.Peek().data;
    }
    #endregion

    #region Internal methods
    protected float Rand(float f) => f + Random.Range(-noise, noise);

    protected OVRPlugin.Vector3f Rand(OVRPlugin.Vector3f v) => new OVRPlugin.Vector3f
    {
        x = v.x + Random.Range(-noise, noise),
        y = v.y + Random.Range(-noise, noise),
        z = v.z + Random.Range(-noise, noise)
    };

    protected OVRPlugin.Quatf Rand(OVRPlugin.Quatf q) => new OVRPlugin.Quatf
    {
        w = q.w + Random.Range(-noise, noise),
        x = q.x + Random.Range(-noise, noise),
        y = q.y + Random.Range(-noise, noise),
        z = q.z + Random.Range(-noise, noise)
    };

    protected OVRPlugin.Posef Rand(OVRPlugin.Posef p) => new OVRPlugin.Posef
    {
        Position = Rand(p.Position),
        Orientation = Rand(p.Orientation)
    };
    #endregion
}
