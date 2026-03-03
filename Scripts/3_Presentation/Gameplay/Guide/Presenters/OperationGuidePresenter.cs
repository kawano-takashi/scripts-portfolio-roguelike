using Roguelike.Presentation.Gameplay.Audio.Contracts;
using Roguelike.Presentation.Gameplay.Audio.Types;
using Roguelike.Presentation.Gameplay.Shell.Core;

namespace Roguelike.Presentation.Gameplay.Guide.Presenters
{
    /// <summary>
    /// 操作説明パネルの開閉を仲介する、Presentation層の軽量コントローラです。
    /// </summary>
    public sealed class OperationGuidePresenter
    {
        private readonly RunUiController _runUiController;
        private readonly IUiSoundPlayer _uiSoundPlayer;

        public OperationGuidePresenter(
            RunUiController runUiController,
            IUiSoundPlayer uiSoundPlayer)
        {
            _runUiController = runUiController;
            _uiSoundPlayer = uiSoundPlayer;
        }

        /// <summary>操作説明を開きます。</summary>
        public bool Open()
        {
            var wasOpen = _runUiController.IsGuideOpen.CurrentValue;
            var opened = _runUiController.OpenGuide();
            var isOpenNow = _runUiController.IsGuideOpen.CurrentValue;
            if (!wasOpen && isOpenNow)
            {
                _uiSoundPlayer.Play(UiSoundCue.UiOpen);
            }

            return opened;
        }

        /// <summary>操作説明を閉じます。</summary>
        public bool Close()
        {
            var wasOpen = _runUiController.IsGuideOpen.CurrentValue;
            var closed = _runUiController.CloseGuide();
            var isOpenNow = _runUiController.IsGuideOpen.CurrentValue;
            if (wasOpen && !isOpenNow)
            {
                _uiSoundPlayer.Play(UiSoundCue.UiClose);
            }

            return closed;
        }

        /// <summary>現在状態に応じて開閉を切り替えます。</summary>
        public void Toggle()
        {
            if (_runUiController.IsGuideOpen.CurrentValue)
            {
                Close();
                return;
            }

            Open();
        }
    }
}
