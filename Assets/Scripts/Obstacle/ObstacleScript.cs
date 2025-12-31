using UnityEngine;

public class ObstacleScript : MonoBehaviour
{
    [SerializeField] private float minZPos = -5f;
    // [SerializeField] private float rotationSpeed = 5f;

    [Space]
    [SerializeField] private float sizeVariant = 0f;
    [SerializeField] protected GameObject meshObject;


    // private float _initRotation;

    // void Start()
    // {
    //     _initRotation = transform.rotation.z;
    // }

    virtual protected void Awake()
    {
        if (meshObject == null)
        {
            meshObject = this.gameObject.GetComponentInChildren<Renderer>().gameObject;
        }
    }

    void Start()
    {
        SpawnSetup();
    }

    virtual protected void Update()
    {
        MoveForward();
        SideRotation();
    }

    virtual protected void SpawnSetup()
    {
        Vector3 currentScale = meshObject.transform.localScale;
        float randScale = Random.Range(-sizeVariant, sizeVariant);

        meshObject.transform.localScale = new Vector3(currentScale.x, currentScale.y + randScale, currentScale.z);

        float randRotation = Random.Range(0f, 360f);
        meshObject.transform.localEulerAngles = new Vector3(0f, randRotation, 0f);
    }

    virtual protected void MoveForward()
    {
        transform.position -= new Vector3(0f, 0f, GameManager.Instance.runningSpeed * Time.deltaTime);

        if (transform.position.z <= minZPos)
        {
            Destroy(this.gameObject);
        }
    }

    virtual protected void SideRotation()
    {
        float sideRotation = GameManager.Instance.GetSideMovement();
        // Debug.Log(sideRotation);

        transform.Rotate(0f, 0f, sideRotation * GameManager.Instance.rotationSpeed * Time.deltaTime);
    }
}
