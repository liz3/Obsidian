using System;
using System.Collections.Generic;
using System.IO;
using Gtk;
using Obsidian;
using Obsidian.Api.IO.WAD;
using Obsidian.Core;

public partial class MainWindow : Gtk.Window
{
    private Manager manager = new Manager();
    private Dictionary<string, TableNode> hiddenEntrys = new Dictionary<string, TableNode>();
    public MainWindow() : base(Gtk.WindowType.Toplevel)
    {

        Build();
        setupGuiElements();

    }
    public class TableNode : TreeNode
    {
        
        string name;
        string type;
        string size;
        string fileDirection;
        public WADEntry entry {get; private set; }

        public TableNode(string name, string type, string size, string fileDirection, WADEntry entry)
        {
            this.name = name;
            this.type = type;
            this.size = size;
            this.fileDirection = fileDirection;
            this.entry = entry;
        }

        [TreeNodeValue(Column = 0)]
        public string colName => name;

        [TreeNodeValue(Column = 1)]
        public string colType => type;

        [TreeNodeValue(Column = 2)]
        public string colSize { get { return size; } set { size = value; }}

        [TreeNodeValue(Column = 3)]
        public string colFileDirection => fileDirection;
    }
 
    protected void OnDeleteEvent(object sender, DeleteEventArgs a)
    {
        Application.Quit();
        a.RetVal = true;
    }



    protected void setupGuiElements() {

        nodeview.AppendColumn("Name", new Gtk.CellRendererText(), "text", 0);
        nodeview.AppendColumn("Type", new Gtk.CellRendererText(), "text", 1);
        nodeview.AppendColumn("Size", new Gtk.CellRendererText(), "text", 2);
        nodeview.AppendColumn("File Direction", new Gtk.CellRendererText(), "text", 3);
        nodeview.ShowAll();
        nodeview.NodeStore = new NodeStore(typeof(TableNode));
        nodeview.NodeSelection.Changed += new System.EventHandler(onSelectionChanged);

   }

    protected void openFileClicked(object sender, EventArgs e)
    {
        if(manager.cleanUp()) {
            
            var filechooser = new FileChooserDialog("Choose the Wad file to open",this,
            FileChooserAction.Open,
            "Cancel", ResponseType.Cancel,
            "Open", ResponseType.Accept);

            if (filechooser.Run() == (int)ResponseType.Accept)
            {
                if(manager.openWadFile(filechooser.Filename)) {
                    var store = new NodeStore(typeof(TableNode));

                    foreach (var entry in manager.mapEntries)
                    {
                        store.AddNode(new TableNode(entry.Key, entry.Value.Type.ToString(), "" + entry.Value.UncompressedSize, "", entry.Value));
                    }
                    nodeview.NodeStore = store;
                }
               
            }

            filechooser.Destroy();
        }
    }

    protected void exportAllClicked(object sender, EventArgs e)
    {
        if(manager.active) {
            var filechooser = new FileChooserDialog("Choose the folder to extract", this,
         FileChooserAction.SelectFolder,
         "Cancel", ResponseType.Cancel,
         "Select", ResponseType.Accept);
            if (filechooser.Run() == (int)ResponseType.Accept) {
                manager.exportAll(filechooser.Filename);
            }
            filechooser.Destroy();

             
        }
    }

    protected void removeEntryClicked(object sender, EventArgs e)
    {
        if (manager.active && nodeview.NodeSelection.SelectedNode != null)
        {

            TableNode selected = (MainWindow.TableNode)nodeview.NodeSelection.SelectedNode;

            manager.activeWad.RemoveEntry(selected.entry);
            manager.mapEntries.Remove(selected.colName);
            nodeview.NodeStore.RemoveNode(selected);
        }
    }

 

    protected void saveClicked(object sender, EventArgs e)
    {
        if(manager.active) {
            var filechooser = new FileChooserDialog("Choose Location to save", this,
         FileChooserAction.Save,
         "Cancel", ResponseType.Cancel,
         "Open", ResponseType.Accept);

            if (filechooser.Run() == (int)ResponseType.Accept)
            {
                
                Console.WriteLine(manager.writeFile(filechooser.Filename));
            }

            filechooser.Destroy();
        }
    }

    protected void filterChangedEvent(object sender, EventArgs e)
    {
        string text = searchBox.Text;

        if(manager.active) {

            if(text == "") {

                foreach(var x in hiddenEntrys) {
                    nodeview.NodeStore.AddNode(x.Value);

                }
                hiddenEntrys.Clear();
                return;
            }
            var toRemove = new List<TableNode>();

            foreach(TableNode entry in nodeview.NodeStore) {
                if(!entry.colName.ToLower().Contains(text.ToLower())) {
                    toRemove.Add(entry);
                }
            }
            foreach(var x in toRemove) {
                nodeview.NodeStore.RemoveNode(x);
                hiddenEntrys.Add(x.colName, x);
            }
            toRemove.Clear();
            foreach (var entry in hiddenEntrys)
            {
                if(entry.Key.ToLower().Contains(text.ToLower())) {
                    toRemove.Add(entry.Value);
                }
            }
            foreach (var x in toRemove)
            {
                hiddenEntrys.Remove(x.colName);
                nodeview.NodeStore.AddNode(x);
            }

        }
    }

