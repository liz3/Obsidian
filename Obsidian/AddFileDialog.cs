using System;
using Gtk;

namespace Obsidian
{
    public partial class AddFileDialog : Dialog
    {
        private Action<string, string> addFileCallback;

     

        public AddFileDialog(Action<string, string> addFileCallback)
        {
            this.addFileCallback = addFileCallback;
        this.Build();
        }

        protected void btnPressed(object sender, EventArgs e)
        {
            addFileCallback(entry1.Text, entry3.Text);
            Hide();
        }

        protected void choosePathPressed(object sender, EventArgs e)
        {
            var filechooser = new FileChooserDialog("Choose the file to add", this,
        FileChooserAction.Open,
        "Cancel", ResponseType.Cancel,
        "Open", ResponseType.Accept);
            if (filechooser.Run() == (int)ResponseType.Accept)
            {
                entry1.Text = filechooser.Filename;
            }

            filechooser.Destroy();
        }

        protected void cancelPressed(object sender, EventArgs e)
        {
            Hide();
        }
    }
}
