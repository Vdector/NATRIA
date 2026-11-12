using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NatriaVpn
{
    internal static class Theme
    {
        public static readonly Color Bg = Color.FromArgb(8, 9, 14);
        public static readonly Color Card = Color.FromArgb(17, 19, 27);
        public static readonly Color Border = Color.FromArgb(33, 37, 51);
        public static readonly Color Text = Color.FromArgb(237, 240, 249);
        public static readonly Color Muted = Color.FromArgb(118, 126, 152);
        public static readonly Color AccentA = Color.FromArgb(124, 92, 255);
        public static readonly Color AccentB = Color.FromArgb(77, 225, 255);
        public static readonly Color Green = Color.FromArgb(55, 229, 143);
    }

    internal static class Ui
    {
        public static GraphicsPath Round(Rectangle r, int radius)
        {
            GraphicsPath p = new GraphicsPath();
            int d = radius * 2;
            if (d > r.Width) d = r.Width;
            if (d > r.Height) d = r.Height;
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        public static void Glow(Graphics g, Point c, int radius, Color color)
        {
            for (int i = 7; i >= 1; i--)
            {
                int rr = radius * i / 7;
                int alpha = (int)(color.A * (8 - i) / 28.0);
                if (alpha <= 0) continue;
                using (SolidBrush br = new SolidBrush(Color.FromArgb(alpha, color)))
                    g.FillEllipse(br, c.X - rr, c.Y - rr, rr * 2, rr * 2);
            }
        }
    }

    internal sealed class LogoMark : Control
    {
        public LogoMark()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Size = new Size(26, 26);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle r = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath p = Ui.Round(r, 8))
            using (LinearGradientBrush b = new LinearGradientBrush(r, Theme.AccentA, Theme.AccentB, 45f))
                g.FillPath(b, p);
            using (Font f = new Font("Segoe UI", 9.5f, FontStyle.Bold))
            using (SolidBrush tb = new SolidBrush(Color.FromArgb(10, 9, 18)))
            using (StringFormat sf = new StringFormat())
            {
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                g.DrawString("N", f, tb, new RectangleF(0, 0, Width, Height), sf);
            }
        }
    }

    internal class CardPanel : Panel
    {
        public bool Hover;
        public int CornerRadius = 12;

        public CardPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            BackColor = Theme.Bg;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            using (GraphicsPath p = Ui.Round(new Rectangle(0, 0, Width - 1, Height - 1), CornerRadius))
                Region = new Region(p);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle r = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath p = Ui.Round(r, CornerRadius))
            {
                using (SolidBrush b = new SolidBrush(Theme.Card))
                    g.FillPath(b, p);
                Color bc = Hover ? Color.FromArgb(110, Theme.AccentA) : Theme.Border;
                using (Pen pen = new Pen(bc))
                    g.DrawPath(pen, p);
            }
        }
    }

    internal sealed class PowerButton : Control
    {
        public int State;
        public double Progress;
        public int Frame;

        public PowerButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            Size = new Size(190, 190);
            Cursor = Cursors.Hand;
            BackColor = Theme.Bg;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Theme.Bg);

            int cx = Width / 2;
            int cy = Height / 2;
            float pulse = 1f;
            if (State == 2) pulse = 1f + 0.018f * (float)Math.Sin(Frame * 0.14);

            if (State == 2)
            {
                for (int i = 3; i >= 1; i--)
                {
                    int sz = (int)((150 + i * 24) * pulse);
                    int alpha = 8 + (3 - i) * 7;
                    using (SolidBrush b = new SolidBrush(Color.FromArgb(alpha, Theme.AccentB)))
                        g.FillEllipse(b, cx - sz / 2, cy - sz / 2, sz, sz);
                }
            }

            int ring = (int)(178 * pulse);
            Rectangle ringRect = new Rectangle(cx - ring / 2, cy - ring / 2, ring, ring);

            using (Pen track = new Pen(Color.FromArgb(38, 43, 60), 2f))
                g.DrawEllipse(track, ringRect);

            if (State == 1)
            {
                using (Pen prog = new Pen(Theme.AccentB, 3f))
                {
                    prog.StartCap = LineCap.Round;
                    prog.EndCap = LineCap.Round;
                    g.DrawArc(prog, ringRect, -90f, (float)(Progress * 3.6));
                }
            }

            int inner = (int)(150 * pulse);
            Rectangle innerRect = new Rectangle(cx - inner / 2, cy - inner / 2, inner, inner);

            if (State == 2)
            {
                using (LinearGradientBrush b = new LinearGradientBrush(innerRect, Theme.AccentA, Theme.AccentB, 45f))
                    g.FillEllipse(b, innerRect);
                using (Pen rim = new Pen(Color.FromArgb(80, 255, 255, 255), 1f))
                    g.DrawEllipse(rim, innerRect);
            }
            else
            {
                using (SolidBrush b = new SolidBrush(Color.FromArgb(16, 18, 26)))
                    g.FillEllipse(b, innerRect);
                using (Pen rim = new Pen(Theme.Border, 1.5f))
                    g.DrawEllipse(rim, innerRect);
            }

            Color icon = State == 2 ? Color.FromArgb(10, 9, 18) : Color.FromArgb(150, 158, 186);
            using (Pen pen = new Pen(icon, 4.5f))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                Rectangle arc = new Rectangle(cx - 30, cy - 28, 60, 60);
                g.DrawArc(pen, arc, -60f, 300f);
                g.DrawLine(pen, cx, cy - 40, cx, cy - 8);
            }

            if (State == 2)
            {
                using (Pen spark = new Pen(Color.FromArgb(190, Color.White), 2f))
                {
                    spark.StartCap = LineCap.Round;
                    spark.EndCap = LineCap.Round;
                    g.DrawLine(spark, cx - 8, cy + 34, cx + 8, cy + 34);
                }
            }
        }
    }

    internal sealed class DarkColors : ProfessionalColorTable
    {
        public override Color ToolStripDropDownBackground { get { return Theme.Card; } }
        public override Color MenuBorder { get { return Theme.Border; } }
        public override Color MenuItemBorder { get { return Theme.Border; } }
        public override Color MenuItemSelected { get { return Color.FromArgb(32, 36, 52); } }
        public override Color MenuItemSelectedGradientBegin { get { return Color.FromArgb(32, 36, 52); } }
        public override Color MenuItemSelectedGradientEnd { get { return Color.FromArgb(32, 36, 52); } }
        public override Color ImageMarginGradientBegin { get { return Theme.Card; } }
        public override Color ImageMarginGradientMiddle { get { return Theme.Card; } }
        public override Color ImageMarginGradientEnd { get { return Theme.Card; } }
    }

    internal sealed class DarkMenuRenderer : ToolStripProfessionalRenderer
    {
        public DarkMenuRenderer() : base(new DarkColors()) { }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = e.Item.Selected ? Color.White : Theme.Text;
            base.OnRenderItemText(e);
        }
    }

    internal sealed class MainForm : Form
    {
        private PowerButton power;
        private Label statusLabel;
        private Label hintLabel;
        private Label ipValue;
        private Label speedValue;
        private Label serverCode;
        private Label serverName;
        private CardPanel serverBtn;
        private readonly Timer tick;
        private readonly Random rnd = new Random();

        private readonly string[] servers = new string[]
        {
            "NL|РќРёРґРµСЂР»Р°РЅРґС‹ вЂ” РђРјСЃС‚РµСЂРґР°Рј",
            "DE|Р“РµСЂРјР°РЅРёСЏ вЂ” Р¤СЂР°РЅРєС„СѓСЂС‚",
            "FR|Р¤СЂР°РЅС†РёСЏ вЂ” РџР°СЂРёР¶",
            "US|РЎРЁРђ вЂ” РќСЊСЋ-Р™РѕСЂРє",
            "GB|Р’РµР»РёРєРѕР±СЂРёС‚Р°РЅРёСЏ вЂ” Р›РѕРЅРґРѕРЅ",
            "TR|РўСѓСЂС†РёСЏ вЂ” РЎС‚Р°РјР±СѓР»",
            "JP|РЇРїРѕРЅРёСЏ вЂ” РўРѕРєРёРѕ",
            "SG|РЎРёРЅРіР°РїСѓСЂ вЂ” РЎРёРЅРіР°РїСѓСЂ",
            "KZ|РљР°Р·Р°С…СЃС‚Р°РЅ вЂ” РђР»РјР°С‚С‹",
            "FI|Р¤РёРЅР»СЏРЅРґРёСЏ вЂ” РҐРµР»СЊСЃРёРЅРєРё"
        };

        private int serverIndex;
        private int state; // 0 off, 1 connecting, 2 on
        private double progress;
        private int tickCount;

        public MainForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(400, 620);
            BackColor = Theme.Bg;
            DoubleBuffered = true;
            MaximizeBox = false;
            MinimizeBox = false;
            Text = "NATRIA VPN";

            using (GraphicsPath p = Ui.Round(new Rectangle(0, 0, ClientSize.Width, ClientSize.Height), 16))
                Region = new Region(p);

            BuildUi();

            tick = new Timer();
            tick.Interval = 30;
            tick.Tick += OnTick;
            tick.Start();

            UpdateIpPreview();
        }

        private Font F(float size, FontStyle style)
        {
            return new Font("Segoe UI", size, style);
        }

        private Label MakeLabel(string text, int x, int y, int w, int h, float size, FontStyle style, Color color)
        {
            Label l = new Label();
            l.Text = text;
            l.Location = new Point(x, y);
            l.Size = new Size(w, h);
            l.Font = F(size, style);
            l.ForeColor = color;
            l.BackColor = Color.Transparent;
            l.TextAlign = ContentAlignment.MiddleLeft;
            l.AutoSize = false;
            return l;
        }

        private void BuildUi()
        {
            LogoMark logo = new LogoMark();
            logo.Location = new Point(20, 17);
            Controls.Add(logo);

            Label brand = MakeLabel("NATRIA", 54, 17, 90, 26, 11f, FontStyle.Bold, Theme.Text);
            brand.AutoSize = true;
            Controls.Add(brand);

            Label brandVpn = MakeLabel("VPN", brand.Right + 5, 18, 40, 26, 11f, FontStyle.Bold, Theme.AccentB);
            brandVpn.AutoSize = true;
            Controls.Add(brandVpn);

            Label min = MakeLabel("вЂ”", 328, 16, 26, 26, 11f, FontStyle.Bold, Theme.Muted);
            min.TextAlign = ContentAlignment.MiddleCenter;
            min.Cursor = Cursors.Hand;
            min.MouseEnter += delegate { min.ForeColor = Theme.Text; };
            min.MouseLeave += delegate { min.ForeColor = Theme.Muted; };
            min.Click += delegate { WindowState = FormWindowState.Minimized; };
            Controls.Add(min);

            Label close = MakeLabel("вњ•", 358, 16, 26, 26, 10f, FontStyle.Bold, Theme.Muted);
            close.TextAlign = ContentAlignment.MiddleCenter;
            close.Cursor = Cursors.Hand;
            close.MouseEnter += delegate { close.ForeColor = Color.FromArgb(255, 94, 120); };
            close.MouseLeave += delegate { close.ForeColor = Theme.Muted; };
            close.Click += delegate { Close(); };
            Controls.Add(close);

            power = new PowerButton();
            power.Location = new Point(105, 82);
            power.Click += delegate { Toggle(); };
            Controls.Add(power);

            statusLabel = MakeLabel("РќР• РџРћР”РљР›Р®Р§Р•РќРћ", 0, 286, 400, 22, 10.5f, FontStyle.Bold, Theme.Muted);
            statusLabel.TextAlign = ContentAlignment.MiddleCenter;
            Controls.Add(statusLabel);

            hintLabel = MakeLabel("РЅР°Р¶РјРё, С‡С‚РѕР±С‹ РїРѕРґРєР»СЋС‡РёС‚СЊСЃСЏ", 0, 310, 400, 16, 8.5f, FontStyle.Regular, Theme.Muted);
            hintLabel.TextAlign = ContentAlignment.MiddleCenter;
            Controls.Add(hintLabel);

            Label serverCaption = MakeLabel("РЎР•Р Р’Р•Р ", 30, 344, 200, 14, 7.5f, FontStyle.Bold, Theme.Muted);
            Controls.Add(serverCaption);

            serverBtn = new CardPanel();
            serverBtn.Location = new Point(30, 362);
            serverBtn.Size = new Size(340, 50);
            serverBtn.Cursor = Cursors.Hand;
            serverBtn.MouseEnter += delegate { serverBtn.Hover = true; serverBtn.Invalidate(); };
            serverBtn.MouseLeave += delegate { serverBtn.Hover = false; serverBtn.Invalidate(); };
            serverBtn.Click += delegate { OpenServerMenu(); };
            Controls.Add(serverBtn);

            serverCode = MakeLabel("NL", 16, 14, 34, 22, 9f, FontStyle.Bold, Theme.AccentB);
            serverCode.BackColor = Theme.Card;
            serverBtn.Controls.Add(serverCode);

            serverName = MakeLabel("РќРёРґРµСЂР»Р°РЅРґС‹ вЂ” РђРјСЃС‚РµСЂРґР°Рј", 52, 13, 230, 24, 9.5f, FontStyle.Regular, Theme.Text);
            serverName.BackColor = Theme.Card;
            serverBtn.Controls.Add(serverName);

            Label chevron = MakeLabel("в–ѕ", 306, 12, 24, 24, 10f, FontStyle.Bold, Theme.Muted);
            chevron.BackColor = Theme.Card;
            chevron.TextAlign = ContentAlignment.MiddleCenter;
            serverBtn.Controls.Add(chevron);

            Control[] kids = new Control[] { serverCode, serverName, chevron };
            for (int i = 0; i < kids.Length; i++)
            {
                kids[i].Cursor = Cursors.Hand;
                kids[i].MouseEnter += delegate { serverBtn.Hover = true; serverBtn.Invalidate(); };
                kids[i].MouseLeave += delegate { serverBtn.Hover = false; serverBtn.Invalidate(); };
                kids[i].Click += delegate { OpenServerMenu(); };
            }

            CardPanel ipCard = new CardPanel();
            ipCard.Location = new Point(30, 428);
            ipCard.Size = new Size(162, 66);
            Controls.Add(ipCard);

            Label ipCaption = MakeLabel("Р’РђРЁ IP", 14, 10, 130, 14, 7f, FontStyle.Bold, Theme.Muted);
            ipCaption.BackColor = Theme.Card;
            ipCard.Controls.Add(ipCaption);

            ipValue = MakeLabel("185.***.**.41", 14, 32, 136, 22, 10f, FontStyle.Bold, Theme.Text);
            ipValue.BackColor = Theme.Card;
            ipCard.Controls.Add(ipValue);

            CardPanel speedCard = new CardPanel();
            speedCard.Location = new Point(208, 428);
            speedCard.Size = new Size(162, 66);
            Controls.Add(speedCard);

            Label speedCaption = MakeLabel("РЎРљРћР РћРЎРўР¬", 14, 10, 130, 14, 7f, FontStyle.Bold, Theme.Muted);
            speedCaption.BackColor = Theme.Card;
            speedCard.Controls.Add(speedCaption);

            speedValue = MakeLabel("вЂ”", 14, 32, 136, 22, 10f, FontStyle.Bold, Theme.Text);
            speedValue.BackColor = Theme.Card;
            speedCard.Controls.Add(speedValue);

            Label kill = MakeLabel("в—Џ  Kill Switch Р°РєС‚РёРІРµРЅ", 0, 518, 400, 16, 8f, FontStyle.Regular, Theme.Green);
            kill.TextAlign = ContentAlignment.MiddleCenter;
            Controls.Add(kill);

            Label ver = MakeLabel("v1.0.4  В·  WireGuardВ®", 22, 580, 200, 16, 7.5f, FontStyle.Regular, Theme.Muted);
            Controls.Add(ver);

            Label site = MakeLabel("natriavpn.store", 260, 580, 120, 16, 7.5f, FontStyle.Regular, Theme.AccentA);
            site.TextAlign = ContentAlignment.MiddleRight;
            Controls.Add(site);

            Label beta = MakeLabel("BETA", 40, 586, 0, 0, 7f, FontStyle.Bold, Theme.Muted);
            beta.Visible = false;
            Controls.Add(beta);
        }

        private void OpenServerMenu()
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.BackColor = Theme.Card;
            menu.ForeColor = Theme.Text;
            menu.ShowImageMargin = false;
            menu.Font = F(9.5f, FontStyle.Regular);
            menu.Renderer = new DarkMenuRenderer();

            for (int i = 0; i < servers.Length; i++)
            {
                int idx = i;
                string text = servers[i].Replace("|", "   В·   ");
                ToolStripMenuItem item = new ToolStripMenuItem(text);
                item.ForeColor = idx == serverIndex ? Theme.AccentB : Theme.Text;
                item.Padding = new Padding(6, 4, 6, 4);
                item.Click += delegate { ApplyServer(idx); };
                menu.Items.Add(item);
            }

            menu.Show(serverBtn, new Point(0, serverBtn.Height + 6));
        }

        private void ApplyServer(int index)
        {
            serverIndex = index;
            string[] parts = servers[index].Split('|');
            serverCode.Text = parts[0];
            serverName.Text = parts[1];
            if (state == 2) Disconnect();
            if (state == 0) UpdateIpPreview();
        }

        private void UpdateIpPreview()
        {
            string code = servers[serverIndex].Substring(0, 2);
            if (state == 2)
            {
                ipValue.Text = code + " В· 45.137." + rnd.Next(10, 220) + "." + rnd.Next(2, 240);
            }
            else
            {
                ipValue.Text = "185.***.**.41";
            }
        }

        private void Toggle()
        {
            if (state == 1) return;
            if (state == 2) { Disconnect(); return; }

            state = 1;
            progress = 0;
            statusLabel.Text = "РџРћР”РљР›Р®Р§Р•РќРР•вЂ¦";
            statusLabel.ForeColor = Theme.AccentB;
            hintLabel.Text = "СѓСЃС‚Р°РЅР°РІР»РёРІР°РµРј С‚СѓРЅРЅРµР»СЊвЂ¦";
            speedValue.Text = "вЂ”";
            ipValue.Text = "РѕРїСЂРµРґРµР»СЏРµРјвЂ¦";
        }

        private void Connect()
        {
            state = 2;
            statusLabel.Text = "Р—РђР©РР©Р•РќРћ";
            statusLabel.ForeColor = Theme.Green;
            hintLabel.Text = "РІРµСЃСЊ С‚СЂР°С„РёРє Р·Р°С€РёС„СЂРѕРІР°РЅ";
            UpdateIpPreview();
            speedValue.Text = (rnd.NextDouble() * 18 + 80).ToString("0.0") + " РњР±РёС‚/СЃ";
        }

        private void Disconnect()
        {
            state = 0;
            progress = 0;
            statusLabel.Text = "РќР• РџРћР”РљР›Р®Р§Р•РќРћ";
            statusLabel.ForeColor = Theme.Muted;
            hintLabel.Text = "РЅР°Р¶РјРё, С‡С‚РѕР±С‹ РїРѕРґРєР»СЋС‡РёС‚СЊСЃСЏ";
            UpdateIpPreview();
            speedValue.Text = "вЂ”";
        }

        private void OnTick(object sender, EventArgs e)
        {
            tickCount++;

            if (state == 1)
            {
                progress += rnd.NextDouble() * 2.6 + 1.4;
                if (progress >= 100)
                {
                    progress = 100;
                    Connect();
                }
            }
            else if (state == 2 && tickCount % 30 == 0)
            {
                speedValue.Text = (rnd.NextDouble() * 18 + 80).ToString("0.0") + " РњР±РёС‚/СЃ";
                if (tickCount % 90 == 0) UpdateIpPreview();
            }

            power.State = state;
            power.Progress = progress;
            power.Frame = tickCount;
            power.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Ui.Glow(e.Graphics, new Point(30, 60), 280, Color.FromArgb(34, Theme.AccentA));
            Ui.Glow(e.Graphics, new Point(390, 600), 300, Color.FromArgb(26, Theme.AccentB));
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x84)
            {
                base.WndProc(ref m);
                if ((int)m.Result == 1)
                    m.Result = (IntPtr)2;
                return;
            }
            base.WndProc(ref m);
        }
    }

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}

