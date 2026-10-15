#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class DroneWaypointTools
{
    [MenuItem("GameObject/Drone/Create StartPoint At Visual Center", false, 30)]
    private static void CreateStartPoint()
    {
        CreateWaypoint("StartPoint");
    }

    [MenuItem("GameObject/Drone/Create EndPoint At Visual Center", false, 31)]
    private static void CreateEndPoint()
    {
        CreateWaypoint("EndPoint");
    }

    [MenuItem("GameObject/Drone/Create StartPoint At Visual Center", true)]
    [MenuItem("GameObject/Drone/Create EndPoint At Visual Center", true)]
    private static bool ValidateCreateWaypoint()
    {
        return Selection.activeGameObject != null;
    }

    private static void CreateWaypoint(string waypointName)
    {
        GameObject parent = Selection.activeGameObject;
        if (parent == null)
            return;

        Vector3 visualCenter = GetVisualCenter(parent.transform);
        GameObject waypoint = new GameObject(waypointName);

        Undo.RegisterCreatedObjectUndo(waypoint, "Create Drone Waypoint");
        waypoint.transform.SetParent(parent.transform, true);
        waypoint.transform.position = visualCenter;

        Selection.activeGameObject = waypoint;
        EditorGUIUtility.PingObject(waypoint);
    }

    private static Vector3 GetVisualCenter(Transform parent)
    {
        Renderer[] renderers = parent.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
            return parent.position;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        return bounds.center;
    }
}
#endif