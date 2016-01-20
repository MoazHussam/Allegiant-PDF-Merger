using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using MahApps.Metro.Controls;
using WPF.JoshSmith.ServiceProviders.UI;
using System.Collections.ObjectModel;
using AllegiantPDFMerger;
using System.IO;
using System.Diagnostics;
using Outlook = NetOffice.OutlookApi;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using MahApps.Metro;
//using System.Runtime.InteropServices;

namespace AllegiantPDFMergerFinal
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        UserMesseging messeging;
        ObservableCollection<Tabs> tabs = new ObservableCollection<Tabs>();

        public MainWindow()
        {

            InitializeComponent();

            this.UseLayoutRounding = true;

            ThemeManager.ChangeTheme(this, new MahApps.Metro.Accent("cobalt", new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Accents/cobalt.xaml")), Theme.Light);

            tabs.Add(new Tabs(tabs.Count + 1));
            tabcontrol.ItemsSource = tabs;
        }

        private void TabItem_DragEnter(object sender, DragEventArgs e)
        {
            TabItem item = (TabItem)sender;

            item.IsSelected = true;
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string destFolder = getDestinationFolder();
            messeging = new UserMesseging(textBlockMessege, textBlockTip);
            if (destFolder == "") messeging.Messege = "Please Choose Destination Folder";
            else
            {
                messeging.Messege = "Ready";
                messeging.Tip = "Current Destination Folder: " + Path.GetFileName(destFolder);
            }
            addHotKey();
        }

        private void listview_Loaded(object sender, RoutedEventArgs e)
        {
            ListView listview = sender as ListView;
            ListViewDragDropManager<ListedFiles> DragAndDropManager = new ListViewDragDropManager<ListedFiles>(listview, true);
            DragAndDropManager.ProcessDrop += DragAndDropManager_ProcessDrop;
        }

        void DragAndDropManager_ProcessDrop(object sender, ProcessDropEventArgs<ListedFiles> e)
        {
            int higherIdx = Math.Max(e.OldIndex, e.NewIndex);
            int lowerIdx = Math.Min(e.OldIndex, e.NewIndex);

            if (lowerIdx < 0)
            {
                // The item came from the lower ListView
                // so just insert it.
                e.ItemsSource.Insert(higherIdx, e.DataItem);
            }
            else
            {
                // null values will cause an error when calling Move.
                // It looks like a bug in ObservableCollection to me.
                if (e.ItemsSource[lowerIdx] == null ||
                    e.ItemsSource[higherIdx] == null)
                    return;

                e.ItemsSource.Move(e.OldIndex, e.NewIndex);
            }

            // Set this to 'Move' so that the OnListViewDrop knows to 
            // remove the item from the other ListView.
            e.Effects = DragDropEffects.Move;
        }

        private void listview_Drop(object sender, DragEventArgs e)
        {
            ListView listview = sender as ListView;
            string[] o = e.Data.GetFormats();

            if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                {
                    ListedFiles newListedFile = new ListedFiles(file);
                    if (newListedFile.fileType != FileType.PDF && newListedFile.fileType != FileType.Word && newListedFile.fileType != FileType.Html && newListedFile.fileType != FileType.Text) continue;
                    (listview.ItemsSource as ObservableCollection<ListedFiles>).Add(newListedFile);
                }
            }
            else if (e.Data.GetDataPresent("FileGroupDescriptor") && e.Data.GetDataPresent("RenPrivateMessages"))
            {
                Outlook.Application _Outlook = null;
                Outlook.MailItem mi = null;

                string newFileName = "";
                //try
                //{
                _Outlook = new Outlook.Application();
                Outlook._Explorer oExplorer = _Outlook.ActiveExplorer();
                Outlook.Selection oSelection = oExplorer.Selection;

                foreach (object item in oSelection)
                {
                    try
                    {
                        mi = (Outlook.MailItem)item;

                        newFileName = mi.Subject + ".doc";
                        int i = 1;

                        foreach (char c in Path.GetInvalidFileNameChars())
                        {
                            newFileName = newFileName.Replace(c, '_');
                        }

                        newFileName = Path.Combine(Path.GetTempPath(), newFileName);

                        while (File.Exists(newFileName)) newFileName = Path.Combine(Path.GetTempPath(), mi.Subject + String.Format("({0})", i++) + ".doc");

                        mi.SaveAs(newFileName, Outlook.Enums.OlSaveAsType.olDoc);

                        ListedFiles newListedFile = new ListedFiles(newFileName, true);
                        if (newListedFile.fileType == FileType.PDF || newListedFile.fileType == FileType.Word || newListedFile.fileType == FileType.Text)
                            (listview.ItemsSource as ObservableCollection<ListedFiles>).Add(newListedFile);
                    }
                    catch
                    {
                        messeging.Messege = "Cannot grab email message, check security options in outlook";
                    }
                }
                //}
                //catch (Exception ex)
                //{
                //    MessageBox.Show("Error in method name listview_drop\nnewFileName :" + newFileName + "\nError msg :" + ex.Message, "Just screenshot this and execution will continue as normal", MessageBoxButton.OK, MessageBoxImage.Error);
                //}
            }
            else if (e.Data.GetDataPresent("FileGroupDescriptor") && e.Data.GetDataPresent("FileContents"))
            {
                string theFile = "";
                string newFileName = "";
                try
                {
                    object s = e.Data.GetData("FileGroupDescriptor");

                    Stream theStream = (Stream)e.Data.GetData("FileGroupDescriptor");
                    byte[] fileGroupDescriptor = new byte[512];
                    theStream.Read(fileGroupDescriptor, 0, 512);

                    StringBuilder fileName = new StringBuilder("");

                    for (int i = 76; fileGroupDescriptor[i] != 0; i++)
                    { fileName.Append(Convert.ToChar(fileGroupDescriptor[i])); }

                    theFile = fileName.ToString();

                    MemoryStream fileContents = (MemoryStream)e.Data.GetData("FileContents");

                    newFileName = Path.Combine(Path.GetTempPath(), theFile);
                    FileStream outStream = File.Create(newFileName);
                    fileContents.WriteTo(outStream);
                    outStream.Close();

                    ListedFiles newListedFile = new ListedFiles(newFileName, true);
                    if (newListedFile.fileType == FileType.PDF || newListedFile.fileType == FileType.Word)
                        (listview.ItemsSource as ObservableCollection<ListedFiles>).Add(newListedFile);
                }

                catch (Exception ex)
                {
                    //MessageBox.Show("Error in method name listview_drop\ntheFile : " + theFile + "\nnewFileName :" + newFileName + "\nError msg :" + ex.Message, "Just screenshot this and execution will continue as normal", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btn_NewFile_Click(object sender, RoutedEventArgs e)
        {
            this.tabs.Add(new Tabs(tabs.Count + 1));
            tabcontrol.SelectedIndex = (tabcontrol.ItemsSource as ObservableCollection<Tabs>).Count - 1;
        }

        private void btn_Remove_Click(object sender, RoutedEventArgs e)
        {
            ListView lbx = FindVisualChildByName<ListView>(this.tabcontrol, "listview");
            if (lbx != null)
            {
                System.Collections.IList selectedFiles = lbx.SelectedItems;
                ObservableCollection<ListedFiles> filesTodelete = new ObservableCollection<ListedFiles>();


                foreach (ListedFiles file in selectedFiles)
                {
                    filesTodelete.Add(file);
                }

                foreach (ListedFiles file in filesTodelete)
                {
                    (lbx.ItemsSource as ObservableCollection<ListedFiles>).Remove(file);
                }
            }
        }

        private void btn_Merge_Click(object sender, RoutedEventArgs e)
        {
            merge(tabcontrol.SelectedItem as Tabs, getFileName(), true);
        }

        private async Task<bool> merge(Tabs selectedTab, string outFile, bool clearList)
        {
            if (selectedTab == null || outFile == "") return false;

            ObservableCollection<ListedFiles> listedFiles = selectedTab.listedFiles;
            if (listedFiles.Count <= 0) return false;

            List<PDFFiles> pdfFiles = new List<PDFFiles>();

            messeging.Messege = "Converting";

            string errorMsg = "";

            Task<bool> convertingTask = Task.Run(() =>
                {
                    foreach (ListedFiles listedFile in listedFiles)
                    {
                        if (listedFile.PDFFile == null)
                        {
                            errorMsg += "File " + listedFile.fileName + " cannot be converted and will be ommited from the merged file\n";
                            continue;
                        }
                        pdfFiles.Add(listedFile.PDFFile);
                    }
                    return true;
                });

            if (await convertingTask) messeging.Messege = "Merging";
            string resultMessage = "";

            Task<bool> mergingTask = convertingTask.ContinueWith((t) =>
                {
                    //if (errorMsg != "") MessageBox.Show(errorMsg, "Just screenshot this error report, excution will continue as normal", MessageBoxButton.OK, MessageBoxImage.Warning);
                    if (pdfFiles.Count == 0) return true;

                    bool mergeSucceeded = false;
                    

                    try
                    {
                        mergeSucceeded = PDFFiles.Merge(pdfFiles, outFile);
                    }
                    catch (System.IO.FileLoadException ex)
                    {
                        resultMessage = ex.Message;
                    }

                    return mergeSucceeded;
                });

            if (await mergingTask)
            {
                if(clearList) listedFiles.Clear();
                messeging.Messege = "Done";
            }
            else
            {
                if (resultMessage != "") messeging.Messege = resultMessage;
                else messeging.Messege = "An error has occurred and cannot merge files";
            }

            return await mergingTask;
        }

        private string getFileName()
        {
            List<string> fileName = getFileName(1);
            if (fileName == null) return "";
            else return fileName[0];
        }

        private T FindVisualChildByName<T>(DependencyObject parent, string name) where T : FrameworkElement
        {
            T child = default(T);
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var ch = VisualTreeHelper.GetChild(parent, i);
                child = ch as T;
                if (child != null && child.Name == name)
                    break;
                else
                    child = FindVisualChildByName<T>(ch, name);

                if (child != null) break;
            }
            return child;
        }



        private List<string> getFileName(int numberOfFiles)
        {
            string date = DateTime.Now.ToString("ddMMyy");
            string destFolder = getDestinationFolder();

            if (!Directory.Exists(destFolder))
            {
                messeging.Messege = "Invalid Destination Folder";
                return null;
            }

            int x = 1;
            List<string> fileName = new List<string>();

            string tempName = Path.Combine(destFolder, String.Format("Bundle_{0}({1}).pdf", date, x));
            while (File.Exists(tempName))
            {
                x++;
                tempName = Path.Combine(destFolder, String.Format("Bundle_{0}({1}).pdf", date, x));
            }

            do
            {
                fileName.Add(tempName);
                x++;
                numberOfFiles--;
                tempName = Path.Combine(destFolder, String.Format("Bundle_{0}({1}).pdf", date, x));
            } while (numberOfFiles > 0);

            return fileName;
        }

        private void btn_MergeAll_Click(object sender, RoutedEventArgs e)
        {
            List<string> fileNames = getFileName(tabcontrol.Items.Count);
            if (fileNames == null) return;

            int i = 0;
            foreach (Tabs selectedTab in tabs)
            {
                merge(selectedTab, fileNames[i++], true);
            }
        }

        private void btn_Settings_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog browse = new System.Windows.Forms.FolderBrowserDialog();
            browse.Description = "Choose destination Folder:";
            browse.ShowNewFolderButton = true;
            System.Windows.Forms.DialogResult result = browse.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                setDestinationFolder(browse.SelectedPath);
                messeging.Tip = "Current Destination Folder: " + Path.GetFileName(getDestinationFolder());
            }
        }

        private string getDestinationFolder()
        {
            return Properties.Settings.Default.DestinationFolder;
        }

        private void setDestinationFolder(string destFolder)
        {
            if (!Directory.Exists(destFolder))
            {
                messeging.Messege = "Invalid Directory";
                return;
            }
            Properties.Settings.Default.DestinationFolder = destFolder;
            Properties.Settings.Default.Save();
            messeging.Messege = "Ready";
        }

        #region // Adding Hotkeys

        public static class User32
        {
            [DllImport("user32.dll")]
            internal static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

            [DllImport("user32.dll")]
            internal static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        }

        private void addHotKey()
        {
            IntPtr _hWnd;
            _hWnd = new WindowInteropHelper(this).Handle;

            bool x = User32.RegisterHotKey(_hWnd, this.GetType().GetHashCode(), 3, (int)'M');
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {

            if (msg == 0x0312)
            {
                if (this.IsVisible) this.Hide();
                else this.Show();
            }

            return IntPtr.Zero;
        }

        #endregion

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void btn_ClearAll_Click(object sender, RoutedEventArgs e)
        {
            tabs.Clear();
            tabs.Add(new Tabs(tabs.Count + 1));
            tabcontrol.SelectedIndex = tabs.Count - 1;
            messeging.Messege = "Ready";
            //(this.tabcontrol.SelectedItem as Tabs).listedFiles.Clear();
        }

        private void btn_OpenFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ListView lbx = FindVisualChildByName<ListView>(this.tabcontrol, "listview");
                if (lbx != null && lbx.SelectedItem != null)
                {
                    System.Diagnostics.Process.Start((lbx.SelectedItem as ListedFiles).filePath);
                }
            }
            catch
            {
                messeging.Messege = "Can't open file";
            }
        }

        private async void btn_Preview_Click(object sender, RoutedEventArgs e)
        {
            string tempFileName = Path.ChangeExtension(Path.GetTempFileName(), "pdf");
            Task<bool> mergeComplete = merge(tabcontrol.SelectedItem as Tabs, tempFileName, false);

            if(await mergeComplete)
                System.Diagnostics.Process.Start(tempFileName);
        }

        private void btn_ArrowUP_Click(object sender, RoutedEventArgs e)
        {
            ListView lbx = FindVisualChildByName<ListView>(this.tabcontrol, "listview");
            if (lbx != null)
            {
                int index = lbx.SelectedIndex;
                if (index < 1) return;
                (lbx.ItemsSource as ObservableCollection<ListedFiles>).Move(index, index - 1);
            }
        }

        private void btn_ArrowDown_Click(object sender, RoutedEventArgs e)
        {
            ListView lbx = FindVisualChildByName<ListView>(this.tabcontrol, "listview");
            if (lbx != null)
            {
                ObservableCollection<ListedFiles> listedFiles = lbx.ItemsSource as ObservableCollection<ListedFiles>;
                int index = lbx.SelectedIndex;
                if (index < 0 || index >= listedFiles.Count - 1) return;
                (lbx.ItemsSource as ObservableCollection<ListedFiles>).Move(index, index + 1);
            }
        }
    }
}
