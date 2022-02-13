namespace WSplitTimer
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Text;
    using System.Linq;
    using System.Windows.Forms;
    using Properties;

    public class DetailedView : Form
    {
        public const int HTCAPTION = 2;
        public const int HTCLIENT = 1;
        public const int HTLEFT = 10;
        public const int HTRIGHT = 11;
        public const int WM_NCHITTEST = 0x84;

        private IContainer components;

        private float plusPct = 0.5f;
        public int widthH = 1;
        public int widthHH = 1;
        public int widthHHH = 1;
        public int widthM = 1;
        public string clockText = "000:00:00.00";

        private WSplit parent;

        private ContextMenuStrip contextMenuStrip;

        private ToolStripMenuItem menuItemSelectColumns;
        private ToolStripMenuItem menuItemSetColors;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem menuItemShowSegs;
        private ToolStripMenuItem menuItemMarkSegments;
        private ToolStripMenuItem menuItemAlwaysOnTop;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem menuItemClose;

        public Brush clockColor;
        public Font clockFont;

        public DataGridView segs;
        private DataGridViewTextBoxColumn SegName;
        private DataGridViewTextBoxColumn Old;
        private DataGridViewTextBoxColumn SumOfBests;
        private DataGridViewTextBoxColumn Best;
        private DataGridViewTextBoxColumn Live;
        private DataGridViewTextBoxColumn Delta;

        public DataGridView finalSeg;
        private DataGridViewTextBoxColumn finalSegName;
        private DataGridViewTextBoxColumn finalOld;
        private DataGridViewTextBoxColumn finalBest;
        private DataGridViewTextBoxColumn finalSumOfBests;
        private DataGridViewTextBoxColumn finalLive;
        private DataGridViewTextBoxColumn finalDelta;

        public List<PointF> deltaPoints = new List<PointF>();
        public List<double> Deltas = new List<double>();

        public Label displayTime;


        public DetailedView(Split useSplits, WSplit callingForm)
        {
            base.Paint += new PaintEventHandler(this.dviewPaint);
            base.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            this.InitializeComponent();
            this.parent = callingForm;
            this.menuItemShowSegs.Checked = Settings.Profile.DViewShowSegs;
            this.menuItemMarkSegments.Checked = Settings.Profile.DViewDeltaMarks;
            this.menuItemAlwaysOnTop.Checked = Settings.Profile.DViewOnTop;
            base.TopMost = Settings.Profile.DViewOnTop;
            this.updateColumns();
            this.clockFont = this.displayTime.Font;
        }

        public void InitializeFonts()
        {
            FontFamily family = FontFamily.Families.FirstOrDefault(f => f.Name == Settings.Profile.FontFamilySegments);

            if (family == null || !family.IsStyleAvailable(FontStyle.Bold))
                this.displayTime.Font = new Font(FontFamily.GenericSansSerif, 17.33333f, FontStyle.Bold, GraphicsUnit.Pixel);
            else
                this.displayTime.Font = new Font(family, 21f, FontStyle.Bold, GraphicsUnit.Pixel);

            family = FontFamily.Families.FirstOrDefault(f => f.Name == Settings.Profile.FontFamilyDView);
            Font font;
            if (family == null || !family.IsStyleAvailable(FontStyle.Regular))
                font = new Font(FontFamily.GenericSansSerif, 10.5f, FontStyle.Regular, GraphicsUnit.Pixel);
            else
                font = new Font(Settings.Profile.FontFamilyDView, 10.5f, FontStyle.Regular, GraphicsUnit.Pixel);

            foreach (DataGridViewColumn c in this.segs.Columns)
                c.DefaultCellStyle.Font = font;

            foreach (DataGridViewColumn c in this.finalSeg.Columns)
                c.DefaultCellStyle.Font = font;

            this.widthM = TextRenderer.MeasureText("00:00.00", this.displayTime.Font).Width;
            this.widthH = TextRenderer.MeasureText("0:00:00.00", this.displayTime.Font).Width;
            this.widthHH = TextRenderer.MeasureText("00:00:00.00", this.displayTime.Font).Width;
            this.widthHHH = TextRenderer.MeasureText("000:00:00.00", this.displayTime.Font).Width;
        }

        private void menuItemAlwaysOnTop_Click(object sender, EventArgs e)
        {
            Settings.Profile.DViewOnTop = !Settings.Profile.DViewOnTop;
            this.menuItemAlwaysOnTop.Checked = Settings.Profile.DViewOnTop;
            base.TopMost = Settings.Profile.DViewOnTop;
        }

        private void menuItemClose_click(object sender, EventArgs e)
        {
            base.Hide();
            this.parent.advancedDetailButton.Checked = false;
        }

        private void DetailedView_Resize(object sender, EventArgs e)
        {
            base.Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void dviewPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
            StringFormat format = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            Rectangle layoutRectangle = new Rectangle(this.displayTime.Left, this.displayTime.Top, this.displayTime.Width, this.displayTime.Height);
            if(Settings.Profile.ShowAdvTimer)
            {
                if (this.clockText.Length == 8)
                {
                    layoutRectangle.Width = this.widthM + 6;
                }
                else if (this.clockText.Length == 10)
                {
                    layoutRectangle.Width = this.widthH + 6;
                }
                else if (this.clockText.Length == 11)
                {
                    layoutRectangle.Width = this.widthHH + 6;
                }
                else if (this.clockText.Length == 12)
                {
                    layoutRectangle.Width = this.widthHHH + 6;
                }
                e.Graphics.DrawString(this.clockText, this.displayTime.Font, this.clockColor, layoutRectangle, format);
            } else
            {
                layoutRectangle.Width = 0;
            }
            
            int right = layoutRectangle.Right + 4;
            int y = layoutRectangle.Top + 4;
            int width = (base.Width - right) - 6;
            int height = (base.Height - y) - 6;
            if (width >= 30)
            {
                e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                Pen pen = new Pen(Brushes.White, 1f);
                Pen pen2 = new Pen(new SolidBrush(Color.FromArgb(0x40, 0, 0, 0)), 1f);
                Pen pen3 = new Pen(new SolidBrush(Color.FromArgb(0x80, 0xff, 0xff, 0xff)), 1f);
                float num5 = height - (height * this.plusPct);
                e.Graphics.FillRectangle(new SolidBrush(ColorSettings.Profile.GraphBehind), (float)right, (float)y, (float)width, num5);
                e.Graphics.FillRectangle(new SolidBrush(ColorSettings.Profile.GraphAhead), (float)right, y + num5, (float)width, height - num5);
                for (int i = 1; i <= (width / 7); i++)
                {
                    PointF tf = new PointF((float)(right + (7 * i)), (float)y);
                    PointF tf2 = new PointF(tf.X, (float)(y + height));
                    e.Graphics.DrawLine(pen2, tf, tf2);
                }
                for (int j = 1; j <= (height / 7); j++)
                {
                    PointF tf3 = new PointF((float)right, (float)(y + (7 * j)));
                    PointF tf4 = new PointF((float)(right + width), tf3.Y);
                    e.Graphics.DrawLine(pen2, tf3, tf4);
                }
                e.Graphics.DrawRectangle(pen3, new Rectangle(right, y, width, height));
                if (this.deltaPoints.Count >= 1)
                {
                    List<PointF> list = new List<PointF> {
                        new PointF((float) right, y + num5)
                    };
                    foreach (PointF tf5 in this.deltaPoints)
                    {
                        float x = (tf5.X * width) + right;
                        float num9 = (height - (tf5.Y * height)) + y;
                        if (Settings.Profile.DViewDeltaMarks)
                        {
                            e.Graphics.FillEllipse(Brushes.White, (float)(x - 2f), (float)(num9 - 2f), (float)4f, (float)4f);
                        }
                        list.Add(new PointF(x, num9));
                    }
                    e.Graphics.DrawLines(pen, list.ToArray());
                }
            }
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DetailedView));
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuItemSelectColumns = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemSetColors = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.menuItemShowSegs = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemMarkSegments = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemAlwaysOnTop = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.menuItemClose = new System.Windows.Forms.ToolStripMenuItem();
            this.segs = new System.Windows.Forms.DataGridView();
            this.SegName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Old = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Best = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SumOfBests = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Live = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Delta = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.displayTime = new System.Windows.Forms.Label();
            this.finalSeg = new System.Windows.Forms.DataGridView();
            this.finalSegName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.finalOld = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.finalBest = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.finalSumOfBests = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.finalLive = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.finalDelta = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.contextMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.segs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.finalSeg)).BeginInit();
            this.SuspendLayout();
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemSelectColumns,
            this.menuItemSetColors,
            this.toolStripSeparator1,
            this.menuItemShowSegs,
            this.menuItemMarkSegments,
            this.menuItemAlwaysOnTop,
            this.toolStripSeparator2,
            this.menuItemClose});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(236, 148);
            // 
            // menuItemSelectColumns
            // 
            this.menuItemSelectColumns.Name = "menuItemSelectColumns";
            this.menuItemSelectColumns.Size = new System.Drawing.Size(235, 22);
            this.menuItemSelectColumns.Text = "Select columns...";
            // 
            // menuItemSetColors
            // 
            this.menuItemSetColors.Name = "menuItemSetColors";
            this.menuItemSetColors.Size = new System.Drawing.Size(235, 22);
            this.menuItemSetColors.Text = "Set colors...";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(232, 6);
            // 
            // menuItemShowSegs
            // 
            this.menuItemShowSegs.Name = "menuItemShowSegs";
            this.menuItemShowSegs.Size = new System.Drawing.Size(235, 22);
            this.menuItemShowSegs.Text = "Show segment times";
            // 
            // menuItemMarkSegments
            // 
            this.menuItemMarkSegments.Name = "menuItemMarkSegments";
            this.menuItemMarkSegments.Size = new System.Drawing.Size(235, 22);
            this.menuItemMarkSegments.Text = "Mark segments on delta graph";
            // 
            // menuItemAlwaysOnTop
            // 
            this.menuItemAlwaysOnTop.Name = "menuItemAlwaysOnTop";
            this.menuItemAlwaysOnTop.Size = new System.Drawing.Size(235, 22);
            this.menuItemAlwaysOnTop.Text = "Always on top";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(232, 6);
            // 
            // menuItemClose
            // 
            this.menuItemClose.Name = "menuItemClose";
            this.menuItemClose.Size = new System.Drawing.Size(235, 22);
            this.menuItemClose.Text = "Close";
            // 
            // segs
            // 
            this.segs.AllowUserToAddRows = false;
            this.segs.AllowUserToDeleteRows = false;
            this.segs.AllowUserToResizeColumns = false;
            this.segs.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.Black;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.Black;
            this.segs.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.segs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.segs.BackgroundColor = System.Drawing.Color.Black;
            this.segs.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.segs.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.segs.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.segs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.segs.ColumnHeadersVisible = false;
            this.segs.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.SegName,
            this.Old,
            this.Best,
            this.SumOfBests,
            this.Live,
            this.Delta});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.Black;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.WhiteSmoke;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.Black;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.WhiteSmoke;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.segs.DefaultCellStyle = dataGridViewCellStyle3;
            this.segs.Enabled = false;
            this.segs.GridColor = System.Drawing.Color.Black;
            this.segs.Location = new System.Drawing.Point(0, 0);
            this.segs.Margin = new System.Windows.Forms.Padding(0);
            this.segs.MultiSelect = false;
            this.segs.Name = "segs";
            this.segs.ReadOnly = true;
            this.segs.RowHeadersVisible = false;
            this.segs.RowTemplate.Height = 16;
            this.segs.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.segs.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.segs.Size = new System.Drawing.Size(173, 12);
            this.segs.TabIndex = 0;
            // 
            // SegName
            // 
            this.SegName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.SegName.HeaderText = "Segment";
            this.SegName.Name = "SegName";
            this.SegName.ReadOnly = true;
            // 
            // Old
            // 
            this.Old.HeaderText = "Old";
            this.Old.MinimumWidth = 2;
            this.Old.Name = "Old";
            this.Old.ReadOnly = true;
            this.Old.Width = 2;
            // 
            // Best
            // 
            this.Best.HeaderText = "Best";
            this.Best.MinimumWidth = 2;
            this.Best.Name = "Best";
            this.Best.ReadOnly = true;
            this.Best.Width = 2;
            // 
            // SumOfBests
            // 
            this.SumOfBests.HeaderText = "SoB";
            this.SumOfBests.MinimumWidth = 2;
            this.SumOfBests.Name = "SumOfBests";
            this.SumOfBests.ReadOnly = true;
            this.SumOfBests.Width = 2;
            // 
            // Live
            // 
            this.Live.HeaderText = "Live";
            this.Live.MinimumWidth = 2;
            this.Live.Name = "Live";
            this.Live.ReadOnly = true;
            this.Live.Width = 2;
            // 
            // Delta
            // 
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.BottomRight;
            this.Delta.DefaultCellStyle = dataGridViewCellStyle2;
            this.Delta.HeaderText = "Delta";
            this.Delta.MinimumWidth = 2;
            this.Delta.Name = "Delta";
            this.Delta.ReadOnly = true;
            this.Delta.Width = 2;
            // 
            // displayTime
            // 
            this.displayTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.displayTime.AutoSize = true;
            this.displayTime.BackColor = System.Drawing.Color.Black;
            this.displayTime.ForeColor = System.Drawing.Color.PaleGoldenrod;
            this.displayTime.Location = new System.Drawing.Point(0, 46);
            this.displayTime.Margin = new System.Windows.Forms.Padding(0);
            this.displayTime.MinimumSize = new System.Drawing.Size(0, 34);
            this.displayTime.Name = "displayTime";
            this.displayTime.Size = new System.Drawing.Size(70, 34);
            this.displayTime.TabIndex = 2;
            this.displayTime.Text = "000:00:00.00";
            this.displayTime.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.displayTime.Visible = false;
            // 
            // finalSeg
            // 
            this.finalSeg.AllowUserToAddRows = false;
            this.finalSeg.AllowUserToDeleteRows = false;
            this.finalSeg.AllowUserToResizeColumns = false;
            this.finalSeg.AllowUserToResizeRows = false;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.Black;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.Black;
            this.finalSeg.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle4;
            this.finalSeg.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.finalSeg.BackgroundColor = System.Drawing.Color.Black;
            this.finalSeg.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.finalSeg.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.finalSeg.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.finalSeg.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.finalSeg.ColumnHeadersVisible = false;
            this.finalSeg.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.finalSegName,
            this.finalOld,
            this.finalBest,
            this.finalSumOfBests,
            this.finalLive,
            this.finalDelta});
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = System.Drawing.Color.Black;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle6.ForeColor = System.Drawing.Color.WhiteSmoke;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.Color.Black;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.Color.WhiteSmoke;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.finalSeg.DefaultCellStyle = dataGridViewCellStyle6;
            this.finalSeg.Enabled = false;
            this.finalSeg.GridColor = System.Drawing.Color.Black;
            this.finalSeg.Location = new System.Drawing.Point(0, 12);
            this.finalSeg.Margin = new System.Windows.Forms.Padding(0);
            this.finalSeg.MultiSelect = false;
            this.finalSeg.Name = "finalSeg";
            this.finalSeg.ReadOnly = true;
            this.finalSeg.RowHeadersVisible = false;
            this.finalSeg.RowTemplate.Height = 16;
            this.finalSeg.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.finalSeg.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.finalSeg.Size = new System.Drawing.Size(173, 12);
            this.finalSeg.TabIndex = 3;
            // 
            // finalSegName
            // 
            this.finalSegName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.finalSegName.HeaderText = "Segment";
            this.finalSegName.Name = "finalSegName";
            this.finalSegName.ReadOnly = true;
            // 
            // finalOld
            // 
            this.finalOld.HeaderText = "Old";
            this.finalOld.MinimumWidth = 2;
            this.finalOld.Name = "finalOld";
            this.finalOld.ReadOnly = true;
            this.finalOld.Width = 2;
            // 
            // finalBest
            // 
            this.finalBest.HeaderText = "Best";
            this.finalBest.MinimumWidth = 2;
            this.finalBest.Name = "finalBest";
            this.finalBest.ReadOnly = true;
            this.finalBest.Width = 2;
            // 
            // finalSumOfBests
            // 
            this.finalSumOfBests.HeaderText = "Sum of Bests";
            this.finalSumOfBests.MinimumWidth = 2;
            this.finalSumOfBests.Name = "finalSumOfBests";
            this.finalSumOfBests.ReadOnly = true;
            this.finalSumOfBests.Width = 2;
            // 
            // finalLive
            // 
            this.finalLive.HeaderText = "Live";
            this.finalLive.MinimumWidth = 2;
            this.finalLive.Name = "finalLive";
            this.finalLive.ReadOnly = true;
            this.finalLive.Width = 2;
            // 
            // finalDelta
            // 
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.BottomRight;
            this.finalDelta.DefaultCellStyle = dataGridViewCellStyle5;
            this.finalDelta.HeaderText = "Delta";
            this.finalDelta.MinimumWidth = 2;
            this.finalDelta.Name = "finalDelta";
            this.finalDelta.ReadOnly = true;
            this.finalDelta.Width = 2;
            // 
            // DetailedView
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(173, 80);
            this.ContextMenuStrip = this.contextMenuStrip;
            this.Controls.Add(this.segs);
            this.Controls.Add(this.finalSeg);
            this.Controls.Add(this.displayTime);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(107, 0);
            this.Name = "DetailedView";
            this.Text = "Detailed View";
            this.Resize += new System.EventHandler(this.DetailedView_Resize);
            this.contextMenuStrip.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.segs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.finalSeg)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void menuItemMarkSegments_Click(object sender, EventArgs e)
        {
            Settings.Profile.DViewDeltaMarks = !Settings.Profile.DViewDeltaMarks;
            this.menuItemMarkSegments.Checked = Settings.Profile.DViewDeltaMarks;
            base.Invalidate();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            return this.parent.timerHotkey(keyData);
        }

        public void resetHeight()
        {
            this.finalSeg.Top = this.segs.Top + this.segs.Height;
            base.Height = this.finalSeg.Bottom + 0x22;
        }

        private void menuItemSelectColumns_Click(object sender, EventArgs e)
        {
            DViewSetColumnsDialog dialog = new DViewSetColumnsDialog();
            base.TopMost = false;
            this.parent.TopMost = false;
            this.parent.modalWindowOpened = true;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                this.parent.updateDetailed();
            }
            this.parent.TopMost = Settings.Profile.OnTop;
            base.TopMost = Settings.Profile.DViewOnTop;
            this.parent.modalWindowOpened = false;
        }

        private void menuItemSetColors_Click(object sender, EventArgs e)
        {
            CustomizeColors colorDialog = new CustomizeColors(true);
            this.parent.TopMost = false;
            base.TopMost = false;
            this.parent.modalWindowOpened = true;

            if (colorDialog.ShowDialog(this) == DialogResult.OK)
                this.parent.updateDetailed();

            this.parent.TopMost = Settings.Profile.OnTop;
            base.TopMost = Settings.Profile.DViewOnTop;
            this.parent.modalWindowOpened = false;
        }

        public void setDeltaPoints()
        {
            this.deltaPoints.Clear();
            double num = 0.0;
            double num2 = 0.0;
            foreach (double num3 in this.Deltas)
            {
                num = Math.Max(num3, num);
                num2 = Math.Min(num3, num2);
            }
            double num4 = this.parent.split.CompTime(this.parent.split.LastIndex);

            // "Temporary"? fix for the bug described below
            if (num4 != 0.0)
            {
                for (int i = 0; (i < this.Deltas.Count) && (i <= this.parent.split.LastIndex); i++)
                {
                    if ((this.parent.split.segments[i].LiveTime != 0.0) && (this.parent.split.CompTime(i) != 0.0))
                    {
                        // This next line causes a graphic crash if the last segment is empty and the segment i is not.
                        float x = (float)(this.parent.split.CompTime(i) / num4);
                        float y = 0.5f;
                        if ((num - num2) > 0.0)
                        {
                            y = (float)((this.Deltas[i] - num2) / (num - num2));
                        }
                        this.deltaPoints.Add(new PointF(x, y));
                    }
                }
            }

            if ((num - num2) > 0.0)
            {
                this.plusPct = (float)((0.0 - num2) / (num - num2));
            }
            else
            {
                this.plusPct = 0.5f;
            }
        }

        private void menuItemShowSegs_Click(object sender, EventArgs e)
        {
            Settings.Profile.DViewShowSegs = !Settings.Profile.DViewShowSegs;
            this.menuItemShowSegs.Checked = Settings.Profile.DViewShowSegs;
            this.parent.updateDetailed();
        }

        public void updateColumns()
        {
            int num = 0x2e;
            if ((this.segs.RowCount > 0) && (this.finalSeg.RowCount > 1))
            {
                if (Settings.Profile.DViewShowSegs)
                {
                    this.segs.Rows[0].Cells[2].Value = "Best [Seg]";
                    this.segs.Rows[0].Cells[4].Value = "Live [Seg]";
                }
                else
                {
                    this.segs.Rows[0].Cells[2].Value = "Best";
                    this.segs.Rows[0].Cells[4].Value = "Live";
                }
                
                // The detailed view used to only show a column if it had an ending time. I decided to change it, because
                // the user can still decide to show a column or not manually.
                if (Settings.Profile.DViewShowOld || (Settings.Profile.DViewShowComp && this.parent.split.ComparingType == Split.CompareType.Old))
                {
                    this.segs.Columns[1].Visible = true;
                    num += 0x22;
                }
                else
                    this.segs.Columns[1].Visible = false;

                if (Settings.Profile.DViewShowBest || (Settings.Profile.DViewShowComp && this.parent.split.ComparingType == Split.CompareType.Best))
                {
                    this.segs.Columns[2].Visible = true;
                    num += 0x22;
                    if (Settings.Profile.DViewShowSegs)
                    {
                        num += 0x24;
                    }
                }
                else
                    this.segs.Columns[2].Visible = false;

                if (Settings.Profile.DViewShowSumOfBests || (Settings.Profile.DViewShowComp && this.parent.split.ComparingType == Split.CompareType.SumOfBests))
                {
                    this.segs.Columns[3].Visible = true;
                    num += 0x22;
                }
                else
                    this.segs.Columns[3].Visible = false;

                this.segs.Columns[4].Visible = Settings.Profile.DViewShowLive;
                if (this.segs.Columns[4].Visible)
                {
                    num += 0x22;
                    if (Settings.Profile.DViewShowSegs)
                        num += 0x24;
                }

                // Again, the deltas used to only show if the comparison time had an ending time. It got changed.
                this.segs.Columns[5].Visible = Settings.Profile.DViewShowDeltas;
                if (this.segs.Columns[5].Visible)
                    num += 0x20;
            }
            num = Math.Max(num, this.displayTime.Width);
            if (base.Size.Width == this.MinimumSize.Width)
            {
                this.MinimumSize = new Size(num, 0);
                base.Width = this.MinimumSize.Width;
            }
            else
            {
                int num2 = base.Width - this.MinimumSize.Width;
                this.MinimumSize = new Size(num, 0);
                base.Width = this.MinimumSize.Width + num2;
            }
            foreach (DataGridViewColumn column in this.finalSeg.Columns)
            {
                column.Visible = this.segs.Columns[column.Index].Visible;
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x84)
            {
                base.WndProc(ref m);
                Point point = base.PointToClient(new Point(m.LParam.ToInt32()));
                if ((point.X <= 5) && (point.X >= 0))
                {
                    m.Result = (IntPtr)10;
                }
                else if ((point.X >= (base.ClientSize.Width - 5)) && (point.X <= base.ClientSize.Width))
                {
                    m.Result = (IntPtr)11;
                }
                else if ((Control.MouseButtons != MouseButtons.Right) && (((int)m.Result) == 1))
                {
                    m.Result = (IntPtr)2;
                }
            }
            else
            {
                base.WndProc(ref m);
            }
        }
    }
}

