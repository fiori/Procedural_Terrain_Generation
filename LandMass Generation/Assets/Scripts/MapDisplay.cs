using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    public Renderer textureRenderer;
    public MeshFilter MeshFilter;
    public MeshRenderer MeshRenderer;

    public void DrawTexture(Texture2D texture)
    {
        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    public void DrawMesh(MeshData mesh, Texture2D texture)
    {
        MeshFilter.sharedMesh = mesh.CreateMesh();
        MeshRenderer.sharedMaterial.mainTexture = texture;
    }


}
