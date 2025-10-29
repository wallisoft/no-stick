using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NoStick;

public class ISOEntry : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private string _location = string.Empty;
    private long _sizeInBytes;
    private string _filePath = string.Empty;
    private bool _isDefault;

    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged();
            }
        }
    }

    public string Location
    {
        get => _location;
        set
        {
            if (_location != value)
            {
                _location = value;
                OnPropertyChanged();
            }
        }
    }

    public long SizeInBytes
    {
        get => _sizeInBytes;
        set
        {
            if (_sizeInBytes != value)
            {
                _sizeInBytes = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SizeFormatted));
            }
        }
    }

    public string FilePath
    {
        get => _filePath;
        set
        {
            if (_filePath != value)
            {
                _filePath = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsDefault
    {
        get => _isDefault;
        set
        {
            if (_isDefault != value)
            {
                _isDefault = value;
                OnPropertyChanged();
            }
        }
    }

    public string SizeFormatted
    {
        get
        {
            if (_sizeInBytes < 1024)
                return $"{_sizeInBytes} B";
            
            double size = _sizeInBytes / 1024.0;
            if (size < 1024)
                return $"{size:F2} KB";
            
            size /= 1024.0;
            if (size < 1024)
                return $"{size:F2} MB";
            
            size /= 1024.0;
            return $"{size:F2} GB";
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
