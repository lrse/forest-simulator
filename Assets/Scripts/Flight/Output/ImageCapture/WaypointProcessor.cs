using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static Mission;
using UnityEngine.Rendering;

public class WaypointProcessor {

    private readonly CameraDefinition cameraDefinition;

    private readonly GroundControlPointManager gcpManager;
    private readonly GeoTagger geoTagger;
    private readonly Georeferencing georeferencing;
    private readonly string folderPath;
    private readonly int totalWaypoints;

    private GameObject gameObject;
    private Camera camera;
    private RenderTexture renderTexture;
    private Queue<WaypointData> waypointsToProcess;
    private int waypointsProcessed = 0;
    private int waypointsGivenForProcessing = 0;

    public delegate void AllWaypointsProcessedEvent();
    public event AllWaypointsProcessedEvent OnProcessedAllWaypoints;

    private bool saveSegmentationImages;
    private Camera segCamera;
    private GameObject segGameObject;
    private RenderTexture segRenderTexture;

    private Dictionary<string, Color> objectTagColors;
    private List<GameObject> objectsToRender = new List<GameObject>();
    private Dictionary<GameObject,Material> originalMaterial = new Dictionary<GameObject, Material>();
    private Dictionary<GameObject, Material> replacementMaterial = new Dictionary<GameObject, Material>();

    private Shader shader;
    private Shader grassShader;
    private ProceduralGrassRenderer grassRenderer;
    private Material grassMaterial;
    private Material replacementGrassMaterial;
    private GameObject terraformer;

    public WaypointProcessor(CameraDefinition cameraDefinition, SurveyArea surveyArea, Georeferencing georeferencing, int totalWaypoints, string folderPath) {
        // Build GameObject
        gameObject = new GameObject("Image Capture");

        // Build the render texture
        renderTexture = new RenderTexture(cameraDefinition.resolutionX, cameraDefinition.resolutionY, 24);

        // Set the camera
        camera = gameObject.AddComponent<Camera>();
        camera.usePhysicalProperties = true;
        camera.focalLength = cameraDefinition.focalLength;
        camera.sensorSize = new Vector2(cameraDefinition.sensorSizeX, cameraDefinition.sensorSizeY);
        camera.targetTexture = renderTexture;
        camera.enabled = false; // We don't want to render constantly, so we disable the camera

        // Initialize queue
        waypointsToProcess = new Queue<WaypointData>();

        // Store pertinent values
        this.cameraDefinition = cameraDefinition;
        this.totalWaypoints = totalWaypoints;
        this.folderPath = folderPath;

        // Initialize necessary entities
        this.gcpManager = new GroundControlPointManager(cameraDefinition, surveyArea);
        this.geoTagger = new GeoTagger(cameraDefinition, georeferencing);
        this.georeferencing = georeferencing;

        terraformer = GameObject.Find("Terraformer");
        if (terraformer == null) { Debug.Log("Terraformer not found"); };
        saveSegmentationImages = Camera.main.GetComponent<DroneControl>().saveSegmentationImages;
        if (saveSegmentationImages)
        {
            this.shader = Shader.Find("Custom/TreeUnlitShader");
            this.grassShader = Shader.Find("Grass/GrassBlades");

            this.objectTagColors = new Dictionary<string, Color>();

            grassRenderer = terraformer.GetComponent<ProceduralGrassRenderer>();
            grassMaterial = grassRenderer.instantiatedMaterial;
            replacementGrassMaterial = new Material(this.grassShader);

            SetObjectTags();
            SetObjectsToSegment();

            // Set Segmentation Camera
            segGameObject = new GameObject("Image Segment Capture");
            segRenderTexture = new RenderTexture(cameraDefinition.resolutionX, cameraDefinition.resolutionY, 24);
            segCamera = segGameObject.AddComponent<Camera>();
            segCamera.CopyFrom(camera);
            segCamera.targetTexture = segRenderTexture;
        }

        // Prepare the folder
        PrepareFolder(folderPath);

        // Start the waypoint listener
        StaticCoroutine.StartCoroutine(ProcessWaypoints());
    }

    public void AddWaypointForProcessing(Waypoint waypoint) {
        waypointsToProcess.Enqueue(new WaypointData(waypoint, ++waypointsGivenForProcessing));
    }

    // This method could really be improved. Some of the last parts could be done in a separate thread. However, the lagging still remains.
    // Apparently this is a known issue, and the only thing I could think of that might help, was to read smaller parts of the camera per frame and then
    // merge them all together. Would need to see if it works.

