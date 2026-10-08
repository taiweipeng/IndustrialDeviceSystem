using System.Drawing;
using System.Windows.Forms;

namespace IndustrialDeviceSystem.WinForms;

internal static class UiTheme
{
    private static readonly Color Primary = Color.FromArgb(37, 99, 235);
    private static readonly Color PrimaryHover = Color.FromArgb(29, 78, 216);

    private static readonly Color Success = Color.FromArgb(22, 163, 74);
    private static readonly Color SuccessHover = Color.FromArgb(21, 128, 61);

    private static readonly Color Danger = Color.FromArgb(220, 38, 38);
    private static readonly Color DangerHover = Color.FromArgb(185, 28, 28);

    private static readonly Color WindowBackground =
        Color.FromArgb(244, 247, 251);

    private static readonly Color PageBackground =
        Color.FromArgb(248, 250, 252);

    private static readonly Color Border =
        Color.FromArgb(203, 213, 225);

    private static readonly Color TextPrimary =
        Color.FromArgb(30, 41, 59);

    private static readonly Color TextSecondary =
        Color.FromArgb(100, 116, 139);

    private static readonly Font UiFont =
        new("Microsoft YaHei UI", 9F, FontStyle.Regular);

    private static readonly Font HeaderFont =
        new("Microsoft YaHei UI", 9F, FontStyle.Bold);

    public static void Apply(Form form)
    {
        form.SuspendLayout();

        try
        {
            form.Font = UiFont;
            form.ForeColor = TextPrimary;
            form.BackColor = WindowBackground;
            form.StartPosition = FormStartPosition.CenterScreen;

            form.MinimumSize = new Size(
                Math.Max(form.MinimumSize.Width, 1180),
                Math.Max(form.MinimumSize.Height, 720));

            ApplyControls(form);
        }
        finally
        {
            form.ResumeLayout(true);
        }
    }

    private static void ApplyControls(Control parent)
    {
        foreach (Control control in parent.Controls)
        {
            switch (control)
            {
                case TabControl tabControl:
                    StyleTabControl(tabControl);
                    break;

                case TabPage tabPage:
                    tabPage.BackColor = PageBackground;
                    tabPage.Padding = new Padding(12);
                    break;

                case Panel panel when panel.Tag is Color:
                    StyleDashboardCard(panel);
                    break;

                case DataGridView grid:
                    StyleGrid(grid);
                    break;

                case Button button:
                    StyleButton(button);
                    break;

                case TextBoxBase textBox:
                    StyleTextBox(textBox);
                    break;

                case ComboBox comboBox:
                    comboBox.FlatStyle = FlatStyle.Flat;
                    comboBox.BackColor = Color.White;
                    comboBox.ForeColor = TextPrimary;
                    break;

                case DateTimePicker dateTimePicker:
                    dateTimePicker.CalendarForeColor = TextPrimary;
                    dateTimePicker.CalendarMonthBackground = Color.White;
                    break;

                case NumericUpDown numericUpDown:
                    numericUpDown.BorderStyle = BorderStyle.FixedSingle;
                    numericUpDown.BackColor = Color.White;
                    numericUpDown.ForeColor = TextPrimary;
                    break;

                case GroupBox groupBox:
                    groupBox.ForeColor = TextPrimary;
                    groupBox.BackColor = Color.White;
                    groupBox.Padding = new Padding(10);
                    break;

                case Label label:
                    if (label.ForeColor.ToArgb() ==
                        SystemColors.ControlText.ToArgb())
                    {
                        label.ForeColor = TextPrimary;
                    }

                    break;

                case StatusStrip statusStrip:
                    StyleToolStrip(statusStrip);
                    break;

                case ToolStrip toolStrip:
                    StyleToolStrip(toolStrip);
                    break;
            }

            if (control.HasChildren)
            {
                ApplyControls(control);
            }
        }
    }

    private static void StyleTabControl(TabControl tabControl)
    {
        tabControl.Appearance = TabAppearance.Normal;
        tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
        tabControl.SizeMode = TabSizeMode.Fixed;
        tabControl.ItemSize = new Size(132, 38);
        tabControl.Font = HeaderFont;

        tabControl.DrawItem -= TabControl_DrawItem;
        tabControl.DrawItem += TabControl_DrawItem;
    }

