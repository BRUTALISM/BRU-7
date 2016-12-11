using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UniRx;

public class MeshPacker : MonoBehaviour
{
	#region Editor public fields

	public int meshesPerPack = 2;

	#endregion

	#region Public properties

	public IObservable<List<GameObject>> GeneratedChildren { get { return generatedChildrenSubject; } }

	#endregion

	#region Private fields

	private List<PartialMesh> partialMeshes = new List<PartialMesh>();

	private List<GameObject> children = new List<GameObject>();

	private Subject<List<GameObject>> generatedChildrenSubject = new Subject<List<GameObject>>();

	private int meshesPerSculpture = 0;

	#endregion

	#region Unity methods

	void Start()
	{
		GetComponent<Hull>().HulledPartialMeshes.Subscribe(PackMesh).AddTo(this);

		meshesPerSculpture = GetComponent<CloudGenerator>().InitialBatches;
	}

	#endregion

	#region Mesh packing

	private void PackMesh(PartialMesh newPartialMesh)
	{
		partialMeshes.Add(newPartialMesh);

		if (partialMeshes.Count >= meshesPerPack)
		{
			var generatedChildren = new List<GameObject>();
			for (int i = 0; i < partialMeshes.Count - 1; i += 2)
			{
				var meshBuilder = new MeshBuilder();
				meshBuilder.Pack(partialMeshes[i]);
				meshBuilder.Pack(partialMeshes[i + 1]);

				var meshes = meshBuilder.Build();
				foreach (var mesh in meshes) mesh.RecalculateNormals();

				generatedChildren.AddRange(CreateChildrenForMeshes(meshes, this.gameObject));
			}
			
			partialMeshes.Clear();
			children.AddRange(generatedChildren);

			// Clear old children (FIFO)
			if (children != null && children.Count > meshesPerSculpture)
			{
				var toBeDestroyed = children.Take(children.Count - meshesPerSculpture);
				foreach (var child in toBeDestroyed) Destroy(child);
				children = children.Except(toBeDestroyed).ToList();
			}

			generatedChildrenSubject.OnNext(generatedChildren);
		}
	}

	public List<GameObject> CreateChildrenForMeshes(IEnumerable<Mesh> meshes, GameObject parent,
		System.Action<GameObject> onEachChild = null)
	{
		var generatedChildren = new List<GameObject>();
		foreach (var mesh in meshes)
		{
			GameObject meshGameObject = new GameObject(string.Format("Mesh 0x{0}", mesh.GetHashCode().ToString("x8")));
			meshGameObject.transform.SetParent(parent.transform, false);

			var meshRenderer = meshGameObject.AddComponent<MeshRenderer>();
			meshRenderer.receiveShadows = false;
			meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

			meshGameObject.AddComponent<MeshFilter>().sharedMesh = mesh;

			if (onEachChild != null) onEachChild(meshGameObject);

			generatedChildren.Add(meshGameObject);
		}

		return generatedChildren;
	}

	#endregion
}
