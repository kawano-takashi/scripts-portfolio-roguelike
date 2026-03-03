using Roguelike.Presentation.Gameplay.Audio.Contracts;
using Roguelike.Presentation.Gameplay.Audio.Types;

namespace Roguelike.Presentation.Gameplay.Audio.Services
{
    /// <summary>
    /// 効果音再生を無効化するフォールバック実装です。
    /// </summary>
    public sealed class NoopUiSoundPlayer : IUiSoundPlayer
    {
        public void Play(UiSoundCue _)
        {
        }
    }
}
