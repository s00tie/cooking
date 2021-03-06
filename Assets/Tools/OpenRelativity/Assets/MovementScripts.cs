using UnityEngine;
using System.Collections;

public class MovementScripts: MonoBehaviour
{
    //Consts 
    private const float SLOW_DOWN_RATE = 0.95f;
	private float acceleration = 0f;
    public float normalAcceleration = 20f;
	public float crazyAcceleration = 5f;
    private const int INIT_FRAME_WAIT = 5;
    private const float DEGREE_TO_RADIAN_CONST = 57.2957795f;
    
    //Affect our rotation speed
    public float rotSpeed;
    //Keep track of the camera transform
    public Transform camTransform;
    //Just turn this negative when they press the Y button for inversion.
    private int inverted;
    //What is our current target for the speed of light?
    public float speedOfLightTarget;
    //What is each step we take to reach that target?
    private float speedOfLightStep;
    // How long does it take the speed of light to change to target
    public float speedOfLightChangeTime = 1.0f;
    // Maximum speed of light
    public float speedOfLightMax = 30.0f;
    // Minium speed of light
    public float speedOfLightMin = 10.0f;
    //For now, you can change this how you like.
    public float mouseSensitivity;
    //So we can use getAxis as keyHit function
    public bool invertKeyDown = false;    
    //Keep track of total frames passed
    int frames;    
	//How fast are we going to shoot the bullets?
    public float viwMax = 3;
    //Gamestate reference for quick access
    GameState state;
    private bool bounce = false;
	private bool jumpedOutRel = false;
	public bool relativityAvailable = true;
	public float minRelativisticSpeed = 2.0f;

	public float speedOfLightIncrement = 1.0f;    
	public Collider parentCollider = null;
	public float nonrelativisticC = 100000000;
	public float relativisticC = 20.0f;
	private bool isRelativistic = false;
	public bool IsRelativistic {
		get { return isRelativistic; }
	}

