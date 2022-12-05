using ImGuiNET;
using ImGuiScene;
using System;
using System.Linq;
using System.Numerics;

namespace DeepDungeonTracker
{
    public class Render
    {
        public float Scale { get; set; } = 1.0f;

        private void DrawObject(TextureWrap textureWrap, float x, float y, float width, float height, float x1, float y1, float x2, float y2, float innerScale, Vector4? color = null)
        {
            var posScale = 1.0f / innerScale;
            var finalScale = this.Scale * innerScale;
            ImGui.SetCursorPos(new(x * finalScale * posScale, y * finalScale * posScale));
            ImGui.Image(textureWrap.ImGuiHandle, new(width * finalScale, height * finalScale), new(x1, y1), new(x2, y2), color ?? Color.White);
        }

        public void DrawText(Font font, float x, float y, string text, Vector4 color, Alignment align, bool drawShadow)
        {
            var fontLayout = font.FontLayout;
            var textSize = (align != Alignment.Left) ? Render.GetTextSize(fontLayout, text) : Vector2.Zero;

            var atlasWidth = fontLayout.Atlas!.Width;
            var atlasHeight = fontLayout.Atlas.Height;
            var atlasSize = fontLayout.Atlas.Size;
            var advance = 0.0;

            var alignFactor = (align == Alignment.Center) ? 2.0f : 1.0f;

            var baseX = x + (-textSize.X / alignFactor);
            var baseY = y + 1.0f + ((fontLayout.Metrics!.Ascender + fontLayout.Metrics.Descender) * atlasSize) - (textSize.Y / alignFactor);

            foreach (var item in text)
            {
                var glyph = fontLayout.Glyphs!.FirstOrDefault(x => x.Unicode == item);
                if (glyph == null)
                    continue;

                if (!char.IsWhiteSpace(item))
                {
                    var ab = glyph.AtlasBounds!;
                    var pb = glyph.PlaneBounds!;

                    x = (float)(baseX + (pb.Left * atlasSize) + advance);
                    y = (float)(baseY + (-pb.Top * atlasSize));

                    var width = (float)(ab.Right - ab.Left);
                    var height = (float)(ab.Top - ab.Bottom);

                    var x1 = (float)(ab.Left / atlasWidth);
                    var y1 = (float)((atlasHeight - ab.Top) / atlasHeight);
                    var x2 = (float)(ab.Right / atlasWidth);
                    var y2 = (float)((atlasHeight - ab.Bottom) / atlasHeight);

                    this.DrawObject(font.TextureAtlas, x, y, width, height, x1, y1, x2, y2, 1.0f, color);
                }
                advance += glyph.Advance * atlasSize;
            }
        }

        public static Vector2 GetTextSize(FontLayout fontLayout, string text)
        {
            var atlasSize = fontLayout.Atlas!.Size;
            var advance = 0.0;
            var maxHeight = (fontLayout.Metrics!.Ascender + fontLayout.Metrics.Descender) * atlasSize;

            foreach (var item in text)
            {
                var glyph = fontLayout.Glyphs!.FirstOrDefault(x => x.Unicode == item);
                if (glyph == null)
                    continue;

                advance += glyph.Advance * atlasSize;
            }
            return new((float)advance, (float)maxHeight);
        }

        public void DrawUIElement(TextureWrap textureWrap, float x, float y, float innerScale, int id, int horizontalElements, int verticalElements, Vector4? color = null, Alignment align = Alignment.Left, bool mirrorHorizontal = false)
        {
            var tWidth = (float)textureWrap.Width;
            var tHeight = (float)textureWrap.Height;
            var width = tWidth / horizontalElements;
            var height = tHeight / verticalElements;

            var mod = (int)tWidth / (int)width;
            var tOffsetX = (id % mod) * width;
            var tOffsetY = (id / mod) * height;

            var x1 = tOffsetX / tWidth;
            var y1 = tOffsetY / tHeight;
            var x2 = (tOffsetX + width) / tWidth;
            var y2 = (tOffsetY + height) / tHeight;

            if (align == Alignment.Center)
            {
                x -= width / 2.0f * innerScale;
                y -= height / 2.0f * innerScale;
            }
            else if (align == Alignment.Right)
            {
                x -= width * innerScale;
                y -= height * innerScale;
            }

            if (mirrorHorizontal)
                (x2, x1) = (x1, x2);

            this.DrawObject(textureWrap, x, y, width, height, x1, y1, x2, y2, innerScale, color);
        }

        public void DrawNumber(TextureWrap textureWrap, float x, float y, float innerScale, int number, Vector4? color, Alignment align)
        {
            var baseX = x;
            var baseY = y;
            var horizontalElements = 10;
            var verticalElements = 1;
            var width = textureWrap.Width / horizontalElements;
            var height = textureWrap.Height / verticalElements;
            var numberString = number.ToString();
            var totalWidth = width * numberString.Length;

            if (align == Alignment.Center)
            {
                baseX = x - (totalWidth / 2.0f * innerScale);
                baseY = y - (height / 2.0f * innerScale);
            }
            else if (align == Alignment.Right)
            {
                baseX = x - (totalWidth * innerScale);
                baseY = y - (height * innerScale);
            }

            foreach (var item in numberString)
            {
                var index = item - '0';
                this.DrawUIElement(textureWrap, baseX, baseY, innerScale, index, horizontalElements, verticalElements, color);
                baseX += width * innerScale;
            }
        }

