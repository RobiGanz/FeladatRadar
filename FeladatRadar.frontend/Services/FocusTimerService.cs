namespace FeladatRadar.frontend.Services;

/// <summary>
/// Singleton service — megtartja a fókusz időzítő állapotát navigáció között.
/// Minden komponens (Dashboard, Fokusz) ugyanezt a példányt használja.
/// </summary>
public class FocusTimerService : IDisposable
{
    // ── Beállítások ──────────────────────────────────────────────────────────
    public int FocusMinutes { get; set; } = 25;
    public int FocusSeconds { get; set; } = 0;
    public int ShortBreakMinutes { get; set; } = 5;
    public int ShortBreakSeconds { get; set; } = 0;
    public int LongBreakMinutes { get; set; } = 15;
    public int LongBreakSeconds { get; set; } = 0;
    public bool AutoSwitch { get; set; } = true;

    // ── Futó állapot ─────────────────────────────────────────────────────────
    public string Mode { get; private set; } = "focus"; // focus | short | long
    public string CurrentSessionType { get; set; } = "tanulas"; // session típus (Fokusz oldalról állítva)
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
    public int TotalSeconds => Math.Max(1, Mode switch
    {
        "short" => ShortBreakMinutes * 60 + ShortBreakSeconds,
        "long" => LongBreakMinutes * 60 + LongBreakSeconds,
        _ => FocusMinutes * 60 + FocusSeconds
    });

    public int CurrentMinutes => Mode switch
    {
        "short" => ShortBreakMinutes,
        "long" => LongBreakMinutes,
        _ => FocusMinutes
    };

    public int CurrentSeconds => Mode switch
    {
        "short" => ShortBreakSeconds,
        "long" => LongBreakSeconds,
        _ => FocusSeconds
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
            return total == 0 ? 0 : 553.0 * ((double)SecondsLeft / total);
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
                SessionLog.Add(new SessionEntry(finishedMode, finishedMinutes, DateTime.Now, CurrentSessionType));
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

    public void SwitchModeReset(string mode)
    {
        if (IsRunning) return;
        Mode = mode;
        switch (mode)
        {
            case "short":
                ShortBreakMinutes = 5;
                ShortBreakSeconds = 0;
                break;
            case "long":
                LongBreakMinutes = 15;
                LongBreakSeconds = 0;
                break;
        }
        SecondsLeft = TotalSeconds;
        OnTick?.Invoke();
    }

    public void ApplyFocusMinutes(int val)
    {
        FocusMinutes = val;
        if (!IsRunning && Mode == "focus") SecondsLeft = TotalSeconds;
        OnTick?.Invoke();
    }

    public void ApplyFocusSeconds(int val)
    {
        FocusSeconds = val;
        if (!IsRunning && Mode == "focus") SecondsLeft = TotalSeconds;
        OnTick?.Invoke();
    }

    public void ApplyShortBreakMinutes(int val)
    {
        ShortBreakMinutes = val;
        if (!IsRunning && Mode == "short") SecondsLeft = TotalSeconds;
        OnTick?.Invoke();
    }

    public void ApplyShortBreakSeconds(int val)
    {
        ShortBreakSeconds = val;
        if (!IsRunning && Mode == "short") SecondsLeft = TotalSeconds;
        OnTick?.Invoke();
    }

    public void ApplyLongBreakMinutes(int val)
    {
        LongBreakMinutes = val;
        if (!IsRunning && Mode == "long") SecondsLeft = TotalSeconds;
        OnTick?.Invoke();
    }

    public void ApplyLongBreakSeconds(int val)
    {
        LongBreakSeconds = val;
        if (!IsRunning && Mode == "long") SecondsLeft = TotalSeconds;
        OnTick?.Invoke();
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }

    public record SessionEntry(string Mode, int Minutes, DateTime FinishedAt, string SessionType = "tanulas");
}