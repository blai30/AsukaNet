using System;
using System.Numerics;
using System.Threading.Tasks;
using Asuka.Commands;
using Discord;
using Discord.Commands;
using SkiaSharp;

namespace Asuka.Modules.Utility
{
    public class ColorModule : CommandModuleBase
    {
        [Command("color")]
        [Summary("Get the color from hex code or RGB.")]
        public async Task ColorAsync(string hex)
        {
            if (hex.StartsWith("#"))
            {
                hex = hex.Substring(1);
            }

            uint value = Convert.ToUInt32(hex, 16);
            var color = new Color(value);
            await ColorAsync(color.R, color.G, color.B);
        }

        [Command("color")]
        [Summary("Get the color from hex code or RGB.")]
        public async Task ColorAsync(byte r = 0, byte g = 0, byte b = 0)
        {
            // Create color objects.
            var skColor = new SKColor(r, g, b);
            var color = new Color(r, g, b);

            // Get HSL and HSV values.
            var hsl = new Vector3();
            var hsv = new Vector3();
            skColor.ToHsl(out hsl.X, out hsl.Y, out hsl.Z);
            skColor.ToHsv(out hsv.X, out hsv.Y, out hsv.Z);

            // Draw color to image.
            var bitmap = new SKBitmap(128, 128);
            var surface = SKSurface.Create(bitmap.Info);
            using (var canvas = surface.Canvas)
            {
                canvas.Clear(skColor);
            }

            // Encode image to stream for uploading.
            var stream = surface.Snapshot().Encode().AsStream();

            // Use hex code as filename but omit the pound sign.
            var fileName = $"{skColor.ToString().Substring(1).ToUpper()}.png";

            // Construct embed with color information and thumbnail.
            var embed = new EmbedBuilder()
                .WithThumbnailUrl($"attachment://{fileName}")
                .WithColor(color)
                .AddField(
                    // Strip alpha from hex.
                    "Hex code",
                    $"`#{skColor.ToString().Substring(3).ToUpper()}`")
                .AddField(
                    "RGB",
                    $"`rgb({skColor.Red}, {skColor.Green}, {skColor.Blue})`")
                .AddField(
                    "HSL",
                    $"**H**: {hsl.X:F2}, **S**: {hsl.Y:F2}, **L**: {hsl.Z:F2}")
                .AddField(
                    "HSV",
                    $"**H**: {hsv.X:F2}, **S**: {hsv.Y:F2}, **V**: {hsv.Z:F2}")
                .Build();

            await Context.Channel.SendFileAsync(stream, fileName, embed: embed);
        }
    }
}
