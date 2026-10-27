using System.Media;
using System.Text;

namespace MeadowPeaks;

public enum SoundCue { Card, Combo, Coins, Gems, Victory }
public interface IGameSoundtrack : ISoundPlayer { /* Request the sound cue through the replaceable soundtrack service. */
    void Play(SoundCue cue); }

// Original short melodies synthesized locally. No external music or audio files.
public sealed class MeadowSoundtrack : IGameSoundtrack, IDisposable
{
    readonly Dictionary<SoundCue, SoundPlayer> players = new();
    readonly List<MemoryStream> streams = new();
    // Prepare the native soundtrack resources owned by this player.
    public MeadowSoundtrack()
    {
        Make(SoundCue.Card, new[] { 523.25, 659.25 }, .055);
        Make(SoundCue.Combo, new[] { 659.25, 783.99, 1046.5 }, .09);
        Make(SoundCue.Coins, new[] { 1046.5, 1318.51, 1567.98 }, .075);
        Make(SoundCue.Gems, new[] { 783.99, 987.77, 1174.66 }, .065);
        Make(SoundCue.Victory, new[] { 523.25, 659.25, 783.99, 1046.5, 987.77, 1046.5 }, .13);
    }
    // Build the native audio data used by the soundtrack.
    void Make(SoundCue cue, double[] notes, double duration)
    {
        const int rate = 22050; int samples = (int)(rate * duration), count = samples * notes.Length;
        var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, Encoding.ASCII, true))
        {
            writer.Write(Encoding.ASCII.GetBytes("RIFF")); writer.Write(36 + count * 2); writer.Write(Encoding.ASCII.GetBytes("WAVEfmt ")); writer.Write(16); writer.Write((short)1); writer.Write((short)1); writer.Write(rate); writer.Write(rate * 2); writer.Write((short)2); writer.Write((short)16); writer.Write(Encoding.ASCII.GetBytes("data")); writer.Write(count * 2);
            foreach (double frequency in notes) for (int i = 0; i < samples; i++)
            {
                double t = i / (double)rate, envelope = Math.Min(1, i / (rate * .008)) * Math.Pow(1 - i / (double)samples, 2);
                writer.Write((short)((Math.Sin(2 * Math.PI * frequency * t) + Math.Sin(4 * Math.PI * frequency * t) * .14) * envelope * 4200));
            }
        }
        stream.Position = 0; streams.Add(stream); players[cue] = new SoundPlayer(stream);
    }
    // Play the requested scene track through this soundtrack owner.
    public void Play() => Play(SoundCue.Card);
    // Play the requested scene track through this soundtrack owner.
    public void Play(SoundCue cue)
    {
        try { players[cue].Play(); }
        catch (Exception ex) when (ex is InvalidOperationException || ex is System.ComponentModel.Win32Exception || ex is IOException) { }
    }
    // Release native audio resources owned by the soundtrack.
    public void Dispose() { foreach (var player in players.Values) player.Dispose(); foreach (var stream in streams) stream.Dispose(); }
}
