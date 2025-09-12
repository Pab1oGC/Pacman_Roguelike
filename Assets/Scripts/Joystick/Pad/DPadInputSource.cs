using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class DPadInputSource : MonoBehaviour, IMoveInputSource
{
    public enum ConflictMode { LastPressedWins, VerticalPriority, HorizontalPriority, AxisLock }

    [Header("Botones (puedes dejar vacío y se autodetectan en hijos)")]
    [SerializeField] private DPadArrowButton up;
    [SerializeField] private DPadArrowButton down;
    [SerializeField] private DPadArrowButton left;
    [SerializeField] private DPadArrowButton right;

    [Header("Resolución de conflictos")]
    [SerializeField] private ConflictMode conflictMode = ConflictMode.AxisLock;

    // estado
    readonly Dictionary<DPadDirection, bool> _pressed = new Dictionary<DPadDirection, bool>();
    readonly Dictionary<DPadDirection, float> _stamp = new Dictionary<DPadDirection, float>();
    int _lockedAxis = 0; // 0 ninguno, 1 horizontal, 2 vertical

    void Awake()
    {
        // Autoregistro si están nulos
        if (!up || !down || !left || !right)
        {
            foreach (var b in GetComponentsInChildren<DPadArrowButton>(true))
            {
                switch (b.direction)
                {
                    case DPadDirection.Up: up = b; break;
                    case DPadDirection.Down: down = b; break;
                    case DPadDirection.Left: left = b; break;
                    case DPadDirection.Right: right = b; break;
                }
            }
        }
        Subscribe(up); Subscribe(down); Subscribe(left); Subscribe(right);

        // init
        foreach (DPadDirection d in System.Enum.GetValues(typeof(DPadDirection)))
        { _pressed[d] = false; _stamp[d] = -999f; }
    }

    void Subscribe(DPadArrowButton b)
    {
        if (!b) return;
        b.OnPressChanged += (dir, isDown, pid) =>
        {
            _pressed[dir] = isDown;
            if (isDown) _stamp[dir] = Time.unscaledTime;

            // liberar lock cuando deja de haber teclas del eje
            if (conflictMode == ConflictMode.AxisLock && !AnyOnLockedAxis())
                _lockedAxis = 0;
        };
    }

    public Vector2 GetMoveInput()
    {
        // Qué ejes tienen algo
        bool anyH = _pressed[DPadDirection.Left] || _pressed[DPadDirection.Right];
        bool anyV = _pressed[DPadDirection.Up] || _pressed[DPadDirection.Down];

        int chosenAxis = 0; // 0 ninguno, 1 H, 2 V
        switch (conflictMode)
        {
            case ConflictMode.VerticalPriority:
                chosenAxis = anyV ? 2 : (anyH ? 1 : 0); break;
            case ConflictMode.HorizontalPriority:
                chosenAxis = anyH ? 1 : (anyV ? 2 : 0); break;
            case ConflictMode.AxisLock:
                if (_lockedAxis == 1 && anyH) chosenAxis = 1;
                else if (_lockedAxis == 2 && anyV) chosenAxis = 2;
                else { chosenAxis = ChooseAxisLastPressed(anyH, anyV); _lockedAxis = chosenAxis; }
                break;
            default: // LastPressedWins
                chosenAxis = ChooseAxisLastPressed(anyH, anyV); break;
        }

        if (chosenAxis == 1) return ChooseH();
        else if (chosenAxis == 2) return ChooseV();
        else return Vector2.zero;
    }

    Vector2 ChooseH()
    {
        bool L = _pressed[DPadDirection.Left], R = _pressed[DPadDirection.Right];
        if (L && R) return (_stamp[DPadDirection.Left] > _stamp[DPadDirection.Right]) ? Vector2.left : Vector2.right;
        if (L) return Vector2.left;
        if (R) return Vector2.right;
        return Vector2.zero;
    }

    Vector2 ChooseV()
    {
        bool U = _pressed[DPadDirection.Up], D = _pressed[DPadDirection.Down];
        if (U && D) return (_stamp[DPadDirection.Up] > _stamp[DPadDirection.Down]) ? Vector2.up : Vector2.down;
        if (U) return Vector2.up;
        if (D) return Vector2.down;
        return Vector2.zero;
    }

    int ChooseAxisLastPressed(bool anyH, bool anyV)
    {
        if (!anyH && !anyV) return 0;
        if (anyH && !anyV) return 1;
        if (!anyH && anyV) return 2;

        float lastH = Mathf.Max(_stamp[DPadDirection.Left], _stamp[DPadDirection.Right]);
        float lastV = Mathf.Max(_stamp[DPadDirection.Up], _stamp[DPadDirection.Down]);
        return (lastH > lastV) ? 1 : 2;
    }

    bool AnyOnLockedAxis()
    {
        if (_lockedAxis == 1) return _pressed[DPadDirection.Left] || _pressed[DPadDirection.Right];
        if (_lockedAxis == 2) return _pressed[DPadDirection.Up] || _pressed[DPadDirection.Down];
        return false;
    }
}
