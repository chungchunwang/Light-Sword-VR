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
    public void slice(Vector3 enterHandle, Vector3 enterTip, Vector3 exitTip, Vector3[] pastPositions, Func<Task<Vector3[]>> getPositionsFromNow, Vector3[] collisionEnterPoints, Vector3[] collisionExitPoints, string otherTag)
    {
        if (sliced) return; //If the object was sliced already, we do not slice it again.
        sliced = true; //Set sliced to true so that we do not slice it again.
        sfxSystem.playSliceAudio(); //Play slice sound effect.

        //Maintain world space copies of sword position variables.
        Vector3 worldSpaceEnterHandle = enterHandle;
        Vector3 worldSpaceEnterTip = enterTip;
        Vector3 worldSpaceExitTip = exitTip;

        //Transform all inputs from world space to local space (relative to the object being sliced).
        enterHandle = collisionEnterMatrix.inverse.MultiplyPoint3x4(enterHandle); //Transforms the enterHandle to the local space (relative to the position of the object when the collision first occured). Why? Because the object will have moved since the collision occured.
        enterTip = collisionEnterMatrix.inverse.MultiplyPoint3x4(enterTip); //Same thing here.
        exitTip = transform.InverseTransformPoint(exitTip); //Transforms the exitTip to the local space (relative to the position of the object currently). Why? Because this method is called when collision ends with the slicing object.
        for (int i = 0; i < pastPositions.Length; i++)
        {
            pastPositions[i] = transform.InverseTransformPoint(pastPositions[i]); //Transforms the pastPositions to the local space (relative to the position of the object currently).
        }
        for (int i = 0; i < collisionEnterPoints.Length; i++)
        {
            collisionEnterPoints[i] = collisionEnterMatrix.inverse.MultiplyPoint3x4(collisionEnterPoints[i]); //Transforms the collisionEnterPoints to the local space (relative to the position of the object when the collision first occured).
        }
        for (int i = 0; i < collisionExitPoints.Length; i++)
        {
            collisionExitPoints[i] = transform.InverseTransformPoint(collisionExitPoints[i]); //Transforms the collisionExitPoints to the local space (relative to the position of the object currently).
        }
        //Calculate normal of slice (cross product). This vector is perpendicular to the slice plane (and thus is representative of it).
        Vector3 normal = Vector3.Cross(enterHandle - enterTip, enterHandle - exitTip).normalized;


        //Check if slice is valid.
        //** Note that in object space a correct slice is downwards - horizontal slices, etc. are all just rotated. **
        bool isValid = true; //Assume slice is valid.

        float xOffset = exitTip.x - enterTip.x; //Calculate how much the sword moved horizontally.
        float yOffset = exitTip.y - enterTip.y; //Calculate how much the sword moved vertically.
        if (Mathf.Abs(yOffset) < 0.01f || Mathf.Abs(xOffset) < 0.01f) //If the sword barely moved horizontally or vertically, this slice is like a false trigger. We exit here.
            return;
        if (!isAll && (yOffset > 0.6f || (Mathf.Abs(xOffset) - 1f) > Mathf.Abs(yOffset)))  //If the sword moved upwards (by a margin) or moved more horizontally than vertically (by a margin), this slice is invalid.
            isValid = false;
        if (noteType == MapSystem.NoteType.LeftNote && otherTag == "Right Sword") isValid = false; //If the note is a left note and the sword is a right sword, this slice is invalid.
        if (noteType == MapSystem.NoteType.RightNote && otherTag == "Left Sword") isValid = false; //If the note is a right note and the sword is a left sword, this slice is invalid.
        if (noteType == MapSystem.NoteType.Bomb) isValid = false; //If the note is a bomb, this slice is invalid.

        if (isValid)
        { //If the slice is valid, we calculate the points earned for the slice.
            int addedPoints = 0; //The points earned for the slice.

            //Add slice accuracy points. This is based on how close the slice was to the center of the object. We do this based on the points where the sword entered and exited the object.
            float averageDistanceFromCenter = 0; //The average distance from the center of the object.
            foreach (Vector3 point in collisionEnterPoints)
            {
                averageDistanceFromCenter += Mathf.Abs(point.x); //Add the distance from the center of the object of all of the points in collisionEnterPoints.
            }
            foreach (Vector3 point in collisionExitPoints)
            {
                averageDistanceFromCenter += Mathf.Abs(point.x); //Add the distance from the center of the object of all of the points in collisionExitPoints.
            }
            averageDistanceFromCenter /= (collisionEnterPoints.Length + collisionExitPoints.Length); //Divide by the total number of points to get the average distance of each point from the center of the object.
            averageDistanceFromCenter = averageDistanceFromCenter * Mathf.Sqrt(averageDistanceFromCenter); //Creating a curved falloff curve for the distance, making getting points easier.

            //Get a value between 0 and 1 based on how close the slice was to the center of the object. We then invert this value so that 1 means a closer slice (this is the -1 then *-1 thing). Then, multiply by 15 to get the points earned for the slice. 15 is the maximum number of points that can be earned for slice accuracy.
            addedPoints += Mathf.RoundToInt(((Mathf.Clamp01(averageDistanceFromCenter / accuracyZoneRadius) - 1) * -1) * 15);



            //Add enter and exit angle points.
            UnityEngine.Plane plane = new UnityEngine.Plane();
            plane.SetNormalAndPosition(normal, enterHandle); //Create a plane that is representative of the slice.
                                                             //CalculateMaxPastPsAngleOnPlane: Calculate and give points based on the magnitude of the angle swung by the sword up to the collision where the sword was in line with the slice plane. 
                                                             //If other words, how much of an angle did the sword swing for this particular cut.
                                                             //100 is the maximum angle that can be earned points for. 70 is the maximum number of points that can be earned for angle.
                                                             //I wrote the function, and it features some calculations - you can check it out in the source if you want, but im omitting as it will make this snippet too long.
            addedPoints += CalculateMaxPastPosAngleOnPlane(plane, pastPositions, enterHandle, exitTip, 100, 70);
            getPositionsFromNow.Invoke().ContinueWith((getPosTask) => { //Additional points can be earned if the sword continues along the slice plane after the slice.
                UnityMainThreadDispatcher.Instance().Enqueue(() => {
                    Vector3[] futurePositions = getPosTask.Result; //Get the future positions of the sword.
                    for (int i = 0; i < futurePositions.Length; i++)
                    {
                        futurePositions[i] = transform.InverseTransformPoint(futurePositions[i]); //Transforms the furturePositions to the local space
                    }
                    addedPoints += CalculateMaxPastPosAngleOnPlane(plane, futurePositions, enterHandle, exitTip, 60, 30); //The same points calculation is done here, but with a different angle and point cap.
                    pointSystem.logPoints(addedPoints, note); //Log the points earned.
                });
            });
        }
        else //If the slice is invalid, we log the appropriate point deficit and play the appropriate sound effect.
        {
            if (noteType == MapSystem.NoteType.Bomb) pointSystem.logBombHit();
            else pointSystem.logBadCut();
            sfxSystem.playBadCutAudio();
        }

        //Slice mesh and add physics to fragments.
        //We use the package Ezy-Slice in SliceObjectRecursive to slice the meshes. However, I wrote the recursive portion (basically also slices children of the object being sliced).
        //Note that Ezy-Slice uses world space for slice planes!
        GameObject[] fragments = SliceObjectRecursive(worldSpaceExitTip, normal, sliceableMesh, crossSectionMaterial); //Slice the mesh alongside the slice plane recursively. This function is pretty cool, but I feel would make this snippet too long.
        if (fragments == null)
        {
            fragments = SliceObjectRecursive(transform.position, normal, sliceableMesh, crossSectionMaterial); //If the slice failed along the slice plane, we try again along the center of the object.
            if (fragments == null)
            {
                Destroy(gameObject); //If the slice failed again, we just destroy the object.
                return;
            }
        }

        Vector3 centerPosition = Vector3.zero;
        foreach (GameObject fragment in fragments)
        {
            fragment.transform.SetParent(transform, false); //Re-adjust the position of the fragments to be on the block.
            fragment.transform.SetParent(debrisParent); //Set the parent of the fragments to the debris parent (just an empty object that holds all the debris pieces).
            fragment.layer = ghostLayer; //Set the layer of the fragments to the ghost layer (this is so that the fragments do not collide with the sword).

            fragment.AddComponent<FragmentManager>().startFadeWithLifespanRecursive(fragmentFade); //Add a FragmentManager to the fragments. This is a script that causes the fragments to disintegrate with a shader and eventually destroys the GameObject.

            MeshCollider fragmentCollider = fragment.AddComponent<MeshCollider>(); //Add a mesh collider to the fragments.
            fragmentCollider.convex = true; //Set the mesh collider to be convex (this is required for physics).
            centerPosition += fragment.transform.position; //Add the position of the fragment to the centerPosition (this is used to calculate the approx. center of mass of the fragments).
        }
        centerPosition /= fragments.Length; //Divide the centerPosition by the number of fragments to get the approx. center of mass of the fragments.
        foreach (GameObject fragment in fragments)
        {
            fragment.AddComponent<Rigidbody>(); //For each fragment, we give it a rigidbody, which enables physics.
            Rigidbody rb = fragment.GetComponent<Rigidbody>(); //Get the rigidbody of the fragment.
            rb.angularDrag = fragmentAngularDrag; //Set the angular drag of the fragment.
            rb.AddForce(0, 0, rb.mass * mover.currentVelocity.z * fragmentVelocityTransfer, ForceMode.Impulse); //Adds an force to the fragment in the z direction of the velocity of the original object. This is so that the fragments maintain part of the momentum of the original object. Should probably allow for other axes if I want Sliceable to be generalizable, but this is ok for now.
            rb.AddExplosionForce(sliceExplosionForce, centerPosition, sliceExplosionRadius, upwardsModifier); //Adds an explosion force at the approx. center of mass to the fragment. This is so that the fragments fly apart from each other.
            rb.AddForce((exitTip - enterTip).normalized * slashForceMultiplier); //Adds a force in the direction of the slice. This is so that the fragments fly in the direction of the slice (to give the illusion that the blade had friction).
        }
        LeanPool.Despawn(gameObject); //Despawn the original (unsliced) object. We use a pool here as the blocks that are sliced are loaded multiple times throughout a game. LeanPool hides them instead of deleting them so that we don't have to instantiate and destroy blocks all the time (this saves the garbage collector from having to do work, which is good for performance).
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