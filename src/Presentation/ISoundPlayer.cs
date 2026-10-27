using System.Media;

namespace MeadowPeaks;

public interface ISoundPlayer { /* Request a UI sound without coupling game commands to a platform audio API. */
    void Play(); }
public sealed class WindowsChime : ISoundPlayer { /* Play the Windows notification sound used by this native feedback adapter. */
    public void Play() => SystemSounds.Asterisk.Play(); }
