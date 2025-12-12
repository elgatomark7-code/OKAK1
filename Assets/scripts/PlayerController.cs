using UnityEngine;

public class PlayerController : MonoBehaviour
{
[Header("Movement Settings")]
    public float rotationSpeed = 100f;
    public float jumpForce = 8f;
    public float moveSpeed = 5f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody rb;
    private bool isGrounded;

    // Для отслеживания угла поворота
    private float currentXRotation = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Инициализируем текущий угол
        currentXRotation = transform.eulerAngles.x;
    }

    void Update()
    {
        // Проверка на земле ли персонаж
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundLayer);

        // Управление поворотом по оси X (A/D или стрелки)
        float rotateInput = Input.GetAxis("Horizontal") * rotationSpeed * Time.deltaTime;

        // Обновляем угол поворота
        currentXRotation += rotateInput;

        // Применяем поворот только по X
        transform.rotation = Quaternion.Euler(currentXRotation, transform.eulerAngles.y, transform.eulerAngles.z);

        // Прыжок (Пробел) - прыгаем ПЕРПЕНДИКУЛЯРНО ногам игрока
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            // Вычисляем направление прыжка на основе угла поворота
            // Когда поворот = 0 (стоит вертикально) - прыжок вверх (0, 1, 0)
            // Когда поворот = 90 (лежит горизонтально) - прыжок вперед (0, 0, 1)

            float angleRad = currentXRotation * Mathf.Deg2Rad;
            float jumpX = Mathf.Sin(angleRad) * jumpForce; // Горизонтальная составляющая
            float jumpY = Mathf.Cos(angleRad) * jumpForce; // Вертикальная составляющая

            // Создаем вектор силы прыжка
            Vector3 jumpDirection = new Vector3(0, jumpY, jumpX);

            // Преобразуем в глобальные координаты если нужно
            Vector3 worldJumpDirection = transform.TransformDirection(jumpDirection);

            // Применяем силу прыжка
            rb.AddForce(worldJumpDirection, ForceMode.Impulse);
        }

        // Движение вперед/назад (W/S) - всегда вдоль оси Z (вперед)
        float moveInput = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;
        transform.Translate(0, 0, moveInput);
    }

    void FixedUpdate()
    {
        // Для более плавной физики можно использовать FixedUpdate для движения
    }
}