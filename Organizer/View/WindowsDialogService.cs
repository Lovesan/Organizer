// Copyright (C) 2011, Dmitry Ignatiev <lovesan.ru at gmail.com>

// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use, copy,
// modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT.  IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE

using System;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Windows;

namespace Organizer.View
{
    public class WindowsFileDialogService : IDialogService
    {
        OpenFileDialog _openFileDialog;
        SaveFileDialog _saveFileDialog;

        public WindowsFileDialogService()
        {
            _openFileDialog = new OpenFileDialog();
            _openFileDialog.Multiselect = false;
            _openFileDialog.ValidateNames = true;
            _saveFileDialog = new SaveFileDialog();
            _saveFileDialog.OverwritePrompt = true;
            _saveFileDialog.ValidateNames = true;
        }

        public String OpenFileDialog(String filter)
        {
            _openFileDialog.Filter = filter;            
            bool? result = _openFileDialog.ShowDialog();
            String filename = null;
            if (result.HasValue && result.Value)
                filename = _openFileDialog.FileName;
            return filename;
        }

        public String SaveFileDialog(String filter)
        {
            _saveFileDialog.Filter = filter;
            bool? result = _saveFileDialog.ShowDialog();
            String filename = null;
            if (result.HasValue && result.Value)
                filename = _saveFileDialog.FileName;
            return filename;
        }


        public bool? MessageBox(String text, String caption, MessageBoxDialogButtons buttons, MessageBoxDialogIcon icon)
        {
            bool? result = null;
            if(null == text)
                text = "";
            if(null == caption)
                caption = "";
            MessageBoxButton mbButtons = MessageBoxButton.OK;
            MessageBoxImage mbIcon = MessageBoxImage.None;
            switch (buttons)
            {
                case MessageBoxDialogButtons.OkCancel:
                    mbButtons = MessageBoxButton.OKCancel;
                    break;
            }
            switch (icon)
            {
                case MessageBoxDialogIcon.Information:
                    mbIcon = MessageBoxImage.Information;
                    break;
                case MessageBoxDialogIcon.Question:
                    mbIcon = MessageBoxImage.Question;
                    break;
                case MessageBoxDialogIcon.Warning:
                    mbIcon = MessageBoxImage.Warning;
                    break;
                case MessageBoxDialogIcon.Error:
                    mbIcon = MessageBoxImage.Error;
                    break;
            }
            MessageBoxResult mbResult = System.Windows.MessageBox.Show(text, caption, mbButtons, mbIcon);
            switch (buttons)
            {
                case MessageBoxDialogButtons.OkCancel:
                    switch (mbResult)
                    {
                        case MessageBoxResult.OK:
                            result = true;
                            break;
                        case MessageBoxResult.Cancel:
                            result = false;
                            break;
                    }
                    break;
                case MessageBoxDialogButtons.Ok:
                    switch (mbResult)
                    {
                        case MessageBoxResult.OK:
                            result = true;
                            break;
                    }
                    break;
            }
            return result;
        }

        public void Dispose()
        {
        }
    }
}
