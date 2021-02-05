using System;
using System.Numerics;
using System.Threading.Tasks;
using Asuka.Commands;
using Asuka.Configuration;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Options;
using SkiaSharp;

namespace Asuka.Modules.Utility
{
    [Group("color")]
    [Remarks("Utility")]
    [Summary("Get the color from hex code or RGB.")]
    public class ColorModule : CommandModuleBase
    {
        public ColorModule(
            IOptions<DiscordOptions> config)
            : base(config)
        {
        }

        [Command]
        [Name("")]
        [Priority(0)]
        public async Task ColorAsync(string hex)
        {
            // Ignore pound sign hex prefix.
            if (hex.StartsWith("#"))
            {
                hex = hex.Substring(1);
            }

            if (hex.Length == 3)
            {
                // Expand 3 digit hex to 6 digits.
                var chars = new[] {hex[0], hex[0], hex[1], hex[1], hex[2], hex[2]};
                hex = new string(chars);
            }
            else if (hex.Length != 6)
            {
                await ReplyAsync("Hex code needs to be 6 characters.");
                return;
            }

            // Convert hex to rgb then execute command.
            var bytes = Convert.FromHexString(hex);
            uint raw = (uint) ((bytes[0] << 16) | (bytes[1] << 8) | bytes[2]);
            await ColorAsync(raw);
        }

        [Command]
        [Name("")]
        [Priority(1)]
        public async Task ColorAsync(int r, int g, int b)
        {
            // Clamp rgb values between 0 and 255.
            r = Math.Clamp(r, 0, 255);
            g = Math.Clamp(g, 0, 255);
            b = Math.Clamp(b, 0, 255);

            // Convert rgb values to raw hex.
            uint raw = (uint) ((r << 16) | (g << 8) | b);
            await ColorAsync(raw);
        }

        public async Task ColorAsync(uint raw)
        {
            // Create SKColor object to draw on SKCanvas and get color info.
            var skColor = new SKColor(raw).WithAlpha(0xFF);

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
                canvas.Flush();
            }

            // Encode image to stream for uploading.
            var stream = surface.Snapshot().Encode().AsStream();

            // Use hex code as filename but omit the pound sign.
            var fileName = $"{raw:X6}.png";

            // Construct embed with color information and thumbnail.
            var embed = new EmbedBuilder()
                .WithThumbnailUrl($"attachment://{fileName}")
                .WithColor(raw)
                .AddField(
                    // Strip alpha from hex.
                    "HEX",
                    $"`#{raw:X6}`")
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