    // Also, by using WaitForEndOfFrame, we now can't exit Game Mode. We could:
    // 1. Move the debug logic into Game Mode
    // 2. Make this a MonoBehaviour and use LateUpdate instead of a Coroutine for the method ProcessWaypoints
    private IEnumerator ProcessWaypoints() {
        while (waypointsProcessed < totalWaypoints) {
            if (waypointsToProcess.Count > 0) {
                Debug.Log(string.Format("Capturing image. {0}/{1}", waypointsProcessed + 1, totalWaypoints));
                WaypointData waypointData = waypointsToProcess.Dequeue();
                Waypoint waypoint = waypointData.waypoint;

                // Move the camera to the waypoint
                gameObject.transform.SetPositionAndRotation(waypoint.dronePosition, waypoint.droneRotation);

                // Wait until the frame ends
                yield return new WaitForEndOfFrame();

                // Render the camera and store it in a texture
                Texture2D screenShot = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
                camera.Render();

                yield return null;

                // Read the render texture into the Texture2D
                RenderTexture.active = renderTexture;
                screenShot.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                RenderTexture.active = null; //Added to avoid errors

                yield return null;

                // Check for visible GCPs from the curreny waypoint
                string imageName = GetImageName(waypointData.waypointNumber);
                gcpManager.CheckForVisibleGCPs(screenShot, camera, imageName);

                yield return null;

                // Save the texture into disk and then delete it
                SaveImage(screenShot, imageName);
                Object.Destroy(screenShot);

                yield return null;

                // Geotag the image properly
                geoTagger.TagImage(waypoint, Path.Combine(folderPath, imageName));
                
                // Save Segmentation Images

                // Render the camera and store it in a texture
                if(saveSegmentationImages)
                {
                    segGameObject.transform.SetPositionAndRotation(waypoint.dronePosition, waypoint.droneRotation);

                    foreach (GameObject go in this.objectsToRender)
                    {
                        Renderer rd = go.GetComponent<Renderer>();
                        if (rd != null)
                        {
                            rd.material = replacementMaterial[go];

                        }
                        else
                        {
                            // Submeshes
                            foreach (GameObject child in go.GetChildren())
                            {
                                rd = child.GetComponent<Renderer>();
                                if (rd != null)
                                {
                                    rd.material = replacementMaterial[child];
                                }
                            }
                        }
                    }

                    grassRenderer.instantiatedMaterial = replacementGrassMaterial;
                    grassRenderer.SetPointsBuffersAndKernel();

                    yield return null;

                    Texture2D screenShotSegmented = new Texture2D(segRenderTexture.width, segRenderTexture.height, TextureFormat.RGB24, false);
                    segCamera.Render();

                    yield return null;

                    // Read the render texture into the Texture2D
                    RenderTexture.active = segRenderTexture;
                    screenShotSegmented.ReadPixels(new Rect(0, 0, segRenderTexture.width, segRenderTexture.height), 0, 0);
                    RenderTexture.active = null; //Added to avoid errors

                    yield return null;

                    foreach (GameObject go in this.objectsToRender)
                    {
                        Renderer rd = go.GetComponent<Renderer>();
                        if (rd != null)
                        {
                            rd.material = originalMaterial[go];
                        }
                        else
                        {
                            // Submeshes
                            foreach (GameObject child in go.GetChildren())
                            {
                                rd = child.GetComponent<Renderer>();
                                if (rd != null)
                                {
                                    rd.material = originalMaterial[child];
                                }
                            }
                        }
                    }
                    grassRenderer.instantiatedMaterial = grassMaterial;
                    grassRenderer.SetPointsBuffersAndKernel();
                    yield return null;

                    // Check for visible GCPs from the curreny waypoint
                    string imageNameSegmented = GetImageSegmentedName(waypointData.waypointNumber);
                    gcpManager.CheckForVisibleGCPs(screenShotSegmented, camera, imageNameSegmented);

                    yield return null;

                    // Save the texture into disk and then delete it
                    SaveImage(screenShotSegmented, imageNameSegmented);
                    Object.Destroy(screenShotSegmented);
                    yield return null;

                    // Geotag the image properly
                    geoTagger.TagImage(waypoint, Path.Combine(folderPath, imageNameSegmented));

                    yield return null;
                }



                // Update number or waypoints processed
                waypointsProcessed++;
            } else {
                // If there are no waypoint to process, wait for a second before checking again
                yield return new WaitForSeconds(1);
            }
        }
        // Finished processing all the waypoints. Wait a few seconds for the possible tagging to end
        yield return new WaitForSeconds(2);

        // Destroy the created GameObject and render texture
        Object.Destroy(gameObject);
        Object.Destroy(renderTexture);

        // Write GCP file to disk
        gcpManager.WriteToFile(georeferencing, folderPath);

        // Generate event for potential listeners
        OnProcessedAllWaypoints?.Invoke();
    }

