using Dalamud.Interface.Internal;
using Dalamud.Interface.ManagedFontAtlas;
using ImGuiNET;
using System;
using System.Globalization;
using System.Numerics;

namespace DeepDungeonTracker;

public class Render
{
    public float Scale { get; set; } = 1.0f;

    private void DrawObject(IDalamudTextureWrap textureWrap, float x, float y, float width, float height, float x1, float y1, float x2, float y2, float innerScale, Vector4? color = null)
    {
        var posScale = 1.0f / innerScale;
        var finalScale = this.Scale * innerScale;
        ImGui.SetCursorPos(new(x * finalScale * posScale, y * finalScale * posScale));
        ImGui.Image(textureWrap.ImGuiHandle, new(width * finalScale, height * finalScale), new(x1, y1), new(x2, y2), color ?? Color.White);
    }

    public Vector2 DrawText(IFontHandle fontHandle, float x, float y, string text, Vector4 color, Alignment align, bool calcTextSize = false)
    {
        fontHandle?.Push();

        var size = (align != Alignment.Left) ? ImGui.CalcTextSize(text) : Vector2.Zero * this.Scale;
        var alignFactor = (align == Alignment.Center) ? 2.0f : 1.0f;
        var distanceX = (align == Alignment.Center) ? (size.X / 2.0f) : (align == Alignment.Right) ? size.X : 0.0f;
        var distanceY = (align == Alignment.Center) ? (size.Y / 2.0f) : (align == Alignment.Right) ? size.Y : 0.0f;
        x = (x - (size.X / alignFactor)) * this.Scale;
        y = (y - (size.Y / alignFactor)) * this.Scale;
        x += distanceX * (this.Scale - 1.0f);
        y += distanceY * (this.Scale - 1.0f);

        ImGui.SetWindowFontScale(this.Scale);
        ImGui.SetCursorPos(new Vector2(x, y));
        ImGui.TextColored(color, text);
        fontHandle?.Pop();

        return calcTextSize ? this.GetTextSize(fontHandle!, text) : Vector2.Zero;
    }

    public Vector2 GetTextSize(IFontHandle fontHandle, string text)
    {
        fontHandle?.Push();
        var size = ImGui.CalcTextSize(text) / this.Scale;
        fontHandle?.Pop();
        return size;
    }

    public void DrawUIElement(IDalamudTextureWrap textureWrap, float x, float y, float innerScale, int id, int horizontalElements, int verticalElements, Vector2? offset = null, Vector2? size = null, Vector4? color = null, Alignment align = Alignment.Left, bool mirrorHorizontal = false, bool mirrorVertical = false)
    {
        if (textureWrap == null)
            return;

        var tWidth = (float)textureWrap.Width;
        var tHeight = (float)textureWrap.Height;

        if (offset == null)
            offset = new Vector2(0.0f, 0.0f);

        if (size == null)
            size = new Vector2(tWidth, tHeight);

        var width = size.Value.X / horizontalElements;
        var height = size.Value.Y / verticalElements;

        var mod = (int)tWidth / (int)width;
        var tOffsetX = offset.Value.X + ((id % mod) * width);
        var tOffsetY = offset.Value.Y + ((id / mod) * height);

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

        if (mirrorVertical)
            (y2, y1) = (y1, y2);

        this.DrawObject(textureWrap, x, y, width, height, x1, y1, x2, y2, innerScale, color);
    }

