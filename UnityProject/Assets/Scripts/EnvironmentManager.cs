using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Linq;

public class EnvironmentManager : MonoBehaviour
{
    #region Fields and properties

    VoxelGrid _voxelGrid;
    int _randomSeed = 666;

    bool _showVoids = true;

    // 45 Create the inference object variable
    Pix2Pix _pix2pix;

    #endregion

    #region Unity Standard Methods

    void Start()
    {
        // Initialise the voxel grid
        Vector3Int gridSize = new Vector3Int(64, 10, 64);
        _voxelGrid = new VoxelGrid(gridSize, Vector3.zero, 1, parent: this.transform);

        // Set the random engine's seed
        Random.InitState(_randomSeed);

        // 46 Create the Pix2Pix inference object
        _pix2pix = new Pix2Pix();
    }

    void Update()
    {
        // Draw the voxels according to their Function Colors
        DrawVoxels();

        // Use the V key to switch between showing voids
        if (Input.GetKeyDown(KeyCode.V))
        {
            _showVoids = !_showVoids;
        }

        // 08 Use the mouse button to select a voxel an create the blob
        if (Input.GetMouseButtonDown(0))
        {
            // 09 Get the clicked voxel
            var voxel = SelectVoxel();

            if (voxel != null)
            {
                // 10 Create a blob in the selected voxel
                _voxelGrid.CreateBlackBlob(voxel.Index, 20, flat: false);

                // 58 Run the inference model on the grid
                PredictAndUpdate(allLayers: true);
            }
        }

        // 11 Use the R key to clear the grid
        if (Input.GetKeyDown(KeyCode.R))
        {
            // 13 Clear the grid
            _voxelGrid.ClearGrid();
        }

        // 15 Use the space bar to start the dataset creation process 
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 16 Create random blobs
            //CreateRandomBlobs(4, 20, 30);

            // 37 Populate random blobs and save
            //PopulateBlobsAndSave(500, 2, 4, 15, 25);
        }
    }

    #endregion

    #region Private Methods

    // 47 Create the method to run predictions on the grid and update
    /// <summary>
    /// Runs the predictioin model on the grid
    /// </summary>
    /// <param name="allLayers">If it should run on all layers. Default is only layer 0</param>
    void PredictAndUpdate(bool allLayers = false)
    {
        // 49 Set the red voxels to empty
        _voxelGrid.ClearReds();
        
        // 50 Define how many layers will be used based on bool
        int layerCount = 1;
        if (allLayers) layerCount = _voxelGrid.GridSize.y;

        // 51 Iterate through all layers, running the model
        for (int i = 0; i < layerCount; i++)
        {
            // 52 Get the image from the grid's layer
            var gridImage = _voxelGrid.ImageFromGrid(layer: i);

            // 53 Resize the image to 256x256
            ImageReadWrite.Resize256(gridImage, Color.grey);

            // 54 Run the inference model on the image
            var image = _pix2pix.Predict(gridImage);

            // 56 Scale the image back to the grid size
            TextureScale.Point(image, _voxelGrid.GridSize.x, _voxelGrid.GridSize.z);

            // 57 Set the layer's voxels' states to the grid
            _voxelGrid.SetStatesFromImage(image, layer: i);
        }
    }

    // 17 Create the method to populate random blobs on the grid and save images
    /// <summary>
    /// Populates random blobs on the grid's first layer and save the image to disk
    /// </summary>
    /// <param name="sampleSize">The amount of images to save</param>
    /// <param name="minAmt">The minimum amount of blobs per image</param>
    /// <param name="maxAmt">The maximum amount of blobs per image</param>
    /// <param name="minRadius">The minimum radius of the blobs</param>
    /// <param name="maxRadius">The maximum radius of the blobs</param>
    void PopulateBlobsAndSave(int sampleSize, int minAmt, int maxAmt, int minRadius, int maxRadius)
    {
        // 18 Create and start a stopwatch to keep track of creation time
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        // 19 Define the name of the folder to save in
        string saveFolder = "Output";

        // 20 Create each of the samples
        for (int i = 0; i < sampleSize; i++)
        {
            // 21 Set a random amount of blobs from the defined range
            int amt = Random.Range(minAmt, maxAmt);

            // 22 Clear the voxel grid completely
            _voxelGrid.ClearGrid();

            // 23 Create the random blobs with the given parameters
            CreateRandomBlobs(amt, minRadius, maxRadius, true);

            // 32 Get the resulting grid image
            Texture2D gridImage = _voxelGrid.ImageFromGrid(transparent: true);
            
            // 33 Resize the image to 256x256
            Texture2D resizedImage = ImageReadWrite.Resize256(gridImage, Color.grey);

            // 34 Save the image to the specified folder
            ImageReadWrite.SaveImage(resizedImage, $"{saveFolder}/Grid_{i}");
        }

        // 35 Stop the stopwatch once the process has finished
        stopwatch.Stop();

        // 36 Print how long it took to generate
        print($"Took {stopwatch.ElapsedMilliseconds} milliseconds to generate {sampleSize} images");
    }

    // 14 Create the method to produce random blobs
    /// <summary>
    /// Creates random blobs on the grid's edges
    /// </summary>
    /// <param name="amt">The amount of blobs to create</param>
    /// <param name="minRadius">The minimum radius of the blob</param>
    /// <param name="maxRadius">The maximum radius of the blob</param>
    /// <param name="picky">If the blob drawing should randomly skip voxels</param>
    void CreateRandomBlobs(int amt, int minRadius, int maxRadius, bool picky = true)
    {
        // 15 Iterate through the amount size
        for (int i = 0; i < amt; i++)
        {
            // 16 Keep track if the process has been sucessful
            bool success = false;
            while (!success)
            {
                // 17 Create a random value to determine blob location
                float rand = Random.value;

                // 18 Define the x and z coordinates of the blob
                int x;
                int z;

                // 19 Set blob to be on x = 0 or x = 1
                if (rand < 0.5f)
                {
                    x = Random.value < 0.5f ? 0 : _voxelGrid.GridSize.x - 1;
                    z = Random.Range(0, _voxelGrid.GridSize.z);
                }
                // 20 Set blob to be on z = 0 or z = 1
                else
                {
                    z = Random.value < 0.5f ? 0 : _voxelGrid.GridSize.z - 1;
                    x = Random.Range(0, _voxelGrid.GridSize.x);
                }

                // 21 Define the blob's origin
                Vector3Int origin = new Vector3Int(x, 0, z);

                // 22 Define the blob's radius
                int radius = Random.Range(minRadius, maxRadius);

                // 23 Try to generate the blob and keep track of result
                success = _voxelGrid.CreateBlackBlob(origin, radius, picky);
            }
        }
    }

    /// <summary>
    /// Populates random rectangles on the grid's first layer and save the image to disk
    /// </summary>
    /// <param name="sampleSize">The amount of images to create</param>
    /// <param name="minAmt">The minimum amount of rectangles per image</param>
    /// <param name="maxAmt">The maximum amount if rectangles per image</param>
    /// <param name="minWidth">The minimum width of the rectangles</param>
    /// <param name="maxWidth">The maximum width of the rectangles</param>
    /// <param name="minDepth">The minimum depth of the rectangles</param>
    /// <param name="maxDepth">The maximum depth of the rectangles</param>
    void PopulateRectanglesAndSave(int sampleSize, int minAmt, int maxAmt, int minWidth, int maxWidth, int minDepth, int maxDepth)
    {
        // Create and start a stopwatch to keep track of creation time
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        // Define the name of the folder to save in
        string saveFolder = "Output";

        // Create each of the samples
        for (int i = 0; i < sampleSize; i++)
        {
            // Set a random amount of rectangles from the defined range
            int amt = Random.Range(minAmt, maxAmt);

            // Clear the voxel grid completely
            _voxelGrid.ClearGrid();

            // Create the rectangles
            CreateRandomRectangles(amt, minWidth, maxWidth, minDepth, maxDepth);
            
            // Get the resulting grid image
            Texture2D gridImage = _voxelGrid.ImageFromGrid();

            // Resize the image to 256x256
            Texture2D resizedImage = ImageReadWrite.Resize256(gridImage, Color.grey);

            // Save the image to the designated folder
            ImageReadWrite.SaveImage(resizedImage, $"{saveFolder}/Grid_{i}");
        }

        // Stop the stopwatch once the process has finished
        stopwatch.Stop();

        // Print how long it took to generate
        print($"Took {stopwatch.ElapsedMilliseconds} milliseconds to generate {sampleSize} images");
    }

    /// <summary>
    /// Create random rectangles on the grid
    /// </summary>
    /// <param name="amt">The amount of rectangles to generate</param>
    /// <param name="minWidth">The minimum width of the rectangles</param>
    /// <param name="maxWidth">The maximum width of the rectangles</param>
    /// <param name="minDepth">The minimum depth of the rectangles</param>
    /// <param name="maxDepth">The maximum depth of the rectangles</param>
    void CreateRandomRectangles(int amt, int minWidth, int maxWidth, int minDepth, int maxDepth)
    {
        for (int i = 0; i < amt; i++)
        {
            bool success = false;
            while (!success)
            {
                int x = Random.Range(0, _voxelGrid.GridSize.x);
                int z = Random.Range(0, _voxelGrid.GridSize.z);

                Vector3Int origin = new Vector3Int(x, 0, z);

                int width = Random.Range(minWidth, maxWidth);
                int depth = Random.Range(minDepth, maxDepth);

                success = _voxelGrid.CreateBlackRectangle(origin, width, depth);
            }
        }
    }

    // 01 Create the method to select the voxel on click
    /// <summary>
    /// Use the mouse position to select a voxel on the bottom layer
    /// </summary>
    /// <returns>The selected voxel</returns>
    Voxel SelectVoxel()
    {
        // 02 Create the variable to store the voxel
        Voxel selected = null;

        // 03 Prepare and cast the ray
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // 04 Get the hit obeject's transform
            Transform objectHit = hit.transform;

            // 05 Check if it is a voxel
            if (objectHit.CompareTag("Voxel"))
            {
                // 06 Get the voxel's index from the collider's name
                string voxelName = objectHit.name;
                var index = voxelName.Split('_').Select(v => int.Parse(v)).ToArray();

                // 07 Get the voxel from the index
                selected = _voxelGrid.Voxels[index[0], index[1], index[2]];
            }
        }

        return selected;
    }

    /// <summary>
    /// Draws the voxels according to it's state and Function Corlor
    /// </summary>
    void DrawVoxels()
    {
        foreach (var voxel in _voxelGrid.Voxels)
        {
            if (voxel.IsActive)
            {
                Vector3 pos = (Vector3)voxel.Index * _voxelGrid.VoxelSize + transform.position;
                if (voxel.FColor    ==   FunctionColor.Black)   Drawing.DrawCube(pos, _voxelGrid.VoxelSize, Color.black);
                else if (voxel.FColor == FunctionColor.Red)     Drawing.DrawCube(pos, _voxelGrid.VoxelSize, Color.red);
                else if (voxel.FColor == FunctionColor.Yellow)  Drawing.DrawCube(pos, _voxelGrid.VoxelSize, Color.yellow);
                else if (voxel.FColor == FunctionColor.Green)   Drawing.DrawCube(pos, _voxelGrid.VoxelSize, Color.green);
                else if (voxel.FColor == FunctionColor.Cyan)    Drawing.DrawCube(pos, _voxelGrid.VoxelSize, Color.cyan);
                else if (voxel.FColor == FunctionColor.Magenta) Drawing.DrawCube(pos, _voxelGrid.VoxelSize, Color.magenta);
                else if (_showVoids && voxel.Index.y == 0)
                    Drawing.DrawTransparentCube(pos, _voxelGrid.VoxelSize);
            }
        }
    }

    #endregion
}
