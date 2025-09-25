using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PrizeWheel : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private RectTransform wheel;   // el círculo que gira
    [SerializeField] private Button spinButton;     // botón “Girar”
    [SerializeField] private Button closeButton;
    [SerializeField] private RectTransform wedgesRoot; // opcional: contenedor de cuñas generadas

    [Header("Segmentos")]
    [SerializeField] private List<WheelSegment> segments = new List<WheelSegment>();

    [Header("Spin")]
    [SerializeField] private int minFullTurns = 3;
    [SerializeField] private int maxFullTurns = 6;
    [SerializeField] private float spinDuration = 3f;
    [SerializeField, Range(0f, 0.5f)] private float randomInsideSegment = 0.25f; // aleatorio dentro de la cuña
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1); // puedes cambiar a ease-out

    [Header("Generar visual (opcional)")]
    [SerializeField] private bool autoGenerateWedges = true;
    [SerializeField] private Sprite circleSprite; // usa "UI/Sprite" o círculo; para Image Filled Radial
    [SerializeField] private Vector2 wedgesPadding = new Vector2(0, 0); // padding si quieres

    [Header("Etiquetas")]
    [SerializeField] private bool showLabels = true;
    [SerializeField] private Font labelFont;          // si lo dejas vacío usa Arial builtin
    [SerializeField] private int labelFontSize = 34;
    [SerializeField, Range(0.2f, 0.6f)] private float labelRadius = 0.35f; // 0 = centro, 0.5 = borde
    [SerializeField] private bool autoTextContrast = true;

    [Header("Calibración visual")]
    [SerializeField] private float pointerOffsetDeg = 0f; // si tu flecha no está EXACTAMENTE arriba
    [SerializeField] private bool wedgesClockwise = true;

    [Header("Costo por giro")]
    [SerializeField] private int spinCost = 1;

    [SerializeField] private ToastServiceMB toasts;

    // Player (auto-bind estilo HeartsUI)
    private PlayerWallet wallet;
    private Health playerHealth;

    private bool spinning = false;
    private float SegAngle => 360f / Mathf.Max(1, segments.Count);

    private void Awake()
    {
        if (spinButton) spinButton.onClick.AddListener(OnSpinClicked);
    }

    private void OnEnable()
    {
        if (wallet == null || playerHealth == null)
            StartCoroutine(WaitAndBindPlayer());
    }

    private IEnumerator WaitAndBindPlayer()
    {
        while (wallet == null || playerHealth == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null)
            {
                wallet = go.GetComponentInChildren<PlayerWallet>();
                playerHealth = go.GetComponentInChildren<Health>();
                if (wallet != null && playerHealth != null) break;
            }
            yield return null;
        }
    }

    private void Start()
    {
        if (autoGenerateWedges) GenerateWedgesUI();
    }

    private void OnDestroy()
    {
        if (spinButton) spinButton.onClick.RemoveListener(OnSpinClicked);
    }

    private void OnSpinClicked()
    {
        if (spinning || segments.Count == 0 || wheel == null) return;

        // --- NUEVO: asegurar wallet y descontar ---
        if (wallet == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) wallet = go.GetComponentInChildren<PlayerWallet>();
        }

        if (wallet == null)
        {
            Debug.LogWarning("[PrizeWheel] No hay PlayerWallet para descontar el giro.");
            return; // si prefieres que gire gratis cuando no hay wallet, quita este return
        }

        if (!wallet.SpendCoinsServer(spinCost))
        {
            Debug.LogWarning($"[PrizeWheel] Monedas insuficientes. Necesitas {spinCost}.");
            toasts.Enqueue("Monedas insuficientes", 1.6f, ToastType.Error);
            return; // bloquea el giro si no alcanza
        }
        // --- FIN NUEVO ---

        int index = WeightedRoll();
        StartCoroutine(SpinToIndex(index));
    }

    private int WeightedRoll()
    {
        int total = 0;
        foreach (var s in segments) total += Mathf.Max(1, s.weight);
        int roll = Random.Range(0, total);
        int acc = 0;
        for (int i = 0; i < segments.Count; i++)
        {
            acc += Mathf.Max(1, segments[i].weight);
            if (roll < acc) return i;
        }
        return segments.Count - 1;
    }

    private IEnumerator SpinToIndex(int index)
    {
        spinning = true;
        if (spinButton) spinButton.interactable = false;

        closeButton.interactable = false ;

        float randInside = Random.Range(-SegAngle * 0.5f * randomInsideSegment,
                                         SegAngle * 0.5f * randomInsideSegment);
        float center = GetSegmentCenterDeg(index) + randInside;

        float currentZ = Normalize360(wheel.eulerAngles.z);
        int fullTurns = Random.Range(minFullTurns, maxFullTurns + 1);

        // Queremos que el centro de la cuña quede justo en el TOP (0° + pointerOffset)
        float targetZ = currentZ - fullTurns * 360f - (center - pointerOffsetDeg);

        float t = 0f, startZ = currentZ;
        while (t < spinDuration)
        {
            t += Time.deltaTime;
            float k = ease.Evaluate(Mathf.Clamp01(t / spinDuration));
            float z = Mathf.Lerp(startZ, targetZ, k);
            wheel.rotation = Quaternion.Euler(0, 0, z);
            yield return null;
        }
        wheel.rotation = Quaternion.Euler(0, 0, targetZ);

        // Verificación visual (debug): ¿qué cuña quedó bajo el puntero?
        int visualIdx = GetVisualIndexAtPointer();
        if (visualIdx != index)
        {
            Debug.LogWarning($"[PrizeWheel] Desfase visual: lógico={index}, visual={visualIdx}. " +
                             $"Ajusta wedgesClockwise/pointerOffsetDeg.");
            index = visualIdx; // aplica el premio según lo que se ve
        }

        ApplyReward(segments[index]);

        spinning = false;

        closeButton.interactable = true;
        if (spinButton) spinButton.interactable = true;
    }

    private float Normalize360(float ang)
    {
        ang %= 360f;
        if (ang < 0f) ang += 360f;
        return ang;
    }

    private void ApplyReward(WheelSegment seg)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        switch (seg.reward)
        {
            case RewardType.Coins:
                PlayerWallet wallet = player.GetComponentInChildren<PlayerWallet>();
                wallet.AddCoinsServer(1);
                toasts.Enqueue("+1 Moneda", 1.9f, ToastType.Success);
                break;
            case RewardType.Heal:
                Health health = player.GetComponentInChildren<Health>();
                health.HealServer(1);
                toasts.Enqueue("+1 Vida", 1.9f, ToastType.Success);
                break;
            case RewardType.Speed:
                Movement movement = player.GetComponentInChildren<Movement>();
                movement.IncrementSpeed(0.025f);
                toasts.Enqueue("Mayor velocidad", 1.9f, ToastType.Success);
                break;
            case RewardType.Range:
                AttackController attack = player.GetComponentInChildren<AttackController>();
                attack.IncrementBulletLifetime(0.15f);
                toasts.Enqueue("Mayor rango", 1.9f, ToastType.Success);
                break;
            case RewardType.NoCoins:
                PlayerWallet nowallet = player.GetComponentInChildren<PlayerWallet>();
                nowallet.SpendCoinsServer(1);
                toasts.Enqueue("-1 Moneda", 1.9f, ToastType.Info);
                break;
            case RewardType.NoHeal:
                Health nohealth = player.GetComponentInChildren<Health>();
                nohealth.ApplyDamageServer(1);
                toasts.Enqueue("-1 Vida", 1.9f, ToastType.Info);
                break;
            case RewardType.NoSpeed:
                Movement nomovement = player.GetComponentInChildren<Movement>();
                nomovement.DecrementSpeed(0.025f);
                toasts.Enqueue("Menor velocidad", 1.9f, ToastType.Info);
                break;
            case RewardType.NoRange:
                AttackController noattack = player.GetComponentInChildren<AttackController>();
                noattack.DecrementBulletLifetime(0.15f);
                toasts.Enqueue("Menor rango", 1.9f, ToastType.Info);
                break;
            default:
                // Nada
                break;
        }
        Debug.Log($"[PrizeWheel] Premio: {seg.name} ({seg.reward}) x{seg.amount}");
    }

    // ---------- Generación visual opcional de cuñas ----------

    private void GenerateWedgesUI()
    {
        if (wheel == null) { Debug.LogError("[PrizeWheel] wheel NO asignado."); return; }
        if (segments == null || segments.Count == 0) { Debug.LogWarning("[PrizeWheel] No hay segments definidos."); return; }

        if (wedgesRoot == null) { wedgesRoot = wheel; }
        else if (wedgesRoot.parent != wheel) { wedgesRoot.SetParent(wheel, false); }

        var parentRT = wedgesRoot;
        parentRT.anchorMin = Vector2.zero;
        parentRT.anchorMax = Vector2.one;
        parentRT.offsetMin = Vector2.zero;
        parentRT.offsetMax = Vector2.zero;

        // limpiar
        for (int i = wedgesRoot.childCount - 1; i >= 0; i--)
            Destroy(wedgesRoot.GetChild(i).gameObject);

        float segAngle = 360f / segments.Count;
        float fill = 1f / segments.Count;

        // tamaño para colocar etiquetas
        var rect = parentRT.rect;
        float r = Mathf.Min(rect.width, rect.height) * labelRadius; // píxeles de radio

        for (int i = 0; i < segments.Count; i++)
        {
            var seg = segments[i];

            // --- CUÑA ---
            var go = new GameObject($"Wedge_{i}_{seg.name}", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(wedgesRoot, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.localRotation = Quaternion.Euler(0, 0, -i * segAngle);

            var img = go.GetComponent<Image>();
            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Radial360;
            img.fillOrigin = (int)Image.Origin360.Top;
            img.fillClockwise = true;                 // <- importante
            img.fillAmount = 1f / segments.Count;
            img.sprite = circleSprite;
            img.preserveAspect = true;

            var c = seg.color; c.a = 1f;              // color visible
            img.color = c;

            // y la rotación de cada cuña:
            rt.localRotation = Quaternion.Euler(0, 0, -i * segAngle);  // -i (horario)

            // --- ETIQUETA (opcional) ---
            if (showLabels)
            {
                var txtGO = new GameObject("Label", typeof(RectTransform), typeof(UnityEngine.UI.Text));
                txtGO.transform.SetParent(go.transform, false);

                var tr = txtGO.GetComponent<RectTransform>();
                tr.anchorMin = tr.anchorMax = new Vector2(0.5f, 0.5f);
                tr.sizeDelta = new Vector2(0, 0);

                // coloca el texto a lo largo del radio de la cuña (hacia afuera)
                tr.anchoredPosition = new Vector2(0f, r);

                // cancela la rotación del padre para que el texto quede "derecho"
                tr.localRotation = Quaternion.Euler(0, 0, i * segAngle);

                var txt = txtGO.GetComponent<UnityEngine.UI.Text>();
                txt.text = BuildLabel(segments[i]);      // ej. "Coins\n+25"
                txt.alignment = TextAnchor.MiddleCenter;
                txt.fontSize = labelFontSize;
                txt.font = labelFont ? labelFont : Resources.GetBuiltinResource<Font>("Arial.ttf");
                txt.raycastTarget = false;
                

                // color legible según fondo
                txt.color = autoTextContrast ? AutoTextColor(c) : Color.white;
            }
        }

        Debug.Log("[PrizeWheel] Cuñas generadas OK.");
    }

    private string BuildLabel(WheelSegment seg)
    {
        // Ajusta a tu gusto:
        switch (seg.reward)
        {
            case RewardType.Coins: return $"{seg.name} +{seg.amount}";
            case RewardType.Heal: return $"{seg.name} +{seg.amount}";
            default: return seg.name;
        }
    }

    private Color AutoTextColor(Color bg)
    {
        // luminancia perceptual para decidir blanco/negro
        float L = 0.2126f * bg.r + 0.7152f * bg.g + 0.0722f * bg.b;
        return (L > 0.55f) ? Color.black : Color.white;
    }

    private float GetSegmentCenterDeg(int i)
    {
        float half = SegAngle * 0.5f;
        return wedgesClockwise ? (i * SegAngle + half) : (-i * SegAngle - half);
    }

    // índice de la cuña que está bajo el puntero actualmente
    private int GetVisualIndexAtPointer()
    {
        float z = Normalize360(wheel.eulerAngles.z);
        // si la rueda tiene z positiva, el gráfico “sube” CCW; usamos -z para convertir a “desde Top”
        float atTop = Normalize360(-z + pointerOffsetDeg);
        int idx = Mathf.FloorToInt(atTop / SegAngle) % segments.Count;
        if (idx < 0) idx += segments.Count;
        // si invertiste sentido:
        if (!wedgesClockwise) idx = (segments.Count - 1 - idx + segments.Count) % segments.Count;
        return idx;
    }
}
