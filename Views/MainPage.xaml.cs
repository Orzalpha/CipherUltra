using CipherUltra.Models;
using CipherUltra.ViewModels;

using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;
using static CipherUltra.Models.TransferHelper;

namespace CipherUltra.Views;

public sealed partial class MainPage : Page
{
    private readonly Random random = new();
    private StorageFile inputFile;
    private StorageFile outputFile;
    private string key;


    public MainViewModel ViewModel
    {
        get;
    }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        //var key = "3d4c4be96a82fdaeb58f641db17b455b";
        //var iv = "84319aa8de6915ca1f6bda6bfbd8c766";
        //zuc = new ZUCHelper(HexStr2BitArr(key), HexStr2BitArr(iv));

        //zucForMAC= new ZUCHelper(HexStr2BitArr(key), HexStr2BitArr(iv));
        InitializeComponent();
    }

    private async void OkButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        TestTextblock.Text = string.Empty;
        // check input file
        if (inputFile == null || inputFile.Path != InputFilepath.Text)
        {
            if (string.IsNullOrEmpty(InputFilepath.Text))
            {
                TestTextblock.Text = "Please choose a file.";
                return;
            }
            else
            {
                try
                {
                    inputFile = await StorageFile.GetFileFromPathAsync(InputFilepath.Text);
                }
                catch (Exception ex)
                {
                    TestTextblock.Text = ex.Message;
                    return;
                }
            }
        }
        var outputpath = OutputFilepath.Text;
        if (string.IsNullOrEmpty(outputpath))
        {
            TestTextblock.Text = "Please set your output filepath！";
            return;
        }
        else
        {
            File.Create(outputpath).Close();
            outputFile = await StorageFile.GetFileFromPathAsync(outputpath);
        }

        // check key
        key = KeyTextbox.Text;
        if (string.IsNullOrEmpty(key))
        {
            TestTextblock.Text = "Please enter your key.";
            return;
        }
        else
        {
            try
            {
                HexStr2BitArr(key);
            }
            catch (Exception ex)
            {
                TestTextblock.Text = "Wrong key!";
                return;
            }
        }

        //  start work!
        StartWorkAsync();
        return;
    }

    private async void Pick_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var filePicker = new FileOpenPicker();
        // Use file picker like normal!
        filePicker.FileTypeFilter.Add("*");

        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(CipherUltra.App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hWnd);

        inputFile = await filePicker.PickSingleFileAsync();
        if (inputFile != null)
        {
            InputFilepath.Text = inputFile.Path;
            SetOutputTextbox();
        }
        else
        {
            TestTextblock.Text = "Operation cancelled.";
        }
        return;
    }

    private void FilepathTextbox_DragEnter(object sender, Microsoft.UI.Xaml.DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;
        e.Handled = true;
    }

    private async void FilepathTextbox_Drop(object sender, Microsoft.UI.Xaml.DragEventArgs e)
    {
        inputFile = (StorageFile)(await e.DataView.GetStorageItemsAsync())[0];
        InputFilepath.Text = inputFile.Path;
        SetOutputTextbox();
    }

    private void SetOutputTextbox()
    {
        if (ModeSelectorCombo.SelectedItem.ToString() == "Encryption")
        {
            var text = InputFilepath.Text;
            if (text != null)
            {
                text += ".zuc";
                OutputFilepath.Text = text;
            }
        }
        else
        {
            var text = InputFilepath.Text;
            if (text != null)
            {
                var length = text.Length;
                var suffix = text.Substring(length - 4, 4);
                if (suffix == ".zuc")
                {
                    OutputFilepath.Text = text.Substring(0, length - 4);
                }
            }
        }

        return;
    }

    private async void StartWorkAsync()
    {
        bool result;
        if (ModeSelectorCombo.SelectedItem.ToString() == "Encryption")
        {
            var zuc = new Zuc(HexStr2BitArr(key), Zuc.Mode.Encryption, inputFile, outputFile);
            result = await zuc.StartWork();
        }
        else
        {
            var zuc = new Zuc(HexStr2BitArr(key), Zuc.Mode.Decryption, inputFile, outputFile);
            result = await zuc.StartWork();
        }
        if (result == true)
        {
            TestTextblock.Text = "Successful!";
        }
        else
        {
            TestTextblock.Text = "Failed!";
        }
        return;
    }

    private void RandomKeyButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var str = string.Empty;
        for (var i = 0; i < 8; i++)
        {
            var num = random.Next(0, 65536);
            str += string.Format("{0:X4}", num);
        }
        key = str;
        KeyTextbox.Text = key;
        return;
    }

    private async void TestButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var filepath = "D:\\DevProj\\testoutput.txt";
        var file = await StorageFile.GetFileFromPathAsync(filepath);
        var stream = await file.OpenStreamForWriteAsync();
        var sw = new StreamWriter(stream);
        sw.Write("Here's another test text.");
        sw.Close();
        return;
    }
}
