using Dalamud.Game.Gui.FlyText;
using ImGuiNET;
using System;
using System.Globalization;

namespace DeepDungeonTracker
{
    public sealed class ScoreWindow : WindowEx, IDisposable
    {
        private Data Data { get; }

        private int PreviousScore { get; set; }

        private int GradualScore { get; set; }

        private DateTime GradualScoreTime { get; set; }

        public ScoreWindow(string id, Configuration configuration, Data data) : base(id, configuration) => this.Data = data;

        public void Dispose() { }

        private void OptionsUpdate()
        {
            var config = this.Configuration.Score;
            this.IsOpen = this.Data.UI.CommonWindowVisibility(config.Show, config.ShowInBetweenFloors, this.Data.Common.IsInDeepDungeonRegion, this.Data.IsInsideDeepDungeon);
            this.Flags = config.Lock ? WindowEx.StaticNoBackground : WindowEx.StaticNoBackgroundMoveInputs;
        }

        private void ScoreFlyText()
        {
            var config = this.Configuration.Score;

            var scoreDifference = this.Data.Common.TotalScore - this.PreviousScore;
            if (config.IsFlyTextScoreVisible && this.Data.Common.EnableFlyTextScore && scoreDifference != 0)
                Service.FlyTextGui.AddFlyText(FlyTextKind.Named, 0, 0, 0, $"{(scoreDifference > 0 ? "+" : string.Empty)}{scoreDifference.ToString("N0", CultureInfo.InvariantCulture)}pts", string.Empty, ImGui.ColorConvertFloat4ToU32(scoreDifference > 0 ? config.FlyTextScoreColor : Color.Red), 0);

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
            this.Data.Common.CalculateScore(this.Configuration.Score.IncludeFloorCompletion);
            this.ScoreFlyText();
            this.GradualScoreUpdate();
        }

        public override void Draw()
        {
            var config = this.Configuration.Score;
            var ui = this.Data.UI;
            ui.Scale = config.Scale;
            var width = 224.0f;
            var height = 84.0f + (config.ShowTitle ? 0.0f : -30.0f);

            ui.DrawBackground(width, height, (!this.Configuration.General.SolidBackgroundWindow && this.IsFocused) || this.Configuration.General.SolidBackgroundWindow);

            var x = width - 14.0f;
            var y = 0.0f;

            if (config.ShowTitle)
            {
                y += 20.0f;
                ui.DrawTextMiedingerMediumW00(width / 2.0f, y, "Score", Color.White, Alignment.Center);

                y += 14.0f;
                ui.DrawDivisorHorizontal(14.0f, y, width - 26.0f);

                y += 36.0f;
            }
            else
                y = 40.0f;

            ui.DrawNumber(x, y, this.GradualScore, true, Color.White, Alignment.Right);
            this.Size = new(width * ui.Scale, height * ui.Scale);
        }
    }
}