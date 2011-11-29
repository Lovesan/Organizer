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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Organizer.Model;
using System.Windows.Data;
using System.ComponentModel;
using System.Windows.Input;

namespace Organizer.ViewModel
{
    public class MainWindowViewModel : WorkspaceViewModel
    {
        IDialogServiceFactory _dialogFactory;
        IDialogService _dialog;
        WorkspaceCollectionViewModel<RepositoryViewModel> _repositories;
        RelayCommand _newCommand;
        RelayCommand _saveCommand;
        RelayCommand _saveAsCommand;
        RelayCommand _aboutCommand;

        public MainWindowViewModel(IDialogServiceFactory fileDialogServiceFactory)
        {
            if (null == fileDialogServiceFactory)
                throw new ArgumentNullException("fileDialogService");
            _dialogFactory = fileDialogServiceFactory;
            _dialog = _dialogFactory.CreateService();
            _repositories = new WorkspaceCollectionViewModel<RepositoryViewModel>();
            _repositories.RequestClose += this.OnRequestClose;
        }

        void New()
        {
            var r = new TaskRepository();
            var rvm = new RepositoryViewModel(r, _dialogFactory);
            _repositories.Add(rvm);
            SetActiveRepository(rvm);
        }

        public ICommand NewCommand
        {
            get
            {
                if (null == _newCommand)
                    _newCommand = new RelayCommand(param => this.New());
                return _newCommand;
            }
        }

        protected override void OnRequestOpen(object sender, WorkspaceRequestEventArgs e)
        {
            String filename = _dialog.OpenFileDialog("Repository files|*.xml|All files|*.*");
            if (null != filename)
            {
                var r = TaskRepository.Load(filename);
                if (null == (r as IDataErrorInfo).Error)
                {
                    var rvm = new RepositoryViewModel(r, _dialogFactory, filename);
                    _repositories.Add(rvm);
                    SetActiveRepository(rvm);
                }
                else
                {
                    _dialog.MessageBox(
                        (r as IDataErrorInfo).Error,
                        "Error loading repository",
                        MessageBoxDialogButtons.Ok,
                        MessageBoxDialogIcon.Error);
                }
            }
        }

        bool CanSave()
        {
            var rvm = GetActiveRepository();
            if(null == rvm)
                return false;
            return rvm.SaveCommand.CanExecute(rvm);
        }

        void Save()
        {
            var rvm = GetActiveRepository();
            if (null != rvm)
                rvm.SaveCommand.Execute(rvm);
        }

        public ICommand SaveCommand
        {
            get
            {
                if (null == _saveCommand)
                    _saveCommand = new RelayCommand(param => this.Save(), param => this.CanSave());
                return _saveCommand;
            }
        }

        void SaveAs()
        {
            var rvm = GetActiveRepository();
            if (null != rvm)
                rvm.SaveAsCommand.Execute(rvm);
        }

        public ICommand SaveAsCommand
        {
            get
            {
                if (null == _saveAsCommand)
                    _saveAsCommand = new RelayCommand(param => this.SaveAs(), param => this.CanSave());
                return _saveAsCommand;
            }
        }

        void About()
        {
            _dialog.MessageBox(
                "Simple task organizer.\n" +
                "Copyright (C) 2011, Dmitry Ignatiev <lovesan.ru at gmail.com>",
                "About",
                MessageBoxDialogButtons.Ok,
                MessageBoxDialogIcon.Information);
        }

        public ICommand AboutCommand
        {
            get
            {
                if (null == _aboutCommand)
                    _aboutCommand = new RelayCommand(param => this.About());
                return _aboutCommand;
            }
        }

        protected override void OnRequestClose(object sender, WorkspaceRequestEventArgs e)
        {
            if (this == e.Workspace)
            {
                if (_repositories.Any(rvm => !rvm.IsSaved))
                {
                    bool? r = _dialog.MessageBox(
                        "Some repositories were not saved. Exit anyway?",
                        "Modified repositories exist.",
                        MessageBoxDialogButtons.OkCancel,
                        MessageBoxDialogIcon.Question);
                    if (r.HasValue && r.Value)
                        base.OnRequestClose(this, e);
                }
                else
                {
                    _repositories.Clear();
                    base.OnRequestClose(this, e);
                }
            }
            else
            {
                _repositories.Remove(e.Workspace as RepositoryViewModel);
            }
        }

        void SetActiveRepository(RepositoryViewModel ws)
        {
            var cv = CollectionViewSource.GetDefaultView(this.Repositories);
            if (null != cv)
                cv.MoveCurrentTo(ws);
        }

        RepositoryViewModel GetActiveRepository()
        {
            var cv = CollectionViewSource.GetDefaultView(this.Repositories);
            return (null == cv) ? null : cv.CurrentItem as RepositoryViewModel;
        }

        public WorkspaceCollectionViewModel<RepositoryViewModel> Repositories
        {
            get { return _repositories; }
        }

        protected override void OnDispose()
        {
            _repositories.RequestClose -= this.OnRequestClose;
            _repositories.Dispose();
            base.OnDispose();
        }
    }
}
