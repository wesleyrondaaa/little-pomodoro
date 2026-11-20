using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using LittlePomodoro;

internal static class Tests
{
    private static void Equal<T>(T actual, T expected, string message)
    {
        if (!object.Equals(actual, expected))
            throw new Exception(message + ": expected " + expected + ", got " + actual);
    }

    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            if (args.Length == 1 && args[0] == "--launch")
            {
                using (Process child = Process.Start(new ProcessStartInfo(AppPath) { UseShellExecute = false }))
                    Console.WriteLine(child.Id);
                return 0;
            }
            TestTiming();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            TestControls();
            TestIndependentExecutable();
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error);
            return 1;
        }
    }

    private static string AppPath
    {
        get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LittlePomodoro.exe"); }
    }

    private static void TestTiming()
    {
        DateTimeOffset now = DateTimeOffset.Parse("2026-01-01T00:00:00Z", CultureInfo.InvariantCulture);
        PomodoroState state = new PomodoroState();
        Equal(state.FormattedTime, "25:00", "Default duration");
        state.Toggle(now);
        state.Update(now.AddMilliseconds(15250));
        Equal(state.FormattedTime, "24:45", "Round up partial seconds");
        state.Toggle(now.AddMilliseconds(15250));
        double paused = state.Remaining;
        state.Update(now.AddHours(1));
        Equal(state.Remaining, paused, "Paused time stays fixed");
        state.Toggle(now.AddHours(1));
        state.Update(now.AddHours(1).AddSeconds(5));
        Equal(state.FormattedTime, "24:40", "Resume preserves fractional time");
        state.SelectMode(TimerMode.Long);
        Equal(state.Running, false, "Mode switch stops timer");
        Equal(state.FormattedTime, "15:00", "Long break duration");
        Equal(state.Completed, 0, "Switch does not complete session");

        state = new PomodoroState();
        for (int i = 1; i <= 4; i++)
        {
            state.Toggle(now);
            Equal(state.Update(now.AddMinutes(25)), true, "Report completion");
            Equal(state.Completed, i, "Count focus sessions");
            Equal(state.Mode, i == 4 ? TimerMode.Long : TimerMode.Short, "Choose correct break");
            Equal(state.Running, false, "Next session waits");
            Equal(state.Update(now.AddDays(1)), false, "No repeated completion");
            state.Toggle(now);
            state.Update(now.AddMinutes(state.Duration(state.Mode)));
            Equal(state.Mode, TimerMode.Focus, "Break returns to focus");
            Equal(state.Completed, i, "Break does not count as focus");
        }
        state.Toggle(now);
        Equal(state.Toggle(now.AddDays(1)), true, "Pause at expiry completes session");
        Equal(state.Completed, 5, "Delayed callback completes exactly one session");
        Equal(state.Running, false, "Delay does not auto-start next session");
        foreach (int value in new int[] { 0, 181 })
        {
            bool rejected = false;
            try { state.ApplyDurations(25, value, 15); }
            catch (ArgumentOutOfRangeException) { rejected = true; }
            Equal(rejected, true, "Reject invalid duration");
            Equal(state.Duration(TimerMode.Focus), 25, "Invalid input leaves durations intact");
        }
        state.ApplyDurations(180, 1, 2);
        state.SelectMode(TimerMode.Focus);
        Equal(state.FormattedTime, "180:00", "Maximum duration");
        Console.WriteLine("PASS: timing, pause/resume, modes, session cycle, delayed callbacks, duration validation.");
    }

    private static T Find<T>(Form form, string name) where T : Control
    {
        return (T)form.Controls.Find(name, false)[0];
    }

    private static void TestControls()
    {
        PomodoroState state = new PomodoroState();
        using (PomodoroForm form = new PomodoroForm(state))
        {
            form.Show();
            Application.DoEvents();
            Button toggle = Find<Button>(form, "Toggle");
            toggle.PerformClick();
            Equal(state.Running, true, "Start button");
            toggle.PerformClick();
            Equal(state.Running, false, "Pause button");
            toggle.PerformClick();
            Equal(state.Running, true, "Resume button");
            Find<Button>(form, "Reset").PerformClick();
            Equal(state.Running, false, "Reset stops timer");
            Equal(Find<Label>(form, "Time").Text, "25:00", "Reset restores duration");
            Find<Button>(form, "Short").PerformClick();
            Equal(Find<Label>(form, "Time").Text, "05:00", "Short break button");
            Find<Button>(form, "Long").PerformClick();
            Equal(Find<Label>(form, "Time").Text, "15:00", "Long break button");
            for (int i = 0; i < 3; i++)
            {
                NumericUpDown input = Find<NumericUpDown>(form, "Duration" + i);
                Equal(input.Minimum, 1m, "Minimum duration");
                Equal(input.Maximum, 180m, "Maximum duration");
                input.Value = 1;
            }
            Find<Button>(form, "Apply").PerformClick();
            Equal(Find<Label>(form, "Time").Text, "01:00", "Apply custom duration");
            Find<Button>(form, "Focus").PerformClick();
            toggle.PerformClick();
            form.WindowState = FormWindowState.Minimized;
            Application.DoEvents();
            ExpireRunningTimer(state);
            Equal(state.Completed, 1, "Native timer fires completion");
            Equal(state.Mode, TimerMode.Short, "Native timer selects break");
            Equal(toggle.Text, "Start", "Native timer waits after completion");
            Equal(form.WindowState, FormWindowState.Normal, "Completion restores minimized window");

            Find<CheckBox>(form, "ShowWhenDone").Checked = false;
            toggle.PerformClick();
            form.WindowState = FormWindowState.Minimized;
            Application.DoEvents();
            ExpireRunningTimer(state);
            Equal(state.Mode, TimerMode.Focus, "Break completes with popup disabled");
            Equal(form.WindowState, FormWindowState.Minimized, "Popup opt-out preserves minimized window");
            Equal(state.Running, false, "Popup preference does not auto-start next session");
            form.Close();
        }
        Console.WriteLine("PASS: native controls, custom durations, Windows timer completion, and form disposal.");
    }

    private static void ExpireRunningTimer(PomodoroState state)
    {
        state.Toggle(DateTimeOffset.UtcNow);
        state.Toggle(DateTimeOffset.UtcNow.AddMinutes(-2));
        Stopwatch wait = Stopwatch.StartNew();
        while (state.Running && wait.ElapsedMilliseconds < 3000)
        {
            Application.DoEvents();
            Thread.Sleep(20);
        }
        Equal(state.Running, false, "Native timer completes within timeout");
    }

    private static void TestIndependentExecutable()
    {
        using (BinaryReader binary = new BinaryReader(File.OpenRead(AppPath)))
        {
            binary.BaseStream.Position = 0x3c;
            int header = binary.ReadInt32();
            binary.BaseStream.Position = header + 24 + 68;
            Equal(binary.ReadUInt16(), (ushort)2, "Windows GUI subsystem (no console)");
        }
        Process app = null;
        try
        {
            string testsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LittlePomodoro.Tests.exe");
            ProcessStartInfo start = new ProcessStartInfo(
                Environment.GetEnvironmentVariable("ComSpec"),
                "/d /c \"\"" + testsPath + "\" --launch\"");
            start.UseShellExecute = false;
            start.CreateNoWindow = true;
            start.RedirectStandardOutput = true;
            using (Process commandPrompt = Process.Start(start))
            {
                if (!commandPrompt.WaitForExit(10000)) throw new Exception("Launcher did not exit.");
                Equal(commandPrompt.ExitCode, 0, "Launcher exits successfully");
                int id = int.Parse(commandPrompt.StandardOutput.ReadToEnd().Trim(), CultureInfo.InvariantCulture);
                app = Process.GetProcessById(id);
            }
            Stopwatch wait = Stopwatch.StartNew();
            do
            {
                Thread.Sleep(50);
                app.Refresh();
                if (app.HasExited) throw new Exception("Timer exited after command prompt closed.");
            } while (app.MainWindowHandle == IntPtr.Zero && wait.ElapsedMilliseconds < 5000);
            Equal(app.MainWindowTitle, "25:00 - Focus | Little Pomodoro", "Independent timer window");
            Thread.Sleep(300);
            app.Refresh();
            Equal(app.HasExited, false, "Timer survives closed command prompt");
            Equal(app.CloseMainWindow(), true, "Close timer window");
            Equal(app.WaitForExit(5000), true, "Closing timer exits process");
            Equal(app.ExitCode, 0, "Clean app exit");
        }
        finally
        {
            if (app != null)
            {
                if (!app.HasExited) app.Kill();
                app.Dispose();
            }
        }
        Console.WriteLine("PASS: console-free EXE, independent process after command prompt closes, clean shutdown.");
    }
}