    public void DrawNumber(IDalamudTextureWrap textureWrap, float x, float y, float innerScale, int number, Vector4? color, Alignment align)
    {
        if (textureWrap == null)
            return;

        var baseX = x;
        var baseY = y;
        var horizontalElements = 10;
        var verticalElements = 1;
        var w = 560.0f;
        var h = 50.0f;
        var width = w / horizontalElements;
        var height = h / verticalElements;
        var numberString = number.ToString(CultureInfo.InvariantCulture);
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
            this.DrawUIElement(textureWrap, baseX, baseY, innerScale, index, horizontalElements, verticalElements, new(2.0f, 100.0f), new(w, h), color);
            baseX += width * innerScale;
        }
    }

    public void DrawTextButtonOver(IDalamudTextureWrap textureWrap, float x, float y, float width, float height)
    {
        if (textureWrap != null)
        {
            void Draw(float x, float y, float width, float height, float tX, float tY, float tW, float tH)
            {
                var color = Color.White;

                var tWidth = (float)textureWrap.Width;
                var tHeight = (float)textureWrap.Height;

                var tOffsetX = tX;
                var tOffsetY = tY;

                var x1 = tOffsetX / tWidth;
                var y1 = tOffsetY / tHeight;
                var x2 = (tOffsetX + tW) / tWidth;
                var y2 = (tOffsetY + tH) / tHeight;

                this.DrawObject(textureWrap, x, y, width, height, x1, y1, x2, y2, 1.0f, new(color.X, color.Y, color.Z, 0.25f));
            }

            var offsetX = 23.0f;
            width *= 1.025f;
            Draw(x, y, offsetX, height, 77.0f, 236.0f, offsetX, 48.0f);
            Draw(x + offsetX, y, width - offsetX, height, 77.0f + offsetX, 236.0f, 134.0f - offsetX, 48.0f);
        }
    }

    public void DrawBar(IDalamudTextureWrap textureWrap, float x, float y, float width, float height)
    {
        if (textureWrap != null)
        {
            void Draw(float x, float y, float width, float height, float tX, float tY, float tW, float tH)
            {
                var tWidth = (float)textureWrap.Width;
                var tHeight = (float)textureWrap.Height;

                var tOffsetX = tX;
                var tOffsetY = tY;

                var x1 = tOffsetX / tWidth;
                var y1 = tOffsetY / tHeight;
                var x2 = (tOffsetX + tW) / tWidth;
                var y2 = (tOffsetY + tH) / tHeight;

                this.DrawObject(textureWrap, x, y, width, height, x1, y1, x2, y2, 1.0f);
            }

            var offsetX = 8.0f;
            var textureX = 95.0f;
            var textureY = 287.0f;
            var textureW = 116.0f;
            var textureH = 18.0f;
            var innerTextureW = textureW - (offsetX * 2.0f);
            Draw(x, y, width, height, textureX + offsetX, textureY, innerTextureW, textureH);
            Draw(x - offsetX, y, offsetX, height, textureX, textureY, offsetX, textureH);
            Draw(x + width, y, offsetX, height, textureX + offsetX + innerTextureW, textureY, offsetX, textureH);
        }
    }

    public void DrawDivisorHorizontal(IDalamudTextureWrap textureWrap, float x, float y, float width)
    {
        if (textureWrap == null)
            return;

        var offset = 4.0f;
        width -= offset * 2.0f;
        var textureX = 326.0f;
        var textureY = 2.0f;
        var textureW = 32.0f;
        var tWidth = (float)textureWrap.Width;
        var tHeight = (float)textureWrap.Height;

        var x1 = textureX / tWidth;
        var x2 = x1 + (offset / tWidth);
        var x3 = x2 + ((textureW - (offset * 2.0f)) / tWidth);
        var x4 = x3 + (offset / tWidth);
        var y1 = textureY / tHeight;
        var y2 = y1 + (offset / tHeight);

        this.DrawObject(textureWrap, x, y, offset, offset, x1, y1, x2, y2, 1.0f);

        x += offset;
        this.DrawObject(textureWrap, x, y, width, offset, x2, y1, x3, y2, 1.0f);

        x += width;
        this.DrawObject(textureWrap, x, y, offset, offset, x3, y1, x4, y2, 1.0f);
    }

    public void DrawDivisorVertical(IDalamudTextureWrap textureWrap, float x, float y, float height)
    {
        if (textureWrap == null)
            return;

        var offset = 4.0f;
        height -= offset * 2.0f;
        var textureX = 457.0f;
        var textureY = 33.0f;
        var textureH = 32.0f;
        var tWidth = (float)textureWrap.Width;
        var tHeight = (float)textureWrap.Height;

        var x1 = textureX / tWidth;
        var x2 = x1 + (offset / tWidth);
        var y1 = textureY / tHeight;
        var y2 = y1 + (offset / tHeight);
        var y3 = y2 + ((textureH - (offset * 2.0f)) / tHeight);
        var y4 = y3 + (offset / tHeight);

        this.DrawObject(textureWrap, x, y, offset, offset, x1, y1, x2, y2, 1.0f);

        y += offset;
        this.DrawObject(textureWrap, x, y, offset, height, x1, y2, x2, y3, 1.0f);

        y += height;
        this.DrawObject(textureWrap, x, y, offset, offset, x1, y3, x2, y4, 1.0f);
    }

    public void DrawBackground(IDalamudTextureWrap textureWrap, float width, float height, bool isFocused)
    {
        if (textureWrap == null)
            return;

        var baseX = 0.0f;
        var baseY = 0.0f;
        var cornerSize = 11.0f;

        var innerWidth = Math.Max((cornerSize * 2) + 1, width - (cornerSize * 2.0f));
        var innerHeight = Math.Max((cornerSize * 2) + 1, height - (cornerSize * 2.0f));

        var tWidth = (float)textureWrap.Width;
        var tHeight = (float)textureWrap.Height;

        var innerScale = 1.0f;

        var textureX = 2.0f;
        var textureY = 2.0f;
        var textureW = 60.0f;
        var textureH = 86.0f;

        var tOffsetX = !isFocused ? (textureW / 2.0f) : 0.0f;
        var tInnerWidth = (textureW / 2.0f) - (cornerSize * 2);
        var tInnerHeight = textureH - (cornerSize * 2);

        var x1 = (tOffsetX + textureX) / tWidth;
        var x2 = x1 + (cornerSize / tWidth);
        var x3 = x2 + (tInnerWidth / tWidth);
        var x4 = x3 + (cornerSize / tWidth);
        var y1 = textureY / tHeight;
        var y2 = y1 + (cornerSize / tHeight);
        var y3 = y2 + (tInnerHeight / tHeight);
        var y4 = y3 + (cornerSize / tHeight);

        //Left x Top
        var x = baseX;
        var y = baseY;
        this.DrawObject(textureWrap, x, y, cornerSize, cornerSize, x1, y1, x2, y2, innerScale);

        //Center x Top
        x += cornerSize;
        this.DrawObject(textureWrap, x, y, innerWidth, cornerSize, x2, y1, x3, y2, innerScale);

        //Right x Top
        x += innerWidth;
        this.DrawObject(textureWrap, x, y, cornerSize, cornerSize, x3, y1, x4, y2, innerScale);

        //Left x Center
        x = baseX;
        y += cornerSize;
        this.DrawObject(textureWrap, x, y, cornerSize, innerHeight, x1, y2, x2, y3, innerScale);

        //Center
        x += cornerSize;
        this.DrawObject(textureWrap, x, y, innerWidth, innerHeight, x2, y2, x3, y3, innerScale);

        //Right x Center
        x += innerWidth;
        this.DrawObject(textureWrap, x, y, cornerSize, innerHeight, x3, y2, x4, y3, innerScale);

        //Left x Bottom
        x = baseX;
        y += innerHeight;
        this.DrawObject(textureWrap, x, y, cornerSize, cornerSize, x1, y3, x2, y4, innerScale);

        //Center x Bottom
        x += cornerSize;
        this.DrawObject(textureWrap, x, y, innerWidth, cornerSize, x2, y3, x3, y4, innerScale);

        //Right x Bottom
        x += innerWidth;
        this.DrawObject(textureWrap, x, y, cornerSize, cornerSize, x3, y3, x4, y4, innerScale);
    }
}