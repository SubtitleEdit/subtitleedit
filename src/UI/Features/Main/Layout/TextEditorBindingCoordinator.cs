using System.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

public class TextEditorBindingCoordinator
{
    private readonly MainViewModel _vm;
    private readonly TextEditorBindingHelper? _textEditorHelper;
    private readonly TextEditorBindingHelper? _originalTextEditorHelper;
    private SubtitleLineViewModel? _currentlySubscribedSubtitle;

    public TextEditorBindingCoordinator(
        MainViewModel vm,
        TextEditorBindingHelper? textEditorHelper,
        TextEditorBindingHelper? originalTextEditorHelper)
    {
        _vm = vm;
        _textEditorHelper = textEditorHelper;
        _originalTextEditorHelper = originalTextEditorHelper;
    }

    public void Initialize()
    {
        _vm.PropertyChanged += OnViewModelPropertyChanged;
        SubscribeToSelectedSubtitle();
    }

    public void DeInitialize()
    {
        _vm.PropertyChanged -= OnViewModelPropertyChanged;
        UnsubscribeFromSelectedSubtitle();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_vm.SelectedSubtitle))
        {
            UnsubscribeFromSelectedSubtitle();
            SubscribeToSelectedSubtitle();
            
            _textEditorHelper?.OnSelectedSubtitleChanged();
            _originalTextEditorHelper?.OnSelectedSubtitleChanged();
        }
    }

    private void SubscribeToSelectedSubtitle()
    {
        if (_vm.SelectedSubtitle != null)
        {
            _currentlySubscribedSubtitle = _vm.SelectedSubtitle;
            _currentlySubscribedSubtitle.PropertyChanged += OnSelectedSubtitlePropertyChanged;
        }
    }

    private void UnsubscribeFromSelectedSubtitle()
    {
        if (_currentlySubscribedSubtitle != null)
        {
            _currentlySubscribedSubtitle.PropertyChanged -= OnSelectedSubtitlePropertyChanged;
            _currentlySubscribedSubtitle = null;
        }
    }

    private void OnSelectedSubtitlePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SubtitleLineViewModel.Text))
        {
            _textEditorHelper?.OnSubtitleTextPropertyChanged();
        }
        else if (e.PropertyName == nameof(SubtitleLineViewModel.OriginalText))
        {
            _originalTextEditorHelper?.OnSubtitleTextPropertyChanged();
        }
    }
}
