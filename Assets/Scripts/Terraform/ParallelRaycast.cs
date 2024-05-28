using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public static class ParallelRaycast {
    [BurstCompile]
    public struct PrepareRaycastJob : IJobParallelFor {
        [ReadOnly] public NativeArray<Vector2> points;
        [ReadOnly] public QueryParameters parameters;
        [ReadOnly] public Bounds bounds;
        [WriteOnly] public NativeArray<RaycastCommand> raycastCommands;

        public void Execute(int i) {
            Vector3 offset = new Vector3(
                -bounds.size.x * 0.5f,
                bounds.max.y + 20f,
                -bounds.size.z * 0.5f
            );
            float maxDst = bounds.max.y * 10;
            Vector3 startPoint = new Vector3(points[i].x, 0, points[i].y) + offset;
            raycastCommands[i] = new RaycastCommand(startPoint, Vector3.down, parameters, maxDst);
        }
    }

    public static RaycastHit[] PointsToSurface(Vector2[] points, string layerName, Bounds bounds) {
        NativeArray<RaycastCommand> raycastCommands = new NativeArray<RaycastCommand>(points.Length, Allocator.Persistent);
        NativeArray<RaycastHit> raycastHits = new NativeArray<RaycastHit>(points.Length, Allocator.Persistent);
        NativeArray<Vector2> pointsOS = new NativeArray<Vector2>(points.Length, Allocator.Persistent);

        pointsOS.CopyFrom(points);
        PrepareRaycastJob prepareRaycastJob = new() {
            points = pointsOS,
            parameters = new QueryParameters(layerMask: 1 << LayerMask.NameToLayer(layerName)),
            bounds = bounds,
            raycastCommands = raycastCommands
        };

        int batchSize = (int)Mathf.Pow(2, 1);
        JobHandle handle = prepareRaycastJob.Schedule(raycastCommands.Length, batchSize, default);
        handle = RaycastCommand.ScheduleBatch(raycastCommands, raycastHits, batchSize, 1, handle);
        handle.Complete();

        RaycastHit[] hits = raycastHits.ToArray();

        raycastCommands.Dispose();
        raycastHits.Dispose();
        pointsOS.Dispose();

        return hits;
    }
}