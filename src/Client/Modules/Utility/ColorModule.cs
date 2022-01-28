using System;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Asuka.Configuration;
using Asuka.Interactions;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SkiaSharp;

namespace Asuka.Modules.Utility;

[Group(
    "color",
    "Get the color from hex code, RGB, or keywords.")]
public class ColorModule : InteractionModule
{
    public ColorModule(
        IOptions<DiscordOptions> config,
        ILogger<ColorModule> logger) :
        base(config, logger)
    {
    }

    [SlashCommand(
        "hex",
        "Get the color from hex code.")]
    public async Task ColorAsync(SKColor hex)
    {
        await GetColorAsync(hex);
    }

    [SlashCommand(
        "rgb",
        "Get the color from RGB.")]
    public async Task ColorAsync(int red, int green, int blue)
    {
        // Clamp rgb values between 0 and 255.
        red = Math.Clamp(red, 0, 255);
        green = Math.Clamp(green, 0, 255);
        blue = Math.Clamp(blue, 0, 255);

        // Create color object from rgb values.
        var color = new SKColor((byte) red, (byte) green, (byte) blue);
        await GetColorAsync(color);
    }

    [SlashCommand(
        "keywords",
        "Get the color from keywords.")]
    public async Task ColorAsync(string keywords)
    {
        // Remove non-alphabetical characters and spaces from keywords.
        var regex = new Regex("[^a-zA-Z]");
        keywords = regex.Replace(keywords, "");
        keywords = string.Join("", keywords.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));

        // SKColors contains predefined named color objects.
        var type = typeof(SKColors);

        // Get color by keyword color name using reflection.
        FieldInfo? info = type.GetField(
            keywords,
            BindingFlags.IgnoreCase |
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.Static);
        object? temp = info?.GetValue(null);

        // Reflection was successful.
        if (temp is SKColor color)
        {
            await GetColorAsync(color);
            return;
        }

        await RespondAsync("Could not understand color keywords... (┬┬﹏┬┬)");
    }

    private async Task GetColorAsync(SKColor color)
    {
        // Force alpha to 255.
        color = color.WithAlpha(0xFF);

        // Get raw uint32 value from color by extracting rgb values.
        uint raw = (uint) (color.Red << 16 | color.Green << 8 | color.Blue);

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
        string fileName = $"{raw.ToString("X6")}.png";

        // Construct embed with color information and thumbnail.
        var embed = new EmbedBuilder()
            .WithThumbnailUrl($"attachment://{fileName}")
            .WithColor(raw)
            .AddField(
                // Strip alpha from hex.
                "HEX",
                $"`#{raw.ToString("X6")}`")
            .AddField(
                "RGB",
                $"`rgb({color.Red.ToString()}, {color.Green.ToString()}, {color.Blue.ToString()})`")
            .AddField(
                "HSL",
                $"**H**: {hsl.X.ToString("F2")}, **S**: {hsl.Y.ToString("F2")}, **L**: {hsl.Z.ToString("F2")}")
            .AddField(
                "HSV",
                $"**H**: {hsv.X.ToString("F2")}, **S**: {hsv.Y.ToString("F2")}, **V**: {hsv.Z.ToString("F2")}")
            .Build();

        await Context.Interaction.RespondWithFileAsync(stream, fileName, embed: embed);
    }
}
