using System;
using System.Linq;
using System.Windows;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using AppPackageInfo;
using Microsoft.Win32;

namespace AppPackageInfoGUI
{
    public partial class MainWindow : Window
    {
        readonly string[] extensionTypes = new string[] { ".appxmanifest", ".xml", ".appx", ".appxbundle", ".msix", ".msixbundle" };
        string lastSID = "";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void FileOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "Files (*.appx; *.appxmanifest; *.appxbundle; *.msix; *.msixbundle; *.xml)|*.appx; *.appxmanifest; *.appxbundle; *.msix; *.msixbundle; *.xml"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                OpenFile(openFileDialog.FileName);
            }
        }

        private void FilePanel_Drop(object sender, DragEventArgs e)
        {

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                OpenFile(files[0]);
            }
        }

        private void OpenFile(string file)
        {
            string fileExtension = Path.GetExtension(file);
            if (extensionTypes.Contains(fileExtension))
            {
                ReadFile(file);
            } else
            {
                ShowMessage("Invalid Filetype: " + fileExtension);
            }
        }

        private void ReadFile(string path)
        {
            string fileContents = File.ReadAllText(path);
            var doc = XDocument.Parse(fileContents);
            var ns = new XmlNamespaceManager(new NameTable());
            ns.AddNamespace("x", "http://schemas.microsoft.com/appx/manifest/foundation/windows10");
            var identity = doc.XPathSelectElement("/x:Package/x:Identity", ns);
            var packageName = identity?.Attribute("Name")?.Value;
            string fileResults = "";
            fileResults += $"Package/Identity/Name = {packageName}";
            var publisher = identity?.Attribute("Publisher")?.Value;
            fileResults += $"\n\nPackage/Identity/Publisher = {publisher}";
            var publisherDisplayName = doc.XPathSelectElement("/x:Package/x:Properties/x:PublisherDisplayName", ns)?.Value;
            fileResults += $"\n\nPackage/Properties/PublisherDisplayName = {publisherDisplayName}";

            var pfn = AppxPackagingHelper.GetPfnFromPackageName(packageName, publisher);
            fileResults += $"\n\nPackage Family Name (PFN) = {pfn}";
            var sid = AppxPackagingHelper.GetSidFromPackageFamilyName(pfn);
            fileResults += $"\n\nPackage SID = {sid}";
            lastSID = sid;

            FileResults.Text = fileResults;
            SIDCopy.Visibility = Visibility.Visible;
        }

        private void SIDCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(lastSID);
            ShowMessage("Package SID copied to clipboard!");
        }

        private void ShowMessage(string message, string title = "Warning")
        {
            MessageBox.Show(message, title);
        }
    }
}
