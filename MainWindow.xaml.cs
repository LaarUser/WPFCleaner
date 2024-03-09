//using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

///Converted to WPF on 3/17/2018 from Winforms version
/// Author: Luis A Ayuso Rivera
/// Revised to MVVM completely on 02/08/2024

namespace WpfProjectCleanerMvvmNet5
{

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{

    public MainWindow()
    {
        InitializeComponent();

        mv.ShowMsgBox += OnShowMessageBox;
        mv.ShowMyDialog += OnShowDialogEvent;
    }

    private void OnShowMessageBox(object sender, MessageEventArgs args)
    {
        //MessageBoxResult result =
        MessageBox.Show(args.Message, args.Caption, args.Buttons, args.Icons);
    }

    private void OnShowDialogEvent(object sender, ShowDialogEventArgs args)
    {
        string dia = args.Dialog;
        if (dia == "Help")
        {
            HelpWindow hlp = new()
            {
                Owner = this
            };
            hlp.Show();
        }

        if (dia == "More")
        {
            AddExtension dlg = new()
            {
                Owner = this
            };

            if (dlg.ShowDialog() == true)
            {
               args.Result = true;
               args.Dialog = dlg.txtExt.Text;
            }

        }
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        mv.ShowMyDialog -= OnShowDialogEvent;
        mv.ShowMsgBox -= OnShowMessageBox;

        mv.RemoveEvents();
        base.OnClosing(e);
    }

}

}
//