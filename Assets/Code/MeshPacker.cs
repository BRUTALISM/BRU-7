using UnityEngine;
using System.Collections.Generic;
using UniRx;

public class MeshPacker : MonoBehaviour
{
	#region Editor public fields
	#endregion

	#region Public properties

	public IObservable<List<GameObject>> GeneratedChildren { get { return generatedChildrenSubject; } }

	#endregion

	#region Private fields

	private List<PartialMesh> partialMeshes = new List<PartialMesh>();

	private List<GameObject> children = new List<GameObject>();

	private int partialMeshesInWindow = 0;

	private Subject<List<GameObject>> generatedChildrenSubject = new Subject<List<GameObject>>();

	#endregion

	#region Unity methods

	void Start()
	{
		GetComponent<Hull>().HulledPartialMeshes.Subscribe(PackMesh).AddTo(this);

		// Times 2 because of mirroring
		partialMeshesInWindow = GetComponent<CloudGenerator>().InitialBatches * 2;
	}

	#endregion

	#region Mesh packing

	private void PackMesh(PartialMesh newPartialMesh)
	{
		partialMeshes.Add(newPartialMesh);

		if (partialMeshes.Count == partialMeshesInWindow)
		{
			if (children != null)
			{
				foreach (var child in children) Destroy(child);
				children.Clear();
			}

			for (int i = 0; i < partialMeshes.Count - 1; i += 2)
			{
				var meshBuilder = new MeshBuilder();
				meshBuilder.Pack(partialMeshes[i]);
				meshBuilder.Pack(partialMeshes[i + 1]);

				var meshes = meshBuilder.Build();
				foreach (var mesh in meshes) mesh.RecalculateNormals();

				children.AddRange(CreateChildrenForMeshes(meshes, this.gameObject));
			}
			
			partialMeshes.Clear();

			generatedChildrenSubject.OnNext(children);
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
