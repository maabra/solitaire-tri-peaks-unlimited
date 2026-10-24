using System.Media;

namespace MeadowPeaks;

public interface ISoundPlayer { void Play(); }
public sealed class WindowsChime : ISoundPlayer { public void Play() => SystemSounds.Asterisk.Play(); }