    private static void TabControl_DrawItem(
        object? sender,
        DrawItemEventArgs e)
    {
        if (sender is not TabControl tabControl ||
            e.Index < 0 ||
            e.Index >= tabControl.TabPages.Count)
        {
            return;
        }

        var selected =
            e.Index == tabControl.SelectedIndex;

        var bounds = e.Bounds;
        bounds.Inflate(-2, -2);

        var backgroundColor =
            selected ? Primary : Color.White;

        var textColor =
            selected ? Color.White : TextSecondary;

        using var backgroundBrush =
            new SolidBrush(backgroundColor);

        e.Graphics.FillRectangle(
            backgroundBrush,
            bounds);

        ControlPaint.DrawBorder(
            e.Graphics,
            bounds,
            selected ? Primary : Border,
            ButtonBorderStyle.Solid);

        TextRenderer.DrawText(
            e.Graphics,
            tabControl.TabPages[e.Index].Text,
            tabControl.Font,
            bounds,
            textColor,
            TextFormatFlags.HorizontalCenter |
            TextFormatFlags.VerticalCenter |
            TextFormatFlags.EndEllipsis);
    }

    private static void StyleGrid(DataGridView grid)
    {
        grid.BackgroundColor = Color.White;
        grid.BorderStyle = BorderStyle.None;
        grid.GridColor = Color.FromArgb(226, 232, 240);

        grid.EnableHeadersVisualStyles = false;
        grid.RowHeadersVisible = false;
        grid.MultiSelect = false;
        grid.SelectionMode =
            DataGridViewSelectionMode.FullRowSelect;

        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;

        grid.ColumnHeadersHeight = 38;
        grid.ColumnHeadersHeightSizeMode =
            DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

        grid.RowTemplate.Height = 34;
        grid.CellBorderStyle =
            DataGridViewCellBorderStyle.SingleHorizontal;

        grid.ColumnHeadersDefaultCellStyle.BackColor =
            Color.FromArgb(30, 41, 59);

        grid.ColumnHeadersDefaultCellStyle.ForeColor =
            Color.White;

        grid.ColumnHeadersDefaultCellStyle.Font =
            HeaderFont;

        grid.ColumnHeadersDefaultCellStyle.Alignment =
            DataGridViewContentAlignment.MiddleLeft;

        grid.ColumnHeadersDefaultCellStyle.Padding =
            new Padding(6, 0, 6, 0);

        grid.DefaultCellStyle.Font = UiFont;
        grid.DefaultCellStyle.BackColor = Color.White;
        grid.DefaultCellStyle.ForeColor = TextPrimary;

        grid.DefaultCellStyle.SelectionBackColor =
            Color.FromArgb(219, 234, 254);

        grid.DefaultCellStyle.SelectionForeColor =
            Color.FromArgb(30, 64, 175);

        grid.DefaultCellStyle.Padding =
            new Padding(6, 0, 6, 0);

        grid.AlternatingRowsDefaultCellStyle.BackColor =
            Color.FromArgb(248, 250, 252);
    }

    private static void StyleTextBox(TextBoxBase textBox)
    {
        textBox.BorderStyle = BorderStyle.FixedSingle;

        if (textBox.Name.Contains(
                "log",
                StringComparison.OrdinalIgnoreCase))
        {
            textBox.BackColor =
                Color.FromArgb(15, 23, 42);

            textBox.ForeColor =
                Color.FromArgb(226, 232, 240);

            textBox.Font =
                new Font("Consolas", 9F);
        }
        else
        {
            textBox.BackColor = Color.White;
            textBox.ForeColor = TextPrimary;
        }
    }

    private static void StyleButton(Button button)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.UseVisualStyleBackColor = false;
        button.Cursor = Cursors.Hand;
        button.MinimumSize = new Size(88, 34);
        button.Padding = new Padding(7, 0, 7, 0);
        button.TextAlign = ContentAlignment.MiddleCenter;
        button.AutoEllipsis = false;

        button.EnabledChanged -= Button_EnabledChanged;
        button.EnabledChanged += Button_EnabledChanged;

