using UnityEngine;
using System.Collections.Generic;
using UniRx;

public class MeshPacker : MonoBehaviour
{
	#region Editor public fields

	public int WindowSize = 10;
	public Material MeshMaterial;

	#endregion

	#region Public properties
	#endregion

	#region Private fields

	private Queue<PartialMesh> partialMeshes = new Queue<PartialMesh>();

	private List<GameObject> children = null;

	#endregion

	#region Unity methods

	void Start()
	{
		GetComponent<Hull>().Hulled.Subscribe(PackMesh).AddTo(this);
	}

	#endregion

	#region Mesh packing

	private void PackMesh(PartialMesh newPartialMesh)
	{
		partialMeshes.Enqueue(newPartialMesh);

		if (partialMeshes.Count > WindowSize) partialMeshes.Dequeue();

		var meshBuilder = new MeshBuilder();
		foreach (var partialMesh in partialMeshes) meshBuilder.Pack(partialMesh);

		var meshes = meshBuilder.Build();
		foreach (var mesh in meshes) mesh.RecalculateNormals();

		if (children != null)
		{
			foreach (var child in children) Destroy(child);
		}

		children = CreateChildrenForMeshes(meshes, this.gameObject, MeshMaterial);
	}

	public List<GameObject> CreateChildrenForMeshes(IEnumerable<Mesh> meshes, GameObject parent, Material material,
		System.Action<GameObject> onEachChild = null)
	{
		var generatedChildren = new List<GameObject>();
		foreach (var mesh in meshes)
		{
			GameObject meshGameObject = new GameObject(string.Format("Mesh 0x{0}", mesh.GetHashCode().ToString("x8")));
			meshGameObject.transform.SetParent(parent.transform, false);

			var meshRenderer = meshGameObject.AddComponent<MeshRenderer>();
			meshRenderer.sharedMaterial = material;
			meshRenderer.receiveShadows = false;
			meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

			meshGameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
			meshGameObject.AddComponent<MeshCollider>().sharedMesh = mesh;

			if (onEachChild != null) onEachChild(meshGameObject);

			generatedChildren.Add(meshGameObject);
		}

		return generatedChildren;
	}

	#endregion
}
