namespace FeladatRadar.frontend.Services;

/// <summary>
/// Singleton service — megtartja a fókusz időzítő állapotát navigáció között.
/// Minden komponens (Dashboard, Fokusz) ugyanezt a példányt használja.
/// </summary>
public class FocusTimerService : IDisposable
{
    // ── Beállítások ──────────────────────────────────────────────────────────
    public int FocusMinutes { get; set; } = 25;
    public int ShortBreakMinutes { get; set; } = 5;
    public int LongBreakMinutes { get; set; } = 15;
    public bool AutoSwitch { get; set; } = true;

    // ── Futó állapot ─────────────────────────────────────────────────────────
    public string Mode { get; private set; } = "focus"; // focus | short | long
    public int SecondsLeft { get; private set; }
    public bool IsRunning { get; private set; }
    public int CompletedSessions { get; private set; }
    public List<SessionEntry> SessionLog { get; } = new();

    // ── Esemény: UI frissítés kérése ─────────────────────────────────────────
    public event Action? OnTick;

    private System.Threading.Timer? _timer;

    public FocusTimerService()
    {
        SecondsLeft = FocusMinutes * 60;
    }

    // ── Csak olvasásra szánt segéd-tulajdonságok ──────────────────────────────
    public int TotalSeconds => Mode switch
    {
        "short" => ShortBreakMinutes * 60,
        "long" => LongBreakMinutes * 60,
        _ => FocusMinutes * 60
    };

    public string TimerDisplay
    {
        get
        {
            var m = SecondsLeft / 60;
            var s = SecondsLeft % 60;
            return $"{m:D2}:{s:D2}";
        }
    }

    public double DashOffset
    {
        get
        {
            var total = TotalSeconds;
            return total == 0 ? 0 : 603.0 * ((double)SecondsLeft / total);
        }
    }

    // 0..1 arány a dashboard mini-körös jelzőhöz (283 = 2πr ahol r=45)
    public double StrokeOffset
    {
        get
        {
            var total = TotalSeconds;
            return total == 0 ? 0 : 283.0 * ((double)SecondsLeft / total);
        }
    }

    public string RingColor => Mode switch
    {
        "short" => "#16a34a",
        "long" => "#7c3aed",
        _ => "#2563eb"
    };

    public string ModeLabel => Mode switch
    {
        "short" => "Rövid szünet",
        "long" => "Hosszú szünet",
        _ => "Fókusz"
    };

    // ── Műveletek ─────────────────────────────────────────────────────────────

    public void ToggleTimer()
    {
        if (IsRunning)
        {
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
            IsRunning = false;
            OnTick?.Invoke();
            return;
        }

        IsRunning = true;
        _timer?.Dispose();
        _timer = new System.Threading.Timer(_ =>
        {
            if (SecondsLeft > 0)
            {
                SecondsLeft--;
                OnTick?.Invoke();
            }
            else
            {
                _timer?.Change(Timeout.Infinite, Timeout.Infinite);
                IsRunning = false;

                var finishedMode = Mode;
                var finishedMinutes = TotalSeconds / 60;
                SessionLog.Add(new SessionEntry(finishedMode, finishedMinutes, DateTime.Now));
                if (finishedMode == "focus") CompletedSessions++;

                if (AutoSwitch)
                {
                    Mode = finishedMode == "focus" ? "short" : "focus";
                    SecondsLeft = TotalSeconds;
                    ToggleTimer(); // auto-start
                }
                else
                {
                    SecondsLeft = TotalSeconds;
                    OnTick?.Invoke();
                }
            }
        }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
    }

    public void ResetTimer()
    {
        _timer?.Change(Timeout.Infinite, Timeout.Infinite);
        IsRunning = false;
        SecondsLeft = TotalSeconds;
        OnTick?.Invoke();
    }

    public void SkipSession()
    {
        _timer?.Change(Timeout.Infinite, Timeout.Infinite);
        IsRunning = false;
        Mode = Mode == "focus" ? "short" : "focus";
        SecondsLeft = TotalSeconds;
        OnTick?.Invoke();
    }

    public void SwitchMode(string mode)
    {
        if (IsRunning) return;
        Mode = mode;
        SecondsLeft = TotalSeconds;
        OnTick?.Invoke();
    }

    public void ApplyFocusMinutes(int val)
    {
        FocusMinutes = val;
        if (!IsRunning && Mode == "focus") SecondsLeft = TotalSeconds;
        OnTick?.Invoke();
    }

    public void ApplyShortBreakMinutes(int val)
    {
        ShortBreakMinutes = val;
        if (!IsRunning && Mode == "short") SecondsLeft = TotalSeconds;
        OnTick?.Invoke();
    }

    public void ApplyLongBreakMinutes(int val)
    {
        LongBreakMinutes = val;
        if (!IsRunning && Mode == "long") SecondsLeft = TotalSeconds;
        OnTick?.Invoke();
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }

    public record SessionEntry(string Mode, int Minutes, DateTime FinishedAt);
}