    private void SaveImage(Texture2D screenShot, string imageName) {
        byte[] bytes = screenShot.EncodeToPNG();
        File.WriteAllBytes(Path.Combine(folderPath, imageName), bytes);
    }

    private static void PrepareFolder(string folderPath) {
        // Prepare the directory
        if (Directory.Exists(folderPath))
            Directory.Delete(folderPath, true);
        Directory.CreateDirectory(folderPath);
    }

    private static string GetImageName(int waypointNumber) {
        string number = waypointNumber.ToString().PadLeft(4, '0');
        return "Image_" + number + ".png";
    }

    class WaypointData {

        public readonly Waypoint waypoint;
        public readonly int waypointNumber;

        public WaypointData(Waypoint waypoint, int waypointNumber) {
            this.waypoint = waypoint;
            this.waypointNumber = waypointNumber;
        }

    }

    private void SetObjectTags()
    {
        this.objectTagColors["Terrain"] = Color.blue;
        this.objectTagColors["Trunk"] = Color.cyan;
        this.objectTagColors["Canopy"] = Color.green;
        this.objectTagColors["Branches"] = new Color(1, 1, 0, 1);//(0.39f, 0.19f, 0.04f, 1); //brown
        this.objectTagColors["Bushes"] = Color.yellow;
        this.objectTagColors["Understorey"] = new Color(1, 0, 1, 1);//(0.976f, 0.39f, 0.04f, 1); //orange
        this.objectTagColors["Grass"] = Color.red; 
        this.objectTagColors["Cactae"] = new Color(0.19f, 0.118f, 0.586f, 1); // violet
        this.objectTagColors["Deadwood"] = Color.grey;
        this.objectTagColors["GCP"] = Color.black;
    }

    private void SetObjectsToSegment()
    {
        List<string> tags = new List<string> { "Terrain", "Trunk", "Canopy", "Branches", "Bushes", "Understorey", "Cactae", "Deadwood", "GCP" };//, "Grass" };
        
        foreach (string tag in tags)
        {
            GameObject[] objectswithTag = GameObject.FindGameObjectsWithTag(tag);
            this.objectsToRender.AddRange(objectswithTag);
        }

        foreach(GameObject go in objectsToRender)
        {
            Renderer rd = go.GetComponent<Renderer>();
            if (rd != null)
            {
            Material replacementMaterial = new Material(this.shader);
            replacementMaterial.CopyMatchingPropertiesFromMaterial(rd.material);
            replacementMaterial.color = this.objectTagColors[go.tag];

            if (this.originalMaterial.ContainsKey(go) != true)
            {
                this.originalMaterial.Add(go, rd.material);
                this.replacementMaterial.Add(go, replacementMaterial);
            }
            } else
            { 
                foreach (GameObject child in go.GetChildren())
                {
                    Renderer rdChild = child.GetComponent<Renderer>();
                    Material replacementMaterial = new Material(this.shader);
                    replacementMaterial.color = this.objectTagColors[child.tag];
                    if (this.originalMaterial.ContainsKey(child) != true)
                    {
                        this.originalMaterial.Add(child, rdChild.material);
                        this.replacementMaterial.Add(child, replacementMaterial);
                    }
                }
            }
        }

        // Add grass color
        // As this was not working we will now create a new material with a grass shader and change ir from the original. 
        replacementGrassMaterial.CopyMatchingPropertiesFromMaterial(grassMaterial);
        replacementGrassMaterial.SetColor("_TipColor", this.objectTagColors["Grass"]);
        replacementGrassMaterial.SetColor("_BaseColor", this.objectTagColors["Grass"]);

    }

    private static string GetImageSegmentedName(int waypointNumber)
    {
        string number = waypointNumber.ToString().PadLeft(4, '0');
        return "Image_Segmented_" + number + ".png";
    }
}