    protected void clearUp(object sender, EventArgs e)
    {
        manager.cleanUp();
        nodeview.NodeStore?.Clear();
        hiddenEntrys.Clear();
    }

    protected void modifyDataClicked(object sender, EventArgs e)
    {

        if ((MainWindow.TableNode)nodeview.NodeSelection.SelectedNode == null) return;
        TableNode selected = (MainWindow.TableNode)nodeview.NodeSelection.SelectedNode;

        var filechooser = new FileChooserDialog("Choose the file to replace the entry with", this,
            FileChooserAction.Open,
            "Cancel", ResponseType.Cancel,
            "Open", ResponseType.Accept);

        if (filechooser.Run() == (int)ResponseType.Accept)
        {
            var data = File.ReadAllBytes(filechooser.Filename);
            selected.entry.EditData(data);
            selected.colSize = selected.entry.UncompressedSize + "";
            nodeview.NodeStore.RemoveNode(selected);
            nodeview.NodeStore.AddNode(selected);
            nodeview.NodeSelection.SelectNode(selected);
        }

        filechooser.Destroy();
    }

    protected void addFileCallback(string filepath, string path) {

        if (filepath == "" || path == "") return;
        if (manager.active)
        {
            var key = manager.addFile(filepath, path);

            if(key != "") {
                var entry = manager.mapEntries[key];

                nodeview.NodeStore.AddNode(new TableNode(key, entry.Type.ToString(), entry.UncompressedSize + "", "", entry));
            }
        }
                            
    }
    protected void addFileClicked(object sender, EventArgs e) {
        if (manager.active) new AddFileDialog(this.addFileCallback);
    }

    protected void exportSelected(object sender, EventArgs e)
    {
        if ((MainWindow.TableNode)nodeview.NodeSelection.SelectedNode == null) return;
        TableNode selected = (MainWindow.TableNode)nodeview.NodeSelection.SelectedNode;

        var filechooser = new FileChooserDialog("Choose the file to replace the entry with", this,
               FileChooserAction.Save,
            "Cancel", ResponseType.Cancel,
            "Open", ResponseType.Accept);

        if (filechooser.Run() == (int)ResponseType.Accept)
        {
            File.WriteAllBytes(filechooser.Filename, selected.entry.GetContent(true));

        }
        filechooser.Destroy();
    }

    void onSelectionChanged(object o, EventArgs args)
    {
        if (o == null) return;
        NodeSelection selection = (NodeSelection)o;
        TableNode node = (TableNode)selection.SelectedNode;


        Console.WriteLine("Selection Changed");

    }

    protected void exportHashtable(object sender, EventArgs e)
    {
        if(manager.active) {

            var filechooser = new FileChooserDialog("Choose the file to save the Hashtable to", this,
            FileChooserAction.Save,
         "Cancel", ResponseType.Cancel,
         "Open", ResponseType.Accept);

            if (filechooser.Run() == (int)ResponseType.Accept)
            {
                manager.writeHashtable(filechooser.Filename);

            }
            filechooser.Destroy();
        }
    }

    protected void importHashtable(object sender, EventArgs e)
    {
        if (manager.active)
        {

            var filechooser = new FileChooserDialog("Choose the file to import the Hashtable from", this,
            FileChooserAction.Open,
         "Cancel", ResponseType.Cancel,
         "Open", ResponseType.Accept);

            if (filechooser.Run() == (int)ResponseType.Accept)
            {
                manager.importHashTable(filechooser.Filename);

            }
            filechooser.Destroy();
        }
    }

    protected void importFolder(object sender, EventArgs e)
    {
        if (manager.active)
        {

            var filechooser = new FileChooserDialog("Choose the Folder to import", this,
            FileChooserAction.SelectFolder,
         "Cancel", ResponseType.Cancel,
         "Open", ResponseType.Accept);

            if (filechooser.Run() == (int)ResponseType.Accept)
            {
                var result = manager.importFolder(filechooser.Filename);

                foreach(string path in result) {
                    var entry = manager.mapEntries[path];

                    nodeview.NodeStore.AddNode(new TableNode(path, entry.Type.ToString(), entry.UncompressedSize + "", "", entry));
                }
            }
            filechooser.Destroy();
        }
    }   
}
