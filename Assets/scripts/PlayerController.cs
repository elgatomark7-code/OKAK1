using UnityEngine;
public class PlayerControler : MonoBehaviour
{
    [Header("Physics")]
    public float moveTorque = 15f;
    public float maxAngularSpeed = 6f;
    public float jumpImpulse = 6f;
    public float aerialTorque = 8f; // "гребля" в воздухе

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Stabilization")]
    public float stabilizationSpeed = 5f;
    public float maxStableAngle = 30f; // не выравниваем, если сильно наклонён

    private Rigidbody rb;
    private bool isGrounded;
    private bool jumpRequested;
    private bool leftPull, rightPull;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (groundCheck == null) groundCheck = transform;
    }

    void Update()
    {
        // Ввод — только в Update!
        float h = Input.GetAxis("Horizontal");
        rb.AddTorque(Vector3.up * h * moveTorque * (isGrounded ? 1f : 0.3f)); // на земле круче

        if (Input.GetButtonDown("Jump")) jumpRequested = true;
        leftPull = Input.GetKey(KeyCode.Q);   // левое ухо → тянем влево-вниз
        rightPull = Input.GetKey(KeyCode.E);  // правое ухо → тянем вправо-вниз
    }

    void FixedUpdate()
    {
        // ——— Ground Check ———
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);

        // ——— Jump ———
        if (jumpRequested && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpImpulse, ForceMode.Impulse);
            jumpRequested = false;
        }

        // ——— Aerial Pull ("гребля ушами") ———
        if (!isGrounded)
        {
            Vector3 pullDir = Vector3.zero;
            if (leftPull) pullDir += -transform.right - transform.up;
            if (rightPull) pullDir += transform.right - transform.up;

            if (pullDir != Vector3.zero)
            {
                pullDir.Normalize();
                Vector3 leftEarPos = transform.TransformPoint(new Vector3(-0.25f, 0.5f, 0));
                Vector3 rightEarPos = transform.TransformPoint(new Vector3(0.25f, 0.5f, 0));

                rb.AddForceAtPosition(pullDir * aerialTorque * 0.5f, leftEarPos);
                rb.AddForceAtPosition(pullDir * aerialTorque * 0.5f, rightEarPos);
            }
        }

        // ——— Ограничение вращения ———
        rb.angularVelocity = Vector3.ClampMagnitude(rb.angularVelocity, maxAngularSpeed);

        // ——— Стабилизация при приземлении ———
        if (isGrounded)
        {
            // Выравниваем только вокруг Y (не мешаем "лёжа" стоять)
            Vector3 euler = rb.rotation.eulerAngles;
            float currentY = euler.y > 180 ? euler.y - 360 : euler.y;
            float targetY = Mathf.Lerp(currentY, 0f, stabilizationSpeed * Time.fixedDeltaTime);
            rb.rotation = Quaternion.Euler(0, targetY, 0);
        }
    }

    // Для отладки
    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
