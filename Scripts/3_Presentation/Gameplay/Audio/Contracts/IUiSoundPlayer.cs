using Roguelike.Presentation.Gameplay.Audio.Types;

namespace Roguelike.Presentation.Gameplay.Audio.Contracts
{
    /// <summary>
    /// UI効果音の再生を抽象化します。
    /// </summary>
    public interface IUiSoundPlayer
    {
        void Play(UiSoundCue cue);
    }
}