    void Start()
    {
		//grab Game State, we need it for many actions
        state = GetComponent<GameState>();
        //Lock and hide cursor
        //Screen.lockCursor = true;
        //Screen.showCursor = false;
		//Set the speed of light to the starting speed of light in GameState
		speedOfLightTarget = (int)state.SpeedOfLight;
        //Inverted, at first
        inverted = -1;
        acceleration = normalAcceleration;
		
        viwMax = Mathf.Min(viwMax,(float)GameObject.FindGameObjectWithTag("Player").GetComponent<GameState>().MaxSpeed);
		
        frames = 0;

		ToggleSpecialRelativity(true, false);
    }
	//Again, use LateUpdate to solve some collision issues.
    void LateUpdate()
    {
	
		
		if(true)
		{
			float viewRotX = 0;
			//If we're not paused, update speed and rotation using player input.
			if(!state.MovementFrozen)
			{
				state.deltaRotation = Vector3.zero;

				//If they press the Y button, invert all Y axes
				if (Input.GetAxis("Invert Button") > 0 && !invertKeyDown)
				{
					inverted *= -1;
					invertKeyDown = true;
				}
				//And if they released it, set the invertkeydown to false.
				else if ( !(Input.GetAxis("Invert Button") > 0))
				{
					invertKeyDown = false;
				}

				#region ControlScheme
				
				//PLAYER MOVEMENT

				//If we press W, move forward, if S, backwards.
				//A adds speed to the left, D to the right. We're using FPS style controls
				//Here's hoping they work.

				//The acceleration relation is defined by the following equation
				//vNew = (v+uParallel+ (uPerpendicular/gamma))/(1+(v*u)/c^2)

				//Okay, so Gerd found a good equation that doesn't break when your velocity is zero, BUT your velocity has to be in the x direction.
				//So, we're gonna reuse some code from our relativisticObject component, and rotate everything to be at the X axis.
                
                //Cache our velocity
                Vector3 playerVelocityVector = state.PlayerVelocityVector;

				//Get our angle between the velocity and the X axis. Get the angle in degrees (radians suck)
				float rotationAroundX = DEGREE_TO_RADIAN_CONST * Mathf.Acos(Vector3.Dot(playerVelocityVector, Vector3.right) / playerVelocityVector.magnitude);
				
				//Make a Quaternion from the angle, one to rotate, one to rotate back. 
				Quaternion rotateX = Quaternion.AngleAxis(rotationAroundX, Vector3.Cross(playerVelocityVector, Vector3.right).normalized);
				Quaternion unRotateX = Quaternion.AngleAxis(rotationAroundX, Vector3.Cross(Vector3.right,playerVelocityVector).normalized);


				//If the magnitude's zero just make these angles zero and the Quaternions identity Q's
				if (playerVelocityVector.sqrMagnitude == 0)
				{
					rotationAroundX = 0;
					rotateX = Quaternion.identity;
					unRotateX = Quaternion.identity;
				}

				//Store our added velocity into temporary variable addedVelocity
				Vector3 addedVelocity = Vector3.zero;

				//Turn our camera rotation into a Quaternion. This allows us to make where we're pointing the direction of our added velocity.
				//If you want to constrain the player to just x/z movement, with no Y direction movement, comment out the next two lines
				//and uncomment the line below that is marked
				//float cameraRotationAngle = -DEGREE_TO_RADIAN_CONST * Mathf.Acos(Vector3.Dot(camTransform.forward, Vector3.forward));
				//Quaternion cameraRotation = Quaternion.AngleAxis(cameraRotationAngle, Vector3.Cross(camTransform.forward, Vector3.forward).normalized);
				
				//UNCOMMENT THIS LINE if you would like to constrain the player to just x/z movement.
				Quaternion cameraRotation = Quaternion.AngleAxis(camTransform.eulerAngles.y, Vector3.up);


				float temp;
				//Movement due to left/right input
				addedVelocity += new Vector3(0, 0, (temp = -Input.GetAxis("Vertical"))*acceleration* (float)Time.deltaTime);
				if (temp != 0)
				{
					state.keyHit = true;
				}

				addedVelocity += new Vector3((temp = -Input.GetAxis("Horizontal"))*acceleration * (float)Time.deltaTime, 0, 0);
				if (temp != 0)
				{
                    state.keyHit = true;
				}

				//And rotate our added velocity by camera angle

				addedVelocity = cameraRotation * addedVelocity;
			
				// Simple collision detection to reset velocity when hitting a wall
				RaycastHit hit;
		        Vector3 p1 = transform.position;
			
				/*if (Physics.Raycast(transform.position, transform.forward, parentCollider.bounds.extents.z) && Vector3.Dot(transform.forward, addedVelocity) < 0)
				{
					playerVelocityVector *= -0.75f;//Vector3.zero;
				}
				if (Physics.Raycast(transform.position, -transform.forward, parentCollider.bounds.extents.z) && Vector3.Dot(transform.forward, addedVelocity) > 0)
				{
					playerVelocityVector *= -0.75f;//Vector3.zero;
				}
				if (Physics.Raycast(transform.position, -transform.right, parentCollider.bounds.extents.x) && Vector3.Dot(transform.forward, addedVelocity) < 0)
				{
					playerVelocityVector *= -0.75f;//Vector3.zero;
				}
				if (Physics.Raycast(transform.position, transform.right, parentCollider.bounds.extents.x) && Vector3.Dot(transform.forward, addedVelocity) > 0)
				{
					playerVelocityVector *= -0.75f;//Vector3.zero;
				}*/
			
			/*if (bounce) {
				playerVelocityVector *= 0.75f;
				bounce = false;
			}*/

				//AUTO SLOW DOWN CODE BLOCK

				//If we are not adding velocity this round to our x direction, slow down
				/*if (addedVelocity.x == 0)
				{
					//find our current direction of movement and oppose it
					addedVelocity += new Vector3(-1*SLOW_DOWN_RATE*playerVelocityVector.x * (float)Time.deltaTime, 0, 0);
				}
				//If we are not adding velocity this round to our z direction, slow down
				if (addedVelocity.z == 0)
				{
					addedVelocity += new Vector3(0, 0, -1*SLOW_DOWN_RATE*playerVelocityVector.z * (float)Time.deltaTime);
				}
				//If we are not adding velocity this round to our y direction, slow down
				if (addedVelocity.y == 0)
				{
					addedVelocity += new Vector3(0, -1*SLOW_DOWN_RATE*playerVelocityVector.y * (float)Time.deltaTime,0);
				}*/
				
				// Fake-it Friction!!!
				bool slowingDown = false;
				if (addedVelocity.sqrMagnitude == 0){
					addedVelocity = playerVelocityVector  - (playerVelocityVector / SLOW_DOWN_RATE);
					slowingDown = true;
				}
				
				//
				/*
				 * IF you turn on this bit of code, you'll get head bob. It's a fun little effect, but if you make the magnitude of the cosine too large it gets sickening.
				if (!double.IsNaN((float)(0.2 * Mathf.Cos((float)GetComponent<GameState>().TotalTimePlayer) * Time.deltaTime)) && frames > 2)
				{
					addedVelocity.y += (float)(0.2 * Mathf.Cos((float)GetComponent<GameState>().TotalTimePlayer) * Time.deltaTime);
				}
				*/	
				//Add the velocities here. remember, this is the equation:
				//vNew = (1/(1+vOld*vAddx/cSqrd))*(Vector3(vAdd.x+vOld.x,vAdd.y/Gamma,vAdd.z/Gamma))
				if (addedVelocity.sqrMagnitude != 0)
				{
					//Rotate our velocity Vector    
					Vector3 rotatedVelocity = rotateX * playerVelocityVector;
					//Rotate our added Velocity
					addedVelocity = rotateX * addedVelocity;

					//get gamma so we don't have to bother getting it every time
					float gamma = (float)state.SqrtOneMinusVSquaredCWDividedByCSquared;
					//Do relativistic velocity addition as described by the above equation.
					rotatedVelocity = (1 / (1 + (rotatedVelocity.x * addedVelocity.x) / (float)state.SpeedOfLightSqrd)) *
						(new Vector3(addedVelocity.x + rotatedVelocity.x, addedVelocity.y * gamma, gamma * addedVelocity.z));

					//Unrotate our new total velocity
					rotatedVelocity = unRotateX * rotatedVelocity;
					
					// Clip veclocity to zero if it is low enough
					float minSpeed = 1.0f;
					if (slowingDown && rotatedVelocity.sqrMagnitude < minSpeed * minSpeed) {
						rotatedVelocity = Vector3.zero;
					}
					
					// Jump to non-relativistic rendering when moving slowly
					if (rotatedVelocity.sqrMagnitude < minRelativisticSpeed * minRelativisticSpeed && isRelativistic) {
						ToggleSpecialRelativity(true, false);
						jumpedOutRel = true;
					}
					// Jump back to relativistic rendering if we jumped out but are not moving fast enough
					if (rotatedVelocity.sqrMagnitude >= minRelativisticSpeed && !isRelativistic && jumpedOutRel)
					{
						ToggleSpecialRelativity(true, true);
						jumpedOutRel = 	false;
					}
					
					//Set it
					state.PlayerVelocityVector = rotatedVelocity;					
				}
				//CHANGE the speed of light
				
				// Toggle between relativistic and nonrelativistic (temporary)
				if (Input.GetKeyDown("t")) {
					ToggleSpecialRelativity(false, false);
				}
			  
				if (isRelativistic) {	
					//Get our input axis (DEFAULT N, M) value to determine how much to change the speed of light
					int temp2 = (int)(Input.GetAxis("Speed of Light"));
					//If it's too low, don't subtract from the speed of light, and reset the speed of light
					if(temp2<0 && speedOfLightTarget<=speedOfLightMin)
					{
						temp2 = 0;
						speedOfLightTarget = speedOfLightMin;
					}
					if (temp2 > 0 && speedOfLightTarget >= speedOfLightMax) {
						temp2 = 0;
						speedOfLightTarget = speedOfLightMax;
					}
					if(temp2!=0)
					{
						speedOfLightTarget += speedOfLightIncrement * (temp2 < 0 ? -1 : 1);		
						
						speedOfLightStep = Mathf.Abs((float)(state.SpeedOfLight - speedOfLightTarget) / speedOfLightChangeTime);
					}
					//Now, if we're not at our target, move towards the target speed that we're hoping for
					if (state.SpeedOfLight < speedOfLightTarget * .995)
					{
						//Then we change the speed of light, so that we get a smooth change from one speed of light to the next.
						state.SpeedOfLight += speedOfLightStep;
					}
					else if (state.SpeedOfLight > speedOfLightTarget * 1.005)
					{
						//See above
						state.SpeedOfLight -= speedOfLightStep;
					}
					//If we're within a +-.05 distance of our target, just set it to be our target.
					else if (state.SpeedOfLight != speedOfLightTarget)
					{
						state.SpeedOfLight = speedOfLightTarget;
					}
				}
				
				//MOUSE CONTROLS
				//Current position of the mouse
				//Difference between last frame's mouse position
				//X axis position change
				float positionChangeX = -(float)Input.GetAxisRaw("Mouse X");

				//Y axis position change
				float positionChangeY = (float)inverted * Input.GetAxisRaw("Mouse Y");


				//Use these to determine camera rotation, that is, to look around the world without changing direction of motion
				//These two are for X axis rotation and Y axis rotation, respectively
				float viewRotY = 0;
				//Take the position changes and translate them into an amount of rotation
				viewRotX = (float)(-positionChangeX * Time.deltaTime * rotSpeed * mouseSensitivity);
				//viewRotY = (float)(positionChangeY * Time.deltaTime * rotSpeed * mouseSensitivity);

				//Perform Rotation on the camera, so that we can look in places that aren't the direction of movement
                //Wait some frames on start up, otherwise we spin during the intialization when we can't see yet
				if (frames > INIT_FRAME_WAIT) 
				{
					camTransform.Rotate(new Vector3(0, viewRotX, 0), Space.World);
					if ((camTransform.eulerAngles.x + viewRotY < 90  && camTransform.eulerAngles.x + viewRotY > 90 - 180) || (camTransform.eulerAngles.x + viewRotY > 270 && camTransform.eulerAngles.x + viewRotY < 270+180))
					{
						camTransform.Rotate(new Vector3(viewRotY, 0, 0));
					}
				}
				else{
				//keep track of our frames
				frames++;                
				}

				//If we have a speed of light less than max speed, fix it.
                //This should never happen
                if (state.SpeedOfLight < state.MaxSpeed)
                {
                    state.SpeedOfLight = state.MaxSpeed;
                }


				#endregion
                
                //Send current speed of light to the shader
				Shader.SetGlobalFloat("_spdOfLight", (float)state.SpeedOfLight);

				if (Camera.main)
				{
					Shader.SetGlobalFloat("xyr", (float)Camera.main.pixelWidth / Camera.main.pixelHeight);
					Shader.SetGlobalFloat("xs", (float)Mathf.Tan(Mathf.Deg2Rad * Camera.main.fieldOfView / 2f));

                    //Don't cull because at high speeds, things come into view that are not visible normally
					//This is due to the lorenz transformation, which is pretty cool but means that basic culling will not work.
					Camera.main.layerCullSpherical = true; 
					Camera.main.useOcclusionCulling = false;
				}
				
				//This code is for an extra test level that should appear soon, to give an idea of what to do with open relativity.
				//Get mouse input for our bullet code
			/*	if(Input.GetMouseButtonDown(0))
				{
					LaunchObject();
				}*/
			}
		}
    


    }
	void LaunchObject()
    {	
		//Instantiate a new Object (You can find this object in the GameObjects folder, it's a prefab.
        GameObject launchedObject = (GameObject)Instantiate(Resources.Load("GameObjects/Bullet", typeof(GameObject)), transform.parent.position, this.transform.parent.rotation);
        //Translate it to our center, and put it so that it's just touching the ground
		launchedObject.transform.Translate((new Vector3(0, launchedObject.GetComponent<MeshFilter>().mesh.bounds.extents.y, 0) ));
		//Their velocity should be in the direction we're facing, at viwMax magnitude
        launchedObject.GetComponent<RelativisticObject>().viw = viwMax * camTransform.forward;
		//And let the object know when it was created, so that it knows when not to be seen by the player
        launchedObject.GetComponent<RelativisticObject>().SetStartTime();
    }

