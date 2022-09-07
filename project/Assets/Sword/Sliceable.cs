using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EzySlice;
using System;
using System.Threading.Tasks;
using Lean.Pool;

[RequireComponent(typeof(Mover))]
public class Sliceable : MonoBehaviour
{
    public GameObject[] SliceObjectRecursive(Vector3 position, Vector3 normal, GameObject obj, Material crossSectionMaterial) {

		// finally slice the requested object and return
        SlicedHull finalHull = obj.Slice(position, normal, crossSectionMaterial);

		if (finalHull != null) {
			GameObject lowerParent = finalHull.CreateLowerHull(obj, crossSectionMaterial);
			GameObject upperParent = finalHull.CreateUpperHull(obj, crossSectionMaterial);

			if (obj.transform.childCount > 0) {
				foreach (Transform child in obj.transform) {
					if (child != null && child.gameObject != null) {

						// if the child has chilren, we need to recurse deeper
						if (child.childCount > 0) {
                            GameObject[] children = SliceObjectRecursive(position, normal, child.gameObject, crossSectionMaterial);

							if (children != null) {
								// add the lower hull of the child if available
								if (children[0] != null && lowerParent != null) {
									children[0].transform.SetParent(lowerParent.transform, false);
								}

								// add the upper hull of this child if available
								if (children[1] != null && upperParent != null) {
									children[1].transform.SetParent(upperParent.transform, false);
								}
							}
						}
						else {
							// otherwise, just slice the child object
                            SlicedHull hull = child.gameObject.Slice(position, normal, crossSectionMaterial);

							if (hull != null) {
								GameObject childLowerHull = hull.CreateLowerHull(child.gameObject, crossSectionMaterial);
								GameObject childUpperHull = hull.CreateUpperHull(child.gameObject, crossSectionMaterial);

								// add the lower hull of the child if available
								if (childLowerHull != null && lowerParent != null) {
									childLowerHull.transform.SetParent(lowerParent.transform, false);
								}

								// add the upper hull of the child if available
								if (childUpperHull != null && upperParent != null) {
									childUpperHull.transform.SetParent(upperParent.transform, false);
								}
							}
						}
					}
				}
			}

			return new GameObject[] {lowerParent, upperParent};
		}

		return null;
	}
    [SerializeField] Material crossSectionMaterial;
	[SerializeField] GameObject sliceableMesh;

    [SerializeField] float sliceExplosionForce = 175f;
	[SerializeField] float sliceExplosionRadius = 1f;
	[SerializeField] float upwardsModifier = 0.3f;

	[SerializeField] float maxDistanceFromPlaneForContinuousSlice = 1;
	[SerializeField] float accuracyZoneRadius = .15f;

	[SerializeField] float fragmentFade = 0.3f;
	[SerializeField] float fragmentAngularDrag = 5;
	[SerializeField, Range(0,1)] float fragmentVelocityTransfer = .5f;
	[SerializeField] float slashForceMultiplier = 20f;

	PointSystem pointSystem;

	SFXSystem sfxSystem;

	Matrix4x4 collisionEnterMatrix;

	Transform debrisParent;
	int ghostLayer;

	Mover mover;

	[SerializeField] MapSystem.NoteType noteType;
	public Note note;
	[SerializeField] bool isAll = false;


	private void Start()
	{
		debrisParent = GameObject.FindGameObjectWithTag("Debris").GetComponent<Transform>();
		ghostLayer = LayerMask.NameToLayer("Ghost");
		mover = GetComponent<Mover>();

		pointSystem = GameObject.FindGameObjectWithTag("Point System").GetComponent<PointSystem>();
		sfxSystem = GameObject.FindGameObjectWithTag("SFX System").GetComponent<SFXSystem>();
	}

