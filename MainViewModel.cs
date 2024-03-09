using System;
using System.IO;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Microsoft.VisualBasic.FileIO;

namespace WpfProjectCleanerMvvmNet5
{
public class ShowDialogEventArgs : EventArgs
{
    private string dialog = string.Empty;
    public string Dialog { get => dialog; set => dialog = value; }
    private bool _result = false;
    public bool Result { get => _result; set => _result = value; }
}


public class MessageEventArgs : EventArgs
{
    public string Message { get; set; }
    public string Caption { get; set; } = string.Empty;
    public MessageBoxButton Buttons { get; set; } = MessageBoxButton.OK;
    public MessageBoxImage Icons { get; set; } = MessageBoxImage.Question;
}

public class TestModel: ViewModelBase
{
    private bool _check;
    public bool Check { get => _check; set { _check = value; OnPropertyChanged(); } }
    private string _file;
    public string File { get => _file; set { _file = value; OnPropertyChanged();} }
}

public class MainViewModel : ViewModelBase
{
    private readonly BackgroundWorker searchWorker = new();
    private BackgroundWorker deleteWorker = new();
    public string Title { get; set; }

    private List<string> listOfFilesToDelete = new();//List of files to delete
    public List<string> ListOfFilesToDelete { get => listOfFilesToDelete; set => listOfFilesToDelete = value; }

    public List<string> ListSelectedExt = new();//List of selected extensions

    public ObservableCollection<Extension> ExtensionCol { get; set; } = new ObservableCollection<Extension>();

    public ObservableCollection<TestModel> TestViewData { get; set; } = new ObservableCollection<TestModel>();

    public event EventHandler<ShowDialogEventArgs> ShowMyDialog;
    public event EventHandler<MessageEventArgs> ShowMsgBox;

    private bool bdeletingWork = false;//if deleting files deleteWorker IsBusy
    public bool DeletingWork { get => bdeletingWork; set { bdeletingWork = value; OnPropertyChanged(); } }

    private bool _bcheckenabled = true; //if searching directories
    public bool CheckEnable { get => _bcheckenabled; set { _bcheckenabled = value; OnPropertyChanged(); } }

    private bool _bCheckOBJDir = true; //if searching directories
    public bool BCheckOBJDir { get => _bCheckOBJDir; set { _bCheckOBJDir = value; OnPropertyChanged(); } }

    public bool WorkerRunning { get; set; } = false;//Any Background worker running

    private readonly OpenFolderDialog folderbrowser = new ();

    private string _sbtext = "Ready"; //StatusBar text
    public string SBText { get => _sbtext; set { _sbtext = value; OnPropertyChanged(); } }

    private double _pbValue = 0; //ProgressBar Value
    public double PBValue { get => _pbValue; set { _pbValue = value; OnPropertyChanged(); } }

    private bool _bIsIndm = false; //ProgressBar IsIndeterminate
    public bool IsINDM { get => _bIsIndm; set { _bIsIndm = value; OnPropertyChanged(); } }

    private string _path = string.Empty;
    public string Path { get => _path; set { _path = value; OnPropertyChanged(); } }

    private RelayCommand _browseCommand = null; //  null conditional assignmet operator ??=
    public ICommand BrowseCommand => _browseCommand ??= new RelayCommand(BrowseCmd, null);
    private void BrowseCmd(object obj)
    {
        if (folderbrowser.ShowDialog() == true)
        {
            Path = folderbrowser.FolderName;
        }

    }
//////////////////////////
    private RelayCommand _checkall = null; //  null conditional assignmet operator ??=
    public ICommand ChechAll => _checkall ??= new RelayCommand(Check_Executed, CanExecute_CheckAll);
    private void Check_Executed(object obj)
    {
        foreach (TestModel item in TestViewData)
        {
            item.Check = true;
        }

    }
    /// <summary>
    /// Determines is we can check all listview items for deletetion
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    private bool CanExecute_CheckAll(object obj)
    {
        return TestViewData.Count > 0 && !deleteWorker.IsBusy && !searchWorker.IsBusy;
    }

//////////////////////////
    private RelayCommand _uncheckall = null; //  null conditional assignmet operator ??=
    public ICommand UnCheckAll => _uncheckall ??= new RelayCommand(UnCheck_Executed, CanExecute_UnCheckAll);
    private void UnCheck_Executed(object obj)
    {
        foreach (TestModel item in TestViewData)
        {
            item.Check = false;
        }

    }
    private bool CanExecute_UnCheckAll(object obj)
    {
        return TestViewData.Count > 0 && !deleteWorker.IsBusy && !searchWorker.IsBusy;
    }
///////////////////
    private RelayCommand _helpCommand = null;
    public ICommand HelpCommand => _helpCommand ??= new RelayCommand(HelpCmd, null);
    private void HelpCmd(object obj)
    {
        //  null conditional operator ?.
        ShowMyDialog?.Invoke(this, new ShowDialogEventArgs()
        {
            Dialog = "Help"
        });

    }