	public void ToggleSpecialRelativity(bool forceToggle, bool forceTo) {
		bool wasRelativistic = isRelativistic;
		if (!relativityAvailable) {
			return;
		}
		
		
		
		if (forceToggle) {
			isRelativistic = forceTo;
		}
		else {
			isRelativistic = !isRelativistic;
		}
		
		if (isRelativistic && !wasRelativistic) {
			if (state.PlayerVelocityVector.sqrMagnitude > 0.5f) {
				state.PlayerVelocityVector = state.PlayerVelocityVector * 0.25f;
			} else {
				
				state.PlayerVelocityVector = ((state.PlayerVelocityVector.sqrMagnitude < Mathf.Pow(minRelativisticSpeed, 2)) ? -transform.forward : state.PlayerVelocityVector.normalized) * Mathf.Min(minRelativisticSpeed * 2, (float)state.maxPlayerSpeed);
			}
		}
		state.SpeedOfLight = (isRelativistic ? relativisticC : nonrelativisticC);
		speedOfLightTarget = (float)state.SpeedOfLight;
		acceleration = (isRelativistic ? crazyAcceleration : normalAcceleration);
		//gameObject.SendMessage("SetCharacterType", isRelativistic);
	}

	public void HandleCollision()
	{
		bounce = true;
	}
}
