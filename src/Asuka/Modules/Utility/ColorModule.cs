using System;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
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
            IOptions<DiscordOptions> config) :
            base(config)
        {
        }

        [Command]
        [Remarks("color <hex>")]
        public async Task ColorAsync(SKColor hex)
        {
            await GetColorAsync(hex);
        }

        [Command]
        [Remarks("color <keywords>")]
        public async Task ColorAsync([Remainder] string keywords)
        {
            // Remove non-alphabetical characters and spaces from keywords.
            var regex = new Regex("[^a-zA-Z]");
            keywords = regex.Replace(keywords, "");
            keywords = string.Join("", keywords.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));

            // SKColors contains predefined named color objects.
            var type = typeof(SKColors);

            // Get color by keyword color name using reflection.
            FieldInfo info = type.GetField(keywords,
                BindingFlags.IgnoreCase |
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.Static);
            var temp = info?.GetValue(null);

            // Reflection was successful.
            if (temp is SKColor color)
            {
                await GetColorAsync(color);
                return;
            }

            await ReplyInlineAsync("Could not understand color keywords... (┬┬﹏┬┬)");
        }

        [Command]
        [Remarks("color <red> <green> <blue>")]
        public async Task ColorAsync(int r, int g, int b)
        {
            // Clamp rgb values between 0 and 255.
            r = Math.Clamp(r, 0, 255);
            g = Math.Clamp(g, 0, 255);
            b = Math.Clamp(b, 0, 255);

            // Create color object from rgb values.
            var color = new SKColor((byte) r, (byte) g, (byte) b);
            await GetColorAsync(color);
        }

        private async Task GetColorAsync(SKColor color)
        {
            // Force alpha to 255.
            color = color.WithAlpha(0xFF);

            // Get raw uint32 value from color by extracting rgb values.
            uint raw = (uint) ((color.Red << 16) | (color.Green << 8) | color.Blue);

            // Get HSL and HSV values.
            var hsl = new Vector3();
            var hsv = new Vector3();
            color.ToHsl(out hsl.X, out hsl.Y, out hsl.Z);
            color.ToHsv(out hsv.X, out hsv.Y, out hsv.Z);

            // Draw color to image.
            var bitmap = new SKBitmap(128, 128);
            var surface = SKSurface.Create(bitmap.Info);
            using (var canvas = surface.Canvas)
            {
                canvas.Clear(color);
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
                    $"`rgb({color.Red}, {color.Green}, {color.Blue})`")
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