    private RelayCommand _checkallCommand = null;
    public ICommand CheckAllCommand => _checkallCommand ??= new RelayCommand(CheckAllCmd, CanExecuteCheckAll);
    /// <summary>
    /// Check all checkboxes in ListBox Extension items
    /// </summary>
    /// <param name="obj"></param>
    private void CheckAllCmd(object obj)
    {
        foreach (Extension item in ExtensionCol)
        {
            item.Checked = true;
        }

    }
    private bool CanExecuteCheckAll(object obj)
    {
        if (WorkerRunning)
            return false;
        return true;
    }

    private RelayCommand _uncheckallCommand = null;
    public ICommand UnCheckAllCommand => _uncheckallCommand ??= new RelayCommand(UnCheckAllCmd, CanExecuteUnCheckAll);

    private void UnCheckAllCmd(object obj)
    {
        foreach (Extension item in ExtensionCol)
        {
            item.Checked = false;
        }

    }
    private bool CanExecuteUnCheckAll(object obj)
    {
        if (WorkerRunning)
            return false;
        return true;
    }

    private RelayCommand _moreCommand = null;
    public ICommand MoreCommand => _moreCommand ??= new RelayCommand(MoreCmd, CanExecuteMore);

    private void MoreCmd(object obj)
    {
        var handler = ShowMyDialog;
        //   var args = new ShowDialogEventArgs() { Dialog = "More"};
        ShowDialogEventArgs args;
        if (handler != null)
        {
            args = new ShowDialogEventArgs() { Dialog = "More"};
            handler(this,
                    args);

            if(args.Result == true)
            {
                string ext = args.Dialog;//dlg.txtExt.Text;
                //do not add if already in items
                if (ext == ".ext")
                    return;
                int ind = ext.IndexOf(".");
                if (ind != 0)
                    return;
                foreach (Extension item in ExtensionCol)
                {
                    if (item.Name == ext)
                        return;
                }

                ExtensionCol.Add(new Extension() {Name = ext, Checked = true});
            }
        }
        //ShowMyDialog?.Invoke(this, new ShowDialogEventArgs()
        //{
        //    Dialog = "More"
        //});


    }
    private bool CanExecuteMore(object obj)
    {
        if (WorkerRunning)
            return false;
        return true;

    }


    public MainViewModel()
    {
        AddExtensions();
        Title = Application.Current.MainWindow.Title;

        searchWorker.DoWork += SearchWorker_DoWork;//<--To Search files
        searchWorker.RunWorkerCompleted += SearchWorker_RunWorkerCompleted;
        searchWorker.WorkerSupportsCancellation = true;

        deleteWorker.DoWork += DeleteWorker_DoWork; //<--To delete files to Recycle bin
        deleteWorker.RunWorkerCompleted += DeleteWorker_RunWorkerCompleted;
        deleteWorker.ProgressChanged += DeleteWorker_ProgressChanged;
        deleteWorker.WorkerReportsProgress = true;
        deleteWorker.WorkerSupportsCancellation = true;
    }
    public void RemoveEvents()
    {
        searchWorker.DoWork -= SearchWorker_DoWork;//<--To Search files
        searchWorker.RunWorkerCompleted -= SearchWorker_RunWorkerCompleted;

        deleteWorker.DoWork -= DeleteWorker_DoWork; //<--To delete files to Recycle bin
        deleteWorker.RunWorkerCompleted -= DeleteWorker_RunWorkerCompleted;
        deleteWorker.ProgressChanged -= DeleteWorker_ProgressChanged;
    }
/////////////////////////////
    private RelayCommand _searchCommand = null;
    public ICommand SearchCmd => _searchCommand ??= new RelayCommand(Search_Executed, CanExecute_Search);

