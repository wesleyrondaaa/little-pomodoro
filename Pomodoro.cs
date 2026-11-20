using System;
using System.Drawing;
using System.Globalization;
using System.Media;
using System.Windows.Forms;

namespace LittlePomodoro
{
    public enum TimerMode { Focus, Short, Long }

    public sealed class PomodoroState
    {
        private readonly int[] durations = { 25, 5, 15 };
        private DateTimeOffset deadline;

        public TimerMode Mode { get; private set; }
        public double Remaining { get; private set; }
        public bool Running { get; private set; }
        public int Completed { get; private set; }
        public string Status { get; private set; }

        public PomodoroState()
        {
            SelectMode(TimerMode.Focus);
            Status = "Ready when you are.";
        }

        public int Duration(TimerMode mode)
        {
            return durations[(int)mode];
        }

        public void SelectMode(TimerMode mode)
        {
            if (!Enum.IsDefined(typeof(TimerMode), mode))
                throw new ArgumentOutOfRangeException("mode");
            Mode = mode;
            Running = false;
            Remaining = Duration(mode) * 60000.0;
            Status = "Timer reset. Ready when you are.";
        }

        public void ApplyDurations(int focus, int shortBreak, int longBreak)
        {
            int[] values = { focus, shortBreak, longBreak };
            foreach (int value in values)
                if (value < 1 || value > 180)
                    throw new ArgumentOutOfRangeException("values", "Use 1 to 180 whole minutes.");
            Array.Copy(values, durations, values.Length);
            SelectMode(Mode);
            Status = "Durations updated. Timer reset.";
        }

        public bool Toggle(DateTimeOffset now)
        {
            if (Running)
            {
                if (Update(now)) return true;
                Running = false;
                Status = "Paused. Take your time.";
            }
            else
            {
                deadline = now.AddMilliseconds(Remaining);
                Running = true;
                Status = Mode == TimerMode.Focus ? "One thing at a time." : "Time to recharge.";
            }
            return false;
        }

        public bool Update(DateTimeOffset now)
        {
            if (!Running) return false;
            Remaining = Math.Max(0, (deadline - now).TotalMilliseconds);
            if (Remaining > 0) return false;
            TimerMode finished = Mode;
            if (finished == TimerMode.Focus)
            {
                Completed++;
                SelectMode(Completed % 4 == 0 ? TimerMode.Long : TimerMode.Short);
            }
            else
            {
                SelectMode(TimerMode.Focus);
            }
            Status = (finished == TimerMode.Focus ? "Focus" : "Break")
                + " complete. Press Start when you are ready.";
            return true;
        }

