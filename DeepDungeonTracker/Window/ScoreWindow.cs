using Dalamud.Game.Gui.FlyText;
using ImGuiNET;
using System;
using System.Globalization;
using System.Numerics;

namespace DeepDungeonTracker;

public sealed class ScoreWindow(string id, Configuration configuration, Data data) : WindowEx(id, configuration), IDisposable
{
    private Data Data { get; } = data;

    private int PreviousScore { get; set; }

    private int GradualScore { get; set; }

    private DateTime GradualScoreTime { get; set; }

    public void Dispose() { }

    private void OptionsUpdate()
    {
        var config = this.Configuration.Score;
        this.IsOpen = this.Data.UI.CommonWindowVisibility(config.Show, config.ShowInBetweenFloors, this.Data.Common.IsInDeepDungeonRegion, this.Data.IsInsideDeepDungeon, this.Data.IsInDeepDungeonSubArea);
        this.Flags = config.Lock ? WindowEx.StaticNoBackground : WindowEx.StaticNoBackgroundMoveInputs;
    }

    private void ScoreFlyText()
    {
        var config = this.Configuration.Score;

        var scoreDifference = this.Data.Common.TotalScore - this.PreviousScore;
        if (config.IsFlyTextScoreVisible && this.Data.Common.EnableFlyTextScore && scoreDifference != 0)
            Service.FlyTextGui.AddFlyText(FlyTextKind.Named, 0, 0, 0, $"{(scoreDifference > 0 ? "+" : string.Empty)}{scoreDifference.ToString("N0", CultureInfo.InvariantCulture)}pts", string.Empty, ImGui.ColorConvertFloat4ToU32(scoreDifference > 0 ? config.FlyTextScoreColor : Color.Red), 0, 0);

        this.PreviousScore = this.Data.Common.TotalScore;
    }

    private void GradualScoreUpdate()
    {
        if (DateTime.Now > this.GradualScoreTime)
        {
            this.GradualScoreTime = DateTime.Now + new TimeSpan(0, 0, 0, 0, 25);
            var score = this.Data.Common.TotalScore;
            var gradualValue = (int)(Math.Abs(score - this.GradualScore) * 0.20);
            if (this.GradualScore < score)
            {
                this.GradualScore += gradualValue;
                if (this.GradualScore > score)
                    this.GradualScore = score;
            }
            else if (this.GradualScore > score)
            {
                this.GradualScore -= gradualValue;
                if (this.GradualScore < score)
                    this.GradualScore = score;
            }
            if (gradualValue == 0)
                this.GradualScore = score;
        }
    }

    public override void PreOpenCheck()
    {
        this.OptionsUpdate();
        this.Data.Common.CalculateScore(this.Configuration.Score.ScoreCalculationType);
        this.ScoreFlyText();
        this.GradualScoreUpdate();
    }

    public override void Draw()
    {
        var config = this.Configuration.Score;
        var ui = this.Data.UI;
        ui.Scale = config.Scale;
        var textScale = 1.00f;

        var size = Vector2.Zero;
        if (config.FontType == FontType.Default)
            size = this.DrawDefault(config, ui);
        else if (config.FontType == FontType.Axis)
            size = this.DrawTextAxis(config, ui, textScale);
        else if (config.FontType == FontType.Miedinger)
            size = this.DrawTextMiedingerMid(config, ui, textScale);

        this.WindowSizeUpdate(size.X, size.Y, ui.Scale);
    }

    private Vector2 DrawDefault(Configuration.ScoreTab config, DataUI ui)
    {
        ui.Scale = config.Scale;
        var width = 224.0f;
        var height = 84.0f + (config.ShowTitle ? 0.0f : -30.0f);

        ui.DrawBackground(width, height, (!config.SolidBackground && this.IsFocused) || config.SolidBackground);

        var x = width - 14.0f;
        var y = 0.0f;

        if (config.ShowTitle)
        {
            y += 20.0f;
            ui.DrawTextMiedingerMid(width / 2.0f, y, "Score", Color.White, Alignment.Center);

            y += 14.0f;
            ui.DrawDivisorHorizontal(14.0f, y, width - 26.0f);

            y += 36.0f;
        }
        else
            y = 40.0f;

        ui.DrawNumber(x, y, this.GradualScore, true, config.TotalScoreColor, Alignment.Right);

        return new(width, height);
    }

    private Vector2 DrawTextAxis(Configuration.ScoreTab config, DataUI ui, float textScale)
    {
        ui.Scale = config.Scale;
        var width = 140.0f;
        var height = 78.0f + (config.ShowTitle ? 0.0f : -30.0f);

        ui.DrawBackground(width, height, (!config.SolidBackground && this.IsFocused) || config.SolidBackground);

        var x = width - 14.0f;
        var y = 0.0f;

        if (config.ShowTitle)
        {
            y += 20.0f;
            ui.DrawTextMiedingerMid(width / 2.0f, y, "Score", Color.White, Alignment.Center);

            y += 14.0f;
            ui.DrawDivisorHorizontal(14.0f, y, width - 26.0f);

            y += 30.0f;
        }
        else
            y = 33.0f;

        var scale = ui.Scale;
        ui.Scale = scale * textScale;
        ui.DrawTextAxis(x / textScale, y / textScale, this.GradualScore.ToString("N0", CultureInfo.InvariantCulture), config.TotalScoreColor, Alignment.Right);
        ui.Scale = scale;

        return new(width, height);
    }

    private Vector2 DrawTextMiedingerMid(Configuration.ScoreTab config, DataUI ui, float textScale)
    {
        ui.Scale = config.Scale;
        var width = 189.0f;
        var height = 78.0f + (config.ShowTitle ? 0.0f : -30.0f);

        ui.DrawBackground(width, height, (!config.SolidBackground && this.IsFocused) || config.SolidBackground);

        var x = width - 14.0f;
        var y = 0.0f;

        if (config.ShowTitle)
        {
            y += 20.0f;
            ui.DrawTextMiedingerMid(width / 2.0f, y, "Score", Color.White, Alignment.Center);

            y += 14.0f;
            ui.DrawDivisorHorizontal(14.0f, y, width - 26.0f);

            y += 32.0f;
        }
        else
            y = 36.0f;

        var scale = ui.Scale;
        ui.Scale = scale * textScale;
        ui.DrawTextMiedingerMidLarge(x / textScale, y / textScale, this.GradualScore.ToString("N0", CultureInfo.InvariantCulture), config.TotalScoreColor, Alignment.Right);
        ui.Scale = scale;

        return new(width, height);
    }
}