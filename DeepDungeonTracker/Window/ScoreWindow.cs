using Dalamud.Game.Gui.FlyText;
using ImGuiNET;
using System;

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

        private void GradualScoreUpdate()
        {
            if (DateTime.Now > this.GradualScoreTime)
            {
                var score = this.Data.Common.Score;

                this.GradualScoreTime = DateTime.Now + new TimeSpan(0, 0, 0, 0, 25);

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
            var config = this.Configuration.Score;
            this.IsOpen = this.Data.UI.CommonWindowVisibility(config.Show, config.ShowInBetweenFloors, this.Data.Common.IsInDeepDungeonRegion, this.Data.IsInsideDeepDungeon);
            this.Flags = config.Lock ? WindowEx.StaticNoBackground : WindowEx.StaticNoBackgroundMoveInputs;

            this.Data.Common.Score = Score.Calculate(this.Data.Common.CurrentSaveSlot ?? new(), true, this.Data.Common.DeepDungeon);

            var scoreDifference = this.Data.Common.Score - this.PreviousScore;
            if (config.IsFlyTextScoreVisible && this.Data.Common.EnableFlyTextScore && scoreDifference != 0)
                Service.FlyTextGui.AddFlyText(FlyTextKind.Named, 0, 0, 0, $"{(scoreDifference > 0 ? "+" : string.Empty)}{scoreDifference}pts", string.Empty, ImGui.ColorConvertFloat4ToU32(scoreDifference > 0 ? config.FlyTextScoreColor : Color.Red), 0);

            this.PreviousScore = this.Data.Common.Score;

            this.GradualScoreUpdate();
        }

        public override void Draw()
        {
            var config = this.Configuration.Score;
            var ui = this.Data.UI;
            ui.Scale = config.Scale;
            var width = 224.0f;
            var height = 84.0f;

            ui.DrawBackground(width, height, (!this.Configuration.General.SolidBackgroundWindow && this.IsFocused) || this.Configuration.General.SolidBackgroundWindow);
            ui.DrawTextMiedingerMediumW00(width / 2.0f, 20.0f, "Score", Color.White, Align.Center);
            ui.DrawDivisorHorizontal(14.0f, 34.0f, width - 26.0f);

            var x = width - 14.0f;
            var y = 70.0f;

            ui.DrawNumber(x, y, this.GradualScore, true, Color.White, Align.Right);
            this.Size = new(width * ui.Scale, height * ui.Scale);
        }
    }
}