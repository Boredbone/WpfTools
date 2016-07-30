using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace WpfTools
{
    public class FolderSelectDialog : IDisposable
    {

        private CommonOpenFileDialog Dialog { get; }
        public string DefaultDirectory
        {
            get { return this.Dialog.DefaultDirectory; }
            set { this.Dialog.DefaultDirectory = value; }
        }

        public string SelectedPath => this.Dialog.FileName;


        public FolderSelectDialog()
        {
            var dialog = new CommonOpenFileDialog()
            {
                // フォルダーを開く設定に
                IsFolderPicker = true,
                // 読み取り専用フォルダ/コントロールパネルは開かない
                //EnsureReadOnly = false,
                AllowNonFileSystemItems = true,
                EnsurePathExists = true,
                EnsureFileExists = false,
                //Multiselect = true,
            };
            // パス指定
            //dialog.DefaultDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            this.Dialog = dialog;
        }

        public bool? ShowDialog()
        {
            var window = System.Windows.Application.Current.Windows
                .OfType<System.Windows.Window>()
                .SingleOrDefault(w => w.IsActive);

            this.Dialog.InitialDirectory = this.DefaultDirectory;

            var result = (window == null) ? this.Dialog.ShowDialog() : this.Dialog.ShowDialog(window);

            switch (result)
            {
                case CommonFileDialogResult.Ok:
                    return true;
                case CommonFileDialogResult.Cancel:
                    return false;
                default:
                    return null;
            }

            //return (result == CommonFileDialogResult.Ok) ? true
            //    : (result == CommonFileDialogResult.Cancel) ? false
            //    : null;
        }

        public void Dispose()
        {
            this.Dialog.Dispose();
        }
    }
}