	private int CalculateMaxPastPosAngleOnPlane(UnityEngine.Plane plane, Vector3[] pastPositions, Vector3 enterHandle, Vector3 exitTip, int maxAngle, int maxPoints)
	{
		//Reverse positions so that the latest positions show up first.
		Array.Reverse(pastPositions);

		int finalIndex = -1;
		float finalAngle = 0;
		for (int i = 0; i < pastPositions.Length; i++)
		{
			if (Mathf.Abs(plane.GetDistanceToPoint(pastPositions[i])) < maxDistanceFromPlaneForContinuousSlice)
			{
				Vector3 currentIndex = plane.ClosestPointOnPlane(pastPositions[i]);
				float currentIndexAngle = Vector3.Angle(currentIndex - enterHandle, exitTip - enterHandle);
				if (finalIndex == -1 || currentIndexAngle > finalAngle)
				{
					finalIndex = i;
					finalAngle = currentIndexAngle;
				}
			}
			else break;
		}
		if (finalIndex != -1)
		{
			return Mathf.RoundToInt(Mathf.Clamp01(finalAngle / maxAngle) * maxPoints);
		}
		return 0;
	}
	private void OnTriggerEnter(Collider other)
	{
        if (other.gameObject.tag == "Block Miss Collider" && sliced == false)
        {
            if (noteType == MapSystem.NoteType.Bomb) return;
			pointSystem.logMiss();
			sfxSystem.playMissAudio();
        }
    }
	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.GetComponent<Slicer>() == null) return;
		collisionEnterMatrix = Matrix4x4.TRS(this.transform.position, this.transform.rotation, this.transform.localScale);
	}

	bool sliced = false;
	private void OnEnable()
	{
		sliced = false;
	}
	public void slice(Vector3 enterHandle, Vector3 enterTip, Vector3 exitTip, Vector3[] pastPositions, Func<Task<Vector3[]>> getPositionsFromNow, Vector3[] collisionEnterPoints, Vector3[] collisionExitPoints, string otherTag){
		if (sliced) return;
		sliced = true;
		sfxSystem.playSliceAudio();
		//Maintain world space copies of variables.
		Vector3 worldSpaceEnterHandle = enterHandle;
		Vector3 worldSpaceEnterTip = enterTip;
		Vector3 worldSpaceExitTip = exitTip;

		//Transform all inputs from world space to local space.
		enterHandle = collisionEnterMatrix.inverse.MultiplyPoint3x4(enterHandle);
		enterTip = collisionEnterMatrix.inverse.MultiplyPoint3x4(enterTip);
		exitTip = transform.InverseTransformPoint(exitTip);
		for (int i = 0; i < pastPositions.Length; i++) {
			pastPositions[i] = transform.InverseTransformPoint(pastPositions[i]);
		}
		for (int i = 0; i < collisionEnterPoints.Length; i++)
		{
			collisionEnterPoints[i] = transform.InverseTransformPoint(collisionEnterPoints[i]);
		}
		for (int i = 0; i < collisionExitPoints.Length; i++)
		{
			collisionExitPoints[i] = transform.InverseTransformPoint(collisionExitPoints[i]);
		}
		//Calculate normal of slice.
		Vector3 normal = Vector3.Cross(enterHandle - enterTip, enterHandle - exitTip).normalized;
		//Vector3 worldSpaceNormal = Vector3.Cross(worldSpaceEnterHandle - worldSpaceEnterTip, worldSpaceEnterHandle - worldSpaceExitTip).normalized;

		//Check if slice is valid.
		//** Note that in object space a correct slice is downwards **
		bool isValid = true;

        float xOffset = exitTip.x - enterTip.x;
		float yOffset = exitTip.y - enterTip.y;
		if (Mathf.Abs(yOffset) < 0.01f || Mathf.Abs(xOffset) < 0.01f)
			return;
		if (!isAll && (yOffset > 0.6f || (Mathf.Abs(xOffset) -1f) > Mathf.Abs(yOffset))) 
			isValid = false; 
		if (noteType == MapSystem.NoteType.LeftNote && otherTag == "Right Sword") isValid = false;
		if (noteType == MapSystem.NoteType.RightNote && otherTag == "Left Sword") isValid = false;
		if (noteType == MapSystem.NoteType.Bomb) isValid = false;
            //Calculate point score.
            if (isValid) {
			int addedPoints = 0;

			//Add slice accuracy points.
			float averageDistanceFromCenter = 0;
			foreach (Vector3 point in collisionEnterPoints) {
				averageDistanceFromCenter += Mathf.Abs(point.x);
			}
			foreach (Vector3 point in collisionExitPoints)
			{
				averageDistanceFromCenter += Mathf.Abs(point.x);
			}
			averageDistanceFromCenter /= (collisionEnterPoints.Length + collisionExitPoints.Length);

			averageDistanceFromCenter = averageDistanceFromCenter * Mathf.Sqrt(averageDistanceFromCenter); //Creating a curved falloff curve + making getting points easier
			
			addedPoints += Mathf.RoundToInt(((Mathf.Clamp01(averageDistanceFromCenter/ accuracyZoneRadius)-1)*-1)*15);
			//Add enter and exit angle points.
			UnityEngine.Plane plane = new UnityEngine.Plane();
			plane.SetNormalAndPosition(normal, enterHandle);
			addedPoints += CalculateMaxPastPosAngleOnPlane(plane,pastPositions,enterHandle, exitTip, 100, 70);
            getPositionsFromNow.Invoke().ContinueWith((getPosTask) => {
				UnityMainThreadDispatcher.Instance().Enqueue(() => { 
					Vector3[] futurePositions = getPosTask.Result;
					addedPoints += CalculateMaxPastPosAngleOnPlane(plane, pastPositions, enterHandle, exitTip, 60, 30);
					pointSystem.logPoints(addedPoints, note);
				});
			});
		}
		else
		{
			if (noteType == MapSystem.NoteType.Bomb) pointSystem.logBombHit();
			else pointSystem.logBadCut();
			sfxSystem.playBadCutAudio();
		}

		//Slice mesh and add physics to fragments.
		//Ezy-Slice uses world space for slice planes!
		GameObject[] fragments = SliceObjectRecursive(worldSpaceExitTip, normal, sliceableMesh, crossSectionMaterial);
		if (fragments == null) {
            fragments = SliceObjectRecursive(transform.position, normal, sliceableMesh, crossSectionMaterial);
            if (fragments == null)
            {
				Destroy(gameObject);
                return;
            }
        }
        
        Vector3 centerPosition = Vector3.zero;
        foreach(GameObject fragment in fragments){
			fragment.transform.SetParent(transform, false); //readjust position
			fragment.transform.SetParent(debrisParent);
			fragment.layer = ghostLayer;

			fragment.AddComponent<FragmentManager>().startFadeWithLifespanRecursive(fragmentFade);

            MeshCollider fragmentCollider = fragment.AddComponent<MeshCollider>();
            fragmentCollider.convex = true;
            centerPosition += fragment.transform.position;
        }
        centerPosition /= fragments.Length;
        foreach(GameObject fragment in fragments){
            fragment.AddComponent<Rigidbody>();
            Rigidbody rb = fragment.GetComponent<Rigidbody>();
			rb.angularDrag = fragmentAngularDrag;
			rb.AddForce(0, 0, rb.mass * mover.currentVelocity.z * fragmentVelocityTransfer, ForceMode.Impulse);
            rb.AddExplosionForce(sliceExplosionForce, centerPosition, sliceExplosionRadius,upwardsModifier);
			rb.AddForce((exitTip - enterTip).normalized * slashForceMultiplier);
			//StartCoroutine(forceAxisVelocityAfterForces(Axis.Z, mover.currentVelocity.z, rb));
        }
		LeanPool.Despawn(gameObject);
    }
	/*
	 * Ignore, this is if you want the blocks' forward movement to be not affected by slices
	IEnumerator forceAxisVelocityAfterForces(Axis axis, float velocity, Rigidbody rb)
	{
		yield return null;
		float x = 0, y = 0, z = 0;

		switch (axis)
		{
			case Axis.X:
				x = velocity - rb.velocity.x;
				break;
			case Axis.Y:
                y = velocity - rb.velocity.y;
                break;
			case Axis.Z:
				z = velocity - rb.velocity.z;
                break;
        }
        rb.AddForce(new Vector3(x* rb.mass,y * rb.mass,z * rb.mass), ForceMode.Impulse);
    }

	enum Axis
	{
		X,
		Y,
		Z
	}
	*/
}