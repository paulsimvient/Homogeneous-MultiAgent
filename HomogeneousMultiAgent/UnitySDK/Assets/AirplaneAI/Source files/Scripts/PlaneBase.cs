using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneBase : MonoBehaviour {

    private bool initialized;

    public int team;

    public enum BaseState {
        Moving,
        Landed,
        Landing,
        TakingOff,
        Idle,
        Parked,
        Crashing
    }

    [HideInInspector]
    public BaseState currentBaseState;

    [HideInInspector]
    public Vector3 targetPos; // position where the plane is moving towards


    [HideInInspector]
    public float currentMovementSpeed;
    public float movementSpeed = 50;
    public float rotationSpeed = 50;

    [HideInInspector]
    public float currentRotation;
    [HideInInspector]
    public Vector3 previousPosition;
    [HideInInspector]
    public float previousYRot;

    public Transform planeModel;

    [HideInInspector]
    float zRot;  // tilt the plane when it turns


    [HideInInspector]
    public float gettingDistanceTimer;
    [HideInInspector]
    public bool gettingDistanceFromTarget; // to get some distance when we go too close to the target, so we can approach it again 
    [HideInInspector]
    private Vector3 gettingDistancePosition; // randomized position we're moving towards to get some distance

    public float tooCloseDistance = 30; // distance to target closer than this triggers the gettingDistance variable

    [HideInInspector]
    public int landingStep;
    [HideInInspector]
    public bool hasLandingSpot;
    [HideInInspector]
    public int takingOffStep;
    [HideInInspector]
    public Scr_Airport currentAirport;
    [HideInInspector]
    public Scr_Airport.ParkingSpot landingSpot;


    [HideInInspector]
    public BodyIntegrity bodyIntegrity;
    [HideInInspector]
    private float currentTerrainY;
    [HideInInspector]
    private Vector3 idlePosition;

    public void Initialize() {
        initialized = true;
        bodyIntegrity = GetComponent<BodyIntegrity>();
        Scr_PlaneHandler.instance.planes.Add(this);
        targetPos = transform.position;
    }

    void Landing() {

        targetPos = currentAirport.landingSpots[landingStep].position; // fly towards the airport

        if (Vector3.Distance(transform.position, targetPos) < tooCloseDistance) { // reached the target, next landing step
            if (hasLandingSpot) {
                landingStep++;
                if (landingStep > 4) { // reached the last landing step
                    Landed();
                }
            } else {
                if (gettingDistanceFromTarget == false) {
                    landingSpot = currentAirport.GetFreeSpot(this); // try to reserve a landing spot from the airport
                    if (landingSpot != null) {
                        hasLandingSpot = true; // found a free landing spot
                    } else {
                        GetDistance(); // airport is full, wait and fly in circles
                    }
                }
            }
        }
        if (landingStep >= 4) {
            currentMovementSpeed = Mathf.MoveTowards(currentMovementSpeed, 10, 5f * Time.deltaTime); // slow down
        } else {
            currentMovementSpeed = Mathf.MoveTowards(currentMovementSpeed, movementSpeed, 10f * Time.deltaTime);
        }
    }
    void Landed() {
        SetState(BaseState.Landed);
        landingStep = 0;
        transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0); //make sure the plane is straight
    }

    void GetDistance() { // got close to the target, fly in a random distance for a while to get some distance 
        gettingDistanceTimer = UnityEngine.Random.Range(1f, 5f);
        if (landingStep < 3) {
            gettingDistanceFromTarget = true;

            if (transform.position.y < 70) {// limit height not to hit the ground
                gettingDistancePosition = transform.position + new Vector3(UnityEngine.Random.Range(-500, 500), 50, UnityEngine.Random.Range(-500, 500));
            } else {
                gettingDistancePosition = transform.position + new Vector3(UnityEngine.Random.Range(-500, 500), UnityEngine.Random.Range(-50, 50), UnityEngine.Random.Range(-500, 500));
            }
        }
    }
    void TakingOff() {
        if (takingOffStep > 0) {
            currentMovementSpeed = Mathf.MoveTowards(currentMovementSpeed, movementSpeed, 10f * Time.deltaTime); // on runway or in the air, increase speed
        } else {
            currentMovementSpeed = 10;// on ground, slow speed
        }
        targetPos = currentAirport.landingSpots[4 - takingOffStep].position;
        if ((Vector3.Distance(transform.position, targetPos) < tooCloseDistance && takingOffStep > 1) || (takingOffStep <= 1 && Vector3.Distance(transform.position, targetPos) < 5)) {
            takingOffStep++;
            if (takingOffStep > 4) {
                TakeOffFinished();
            }
        }
    }

    void TakeOffFinished() {
        takingOffStep = 0;
        SetState(BaseState.Idle);
    }


    public void Death() { // start crashing
        SetState(BaseState.Crashing);
        GFXHandler.instance.CreateGFX(2, transform.position, transform);
    }

    void Crash() { // remove plane
        Scr_PlaneHandler.instance.planes.Remove(this);
        GFXHandler.instance.CreateGFX(1, transform.position);
        GameObject.Destroy(gameObject);
    }

    public void StartLanding() {
        StartLanding(Scr_AirportHandler.instance.GetClosestAirport(transform.position)); 
    }
    public void StartLanding(Scr_Airport airport) {
        if (airport != null) {
            if (currentBaseState != BaseState.Landed && currentBaseState != BaseState.Landing) {
                currentAirport = airport;
                SetState(BaseState.Landing);
                landingStep = 0;
            }
        } else {
            SetState(BaseState.Idle);//airport not found
        }
    }
    public void StartTakeoff() {
        StartTakeoff(Scr_AirportHandler.instance.GetClosestAirport(transform.position));
    }
    public void StartTakeoff(Scr_Airport airport) {
        if (currentBaseState == BaseState.Parked) {

            currentAirport = airport;
            SetState(BaseState.TakingOff);
            takingOffStep = 0;
        } else {
            Debug.Log("The plane must be landed in order to take off!");
        }
    }

    public void UpdateTick() {
        if (initialized == false) {
            Initialize();
        }

        float dist = 150;
        RaycastHit hit;
        Vector3 dir = new Vector3(0, -10, 0);
        if (Physics.Raycast(transform.position + new Vector3(0, 100, 0), dir, out hit, dist)) { // raycast from above to detect terrain collisions
            if (hit.collider.gameObject.tag == "Terrain") {
                currentTerrainY = hit.point.y;

            }
        } else {
            currentTerrainY = 0;
        }

        if (transform.position.y < currentTerrainY) {
            Crash(); // hit terrain, crash
        }


        if (currentBaseState == BaseState.Crashing) {
            MoveForward(currentMovementSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(90, 0, 0), 0.25f * Time.deltaTime);
        } else if (currentBaseState == BaseState.Landing) {
            Landing();
            MoveTowardsPosition();
        } else if (currentBaseState == BaseState.Idle) {
            currentMovementSpeed = Mathf.MoveTowards(currentMovementSpeed, movementSpeed, 10f * Time.deltaTime);
            SetTargetPosition(idlePosition);
            MoveTowardsPosition();
        } else if (currentBaseState == BaseState.TakingOff) {
            TakingOff();
            MoveTowardsPosition();
        } else if (currentBaseState == BaseState.Moving) {
            currentMovementSpeed = Mathf.MoveTowards(currentMovementSpeed, movementSpeed, 10f * Time.deltaTime);
            MoveTowardsPosition();
        } else if (currentBaseState == BaseState.Landed) {
            if (Vector3.Distance(transform.position, landingSpot.spot.position) > 2) {
                MoveForward(10);
                RotateTowardsPosition(landingSpot.spot.position); // slowly roll towards parking spot
            } else {
                SetState(BaseState.Parked); // reached parking spot
            }
        }
    }


    public void SetState(BaseState b) {
        currentBaseState = b;
        if (currentBaseState == BaseState.TakingOff) {
            landingSpot = null;
            hasLandingSpot = false;
            currentAirport.ClearLandingSpot(this); // clear the current parking spot so other planes can land
        }
        if (currentBaseState == BaseState.Idle) {
            idlePosition = transform.position;
        }
    }
    void MoveForward(float speed) {

        transform.position += transform.forward * speed * Time.deltaTime;
    }
    public void RotateTowardsPosition(Vector3 pos) {

        Quaternion oldRot = transform.rotation;
        transform.LookAt(pos);
        Quaternion desiredRot = transform.rotation;
        if (currentBaseState == BaseState.Landed || (currentBaseState == BaseState.TakingOff && takingOffStep <= 1)) {
            transform.rotation = Quaternion.RotateTowards(oldRot, desiredRot, 300 * Time.deltaTime);
        } else {
            transform.rotation = Quaternion.RotateTowards(oldRot, desiredRot, rotationSpeed * Time.deltaTime);
        }
    }

    public void SetTargetPosition(Vector3 v) {
        targetPos = new Vector3(v.x, Mathf.Clamp(v.y, currentTerrainY + 25, float.MaxValue), v.z); // clamp Y value so we're not flying into the ground
    }

    void MoveTowardsPosition() {
        MoveForward(currentMovementSpeed);

        float dist = Vector3.Distance(transform.position, targetPos);

        if (gettingDistanceTimer > 0) {
            gettingDistanceTimer -= 1 * Time.deltaTime;

            if (gettingDistanceTimer <= 0) {
                gettingDistanceFromTarget = false;
                if (dist < tooCloseDistance) {
                    GetDistance();
                }
            }
        }

        if (currentBaseState == BaseState.Moving && dist < tooCloseDistance && gettingDistanceFromTarget == false) {
            GetDistance(); // got close to target, get some distance and reapproach
        }

        if (gettingDistanceFromTarget == false) {
            RotateTowardsPosition(targetPos);
        } else {
            RotateTowardsPosition(gettingDistancePosition);
        }


        if (currentBaseState == BaseState.Moving || (currentBaseState == BaseState.Landing && landingStep < 2) || (currentBaseState == BaseState.TakingOff && takingOffStep > 1)) {
            RotateZ(); // tilt the plane
        } else {
            zRot = 0; // on ground, no tilt
        }

        planeModel.localRotation = Quaternion.Euler(0, 0, zRot);


        previousPosition = transform.position;
        previousYRot = transform.eulerAngles.y;
    }
    void RotateZ() {
        float r = Mathf.Clamp(previousYRot - transform.eulerAngles.y, -2, 2);
        if (previousYRot - transform.eulerAngles.y > 0) {
            zRot = Mathf.MoveTowards(zRot, Mathf.Abs(r) * 50, 50 * Time.deltaTime);
        } else {
            zRot = Mathf.MoveTowards(zRot, Mathf.Abs(r) * (-50), 50 * Time.deltaTime);
        }
        planeModel.localRotation = Quaternion.Euler(0, 0, zRot);
    }
}