    private void Search_Executed(object obj)
    {
        if (!System.IO.Directory.Exists(Path))
        {
            string msg = Path + " is not a valid directory.";
            ShowMsgBox?.Invoke(this, new MessageEventArgs()
            {
                Message = msg,
                Caption = Title
            });
            return;
        }
        if (TestViewData.Count > 0)
            TestViewData.Clear();
        ListOfFilesToDelete.Clear();
        ListSelectedExt.Clear();
        DeletingWork = false;

        foreach (var m in ExtensionCol)
        {
          
            if (m.Checked)
            {
                ListSelectedExt.Add(m.Name);
            }
           
        }

        SBText = "Searching, please wait...";
        PBValue = 0;
        IsINDM = true;
        CheckEnable = false;//Prevents user from changing checkbox in the middle of a search
        searchWorker.RunWorkerAsync(Path);
    }
    private bool  CanExecute_Search(object obj)
    {
        return !string.IsNullOrEmpty(Path) && (!searchWorker.IsBusy && !deleteWorker.IsBusy);
    }
///////////////////////////
    private RelayCommand _stopCommand = null;
    public ICommand StopCmd => _stopCommand  ??= new RelayCommand(Stop_Executed, CanExecute_Stop);

    private void Stop_Executed(object obj)
    {
        if (DeletingWork)
            deleteWorker.CancelAsync();
        else
        {
            searchWorker.CancelAsync();
        }
    }
    private bool CanExecute_Stop(object obj)
    {
        return searchWorker.IsBusy || deleteWorker.IsBusy;
    }


    private RelayCommand _deleteCommand = null;
    public ICommand DeleteCmd => _deleteCommand  ??= new RelayCommand(Delete_Executed, CanExecute_Delete);


    private void Delete_Executed(object obj)
    {
        ListOfFilesToDelete.Clear();

        foreach (TestModel item in TestViewData)
        {
            if (item.Check == true)
            {
                ListOfFilesToDelete.Add(item.File);
            }

        }

        if (ListOfFilesToDelete.Count > 0)
        {
            SBText = string.Format("Deleting {0} files, please wait...", ListOfFilesToDelete.Count);
            DeletingWork = true;
            CheckEnable = false;
            IsINDM = false;
            PBValue = 0;
            deleteWorker.RunWorkerAsync();
        }

    }
    private bool  CanExecute_Delete(object obj)
    {
        return TestViewData.Count > 0 && !searchWorker.IsBusy && !deleteWorker.IsBusy;
    }

    private void DeleteWorker_DoWork(object sender, DoWorkEventArgs e)
    {

        double i = 0.0;
        WorkerRunning = true;
        double count = ListOfFilesToDelete.Count;
        double last = 0.0;
        double percentDone;
        //Dictionary<string,TestModel> myObjects = new Dictionary<string,TestModel>();
       //// Hashtable myObjects = new Hashtable();

        //foreach (TestModel item in TestViewData)
        //{
        //    if (item.Check == true)
        //    {
        //        myObjects.Add(item.File, item);
        //    }

        //}


        foreach (string str in ListOfFilesToDelete)
        {
            if (deleteWorker.CancellationPending)
            {
                e.Cancel = true;
                return;
            }
            if (BCheckOBJDir)
                FileSystem.DeleteDirectory(str, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            else
                FileSystem.DeleteFile(str, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);

            i++;

            //var myitem = myObjects[str];
            //Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, () =>
            //{

            //    TestViewData.Remove(myitem);

            //});


            percentDone = (double)(i / count * 100);

            if (percentDone > last)
                deleteWorker.ReportProgress((int)percentDone);

            last = percentDone;

        }
        e.Result = i;
    }

    private void SearchWorker_DoWork(object sender, DoWorkEventArgs e)
    {

        string sDir = string.Empty;

        if (e.Argument != null)
            sDir = (string)e.Argument;

        WorkerRunning = true;
        try
        {
            int mycount = 0;

            if (BCheckOBJDir)
            {
                string dir;
                bool adding = false;

                foreach (string d in System.IO.Directory.EnumerateDirectories(sDir, "*.*", System.IO.SearchOption.AllDirectories))
                {
                    if (searchWorker.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }

                    dir = d.ToLower();
                    adding = false;

                    if (dir.EndsWith("\\obj"))
                        adding = true;
                    else if (dir.EndsWith("\\.vs"))
                        adding = true;
                    else if (dir.EndsWith(".tlog"))
                        adding = true;
                    else if (dir.EndsWith(".clangtidy"))
                        adding = true;
                    else if (dir.EndsWith("\\microsoft"))//.git
                        adding = true;
                    else
                        adding = false;

                    if (adding)
                    {
                        mycount++;
                        //BeginInvoke You also are required to set the DispatcherPriority property, which determines the priority with
                        //which the delegate is executed
                        Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, () =>
                        {
                            TestViewData.Add(new TestModel()
                            {
                                Check = true,
                                File = d,

                            });
                        });

                    }

                }
            }

            if (!BCheckOBJDir)
            {

                foreach (string file in System.IO.Directory.EnumerateFiles(sDir, "*.*", System.IO.SearchOption.AllDirectories))
                {
                    if (searchWorker.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }

                    if (Check_File(file))
                    {
                        mycount++;

                        Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, () =>
                        {
                            TestViewData.Add(new TestModel()
                            {
                                Check = true,
                                File = file,
                            });
                        });
                    }

                }
            }
            e.Result = mycount;

        }
        catch (System.Exception ex)
        {
            ShowMsgBox?.Invoke(this, new MessageEventArgs()
            {
                Message = ex.Message,
                Caption = Title
            });
        }
    }
///////////////////////////////
    private void DeleteWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
    {
        PBValue = e.ProgressPercentage;
    }
    private bool Check_File(string file)
    {
        string ext;
        int ind = file.LastIndexOf(".");
        // int ind = file.IndexOf(".");// finds not the first occurrence of a character but the last
        if (ind != -1)
        {
            ext = file.Substring(ind).ToLower();
        }
        else
            return false;

        if (!ListSelectedExt.Contains(ext))
            return false;
        // string ext = System.IO.Path.GetExtension(file);//if path is null return Null if path doesn have extension return string.Empty

        if (!string.IsNullOrEmpty(ext))
        {
            if (ext == ".txt" )
            {
                if (file.EndsWith("Absolute.txt") || file.EndsWith("Manifest.txt"))
                    return true;
                return false;
            }

            return true;
        }
        return false;
    }

