using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

using LiveSplit.Model;
using LiveSplit.TimeFormatters;

namespace LiveSplit.UI.Components;

public class BooleanComponent : IComponent
{
    protected InfoTextComponent InternalComponent { get; set; }
    public BooleanSettings Settings { get; set; }
    private GeneralTimeFormatter Formatter { get; set; }

    public float PaddingTop => InternalComponent.PaddingTop;
    public float PaddingLeft => InternalComponent.PaddingLeft;
    public float PaddingBottom => InternalComponent.PaddingBottom;
    public float PaddingRight => InternalComponent.PaddingRight;

    public IDictionary<string, Action> ContextMenuControls => null;

    public BooleanComponent(LiveSplitState state)
    {
        Settings = new BooleanSettings()
        {
            CurrentState = state
        };
        Formatter = new GeneralTimeFormatter()
        {
            NullFormat = NullFormat.Dash,
            Accuracy = Settings.Accuracy,
            DropDecimals = Settings.DropDecimals,
        };
        InternalComponent = new InfoTextComponent(null, null);
        state.ComparisonRenamed += state_ComparisonRenamed;
    }

    private void state_ComparisonRenamed(object sender, EventArgs e)
    {
        var args = (RenameEventArgs)e;
        if (Settings.Comparison == args.OldName)
        {
            Settings.Comparison = args.NewName;
            ((LiveSplitState)sender).Layout.HasChanged = true;
        }
    }

    private void PrepareDraw(LiveSplitState state)
    {
        InternalComponent.DisplayTwoRows = Settings.Display2Rows;

        InternalComponent.NameLabel.HasShadow
            = InternalComponent.ValueLabel.HasShadow
            = state.LayoutSettings.DropShadows;

        Formatter.Accuracy = Settings.Accuracy;
        Formatter.DropDecimals = Settings.DropDecimals;

        InternalComponent.NameLabel.ForeColor = Settings.OverrideTextColor ? Settings.TextColor : state.LayoutSettings.TextColor;
    }

    private void DrawBackground(Graphics g, LiveSplitState state, float width, float height)
    {
        if (Settings.BackgroundColor.A > 0
            || (Settings.BackgroundGradient != GradientType.Plain
            && Settings.BackgroundColor2.A > 0))
        {
            var gradientBrush = new LinearGradientBrush(
                        new PointF(0, 0),
                        Settings.BackgroundGradient == GradientType.Horizontal
                        ? new PointF(width, 0)
                        : new PointF(0, height),
                        Settings.BackgroundColor,
                        Settings.BackgroundGradient == GradientType.Plain
                        ? Settings.BackgroundColor
                        : Settings.BackgroundColor2);
            g.FillRectangle(gradientBrush, 0, 0, width, height);
        }
    }

    public void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion)
    {
        DrawBackground(g, state, width, VerticalHeight);
        PrepareDraw(state);
        InternalComponent.DrawVertical(g, state, width, clipRegion);
    }

    public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion)
    {
        DrawBackground(g, state, HorizontalWidth, height);
        PrepareDraw(state);
        InternalComponent.DrawHorizontal(g, state, height, clipRegion);
    }

    public float VerticalHeight => InternalComponent.VerticalHeight;

    public float MinimumWidth => InternalComponent.MinimumWidth;

    public float HorizontalWidth => InternalComponent.HorizontalWidth;

    public float MinimumHeight => InternalComponent.MinimumHeight;

    public string ComponentName
        => "BPT < PB?";

    public Control GetSettingsControl(LayoutMode mode)
    {
        Settings.Mode = mode;
        return Settings;
    }

    public void SetSettings(System.Xml.XmlNode settings)
    {
        Settings.SetSettings(settings);
    }

    public System.Xml.XmlNode GetSettings(System.Xml.XmlDocument document)
    {
        return Settings.GetSettings(document);
    }

    public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
    {
        TimeSpan pbTime = state.Run.Last().PersonalBestSplitTime.RealTime.Value;

        TimeSpan bcst = state.Run[state.CurrentSplitIndex].BestSegmentTime.RealTime.Value;
        TimeSpan cst = state.CurrentTime.RealTime.Value;

        if (state.CurrentSplitIndex > 0)
        {
            bcst += state.Run[state.CurrentSplitIndex - 1].SplitTime.RealTime.Value;
        }

        TimeSpan t = (bcst > cst) ? bcst : cst;

        for (int i = state.CurrentSplitIndex + 1; i < state.Run.Count; i++)
        {
            t += state.Run[i].BestSegmentTime.RealTime.Value;
        }

        bool canPB = TimeSpan.Compare(pbTime, t) > 0;

        //string ts = Formatter.Format(bcst) + " / " + Formatter.Format(cst) + " / " + Formatter.Format(t);
        InternalComponent.InformationValue = canPB ? "Yes" : "No"; // "Yes: " + ts : "No: " + ts;
        InternalComponent.InformationName = ComponentName;

        Color? color = canPB ? state.LayoutSettings.AheadGainingTimeColor : state.LayoutSettings.BehindLosingTimeColor;
        if (state.CurrentSplitIndex > 0)
        {

            bool isGold = LiveSplitStateHelper.CheckBestSegment(state, state.CurrentSplitIndex - 1, state.CurrentTimingMethod);
            if (isGold)
            {
                state.LayoutSettings.UseRainbowColor = true;
                color = LiveSplitStateHelper.GetBestSegmentColor(state);
            }
        }

        InternalComponent.ValueLabel.ForeColor = color.Value;

        InternalComponent.Update(invalidator, state, width, height, mode);
    }

    public void Dispose()
    {
    }

    public int GetSettingsHashCode()
    {
        return Settings.GetSettingsHashCode();
    }
}
