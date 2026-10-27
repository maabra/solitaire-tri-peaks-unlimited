namespace MeadowPeaks;

internal sealed record WorldPalette(Color Sky, Color Horizon, Color Grass, Color Accent, bool Night, bool Snow, bool Coast)
{
    static readonly (string sky, string horizon, string grass, string accent)[] Colors = {
        ("#BEEADD", "#FFF1C5", "#69A97E", "#F5A9BE"), ("#545F9E", "#D0D7EE", "#527E8B", "#D8C5FA"),
        ("#F9CFAB", "#FFF0C2", "#AD965D", "#ED9863"), ("#CCE8F1", "#F6F4F0", "#BCD9D6", "#C9A5DB"),
        ("#B9DDD3", "#E5F1CA", "#5C9C7D", "#F6CB91"), ("#D2D4F6", "#FCEDDC", "#8AAA91", "#D8ADE8"),
        ("#C7DFF1", "#FCE0E9", "#93AEBA", "#BFA0DE"), ("#F4D59E", "#FFF1BE", "#B4AB67", "#F4BC59"),
        ("#43466E", "#8B9AB8", "#5D828C", "#E4BFED"), ("#BFD9E7", "#FBE6C2", "#8DAD91", "#F2C588"),
        ("#C6E5EE", "#FFE5E4", "#81B6A3", "#F8AABC"), ("#ACE0F0", "#FFF3BD", "#97B36C", "#FFCD59"),
        ("#ACE5ED", "#FFDDCE", "#79B9AD", "#F2A899"), ("#414F72", "#A4C1B0", "#60867C", "#D7EA9C"),
        ("#F8D7C6", "#FFF0D5", "#A6BB8D", "#F3ACA6"), ("#C5D8F0", "#FFE9D9", "#88B3A1", "#D6A8EB"),
        ("#C3D8EA", "#E9EDDD", "#7AA6A3", "#A8A7EA"), ("#F7DDA7", "#FFF1CC", "#B0AC75", "#EDB35F"),
        ("#344A71", "#ADCFCB", "#AFCFD5", "#B9EDCF"), ("#BCE7E8", "#FFE4D1", "#8DBAAA", "#F7A8C4")
    };
    // Resolve the chapter palette used by the native scenery.
    public static WorldPalette For(int index)
    {
        int n = Math.Abs(index % Colors.Length); var c = Colors[n];
        return new(ColorTranslator.FromHtml(c.sky), ColorTranslator.FromHtml(c.horizon), ColorTranslator.FromHtml(c.grass), ColorTranslator.FromHtml(c.accent), n is 1 or 8 or 13 or 18, n is 3 or 18, n is 6 or 10 or 12 or 19);
    }
    // Interpolate color channels for native scene shading.
    public static Color Mix(Color a, Color b, float amount) => Color.FromArgb((int)(a.R * (1 - amount) + b.R * amount), (int)(a.G * (1 - amount) + b.G * amount), (int)(a.B * (1 - amount) + b.B * amount));
}