        ApplyButtonState(button);
    }

    private static void Button_EnabledChanged(
        object? sender,
        EventArgs e)
    {
        if (sender is Button button)
        {
            ApplyButtonState(button);
        }
    }

    private static void ApplyButtonState(Button button)
    {
        if (!button.Enabled)
        {
            ApplyButtonColors(
                button,
                Color.FromArgb(226, 232, 240),
                Color.FromArgb(148, 163, 184),
                Color.FromArgb(203, 213, 225),
                Color.FromArgb(226, 232, 240));

            button.Cursor = Cursors.Default;
            return;
        }

        button.Cursor = Cursors.Hand;

        switch (ResolveButtonKind(button))
        {
            case ButtonKind.Primary:
                ApplyButtonColors(
                    button,
                    Primary,
                    Color.White,
                    Primary,
                    PrimaryHover);
                break;

            case ButtonKind.Success:
                ApplyButtonColors(
                    button,
                    Success,
                    Color.White,
                    Success,
                    SuccessHover);
                break;

            case ButtonKind.Danger:
                ApplyButtonColors(
                    button,
                    Danger,
                    Color.White,
                    Danger,
                    DangerHover);
                break;

            default:
                ApplyButtonColors(
                    button,
                    Color.White,
                    TextPrimary,
                    Border,
                    Color.FromArgb(241, 245, 249));
                break;
        }
    }

    private static void StyleDashboardCard(Panel panel)
    {
        panel.BackColor = Color.White;
        panel.BorderStyle = BorderStyle.None;

        panel.Paint -= DashboardCard_Paint;
        panel.Paint += DashboardCard_Paint;
    }

    private static void DashboardCard_Paint(
        object? sender,
        PaintEventArgs e)
    {
        if (sender is not Panel panel ||
            panel.Tag is not Color accentColor)
        {
            return;
        }

        var borderBounds = panel.ClientRectangle;

        if (borderBounds.Width <= 0 ||
            borderBounds.Height <= 0)
        {
            return;
        }

        borderBounds.Width -= 1;
        borderBounds.Height -= 1;

        using var accentBrush =
            new SolidBrush(accentColor);

        e.Graphics.FillRectangle(
            accentBrush,
            0,
            0,
            6,
            panel.ClientSize.Height);

        ControlPaint.DrawBorder(
            e.Graphics,
            borderBounds,
            Border,
            ButtonBorderStyle.Solid);
    }

    private static void ApplyButtonColors(
        Button button,
        Color background,
        Color foreground,
        Color border,
        Color hover)
    {
        button.BackColor = background;
        button.ForeColor = foreground;
        button.FlatAppearance.BorderColor = border;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.MouseOverBackColor = hover;
        button.FlatAppearance.MouseDownBackColor = hover;
    }

    private static ButtonKind ResolveButtonKind(Button button)
    {
        var tag = button.Tag?.ToString()?.Trim().ToLowerInvariant();

        if (tag == "primary")
            return ButtonKind.Primary;

        if (tag == "success")
            return ButtonKind.Success;

        if (tag == "danger")
            return ButtonKind.Danger;

        if (tag == "secondary")
            return ButtonKind.Secondary;

        var text = button.Text.Replace("&", string.Empty);

        if (ContainsAny(
                text,
                "完成",
                "验收",
                "导出",
                "关闭工单"))
        {
            return ButtonKind.Success;
        }

        if (ContainsAny(
                text,
                "删除",
                "取消",
                "停止",
                "清除",
                "断开"))
        {
            return ButtonKind.Danger;
        }

        if (ContainsAny(
                text,
                "新增",
                "创建",
                "保存",
                "提交",
                "预约",
                "确认",
                "分配",
                "开始",
                "启动",
                "连接"))
        {
            return ButtonKind.Primary;
        }

        return ButtonKind.Secondary;
    }

    private static bool ContainsAny(
        string source,
        params string[] keywords)
    {
        return keywords.Any(source.Contains);
    }

    private static void StyleToolStrip(ToolStrip toolStrip)
    {
        toolStrip.BackColor = Color.White;
        toolStrip.ForeColor = TextSecondary;
        toolStrip.RenderMode =
            ToolStripRenderMode.System;

        foreach (ToolStripItem item in toolStrip.Items)
        {
            item.ForeColor = TextSecondary;
        }
    }

    private enum ButtonKind
    {
        Primary,
        Success,
        Danger,
        Secondary
    }
}