        public string FormattedTime
        {
            get
            {
                int seconds = (int)Math.Ceiling(Remaining / 1000);
                return string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}",
                    seconds / 60, seconds % 60);
            }
        }
    }

    public sealed class PomodoroForm : Form
    {
        private readonly PomodoroState state;
        private readonly Timer timer = new Timer();
        private readonly Button[] modeButtons = new Button[3];
        private readonly NumericUpDown[] inputs = new NumericUpDown[3];
        private readonly string[] modeLabels = { "Focus", "Short break", "Long break" };
        private readonly Label time;
        private readonly Label phase;
        private readonly Label status;
        private readonly Label sessions;
        private readonly Button toggle;
        private readonly CheckBox sound;
        private readonly CheckBox showWhenDone;
        private readonly Icon appIcon;
        private readonly Font normalFont = new Font("Segoe UI", 10);
        private readonly Font headingFont = new Font("Segoe UI", 14, FontStyle.Bold);
        private readonly Font timeFont = new Font("Segoe UI", 52);

        public PomodoroForm(PomodoroState state)
        {
            if (state == null) throw new ArgumentNullException("state");
            this.state = state;
            appIcon = System.Drawing.Icon.ExtractAssociatedIcon(typeof(PomodoroForm).Assembly.Location);
            Icon = appIcon;
            ClientSize = new Size(440, 600);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            AutoScaleDimensions = new SizeF(96, 96);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.FromArgb(245, 244, 239);
            ForeColor = Color.FromArgb(41, 61, 55);
            Font = normalFont;

            Label heading = AddLabel("Heading", 24, 24, 392, 32);
            heading.Text = "LITTLE POMODORO";
            heading.Font = headingFont;
            for (int i = 0; i < modeButtons.Length; i++)
            {
                Button button = AddButton(((TimerMode)i).ToString(), modeLabels[i],
                    24 + i * 132, 80, 128, 36);
                button.Tag = (TimerMode)i;
                button.Click += delegate(object sender, EventArgs args)
                {
                    this.state.SelectMode((TimerMode)((Button)sender).Tag);
                    Render();
                };
                modeButtons[i] = button;
            }

            phase = AddLabel("Phase", 24, 140, 392, 24);
            time = AddLabel("Time", 24, 170, 392, 100);
            time.Font = timeFont;
            time.AccessibleName = "Time remaining";
            toggle = AddButton("Toggle", "Start", 94, 290, 150, 42);
            toggle.BackColor = Color.FromArgb(40, 99, 78);
            toggle.ForeColor = Color.White;
            toggle.FlatStyle = FlatStyle.Flat;
            toggle.Click += delegate
            {
                bool completed = this.state.Toggle(DateTimeOffset.UtcNow);
                Render();
                if (completed) NotifyCompletion();
            };
            AcceptButton = toggle;
            Button reset = AddButton("Reset", "Reset", 254, 290, 92, 42);
            reset.Click += delegate { this.state.SelectMode(this.state.Mode); Render(); };
            status = AddLabel("Status", 24, 344, 392, 46);
            sessions = AddLabel("Sessions", 24, 394, 392, 24);

            for (int i = 0; i < inputs.Length; i++)
            {
                Label label = AddLabel("DurationLabel" + i, 24 + i * 132, 436, 124, 24);
                label.Text = modeLabels[i] + " (min)";
                label.TextAlign = ContentAlignment.MiddleLeft;
                NumericUpDown input = new NumericUpDown();
                input.Name = "Duration" + i;
                input.AccessibleName = modeLabels[i] + " minutes";
                input.Minimum = 1;
                input.Maximum = 180;
                input.Value = state.Duration((TimerMode)i);
                input.SetBounds(24 + i * 132, 462, 124, 28);
                inputs[i] = input;
                Controls.Add(input);
            }
            Button apply = AddButton("Apply", "Apply minutes", 24, 510, 124, 32);
            apply.Click += delegate
            {
                this.state.ApplyDurations((int)inputs[0].Value, (int)inputs[1].Value, (int)inputs[2].Value);
                Render();
            };
            sound = new CheckBox();
            sound.Name = "Sound";
            sound.Text = "Completion sound";
            sound.SetBounds(172, 510, 220, 32);
            Controls.Add(sound);
            showWhenDone = new CheckBox();
            showWhenDone.Name = "ShowWhenDone";
            showWhenDone.Text = "Show window when done";
            showWhenDone.Checked = true;
            showWhenDone.SetBounds(172, 546, 244, 32);
            Controls.Add(showWhenDone);
            timer.Interval = 200;
            timer.Tick += delegate
            {
                bool completed = this.state.Update(DateTimeOffset.UtcNow);
                Render();
                if (completed) NotifyCompletion();
            };
            Render();
        }

        private Label AddLabel(string name, int x, int y, int width, int height)
        {
            Label label = new Label();
            label.Name = name;
            label.TextAlign = ContentAlignment.MiddleCenter;
            label.SetBounds(x, y, width, height);
            Controls.Add(label);
            return label;
        }

        private Button AddButton(string name, string text, int x, int y, int width, int height)
        {
            Button button = new Button();
            button.Name = name;
            button.Text = text;
            button.SetBounds(x, y, width, height);
            Controls.Add(button);
            return button;
        }

        private void NotifyCompletion()
        {
            if (showWhenDone.Checked)
            {
                Show();
                if (WindowState == FormWindowState.Minimized)
                    WindowState = FormWindowState.Normal;
                Activate();
            }
            if (!sound.Checked) return;
            try
            {
                SystemSounds.Asterisk.Play();
            }
            catch (Exception error)
            {
                sound.Checked = false;
                status.Text = "Sound unavailable: " + error.Message;
            }
        }

        private void Render()
        {
            timer.Enabled = state.Running;
            time.Text = state.FormattedTime;
            Text = time.Text + " - " + modeLabels[(int)state.Mode] + " | Little Pomodoro";
            phase.Text = state.Mode == TimerMode.Focus ? "TIME TO FOCUS" : "TAKE A BREAK";
            toggle.Text = state.Running ? "Pause"
                : state.Remaining < state.Duration(state.Mode) * 60000.0 ? "Resume" : "Start";
            status.Text = state.Status;
            sessions.Text = state.Completed + " focus sessions completed";
            for (int i = 0; i < modeButtons.Length; i++)
                modeButtons[i].BackColor = i == (int)state.Mode
                    ? Color.FromArgb(216, 230, 218) : SystemColors.Control;
        }

        protected override void OnFormClosed(FormClosedEventArgs args)
        {
            timer.Stop();
            base.OnFormClosed(args);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) timer.Dispose();
            base.Dispose(disposing);
            if (disposing)
            {
                if (appIcon != null) appIcon.Dispose();
                normalFont.Dispose();
                headingFont.Dispose();
                timeFont.Dispose();
            }
        }
    }

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
            try
            {
                using (PomodoroForm form = new PomodoroForm(new PomodoroState()))
                    Application.Run(form);
            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString(), "Little Pomodoro - error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.ExitCode = 1;
            }
        }
    }
}