    private void SearchWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        WorkerRunning = false;
        CheckEnable = true;
        IsINDM = false;
        PBValue = 0;
        if (e.Error != null)
        {
            ShowMsgBox?.Invoke(this, new MessageEventArgs()
            {
                Message = e.Error.Message,
                Caption = Title
            });
        }
        else if (e.Cancelled)
        {
            SBText  = $"Search cancelled but found {TestViewData.Count} files.";
            ShowMsgBox?.Invoke(this, new MessageEventArgs()
            {
                Message = SBText,
                Caption = Title
            });
        }
        else
        {
            SBText = $"Found {e.Result} files";
            ShowMsgBox?.Invoke(this, new MessageEventArgs()
            {
                Message = SBText,
                Caption = Title
            });
        }

    }

    private void DeleteWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        WorkerRunning = false;
        CheckEnable = true;

        if (e.Error != null)
        {
            ShowMsgBox?.Invoke(this, new MessageEventArgs()
            {
                Message = e.Error.Message,
                Caption = Title
            });
        }
        else if (e.Cancelled)
        {
            SBText = "Delete Canceled";
            ShowMsgBox?.Invoke(this, new MessageEventArgs()
            {
                Message = SBText,
                Caption = Title
            });
        }
        else
        {
            int count = ListOfFilesToDelete.Count;
            ListOfFilesToDelete.Clear();

            foreach (TestModel item in TestViewData)
            {
                if (item.Check == false)
                {
                    ListOfFilesToDelete.Add(item.File);
                }

            }

            if ( TestViewData.Count > 0)
                TestViewData.Clear();//Remove all items from LisView

            //Add to ListView only unselected items
            foreach (string con in ListOfFilesToDelete)
            {
                TestViewData.Add(new TestModel()
                {
                    Check = false,
                    File = con,
                });

            }

            ListOfFilesToDelete.Clear();


            SBText = $"{count} files deleted to Recycled bin.";
            ShowMsgBox?.Invoke(this, new MessageEventArgs()
            {
                Message = SBText,
                Caption = Title
            });
        }
    }
    private void AddExtensions()
    {
        ExtensionCol.Add(new Extension() { Name = ".obj", Checked = true });
        ExtensionCol.Add(new Extension() { Name = ".res", Checked = true });
        ExtensionCol.Add(new Extension() { Name = ".suo", Checked = true });
        ExtensionCol.Add(new Extension() { Name = ".pch", Checked = true });
        ExtensionCol.Add(new Extension() { Name = ".log", Checked = true });
        ExtensionCol.Add(new Extension() { Name = ".idb", Checked = true });
        ExtensionCol.Add(new Extension() { Name = ".tlog", Checked = true });
        //ExtensionCol.Add(new Extension() { Name = ".idl", Checked = true });//interface definition language
        // ExtensionCol.Add(new Extension() { Name = ".vshost.exe", Checked = true });
        // ExtensionCol.Add(new Extension() { Name = ".vshost.exe.config", Checked = true });
        // ExtensionCol.Add(new Extension() { Name = ".vshost.exe.manifest", Checked = true });
        //ExtensionCol.Add(new Extension() { Name = ".vcxproj.filelistabsolute.txt", Checked = true });
        // ExtensionCol.Add(new Extension() { Name = "._manifest.rc", Checked = false });//have to use 2 underscore to scape 1
        ExtensionCol.Add(new Extension() { Name = ".enc", Checked = true });
        ExtensionCol.Add(new Extension() { Name = ".recipe", Checked = true });
        // ExtensionCol.Add(new Extension() { Name = ".db", Checked = true });
        ExtensionCol.Add(new Extension() { Name = ".pdb", Checked = false });
        ExtensionCol.Add(new Extension() { Name = ".command", Checked = false });
        ExtensionCol.Add(new Extension() { Name = ".txt", Checked = false });
        ExtensionCol.Add(new Extension() { Name = ".user", Checked = false });
        ExtensionCol.Add(new Extension() { Name = ".aps", Checked = true });
        ExtensionCol.Add(new Extension() { Name = ".ncb", Checked = true });
        ExtensionCol.Add(new Extension() { Name = ".opt", Checked = true });
        ExtensionCol.Add(new Extension() { Name = ".ipch", Checked = true });
        //ExtensionCol.Add(new Extension() { Name = ".sdf", Checked = false });//data base
        ExtensionCol.Add(new Extension() { Name = ".bsc", Checked = true });
        ExtensionCol.Add(new Extension() { Name = ".map", Checked = true });
        ExtensionCol.Add(new Extension() { Name = ".ipdb", Checked = true });
        ExtensionCol.Add(new Extension() { Name = ".iobj", Checked = true });
        ExtensionCol.Add(new Extension() { Name = ".clw", Checked = true });
        ExtensionCol.Add(new Extension() { Name = ".dsw", Checked = true });
        ExtensionCol.Add(new Extension() { Name = ".pgl", Checked = true });
        ExtensionCol.Add(new Extension() { Name = ".o", Checked = true });
        ExtensionCol.Add(new Extension() { Name = ".d", Checked = true });
        ExtensionCol.Add(new Extension() { Name = ".tlb", Checked = true });
        ExtensionCol.Add(new Extension() { Name = ".old", Checked = true });
        ExtensionCol.Add(new Extension() { Name = ".exp", Checked = true });
        ExtensionCol.Add(new Extension() { Name = ".ilk", Checked = true });
        ExtensionCol.Add(new Extension() { Name = ".sbr", Checked = true });
        ExtensionCol.Add(new Extension() { Name = ".rsp", Checked = true });
        ExtensionCol.Add(new Extension() { Name = ".pchast", Checked = true });
        // ExtensionCol.Add(new Extension() { Name = ".vsp", Checked = false });//performance report
        //ExtensionCol.Add(new Extension() { Name = ".psess", Checked = false });//performance session
// ExtensionCol.Add(new Extension() { Name = ".runtimeconfig.dev.json", Checked = false });
        ExtensionCol.Add(new Extension() { Name = ".json", Checked = false });
        ExtensionCol.Add(new Extension() { Name = ".ifc", Checked = true });
        //ExtensionCol.Add(new Extension() { Name = ".ifcast", Checked = true });
        ExtensionCol.Add(new Extension() { Name = ".depend", Checked = true });//CodeBlocks dependency file
//   ExtensionCol.Add(new Extension() { Name = ".ifcast", Checked = true });
        ExtensionCol.Add(new Extension() { Name = ".lastbuildstate", Checked = true });
        ExtensionCol.Add(new Extension() { Name = ".unsuccessfulbuild", Checked = true });
        ExtensionCol.Add(new Extension() { Name = ".lastcodeanalysissucceeded", Checked = true });
        ExtensionCol.Add(new Extension() { Name = ".codeanalysis", Checked = true });
        ExtensionCol.Add(new Extension() { Name = ".sarif", Checked = true });
        ExtensionCol.Add(new Extension() { Name = ".xml", Checked = false });
    }
}
}