        public void DrawDivisorHorizontal(TextureWrap textureWrap, float x, float y, float width)
        {
            var offset = 4.0f;
            width -= offset * 2.0f;
            var tWidth = (float)textureWrap.Width;

            this.DrawObject(textureWrap, x, y, offset, offset, 0.0f, 0.0f, offset / tWidth, 1.0f, 1.0f);

            x += offset;
            this.DrawObject(textureWrap, x, y, width, offset, offset / tWidth, 0.0f, (tWidth - offset) / tWidth, 1.0f, 1.0f);

            x += width;
            this.DrawObject(textureWrap, x, y, offset, offset, (tWidth - offset) / tWidth, 0.0f, 1.0f, 1.0f, 1.0f);
        }

        public void DrawDivisorVertical(TextureWrap textureWrap, float x, float y, float height)
        {
            var offset = 4.0f;
            height -= offset * 2.0f;
            var tHeight = (float)textureWrap.Height;

            this.DrawObject(textureWrap, x, y, offset, offset, 0.0f, 0.0f, 1.0f, offset / tHeight, 1.0f);

            y += offset;
            this.DrawObject(textureWrap, x, y, offset, height, 0.0f, offset / tHeight, 1.0f, (tHeight - offset) / tHeight, 1.0f);

            y += height;
            this.DrawObject(textureWrap, x, y, offset, offset, 0.0f, (tHeight - offset) / tHeight, 1.0f, 1.0f, 1.0f);
        }

        public void DrawBackground(TextureWrap textureWrap, float width, float height, bool isFocused)
        {
            var baseX = 0.0f;
            var baseY = 0.0f;
            var cornerSize = 11.0f;

            var innerWidth = Math.Max((cornerSize * 2) + 1, width - (cornerSize * 2.0f));
            var innerHeight = Math.Max((cornerSize * 2) + 1, height - (cornerSize * 2.0f));

            var tWidth = (float)textureWrap.Width;
            var tHeight = (float)textureWrap.Height;

            var tInnerWidth = (tWidth / 2.0f) - (cornerSize * 2);
            var tInnerHeight = tHeight - (cornerSize * 2);

            var tOffsetX = !isFocused ? (tWidth / 2.0f) : 0.0f;

            var innerScale = 1.0f;

            //Left x Top
            var x = baseX;
            var y = baseY;
            this.DrawObject(textureWrap, x, y, cornerSize, cornerSize, tOffsetX / tWidth, 0.0f, (tOffsetX + cornerSize) / tWidth, cornerSize / tHeight, innerScale);

            //Center x Top
            x += cornerSize;
            this.DrawObject(textureWrap, x, y, innerWidth, cornerSize, (tOffsetX + cornerSize) / tWidth, 0.0f, (tOffsetX + cornerSize + tInnerWidth) / tWidth, cornerSize / tHeight, innerScale);

            //Right x Top
            x += innerWidth;
            this.DrawObject(textureWrap, x, y, cornerSize, cornerSize, (tOffsetX + cornerSize + tInnerWidth) / tWidth, 0.0f, (tOffsetX + (cornerSize * 2.0f) + tInnerWidth) / tWidth, cornerSize / tHeight, innerScale);

            //Left x Center
            x = baseX;
            y += cornerSize;
            this.DrawObject(textureWrap, x, y, cornerSize, innerHeight, tOffsetX / tWidth, cornerSize / tHeight, (tOffsetX + cornerSize) / tWidth, (cornerSize + tInnerHeight) / tHeight, innerScale);

            //Center
            x += cornerSize;
            this.DrawObject(textureWrap, x, y, innerWidth, innerHeight, (tOffsetX + cornerSize) / tWidth, cornerSize / tHeight, (tOffsetX + cornerSize + tInnerWidth) / tWidth, (cornerSize + tInnerHeight) / tHeight, innerScale);

            //Right x Center
            x += innerWidth;
            this.DrawObject(textureWrap, x, y, cornerSize, innerHeight, (tOffsetX + cornerSize + tInnerWidth) / tWidth, cornerSize / tHeight, (tOffsetX + (cornerSize * 2.0f) + tInnerWidth) / tWidth, (cornerSize + tInnerHeight) / tHeight, innerScale);

            //Left x Bottom
            x = baseX;
            y += innerHeight;
            this.DrawObject(textureWrap, x, y, cornerSize, cornerSize, tOffsetX / tWidth, (tHeight - cornerSize) / tHeight, (tOffsetX + cornerSize) / tWidth, ((tHeight - cornerSize) + cornerSize) / tHeight, innerScale);

            //Center x Bottom
            x += cornerSize;
            this.DrawObject(textureWrap, x, y, innerWidth, cornerSize, (tOffsetX + cornerSize) / tWidth, (tHeight - cornerSize) / tHeight, (tOffsetX + cornerSize + tInnerWidth) / tWidth, tHeight / tHeight, innerScale);

            //Right x Bottom
            x += innerWidth;
            this.DrawObject(textureWrap, x, y, cornerSize, cornerSize, (tOffsetX + cornerSize + tInnerWidth) / tWidth, (tHeight - cornerSize) / tHeight, (tOffsetX + (cornerSize * 2.0f) + tInnerWidth) / tWidth, tHeight / tHeight, innerScale);
        }
    }
}