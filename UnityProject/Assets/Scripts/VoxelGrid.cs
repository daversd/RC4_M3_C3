using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class VoxelGrid
{
    #region Public fields

    public Vector3Int GridSize;
    public Voxel[,,] Voxels;
    public Corner[,,] Corners;
    public Face[][,,] Faces = new Face[3][,,];
    public Edge[][,,] Edges = new Edge[3][,,];
    public Vector3 Origin;
    public Vector3 Corner;
    public float VoxelSize { get; private set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Constructor for a basic <see cref="VoxelGrid"/>
    /// Adds a game object containing a collider to each of first layer voxels
    /// </summary>
    /// <param name="size">Size of the grid</param>
    /// <param name="origin">Origin of the grid</param>
    /// <param name="voxelSize">The size of each <see cref="Voxel"/></param>
    public VoxelGrid(Vector3Int size, Vector3 origin, float voxelSize, Transform parent = null)
    {
        GridSize = size;
        Origin = origin;
        VoxelSize = voxelSize;

        Voxels = new Voxel[GridSize.x, GridSize.y, GridSize.z];

        for (int x = 0; x < GridSize.x; x++)
        {
            for (int y = 0; y < GridSize.y; y++)
            {
                for (int z = 0; z < GridSize.z; z++)
                {
                    if (y == 0)
                    {
                        Voxels[x, y, z] = new Voxel(
                            new Vector3Int(x, y, z),
                            this,
                            createCollider: true,
                            parent: parent);
                    }
                    else
                    {
                        Voxels[x, y, z] = new Voxel(
                            new Vector3Int(x, y, z),
                            this);
                    }
                    
                }
            }
        }

        MakeFaces();
        MakeCorners();
        MakeEdges();
    }

    #endregion

    #region Grid elements constructors

    /// <summary>
    /// Creates the Faces of each <see cref="Voxel"/>
    /// </summary>
    private void MakeFaces()
    {
        // make faces
        Faces[0] = new Face[GridSize.x + 1, GridSize.y, GridSize.z];

        for (int x = 0; x < GridSize.x + 1; x++)
            for (int y = 0; y < GridSize.y; y++)
                for (int z = 0; z < GridSize.z; z++)
                {
                    Faces[0][x, y, z] = new Face(x, y, z, Axis.X, this);
                }

        Faces[1] = new Face[GridSize.x, GridSize.y + 1, GridSize.z];

        for (int x = 0; x < GridSize.x; x++)
            for (int y = 0; y < GridSize.y + 1; y++)
                for (int z = 0; z < GridSize.z; z++)
                {
                    Faces[1][x, y, z] = new Face(x, y, z, Axis.Y, this);
                }

        Faces[2] = new Face[GridSize.x, GridSize.y, GridSize.z + 1];

        for (int x = 0; x < GridSize.x; x++)
            for (int y = 0; y < GridSize.y; y++)
                for (int z = 0; z < GridSize.z + 1; z++)
                {
                    Faces[2][x, y, z] = new Face(x, y, z, Axis.Z, this);
                }
    }

    /// <summary>
    /// Creates the Corners of each Voxel
    /// </summary>
    private void MakeCorners()
    {
        Corner = new Vector3(Origin.x - VoxelSize / 2, Origin.y - VoxelSize / 2, Origin.z - VoxelSize / 2);

        Corners = new Corner[GridSize.x + 1, GridSize.y + 1, GridSize.z + 1];

        for (int x = 0; x < GridSize.x + 1; x++)
            for (int y = 0; y < GridSize.y + 1; y++)
                for (int z = 0; z < GridSize.z + 1; z++)
                {
                    Corners[x, y, z] = new Corner(new Vector3Int(x, y, z), this);
                }
    }

    /// <summary>
    /// Creates the Edges of each Voxel
    /// </summary>
    private void MakeEdges()
    {
        Edges[2] = new Edge[GridSize.x + 1, GridSize.y + 1, GridSize.z];

        for (int x = 0; x < GridSize.x + 1; x++)
            for (int y = 0; y < GridSize.y + 1; y++)
                for (int z = 0; z < GridSize.z; z++)
                {
                    Edges[2][x, y, z] = new Edge(x, y, z, Axis.Z, this);
                }

        Edges[0] = new Edge[GridSize.x, GridSize.y + 1, GridSize.z + 1];

        for (int x = 0; x < GridSize.x; x++)
            for (int y = 0; y < GridSize.y + 1; y++)
                for (int z = 0; z < GridSize.z + 1; z++)
                {
                    Edges[0][x, y, z] = new Edge(x, y, z, Axis.X, this);
                }

        Edges[1] = new Edge[GridSize.x + 1, GridSize.y, GridSize.z + 1];

        for (int x = 0; x < GridSize.x + 1; x++)
            for (int y = 0; y < GridSize.y; y++)
                for (int z = 0; z < GridSize.z + 1; z++)
                {
                    Edges[1][x, y, z] = new Edge(x, y, z, Axis.Y, this);
                }
    }

    #endregion

    #region Grid operations


    /// <summary>
    /// Get the Faces of the <see cref="VoxelGrid"/>
    /// </summary>
    /// <returns>All the faces</returns>
    public IEnumerable<Face> GetFaces()
    {
        for (int n = 0; n < 3; n++)
        {
            int xSize = Faces[n].GetLength(0);
            int ySize = Faces[n].GetLength(1);
            int zSize = Faces[n].GetLength(2);

            for (int x = 0; x < xSize; x++)
                for (int y = 0; y < ySize; y++)
                    for (int z = 0; z < zSize; z++)
                    {
                        yield return Faces[n][x, y, z];
                    }
        }
    }

    /// <summary>
    /// Get the Voxels of the <see cref="VoxelGrid"/>
    /// </summary>
    /// <returns>All the Voxels</returns>
    public IEnumerable<Voxel> GetVoxels()
    {
        for (int x = 0; x < GridSize.x; x++)
            for (int y = 0; y < GridSize.y; y++)
                for (int z = 0; z < GridSize.z; z++)
                {
                    yield return Voxels[x, y, z];
                }
    }

    /// <summary>
    /// Get the Corners of the <see cref="VoxelGrid"/>
    /// </summary>
    /// <returns>All the Corners</returns>
    public IEnumerable<Corner> GetCorners()
    {
        for (int x = 0; x < GridSize.x + 1; x++)
            for (int y = 0; y < GridSize.y + 1; y++)
                for (int z = 0; z < GridSize.z + 1; z++)
                {
                    yield return Corners[x, y, z];
                }
    }

    /// <summary>
    /// Get the Edges of the <see cref="VoxelGrid"/>
    /// </summary>
    /// <returns>All the edges</returns>
    public IEnumerable<Edge> GetEdges()
    {
        for (int n = 0; n < 3; n++)
        {
            int xSize = Edges[n].GetLength(0);
            int ySize = Edges[n].GetLength(1);
            int zSize = Edges[n].GetLength(2);

            for (int x = 0; x < xSize; x++)
                for (int y = 0; y < ySize; y++)
                    for (int z = 0; z < zSize; z++)
                    {
                        yield return Edges[n][x, y, z];
                    }
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Tries to create a black rectangle from the
    /// specified origin and with the specified size
    /// </summary>
    /// <param name="origin">The index of the origin</param>
    /// <param name="width">The width of the rectangle, in X</param>
    /// <param name="depth">The depth of the rectangle, in Z</param>
    /// <param name="layer">The layer to draw in, default is 0</param>
    /// <returns>If the process was successful</returns>
    public bool CreateBlackRectangle(Vector3Int origin, int width, int depth, int layer = 0)
    {
        FunctionColor fcolor = FunctionColor.Black;

        int oX = origin.x;
        int oZ = origin.z;

        List<Voxel> recVoxels = new List<Voxel>();

        for (int x = oX; x < oX + width; x++)
        {
            for (int z = oZ; z < oZ + depth; z++)
            {
                Vector3Int idx = new Vector3Int(x, 0, z);
                if (Util.ValidateIndex(GridSize, idx))
                {
                    var voxel = Voxels[x, layer, z];
                    if (voxel.FColor == FunctionColor.Empty)
                    {
                        recVoxels.Add(voxel);
                    }
                    else return false;
                }
                else return false;
            }
        }

        foreach (var voxel in recVoxels)
        {
            voxel.FColor = fcolor;
        }

        return true;
    }

    /// <summary>
    /// Tries to create a black blob from the
    /// specified origin and with the specified size
    /// </summary>
    /// <param name="origin">The index of the origin</param>
    /// <param name="radius">The radius of the blob in voxels</param>
    /// <param name="picky">If the blob should skip voxels randomly as it expands</param>
    /// <param name="flat">If the blob should be located on the first layer or use all</param>
    /// <returns></returns>
    public bool CreateBlackBlob(Vector3Int origin, int radius, bool picky = true, bool flat = true)
    {
        // Create the list to store the blob voxels
        List<Voxel> blobVoxels = new List<Voxel>();

        // Set the target fucntion color to black
        FunctionColor fcolor = FunctionColor.Black;

        // Check if the origin is valid and add it to the list of voxels
        if (Util.ValidateIndex(GridSize, origin)) blobVoxels.Add(Voxels[origin.x, origin.y, origin.z]);
        else return false;

        // Iterate through each of the neighbouring layers
        for (int i = 0; i < radius; i++)
        {
            // Create a list to store the new voxels
            List<Voxel> newVoxels = new List<Voxel>();

            // Check the neighbours of each of the voxels already collected
            foreach (var voxel in blobVoxels)
            {
                // Get it's neighbours
                Voxel[] neighbours;
                if (flat) neighbours = voxel.GetFaceNeighboursXZ().ToArray();
                else neighbours = voxel.GetFaceNeighbours().ToArray();

                // Iterate through each neighbour
                foreach (var neighbour in neighbours)
                {
                    // Decide if neighbour should be taken if picky is true
                    bool takeMe = true;
                    if (picky && (Random.value < 0.6f)) takeMe = false;

                    // Check if neighbour is valid
                    if (takeMe &&
                        neighbour.IsActive &&
                        Util.ValidateIndex(GridSize, neighbour.Index) &&
                        neighbour.FColor == FunctionColor.Empty &&
                        !blobVoxels.Contains(neighbour) &&
                        !newVoxels.Contains(neighbour))
                    {
                        // Add to temp list
                        newVoxels.Add(neighbour);
                    }
                }
            }

            // If no new voxels were added, break
            if (newVoxels.Count == 0) break;

            // Add new voxels to main list
            foreach (var newVoxel in newVoxels)
            {
                blobVoxels.Add(newVoxel);
            }
        }

        // If the resulting number of voxels is less than the radius, return false
        if (blobVoxels.Count < radius) return false;

        // Set the Function color of the voxels
        foreach (var voxel in blobVoxels)
        {
            voxel.FColor = fcolor;
        }

        return true;
    }

    // 24 Create the image from grid method
    /// <summary>
    /// Get an image from the grid at the selected layer
    /// using the <see cref="FunctionColor"/> of the voxels
    /// </summary>
    /// <param name="layer">Default layer set to 0</param>
    /// <returns>The image with the colored states</returns>
    public Texture2D ImageFromGrid(int layer = 0, bool transparent = false)
    {
        // 25 Setup the image
        TextureFormat textureFormat;
        if (transparent) textureFormat = TextureFormat.RGBA32;
        else textureFormat = TextureFormat.RGB24;
        
        // 26 Create the texture
        Texture2D gridImage = new Texture2D(GridSize.x, GridSize.z, textureFormat, true, true);

        // 27 Iterate through voxels in X and Z for the given layer
        for (int i = 0; i < gridImage.width; i++)
        {
            for (int j = 0; j < gridImage.height; j++)
            {
                // 28 Get the voxel
                var voxel = Voxels[i, layer, j];

                // 29 Define the pixel's color based on its Function color
                Color c;
                if (voxel.FColor == FunctionColor.Black) c = Color.black;
                else if (voxel.FColor == FunctionColor.Red) c = Color.red;
                else if (voxel.FColor == FunctionColor.Yellow) c = Color.yellow;
                else if (voxel.FColor == FunctionColor.Green) c = Color.green;
                else if (voxel.FColor == FunctionColor.Cyan) c = Color.cyan;
                else if (voxel.FColor == FunctionColor.Magenta) c = Color.magenta;
                else c = new Color(1f, 1f, 1f, 0f);

                // 30 Set the color to the texture
                gridImage.SetPixel(i, j, c);
            }
        }

        // 31 Apply the pixels to the texture
        gridImage.Apply();

        return gridImage;
    }

    // 12 Create the voxel grid clear method
    /// <summary>
    /// Clears the grid's voxels' <see cref="FunctionColor"/> by setting them to Empty
    /// </summary>
    public void ClearGrid()
    {
        foreach (var voxel in Voxels)
        {
            voxel.FColor = FunctionColor.Empty;
        }
    }

    // 48 Create the method to clear red data
    /// <summary>
    /// Clears the grid's red voxels' <see cref="FunctionColor"/> by setting them to Empty
    /// </summary>
    public void ClearReds()
    {
        foreach (var voxel in Voxels)
        {
            if (voxel.FColor == FunctionColor.Red) voxel.FColor = FunctionColor.Empty;
        }
    }

    /// <summary>
    /// Reads an image pixel data and set the red pixels to the grid
    /// </summary>
    /// <param name="image">The reference image</param>
    /// <param name="layer">The target layer</param>
    public void SetStatesFromImage(Texture2D image, int layer = 0)
    {
        // Iterate through the XZ plane
        for (int x = 0; x < GridSize.x; x++)
        {
            for (int z = 0; z < GridSize.z; z++)
            {
                // Get the pixel color from the image
                var pixel = image.GetPixel(x, z);

                // Check if pixel is red
                if (pixel.r >pixel.g)
                {
                    // Set respective color to voxel
                    Voxels[x, layer, z].FColor = FunctionColor.Red;
                }
            }
        }
    }


    #endregion
}

/// <summary>
/// Color coded values
/// </summary>
public enum FunctionColor
{
    Empty = -1,
    Black = 0,
    Red = 1,
    Yellow = 2,
    Green = 3,
    Cyan = 4,
    Magenta = 5
}